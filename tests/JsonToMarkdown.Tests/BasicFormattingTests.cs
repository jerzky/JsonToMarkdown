using System.Text.Json;
using Xunit;

namespace JsonToMarkdown.Tests;

public class BasicFormattingTests
{
    [Fact]
    public void Convert_Primitives_ReturnsExpectedText()
    {
        Assert.Equal("123", JsonMarkdown.Convert("123"));
        Assert.Equal("true", JsonMarkdown.Convert("true"));
        Assert.Equal("false", JsonMarkdown.Convert("false"));
        Assert.Equal("null", JsonMarkdown.Convert("null"));
        Assert.Equal("hello world", JsonMarkdown.Convert("\"hello world\""));
    }

    [Fact]
    public void Convert_SimpleObject_RendersBulletList()
    {
        const string json = """
        {
            "username": "johndoe",
            "age": 30,
            "isAdmin": true
        }
        """;

        var md = JsonMarkdown.Convert(json);

        Assert.Contains("* **username:** johndoe", md);
        Assert.Contains("* **age:** `30`", md);
        Assert.Contains("* **isAdmin:** `true`", md);
    }

    [Fact]
    public void Convert_TopLevelTitle_RendersAsHeading()
    {
        const string json = """
        {
            "id": 101,
            "title": "Fix login crash",
            "status": "closed"
        }
        """;

        var md = JsonMarkdown.Convert(json);

        Assert.StartsWith("# [101] Fix login crash", md);
        Assert.Contains("* **status:** closed", md);
    }

    [Fact]
    public void Convert_NestedObject_RendersIndentedList()
    {
        const string json = """
        {
            "user": {
                "name": "Alice",
                "address": {
                    "city": "Seattle"
                }
            }
        }
        """;

        var md = JsonMarkdown.Convert(json);

        Assert.Contains("* **user:**", md);
        Assert.Contains("  * **name:** Alice", md);
        Assert.Contains("  * **address:**", md);
        Assert.Contains("    * **city:** Seattle", md);
    }
}
