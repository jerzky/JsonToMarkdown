using System.Text.Json;

namespace JsonToMarkdown;

/// <summary>
/// Extension methods for converting JsonElement and JsonDocument into Markdown.
/// </summary>
public static class JsonMarkdownExtensions
{
    /// <summary>
    /// Converts this JsonElement into token-efficient Markdown.
    /// </summary>
    public static string ToMarkdown(this JsonElement element, JsonMarkdownOptions? options = null)
    {
        return JsonMarkdown.Convert(element, options);
    }

    /// <summary>
    /// Converts this JsonDocument into token-efficient Markdown.
    /// </summary>
    public static string ToMarkdown(this JsonDocument document, JsonMarkdownOptions? options = null)
    {
        return JsonMarkdown.Convert(document, options);
    }
}
