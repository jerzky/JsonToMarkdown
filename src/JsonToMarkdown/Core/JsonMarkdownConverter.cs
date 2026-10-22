using System.Text;
using System.Text.Json;

namespace JsonToMarkdown.Core;

internal sealed class JsonMarkdownConverter
{
    private readonly JsonMarkdownOptions _options;
    private readonly StringBuilder _sb = new();

    public JsonMarkdownConverter(JsonMarkdownOptions options)
    {
        _options = options ?? JsonMarkdownOptions.Default;
    }

    public string Convert(JsonElement element)
    {
        _sb.Clear();
        RenderElement(element, depth: 0, indent: "");
        return _sb.ToString().Trim();
    }

    private void RenderElement(JsonElement element, int depth, string indent)
    {
        if (depth > _options.MaxDepth)
        {
            _sb.Append(indent).AppendLine("*(truncated: max depth reached)*");
            return;
        }

        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                RenderObject(element, depth, indent);
                break;

            case JsonValueKind.Array:
                RenderArray(element, depth, indent);
                break;

            case JsonValueKind.String:
                _sb.Append(indent).AppendLine(FormatString(element.GetString()));
                break;

            case JsonValueKind.Number:
                _sb.Append(indent).AppendLine(element.GetRawText());
                break;

            case JsonValueKind.True:
                _sb.Append(indent).AppendLine("true");
                break;

            case JsonValueKind.False:
                _sb.Append(indent).AppendLine("false");
                break;

            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                _sb.Append(indent).AppendLine("null");
                break;
        }
    }

    private void RenderObject(JsonElement obj, int depth, string indent)
    {
        var properties = new List<JsonProperty>();
        foreach (var prop in obj.EnumerateObject())
        {
            if (!_options.ExcludedKeys.Contains(prop.Name))
            {
                properties.Add(prop);
            }
        }

        if (properties.Count == 0)
        {
            _sb.Append(indent).AppendLine("*(empty object)*");
            return;
        }

        // If top-level object, detect if there is a primary title / identifier
        if (depth == 0)
        {
            var titleProp = properties.FirstOrDefault(p =>
                string.Equals(p.Name, "title", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Name, "name", StringComparison.OrdinalIgnoreCase));

            var idProp = properties.FirstOrDefault(p =>
                string.Equals(p.Name, "id", StringComparison.OrdinalIgnoreCase));

            if (titleProp.Value.ValueKind == JsonValueKind.String)
            {
                var titleText = titleProp.Value.GetString();
                if (idProp.Value.ValueKind != JsonValueKind.Undefined && idProp.Value.ValueKind != JsonValueKind.Null)
                {
                    _sb.AppendLine($"# [{idProp.Value.GetRawText()}] {titleText}");
                }
                else
                {
                    _sb.AppendLine($"# {titleText}");
                }
                _sb.AppendLine();
                properties.Remove(titleProp);
                if (idProp.Value.ValueKind != JsonValueKind.Undefined)
                {
                    properties.Remove(idProp);
                }
            }
        }

        foreach (var prop in properties)
        {
            RenderProperty(prop, depth, indent);
        }
    }

    private void RenderProperty(JsonProperty prop, int depth, string indent)
    {
        var key = prop.Name;
        var val = prop.Value;

        switch (val.ValueKind)
        {
            case JsonValueKind.Object:
                _sb.Append(indent).AppendLine($"* **{key}:**");
                RenderElement(val, depth + 1, indent + "  ");
                break;

            case JsonValueKind.Array:
                if (_options.AutoDetectTables && TryRenderTable(val, key, indent))
                {
                    // Table rendered directly
                }
                else
                {
                    _sb.Append(indent).AppendLine($"* **{key}:**");
                    RenderElement(val, depth + 1, indent + "  ");
                }
                break;

            case JsonValueKind.String:
                var str = val.GetString();
                if (_options.ConvertHtml && HtmlSanitizer.LooksLikeHtml(str ?? ""))
                {
                    var converted = HtmlSanitizer.ConvertToMarkdown(str ?? "");
                    _sb.Append(indent).AppendLine($"* **{key}:**");
                    foreach (var line in converted.Split('\n'))
                    {
                        _sb.Append(indent).Append("  ").AppendLine(line);
                    }
                }
                else
                {
                    _sb.Append(indent).AppendLine($"* **{key}:** {FormatString(str)}");
                }
                break;

            default:
                _sb.Append(indent).AppendLine($"* **{key}:** `{val.GetRawText()}`");
                break;
        }
    }

    private void RenderArray(JsonElement array, int depth, string indent)
    {
        var count = array.GetArrayLength();
        if (count == 0)
        {
            _sb.Append(indent).AppendLine("*(empty list)*");
            return;
        }

        if (_options.AutoDetectTables && TryRenderTable(array, null, indent))
        {
            return;
        }

        var limit = Math.Min(count, _options.MaxArrayItems);
        var idx = 0;

        foreach (var item in array.EnumerateArray())
        {
            if (idx >= limit)
            {
                _sb.Append(indent).AppendLine($"* *(... and {count - limit} more items)*");
                break;
            }

            if (item.ValueKind == JsonValueKind.Object)
            {
                _sb.Append(indent).AppendLine($"* Item {idx + 1}:");
                RenderElement(item, depth + 1, indent + "  ");
            }
            else
            {
                _sb.Append(indent).Append("* ");
                RenderElement(item, depth + 1, "");
            }

            idx++;
        }
    }

    private bool TryRenderTable(JsonElement array, string? title, string indent)
    {
        var count = array.GetArrayLength();
        if (count < _options.MinTableRows)
        {
            return false;
        }

        // Check if elements are objects
        var sampleItems = new List<JsonElement>();
        var columnFrequencies = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        var limit = Math.Min(count, _options.MaxArrayItems);
        var idx = 0;

        foreach (var item in array.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            if (idx < limit)
            {
                sampleItems.Add(item);
                foreach (var prop in item.EnumerateObject())
                {
                    if (_options.ExcludedKeys.Contains(prop.Name)) continue;
                    // Only scalar properties or simple strings/numbers in table cells
                    if (prop.Value.ValueKind is JsonValueKind.String or JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False)
                    {
                        columnFrequencies[prop.Name] = columnFrequencies.GetValueOrDefault(prop.Name, 0) + 1;
                    }
                }
            }

            idx++;
        }

        if (columnFrequencies.Count < 2)
        {
            return false;
        }

        // Pick top columns ordered by frequency, then appearance
        var columns = columnFrequencies
            .OrderByDescending(kv => kv.Value)
            .Take(_options.MaxTableColumns)
            .Select(kv => kv.Key)
            .ToList();

        if (columns.Count < 2)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(title))
        {
            _sb.AppendLine();
            _sb.Append(indent).AppendLine($"### {title} ({count} items)");
            _sb.AppendLine();
        }

        // Header Row
        _sb.Append(indent).Append("| ");
        foreach (var col in columns)
        {
            _sb.Append(col).Append(" | ");
        }
        _sb.AppendLine();

        // Separator Row
        _sb.Append(indent).Append("| ");
        foreach (var _ in columns)
        {
            _sb.Append(":--- | ");
        }
        _sb.AppendLine();

        // Data Rows
        foreach (var item in sampleItems)
        {
            _sb.Append(indent).Append("| ");
            foreach (var col in columns)
            {
                var valStr = "-";
                if (item.TryGetProperty(col, out var p))
                {
                    valStr = FormatCell(p);
                }
                _sb.Append(valStr).Append(" | ");
            }
            _sb.AppendLine();
        }

        if (count > limit)
        {
            _sb.Append(indent).AppendLine($"\n*(... and {count - limit} more rows)*");
        }

        return true;
    }

    private string FormatCell(JsonElement prop)
    {
        string text = prop.ValueKind switch
        {
            JsonValueKind.String => prop.GetString() ?? "",
            JsonValueKind.Number => prop.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => "-",
            _ => prop.GetRawText()
        };

        if (_options.CollapseUrls && (text.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || text.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
        {
            return $"[Link]({text})";
        }

        // Clean table cell: replace pipes and newlines
        text = text.Replace("|", "\\|").Replace("\r\n", " ").Replace("\n", " ");

        if (text.Length > 100)
        {
            text = text[..97] + "...";
        }

        return text;
    }

    private string FormatString(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "\"\"";

        var text = value;
        if (text.Length > _options.MaxStringLength)
        {
            text = text[.._options.MaxStringLength] + "... *(truncated)*";
        }

        if (_options.CollapseUrls && (text.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || text.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
        {
            return $"[{text}]({text})";
        }

        return text;
    }
}
