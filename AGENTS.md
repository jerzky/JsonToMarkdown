# Agent Instructions & Guidelines (AGENTS.md)

This file contains critical context, conventions, invariants, and guidelines for AI coding agents operating in this repository.

---

## 🔒 Invariants & Ground Rules

1. **Zero External Dependencies**:
   - Do NOT add external third-party packages (such as `Newtonsoft.Json`, `AngleSharp`, `HtmlAgilityPack`, `Markdig`, etc.).
   - Use only the built-in BCL:
     - `System.Text.Json`
     - `System.Text.RegularExpressions`
     - `System.Memory`
2. **Central Package Management (CPM)**:
   - `<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>` is enabled in [Directory.Packages.props](file:///c:/Users/Jensg/Documents/antigravity/JsonToMarkdown/Directory.Packages.props).
   - **Never** add a `Version="..."` attribute to `<PackageReference>` in any `.csproj` file.
   - If adding a new test package, declare its version in `Directory.Packages.props` first, then add `<PackageReference Include="..." />` in the project file.
3. **Multi-Targeting Compliance**:
   - `JsonToMarkdown` MUST target `net8.0;net9.0;net10.0`.
   - Test projects target `net9.0;net10.0`.
   - Do not use APIs that only exist in .NET 10 inside the library project unless guarded by `#if NET10_0_OR_GREATER`.
4. **Performance & Allocation Awareness**:
   - Prefer `ReadOnlySpan<char>`, `StringComparison.OrdinalIgnoreCase`, and single-pass string builders where feasible.
   - Do not deserialize JSON into intermediate dictionary objects; operate directly on `JsonElement` and `Utf8JsonReader`.

---

## 📁 Repository Structure

```
├── Directory.Build.props          # Global build settings, warnings, analysis level
├── Directory.Packages.props       # Central Package Management package versions
├── JsonToMarkdown.slnx            # Solution file
├── LICENSE                        # MIT License
├── README.md                      # Consumer-facing documentation
├── Architecture.md                # System architecture documentation
├── AGENTS.md                      # Agent rules and codebase context (this file)
├── src/
│   └── JsonToMarkdown/            # Core library (net8.0;net9.0;net10.0)
│       ├── Core/                  # Heuristic engine, table detection, HTML sanitizer
│       ├── JsonMarkdown.cs        # Static entry points
│       ├── JsonMarkdownExtensions.cs # JsonElement/JsonDocument extensions
│       └── JsonMarkdownOptions.cs # Formatting options & token budget limits
└── tests/
    └── JsonToMarkdown.Tests/      # Unit tests (xUnit)
```

---

## 🧪 Testing & Verification

Before concluding any task or submitting changes:

1. **Run Unit Tests**:
   ```bash
   dotnet test
   ```
   All tests must pass across all target frameworks with zero failures.
2. **Build Release**:
   ```bash
   dotnet build -c Release
   ```
   Must build with 0 errors and 0 warnings.
3. **Table & Cell Escaping**:
   - When modifying table formatting, always ensure pipe characters (`|`) inside cells are escaped (`\|`) and line breaks are replaced with spaces to avoid breaking Markdown table parsers.

---

## 🔄 Documentation Maintenance

Whenever making changes:
* Update [Architecture.md](file:///c:/Users/Jensg/Documents/antigravity/JsonToMarkdown/Architecture.md) if conversion pipeline, heuristics, or dependencies change.
* Update [README.md](file:///c:/Users/Jensg/Documents/antigravity/JsonToMarkdown/README.md) if user-facing API surface, options, or configuration change.
* Update [AGENTS.md](file:///c:/Users/Jensg/Documents/antigravity/JsonToMarkdown/AGENTS.md) if development rules, compiler settings, or invariants change.
