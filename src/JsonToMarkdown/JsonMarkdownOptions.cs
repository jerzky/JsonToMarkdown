namespace JsonToMarkdown;

/// <summary>
/// Configuration options for converting JSON into token-efficient Markdown.
/// </summary>
public sealed class JsonMarkdownOptions
{
    /// <summary>
    /// Default singleton instance of JsonMarkdownOptions.
    /// </summary>
    public static readonly JsonMarkdownOptions Default = new();

    /// <summary>
    /// Maximum number of items in an array to render before truncating.
    /// Default is 50.
    /// </summary>
    public int MaxArrayItems { get; set; } = 50;

    /// <summary>
    /// Maximum character length for individual string values before truncating.
    /// Default is 1000 characters.
    /// </summary>
    public int MaxStringLength { get; set; } = 1000;

    /// <summary>
    /// Maximum nesting depth for JSON objects and arrays.
    /// Default is 5.
    /// </summary>
    public int MaxDepth { get; set; } = 5;

    /// <summary>
    /// Whether to automatically detect and convert rich text HTML inside string values to Markdown.
    /// Default is true.
    /// </summary>
    public bool ConvertHtml { get; set; } = true;

    /// <summary>
    /// Whether to detect arrays of uniform objects and render them as GitHub Flavored Markdown (GFM) tables.
    /// Default is true.
    /// </summary>
    public bool AutoDetectTables { get; set; } = true;

    /// <summary>
    /// Minimum number of rows required in an array of objects to format as a Markdown table.
    /// Default is 2.
    /// </summary>
    public int MinTableRows { get; set; } = 2;

    /// <summary>
    /// Maximum number of columns to include when auto-generating a Markdown table.
    /// Default is 8.
    /// </summary>
    public int MaxTableColumns { get; set; } = 8;

    /// <summary>
    /// Set of property keys to exclude from the Markdown output (e.g., noisy internal API metadata).
    /// Comparison is case-insensitive.
    /// </summary>
    public HashSet<string> ExcludedKeys { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "_links",
        "etag",
        "watermark",
        "__v"
    };

    /// <summary>
    /// Whether to collapse long HTTP/HTTPS URLs into concise markdown links [Link](url).
    /// Default is true.
    /// </summary>
    public bool CollapseUrls { get; set; } = true;
}
