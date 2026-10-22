using System.Net;
using System.Text.RegularExpressions;

namespace JsonToMarkdown.Core;

internal static partial class HtmlSanitizer
{
    private static readonly Regex AnchorRegex = new(@"<a\s+[^>]*href=[""']([^""']+)[""'][^>]*>(.*?)</a>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex CodeBlockRegex = new(@"<pre><code>(.*?)</code></pre>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex InlineCodeRegex = new(@"<code>(.*?)</code>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex BoldRegex = new(@"<(?:b|strong)>(.*?)</(?:b|strong)>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex ItalicRegex = new(@"<(?:i|em)>(.*?)</(?:i|em)>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex RemainingTagsRegex = new(@"<[^>]+>", RegexOptions.Compiled);

    public static bool LooksLikeHtml(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length < 3) return false;
        var open = text.IndexOf('<');
        if (open < 0) return false;
        var close = text.IndexOf('>', open);
        return close > open;
    }

    public static string ConvertToMarkdown(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        var result = html;

        // Code blocks
        result = CodeBlockRegex.Replace(result, m => $"\n```\n{WebUtility.HtmlDecode(m.Groups[1].Value.Trim())}\n```\n");

        // Inline code
        result = InlineCodeRegex.Replace(result, m => $"`{WebUtility.HtmlDecode(m.Groups[1].Value.Trim())}`");

        // Bold & Italic
        result = BoldRegex.Replace(result, m => $"**{m.Groups[1].Value.Trim()}**");
        result = ItalicRegex.Replace(result, m => $"*{m.Groups[1].Value.Trim()}*");

        // Links: <a href="url">text</a> -> [text](url)
        result = AnchorRegex.Replace(result, m =>
        {
            var href = m.Groups[1].Value;
            var label = m.Groups[2].Value.Trim();
            if (string.IsNullOrEmpty(label)) label = href;
            return $"[{label}]({href})";
        });

        // Common structure
        result = result
            .Replace("<h1>", "\n# ")
            .Replace("</h1>", "\n")
            .Replace("<h2>", "\n## ")
            .Replace("</h2>", "\n")
            .Replace("<h3>", "\n### ")
            .Replace("</h3>", "\n")
            .Replace("<h4>", "\n#### ")
            .Replace("</h4>", "\n")
            .Replace("<p>", "\n\n")
            .Replace("</p>", "")
            .Replace("<div>", "\n")
            .Replace("</div>", "")
            .Replace("<br>", "\n")
            .Replace("<br/>", "\n")
            .Replace("<br />", "\n")
            .Replace("<li>", "\n* ")
            .Replace("</li>", "");

        // Strip remaining HTML tags
        result = RemainingTagsRegex.Replace(result, string.Empty);

        // Decode remaining HTML entities
        result = WebUtility.HtmlDecode(result);

        // Normalize multiple blank lines
        result = Regex.Replace(result, @"\n{3,}", "\n\n");

        return result.Trim();
    }
}
