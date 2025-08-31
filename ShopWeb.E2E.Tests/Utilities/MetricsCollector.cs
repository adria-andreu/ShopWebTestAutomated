using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace ShopWeb.E2E.Tests.Utilities;

public static class MetricsCollector
{
    private static readonly ConcurrentBag<TestMetric> _testMetrics = new();
    private static readonly object _lockObject = new();
    private static DateTime _runStartTime = DateTime.UtcNow;

    public static void RecordTestMetric(TestMetric metric)
    {
        _testMetrics.Add(metric);
        WriteTestMetricToFile(metric);
    }

    public static void WriteTestMetricToFile(TestMetric metric)
    {
        try
        {
            var artifactsDir = Path.Combine(Directory.GetCurrentDirectory(), "artifacts");
            Directory.CreateDirectory(artifactsDir);

            var metricsFile = Path.Combine(artifactsDir, "test-metrics.jsonl");
            var jsonLine = JsonConvert.SerializeObject(metric, Formatting.None);

            lock (_lockObject)
            {
                File.AppendAllText(metricsFile, jsonLine + Environment.NewLine);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to write test metric: {ex.Message}");
        }
    }

    public static void GenerateRunMetrics()
    {
        try
        {
            var metrics = _testMetrics.ToList();
            if (!metrics.Any()) return;

            var runMetric = CalculateRunMetrics(metrics);
            WriteRunMetricsToFile(runMetric);

            // Advanced Observability: Store metrics for flaky detection analysis
            StoreMetricsForFlakyDetection(metrics, runMetric.PipelineId);
            
            // Run flaky detection analysis
            RunFlakyDetectionAnalysis();
            
            // Generate performance trending dashboard
            RunPerformanceTrendingAnalysis();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to generate run metrics: {ex.Message}");
        }
    }

    private static RunMetric CalculateRunMetrics(List<TestMetric> metrics)
    {
        var total = metrics.Count;
        var passed = metrics.Count(m => m.Status == "Passed");
        var failed = metrics.Count(m => m.Status == "Failed");
        var skipped = metrics.Count(m => m.Status == "Skipped");

        var durations = metrics.Where(m => m.DurationMs > 0).Select(m => m.DurationMs).OrderBy(d => d).ToList();
        var p95Index = (int)Math.Ceiling(durations.Count * 0.95) - 1;
        var p95Duration = durations.Any() && p95Index >= 0 && p95Index < durations.Count 
            ? durations[p95Index] 
            : 0;

        var flakyRatio = CalculateFlakyRatio(metrics);

        var firstMetric = metrics.OrderBy(m => m.TimestampUtc).FirstOrDefault();
        var browser = firstMetric?.Browser ?? Environment.GetEnvironmentVariable("BROWSER") ?? "chromium";
        var siteId = firstMetric?.SiteId ?? Environment.GetEnvironmentVariable("SITE_ID") ?? "A";
        var commitSha = Environment.GetEnvironmentVariable("GIT_SHA") ?? Environment.GetEnvironmentVariable("GITHUB_SHA") ?? "unknown";
        var pipelineId = Environment.GetEnvironmentVariable("PIPELINE_ID") ?? Environment.GetEnvironmentVariable("GITHUB_RUN_ID") ?? "local";

        return new RunMetric
        {
            Total = total,
            Passed = passed,
            Failed = failed,
            Skipped = skipped,
            FlakyRatio = flakyRatio,
            P95DurationMs = p95Duration,
            StartedAtUtc = _runStartTime,
            FinishedAtUtc = DateTime.UtcNow,
            Browser = browser,
            SiteId = siteId,
            CommitSha = commitSha,
            PipelineId = pipelineId
        };
    }

    private static double CalculateFlakyRatio(List<TestMetric> metrics)
    {
        var testsWithRetries = metrics.Count(m => m.Retries > 0);
        return metrics.Count > 0 ? (double)testsWithRetries / metrics.Count : 0.0;
    }

    private static void WriteRunMetricsToFile(RunMetric runMetric)
    {
        var artifactsDir = Path.Combine(Directory.GetCurrentDirectory(), "artifacts");
        Directory.CreateDirectory(artifactsDir);

        var runMetricsFile = Path.Combine(artifactsDir, "run-metrics.json");
        var json = JsonConvert.SerializeObject(runMetric, Formatting.Indented);

        File.WriteAllText(runMetricsFile, json);

        // Also save to history
        var historyDir = Path.Combine(artifactsDir, "history");
        Directory.CreateDirectory(historyDir);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var historyFile = Path.Combine(historyDir, $"run-{timestamp}.json");
        File.WriteAllText(historyFile, json);

        Console.WriteLine($"Run metrics saved to: {runMetricsFile}");
        Console.WriteLine($"History saved to: {historyFile}");
    }

    public static void Reset()
    {
        _testMetrics.Clear();
        _runStartTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Store metrics in historical repository for flaky detection analysis
    /// </summary>
    private static void StoreMetricsForFlakyDetection(List<TestMetric> metrics, string runId)
    {
        try
        {
            var repository = new HistoricalMetricsRepository();
            repository.StoreTestMetrics(metrics, runId);
            Console.WriteLine($"[Advanced Observability] Stored {metrics.Count} metrics for flaky detection analysis");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Advanced Observability] Failed to store metrics for flaky detection: {ex.Message}");
        }
    }

    /// <summary>
    /// Run flaky detection analysis and generate alerts
    /// </summary>
    private static void RunFlakyDetectionAnalysis()
    {
        try
        {
            var engine = new FlakyDetectionEngine();
            var analysisResults = engine.AnalyzeFlakyTests();
            
            if (analysisResults.Any())
            {
                GenerateFlakyDetectionAlerts(analysisResults);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Advanced Observability] Flaky detection analysis failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Generate alerts and recommendations for flaky tests
    /// </summary>
    private static void GenerateFlakyDetectionAlerts(List<FlakyAnalysisResult> results)
    {
        var criticalTests = results.Where(r => r.Status == FlakyTestStatus.Quarantined).ToList();
        var flakyTests = results.Where(r => r.Status == FlakyTestStatus.Flaky).ToList();
        
        if (criticalTests.Any())
        {
            Console.WriteLine("");
            Console.WriteLine("🚨 CRITICAL FLAKY TESTS DETECTED - AUTO-QUARANTINE ACTIVATED:");
            foreach (var test in criticalTests)
            {
                Console.WriteLine($"   ⚠️  {test.TestName} ({test.Browser}/{test.SiteId}): {test.FailureRate:P1} failure rate");
                Console.WriteLine($"      Recommendation: {test.Recommendation}");
            }
        }

        if (flakyTests.Any())
        {
            Console.WriteLine("");
            Console.WriteLine("⚠️  FLAKY BEHAVIOR DETECTED:");
            foreach (var test in flakyTests)
            {
                Console.WriteLine($"   • {test.TestName} ({test.Browser}/{test.SiteId}): {test.FailureRate:P1} failure rate ({test.Severity})");
            }
        }

        var recoveryReady = results.Where(r => r.Status == FlakyTestStatus.RecoveryCandidate).ToList();
        if (recoveryReady.Any())
        {
            Console.WriteLine("");
            Console.WriteLine("✅ RECOVERY CANDIDATES:");
            foreach (var test in recoveryReady)
            {
                Console.WriteLine($"   ✓ {test.TestName} ({test.Browser}/{test.SiteId}): Stable for {test.ConsecutiveSuccesses} runs");
            }
        }
        
        Console.WriteLine("");
    }

    /// <summary>
    /// Generate performance trending dashboard and analysis
    /// </summary>
    private static void RunPerformanceTrendingAnalysis()
    {
        try
        {
            var engine = new PerformanceTrendingEngine();
            var dashboard = engine.GenerateDashboard();
            
            Console.WriteLine($"[Advanced Observability] Performance trending dashboard generated with {dashboard.TestResults.Count} test analyses");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Advanced Observability] Performance trending analysis failed: {ex.Message}");
        }
    }
}