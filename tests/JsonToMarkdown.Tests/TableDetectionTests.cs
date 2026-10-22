using Xunit;

namespace JsonToMarkdown.Tests;

public class TableDetectionTests
{
    [Fact]
    public void Convert_ArrayOfUniformObjects_RendersMarkdownTable()
    {
        const string json = """
        [
            { "id": 1, "name": "Alpha", "role": "Admin" },
            { "id": 2, "name": "Beta", "role": "User" },
            { "id": 3, "name": "Gamma", "role": "Guest" }
        ]
        """;

        var md = JsonMarkdown.Convert(json);

        Assert.Contains("| id | name | role |", md);
        Assert.Contains("| :--- | :--- | :--- |", md);
        Assert.Contains("| 1 | Alpha | Admin |", md);
        Assert.Contains("| 2 | Beta | User |", md);
        Assert.Contains("| 3 | Gamma | Guest |", md);
    }

    [Fact]
    public void Convert_TableWithPipeCharacter_EscapesPipeInCell()
    {
        const string json = """
        [
            { "id": 1, "rule": "A | B" },
            { "id": 2, "rule": "C | D" }
        ]
        """;

        var md = JsonMarkdown.Convert(json);

        Assert.Contains(@"A \| B", md);
    }

    [Fact]
    public void Convert_PrimitiveArray_RendersListInsteadOfTable()
    {
        const string json = """["red", "green", "blue"]""";

        var md = JsonMarkdown.Convert(json);

        Assert.DoesNotContain("|", md);
        Assert.Contains("* red", md);
        Assert.Contains("* green", md);
        Assert.Contains("* blue", md);
    }
}
