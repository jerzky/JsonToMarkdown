using Xunit;

namespace JsonToMarkdown.Tests;

public class HtmlAndBudgetTests
{
    [Fact]
    public void Convert_HtmlDescription_ConvertsToMarkdown()
    {
        const string json = """
        {
            "description": "<div><p>This is a <b>critical</b> issue.</p><pre><code>var x = 1;</code></pre>Check <a href=\"https://example.com\">docs</a>.</div>"
        }
        """;

        var md = JsonMarkdown.Convert(json);

        Assert.Contains("**critical**", md);
        Assert.Contains("var x = 1;", md);
        Assert.Contains("[docs](https://example.com)", md);
        Assert.DoesNotContain("<div>", md);
        Assert.DoesNotContain("<p>", md);
    }

    [Fact]
    public void Convert_ExcludedKeys_PrunesNoiseProperties()
    {
        const string json = """
        {
            "id": 12,
            "title": "Bug",
            "_links": { "self": { "href": "http://api/12" } },
            "etag": "\"W/12345\"",
            "watermark": 98765
        }
        """;

        var md = JsonMarkdown.Convert(json);

        Assert.Contains("# [12] Bug", md);
        Assert.DoesNotContain("_links", md);
        Assert.DoesNotContain("etag", md);
        Assert.DoesNotContain("watermark", md);
    }

    [Fact]
    public void Convert_ArrayOverBudget_TruncatesWithCount()
    {
        const string json = """
        [
            { "id": 1, "name": "Item 1" },
            { "id": 2, "name": "Item 2" },
            { "id": 3, "name": "Item 3" },
            { "id": 4, "name": "Item 4" },
            { "id": 5, "name": "Item 5" }
        ]
        """;

        var options = new JsonMarkdownOptions { MaxArrayItems = 2 };
        var md = JsonMarkdown.Convert(json, options);

        Assert.Contains("Item 1", md);
        Assert.Contains("Item 2", md);
        Assert.DoesNotContain("Item 3", md);
        Assert.Contains("and 3 more rows", md);
    }

    [Fact]
    public void Convert_StringOverLength_TruncatesWithSuffix()
    {
        var longStr = new string('A', 200);
        var json = $"{{\"comment\": \"{longStr}\"}}";

        var options = new JsonMarkdownOptions { MaxStringLength = 50 };
        var md = JsonMarkdown.Convert(json, options);

        Assert.Contains("... *(truncated)*", md);
        Assert.DoesNotContain(longStr, md);
    }
}
