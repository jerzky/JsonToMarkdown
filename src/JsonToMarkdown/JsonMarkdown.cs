using System.Text.Json;
using JsonToMarkdown.Core;

namespace JsonToMarkdown;

/// <summary>
/// Static entry point for converting JSON to token-efficient Markdown.
/// </summary>
public static class JsonMarkdown
{
    /// <summary>
    /// Converts a raw JSON string into token-efficient Markdown.
    /// </summary>
    public static string Convert(string json, JsonMarkdownOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return string.Empty;
        }

        using var doc = JsonDocument.Parse(json);
        return Convert(doc.RootElement, options);
    }

    /// <summary>
    /// Converts a JsonDocument into token-efficient Markdown.
    /// </summary>
    public static string Convert(JsonDocument document, JsonMarkdownOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        return Convert(document.RootElement, options);
    }

    /// <summary>
    /// Converts a JsonElement into token-efficient Markdown.
    /// </summary>
    public static string Convert(JsonElement element, JsonMarkdownOptions? options = null)
    {
        var converter = new JsonMarkdownConverter(options ?? JsonMarkdownOptions.Default);
        return converter.Convert(element);
    }
}
