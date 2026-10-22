using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using JsonToMarkdown;
using JsonToMarkdown.Benchmarks;

if (args.Length > 0 && args[0] == "--benchmark")
{
    BenchmarkRunner.Run<ConversionBenchmarks>();
    return;
}

// Default run: Print Token Reduction Analysis & Quick Performance Benchmark
TokenReductionAnalysis.Run();

namespace JsonToMarkdown.Benchmarks
{
    [MemoryDiagnoser]
    public class ConversionBenchmarks
    {
        private string _azdoWorkItemJson = null!;
        private string _githubPrsJson = null!;

        [GlobalSetup]
        public void Setup()
        {
            _azdoWorkItemJson = SampleData.AzureDevOpsWorkItem;
            _githubPrsJson = SampleData.GitHubPullRequests;
        }

        [Benchmark(Description = "Convert Single Azure DevOps Work Item")]
        public string ConvertSingleWorkItem()
        {
            return JsonMarkdown.Convert(_azdoWorkItemJson);
        }

        [Benchmark(Description = "Convert GitHub PR Table (10 items)")]
        public string ConvertGitHubPrs()
        {
            return JsonMarkdown.Convert(_githubPrsJson);
        }
    }

    public static class TokenReductionAnalysis
    {
        public static void Run()
        {
            Console.WriteLine("==========================================================================");
            Console.WriteLine("        JsonToMarkdown Token Compression & Reduction Analysis             ");
            Console.WriteLine("==========================================================================");
            Console.WriteLine();

            Analyze("Azure DevOps Work Item", SampleData.AzureDevOpsWorkItem);
            Analyze("GitHub Pull Requests (Table)", SampleData.GitHubPullRequests);
            Analyze("Stripe Payment Intents", SampleData.StripePayments);

            Console.WriteLine();
            Console.WriteLine("Run with '--benchmark' to execute full BenchmarkDotNet performance suite.");
        }

        private static void Analyze(string name, string rawJson)
        {
            var markdown = JsonMarkdown.Convert(rawJson);

            var rawChars = rawJson.Length;
            var mdChars = markdown.Length;

            var rawTokens = (int)Math.Ceiling(rawChars / 4.0);
            var mdTokens = (int)Math.Ceiling(mdChars / 4.0);

            var savings = (1.0 - ((double)mdTokens / rawTokens)) * 100.0;

            Console.WriteLine($"📊 Payload: {name}");
            Console.WriteLine($"   Raw JSON:    {rawChars,6:N0} chars (~{rawTokens,5:N0} tokens)");
            Console.WriteLine($"   Markdown:    {mdChars,6:N0} chars (~{mdTokens,5:N0} tokens)");
            Console.WriteLine($"   Reduction:   {savings,6:F1}% tokens saved! 🚀");
            Console.WriteLine(new string('-', 74));
        }
    }

    public static class SampleData
    {
        public const string AzureDevOpsWorkItem = """
        {
          "id": 4821,
          "rev": 14,
          "fields": {
            "System.AreaPath": "BillingApp\\Payments",
            "System.TeamProject": "BillingApp",
            "System.IterationPath": "BillingApp\\Sprint 24",
            "System.WorkItemType": "Bug",
            "System.State": "Active",
            "System.Reason": "New",
            "System.AssignedTo": {
              "displayName": "Alice Smith",
              "url": "https://spsprodneu3.vssps.visualstudio.com/...",
              "_links": { "avatar": { "href": "https://dev.azure.com/..." } },
              "id": "e43b6794-f8b8-4c8d-bda7-76b4a6f2384a",
              "uniqueName": "alice@company.com"
            },
            "System.CreatedDate": "2026-10-15T09:12:00.32Z",
            "System.CreatedBy": { "displayName": "Bob Jones", "id": "uuid-1234" },
            "System.ChangedDate": "2026-10-22T14:02:18.11Z",
            "System.Watermark": 1948210,
            "System.CommentCount": 3,
            "System.Title": "Payment webhook fails with 500 on timeout",
            "Microsoft.VSTS.TCM.ReproSteps": "<div><p>Steps to reproduce:</p><ol><li>Trigger Stripe webhook</li><li>Drop database connection</li></ol></div>",
            "WEF_EB827361_Kanban.Column": "In Progress"
          },
          "_links": {
            "self": { "href": "https://dev.azure.com/org/proj/_apis/wit/workitems/4821" },
            "workItemUpdates": { "href": "https://dev.azure.com/org/proj/_apis/wit/workitems/4821/updates" },
            "workItemRevisions": { "href": "https://dev.azure.com/org/proj/_apis/wit/workitems/4821/revisions" },
            "html": { "href": "https://dev.azure.com/org/proj/_workitems/edit/4821" }
          },
          "url": "https://dev.azure.com/org/proj/_apis/wit/workitems/4821"
        }
        """;

        public const string GitHubPullRequests = """
        [
          { "id": 1, "number": 101, "title": "Upgrade to .NET 10", "state": "open", "user": "octocat", "created_at": "2026-10-20T10:00:00Z" },
          { "id": 2, "number": 102, "title": "Add OAuth2 authentication", "state": "open", "user": "developer", "created_at": "2026-10-21T11:00:00Z" },
          { "id": 3, "number": 103, "title": "Fix memory leak in buffer pool", "state": "merged", "user": "alice", "created_at": "2026-10-21T12:00:00Z" },
          { "id": 4, "number": 104, "title": "Update documentation", "state": "closed", "user": "bob", "created_at": "2026-10-22T13:00:00Z" },
          { "id": 5, "number": 105, "title": "Add retry policy to HttpClient", "state": "open", "user": "charlie", "created_at": "2026-10-22T14:00:00Z" }
        ]
        """;

        public const string StripePayments = """
        [
          { "id": "pi_1001", "amount": 4900, "currency": "usd", "status": "succeeded", "customer": "cus_N1", "created": 1729600000 },
          { "id": "pi_1002", "amount": 12500, "currency": "usd", "status": "requires_payment_method", "customer": "cus_N2", "created": 1729601000 },
          { "id": "pi_1003", "amount": 299, "currency": "eur", "status": "succeeded", "customer": "cus_N3", "created": 1729602000 },
          { "id": "pi_1004", "amount": 8900, "currency": "usd", "status": "canceled", "customer": "cus_N4", "created": 1729603000 }
        ]
        """;
    }
}
