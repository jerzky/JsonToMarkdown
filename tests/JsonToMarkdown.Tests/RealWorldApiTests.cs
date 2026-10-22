using Xunit;

namespace JsonToMarkdown.Tests;

public class RealWorldApiTests
{
    [Fact]
    public void Convert_AzureDevOpsWorkItem_DistillsKeyFields()
    {
        const string azdoJson = """
        {
            "id": 8841,
            "fields": {
                "System.WorkItemType": "Task",
                "System.Title": "Deploy database migration",
                "System.State": "Active",
                "System.AssignedTo": { "displayName": "Sarah Connor" },
                "System.Description": "<div>Execute <b>001_init.sql</b> on production.</div>"
            },
            "_links": {
                "self": { "href": "https://dev.azure.com/org/proj/_apis/wit/workitems/8841" }
            },
            "watermark": 29841
        }
        """;

        var md = JsonMarkdown.Convert(azdoJson);

        Assert.Contains("Sarah Connor", md);
        Assert.Contains("Deploy database migration", md);
        Assert.Contains("**001_init.sql**", md);
        Assert.DoesNotContain("watermark", md);
        Assert.DoesNotContain("_links", md);
    }

    [Fact]
    public void Convert_GitHubPullRequestList_FormatsTable()
    {
        const string prJson = """
        [
            { "number": 101, "title": "Add OAuth2 authentication", "state": "open", "author": "octocat" },
            { "number": 102, "title": "Upgrade to .NET 10", "state": "merged", "author": "developer" }
        ]
        """;

        var md = JsonMarkdown.Convert(prJson);

        Assert.Contains("| number | title | state | author |", md);
        Assert.Contains("| 101 | Add OAuth2 authentication | open | octocat |", md);
        Assert.Contains("| 102 | Upgrade to .NET 10 | merged | developer |", md);
    }
}
