# Text Queries

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

## Example

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
        return issue => issue.Labels.Any(l => string.Equals(l, label, StringComparison.OrdinalIgnoreCase));
    }

    [QueryFieldHandler("no:label", Description = "Checks for issues that have no labels")]
    public static Func<Issue, bool> HasNoLabel()
    {
        return issue => !issue.Labels.Any();
    }

    protected override Func<Issue, bool> GenerateText(string text)
    {
        return issue => issue.Title.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                        issue.Body.Contains(text, StringComparison.OrdinalIgnoreCase);
    }
}

public IEnumerable<Issue> QueryIssues(IEnumerable<Issue> issues, string queryText)
{
    Query query = IssueQueryContext.Instance.CreateQuery(queryText);
    Func<Issue, bool> predicate = IssueQueryContext.Instance.GenerateNode(query);
    return issues.Where(predicate);
}
```

With that, you can run queries like those:

* `is:open` -- Finds all open issues
* `is:closed` -- Finds all closed issues
* `is:open label:bug feature` -- Finds all open issues labeled `bug` whose title or body contains the word "feature""
* `is:open no:label (feature or bug)` -- Finds all issues without labels whose title or body contains either the word "feature" or "bug"
