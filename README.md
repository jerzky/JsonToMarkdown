# JsonToMarkdown

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-purple.svg)](https://dotnet.microsoft.com/)

**The token distiller for AI tool calling and MCP servers.**

`JsonToMarkdown` transforms verbose, noisy JSON responses from REST APIs into clean, token-efficient GitHub Flavored Markdown (GFM). Designed specifically for LLMs, Model Context Protocol (MCP) servers, and agent tool execution.

---

## ⚡ Why JsonToMarkdown?

APIs (GitHub, Azure DevOps, Jira, Stripe, Salesforce) are designed for machine-to-machine deserialization. When returned directly to LLMs:
* **70%–90% of tokens are wasted** on internal GUIDs, ETags, avatar links, and schema boilerplate.
* Large JSON payloads cause **attention drift** and degrade model reasoning.
* Context window consumption leads to higher API bills and slower response times.

`JsonToMarkdown` solves this in **1 line of code**, reducing token counts by up to **90%** while preserving the critical semantic signal.

---

## 🚀 Quick Start

### Installation
```bash
dotnet add package JsonToMarkdown
```

### 1-Line Usage
```csharp
using JsonToMarkdown;

string json = await httpClient.GetStringAsync("https://api.github.com/repos/dotnet/runtime/issues");

// Converts raw JSON into token-efficient Markdown
string markdown = JsonMarkdown.Convert(json);
```

Or using `JsonElement` extension methods:
```csharp
JsonElement element = jsonDocument.RootElement;
string markdown = element.ToMarkdown();
```

---

## ✨ Features

* **Zero 3rd-Party Dependencies**: Pure BCL (`System.Text.Json`), Native AOT compatible.
* **Automatic Table Detection**: Arrays of uniform objects automatically render as formatted GFM tables (`| Col1 | Col2 |`).
* **Noise Filtering**: Automatically drops common metadata noise (`_links`, `etag`, `watermark`, `__v`).
* **HTML Sanitization**: Detects rich text fields (`<p>`, `<div>`, `<b>`, `<code>`, `<a href="...">`) and translates them into clean Markdown.
* **Token Budget Guards**: Configurable `MaxArrayItems`, `MaxStringLength`, and `MaxDepth` to prevent runaway token usage.
* **URL Collapsing**: Automatically turns long HTTP/HTTPS URLs into clean `[Link](url)` anchors.

---

## ⚙️ Configuration Options

```csharp
var options = new JsonMarkdownOptions
{
    // Token & size limits
    MaxArrayItems = 25,                 // Maximum rows/items before "... and N more rows"
    MaxStringLength = 1000,             // Truncates long text values
    MaxDepth = 4,                       // Maximum nesting depth
    
    // Heuristics
    AutoDetectTables = true,            // Auto-render object arrays as GFM tables
    MaxTableColumns = 6,                // Limit wide tables to top N columns
    ConvertHtml = true,                 // Convert <div>, <b>, <code> to markdown
    CollapseUrls = true,                // Format URLs as [Link](url)
    
    // Noise filtering (case-insensitive)
    ExcludedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "_links", "etag", "watermark", "revision", "nodeId"
    }
};

string markdown = JsonMarkdown.Convert(json, options);
```

---

## 📊 Before & After Example

### Raw API JSON (~850 tokens):
```json
{
  "id": 4821,
  "title": "Payment webhook fails on timeout",
  "fields": {
    "status": "Active",
    "assignedTo": { "displayName": "Alice Smith", "id": "guid-1234" },
    "description": "<div>Trigger Stripe webhook and <b>drop database connection</b>.</div>"
  },
  "_links": {
    "self": { "href": "https://api.example.com/items/4821" },
    "avatar": { "href": "https://api.example.com/users/alice/avatar.png" }
  },
  "watermark": 918231
}
```

### `JsonToMarkdown` Output (~75 tokens, 91% reduction):
```markdown
# [4821] Payment webhook fails on timeout

* **fields:**
  * **status:** Active
  * **assignedTo:**
    * **displayName:** Alice Smith
    * **id:** guid-1234
  * **description:**
    Trigger Stripe webhook and **drop database connection**.
```

---

## 🧪 Testing & Multi-Targeting

`JsonToMarkdown` is tested and multi-targeted for:
* **.NET 8.0** (LTS)
* **.NET 9.0** (Current)
* **.NET 10.0** (Preview / Latest)

```bash
dotnet test
```

---

## 📄 License

MIT © Jens
