# Text Queries

## TODO

### Completions

Right now, you can only invoke completions by passing in a context.

How would we bind this to a query editor?

### All/Any Syntax

```text
assignee:any(immol, artl, ericstj)
label:all(feature, untriaged)
```

We'd lower those into:

```
(assignee:immol OR assignee:artl OR assignee:ericstj)
(label:feature AND label:untriaged)
```

## Introduction

This library supports a query syntax very similar to GitHub issues or Lucene,
such as:

    is:open hello body:test

It supports:

* Simple text filters (such as `hello` or `"a longer text"`)
* Field-qualified text filters (such as `body:test` or `body:"some text"`)
* Field-qualified keywords (such as `is:open` or `no:assignee`)
* Combinations with `AND` and `OR` as well as parenthesis
    - `AND` is optional `text AND field:value` is the same as `text field:value`
* Logical negation (such `NOT is:open` or `-label:test`)
    - `-` and `NOT` are equivalent

You can translate the queries into `Func<T, bool>` (for in-memory filtering),
`Expression<Func<T, bool>>` (if you use an OR mapper like EF Core), or your own
representation.

## Example: in-memory processing using `Func<T, bool>`

```C#
public class Issue
{
    public bool IsOpen { get; }
    public string Title { get; }
    public string Body { get; }
    public IReadOnlyList<string> Labels { get; }
}

public class IssueQueryContext : PredicateQueryContext<Issue>
{
    public static IssueQueryContext Instance { get; } = new();

    [QueryFieldHandler("is:open", Description = "Checks for open issues")]
    public static Func<Issue, bool> IsOpen()
    {
        return issue => issue.IsOpen;
    }

    [QueryFieldHandler("is:closed", Description = "Checks for closed issues")]
    public static Func<Issue, bool> IsClosed()
    {
        return issue => !issue.IsOpen;
    }

    [QueryFieldHandler("label", Description = "Checks for issues with a specific label")]
    public static Func<Issue, bool> HasLabel(string label)
    {
        return issue => issue.Labels.Contains(label);
    }

    [QueryFieldHandler("no:label", Description = "Checks for issues that have no labels")]
    public static Func<Issue, bool> HasNoLabel()
    {
        return issue => !issue.Labels.Any();
    }

    protected override Func<Issue, bool> GenerateText(string text)
    {
        return issue => issue.Title.Contains(text) ||
                        issue.Body.Contains(text);
    }
}

public IEnumerable<Issue> QueryIssues(IEnumerable<Issue> issues, string queryText)
{
    Func<Issue, bool> predicate = IssueQueryContext.Instance.Generate(queryText);
    return issues.Where(predicate);
}
```

With that, you can run queries like those:

* `is:open` -- Finds all open issues
* `is:closed` -- Finds all closed issues
* `is:open label:bug feature` -- Finds all open issues labeled `bug` whose title
  or body contains the word "feature""
* `is:open no:label (feature or bug)` -- Finds all issues without labels whose
  title or body contains either the word "feature" or "bug"

## Example: Database execution using `Expression<Func<T, bool>>` and EF Core

Instead of using `Func<T, bool>` as the return type, you can also use
`Expression<Func<T, bool>>` which allows you to use an OR mapper like EF Core
to execute your queries:

```C#
public class IssueQueryContext : ExpressionQueryContext<Issue>
{
    public static IssueQueryContext Instance { get; } = new();

    [QueryFieldHandler("is:open", Description = "Checks for open issues")]
    public static Expression<Func<Issue, bool>> IsOpen()
    {
        return issue => issue.IsOpen;
    }

    [QueryFieldHandler("is:closed", Description = "Checks for closed issues")]
    public static Expression<Func<Issue, bool>> IsClosed()
    {
        return issue => !issue.IsOpen;
    }

    [QueryFieldHandler("label", Description = "Checks for issues with a specific label")]
    public static Expression<Func<Issue, bool>> HasLabel(string label)
    {
        return issue => issue.Labels.Contains(label);
    }

    [QueryFieldHandler("no:label", Description = "Checks for issues that have no labels")]
    public static Expression<Func<Issue, bool>> HasNoLabel()
    {
        return issue => !issue.Labels.Any();
    }

    protected override Expression<Func<Issue, bool>> GenerateText(string text)
    {
        return issue => issue.Title.Contains(text) ||
                        issue.Body.Contains(text);
    }
}
```

Assuming you have `DbContext` like this:

```C#
public class Issue
{
    public int Id { get; set;}
    public bool IsOpen { get; set; }
    public required string Title { get; set; }
    public required string Body { get; set; }
    public List<string> Labels { get; } = [];
}

class IssueDbContext
{
    public DbSet<Issue> Issues { get; set; } = null!;
}
```

Then you can execute queries against the database like this:

```C#
public IEnumerable<Issue> QueryIssues(IssueDbContext dbContext, string queryText)
{
    Expression<Func<Issue, bool>> expression = IssueQueryContext.Instance.Generate(queryText);    
    return dbContext.Issues.Where(expression);
}
```

## Example: Custom Representation

You query processing needs might be non-standard. For example, you might want to
cache all open issues and if the query only includes those, do an in-memory
lookup and otherwise a DB lookup.

In those cases you typically want your own query representation. However, you
generally want a flattened representation, rather than a tree. A pretty
standard representation is [Disjunctive Normal Form (DNF)][DNF].

In that representation, queries are represented in the form `(A1 AND A2 AND ...
aN) OR (B1 AND B2 AND ... BN)`. In other words, `OR` only appears at the
top-level and underneath each `OR` is only `AND`. The nice thing is that any
query can be rewritten in DNF.

For example, `(A OR B) AND C` becomes `(A AND C) OR (B AND C)`. And `NOT(A OR
B)` becomes `NOT A AND NOT B`.

This allows you to have fairly simplistic query representation, you only need
two types, a representation for the filter where all options are combined with
`AND` and a representation of multiple filters that are each combined using
`OR`:

```C#
public class IssueQuery
{
    public ImmutableArray<IssueFilter> Filters { get; set; } = [];
}

public class IssueFilter
{
    public bool? IsOpen { get; set;}

    public bool? NoLabels { get; set; }
    public List<string> IncludedLabels { get; set;}
    public List<string> ExcludedLabels { get; set;}

    public List<string> IncludedText { get; set;}
    public List<string> ExcludedText { get; set;}
}
```

To construct such queries, you need to derive from `DnfQueryContext<TDisjunction, TConjunction>`:

```C#
public class IssueQueryContext : DnfQueryContext<IssueQuery, IssueFilter>
{
    public static IssueQueryContext Instance { get; } = new();

    private IssueQueryContext()
    {
    }

    [QueryFieldHandler("is:open")]
    public static void ApplyIsOpen(IssueFilter filter, bool isNegated)
    {
        filter.IsOpen = !isNegated;
    }

    [QueryFieldHandler("is:closed")]
    public static void ApplyIsClosed(IssueFilter filter, bool isNegated)
    {
        ApplyIsOpen(filter, !isNegated);
    }

    [QueryFieldHandler("label")]
    public static void ApplyLabel(IssueFilter filter, bool isNegated, string label)
    {
        var target = isNegated ? filter.ExcludedLabels : filter.IncludedLabels;
        target.Add(label);
    }

    [QueryFieldHandler("no:label")]
    public static void ApplyNoLabel(IssueFilter filter, bool isNegated)
    {
        filter.NoLabels = !isNegated;
    }

    protected override void ApplyText(IssueFilter filter, bool isNegated, string text)
    {
        var target = isNegated ? filter.ExcludedText : filter.IncludedText;
        target.Add(text);
    }

    protected override IssueFilter GenerateConjunction()
    {
        return new IssueFilter();
    }

    protected override IssueQuery GenerateDisjunction(ImmutableArray<IssueFilter> filters)
    {
        return new IssueQuery()
        {
            Filters = filters
        };
    }
}
```

To get an instance of `IssueFilter` you just need to call the `Generate` method
like before:

```C#
public IssueQuery CreateQuery(string queryText)
{
    return IssueQueryContext.Instance.Generate(queryText);    
}
```

Of course, in this scheme the execution of the query is up to you, but the query
context handles the resolution of handlers and re-writing it into DNF for you.

[DNF]: https://en.wikipedia.org/wiki/Disjunctive_normal_form