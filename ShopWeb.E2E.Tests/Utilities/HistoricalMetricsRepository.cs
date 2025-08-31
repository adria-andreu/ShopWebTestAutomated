using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace ShopWeb.E2E.Tests.Utilities;

/// <summary>
/// Repository for managing historical test metrics and flaky detection data
/// Provides efficient storage, retrieval, and cleanup of test execution history
/// </summary>
public class HistoricalMetricsRepository
{
    private readonly string _baseDirectory;
    private readonly FlakyDetectionConfig _config;
    private static readonly object _lockObject = new();

    public HistoricalMetricsRepository(FlakyDetectionConfig? config = null)
    {
        _baseDirectory = Path.Combine(Directory.GetCurrentDirectory(), "artifacts", "observability");
        _config = config ?? new FlakyDetectionConfig();
        EnsureDirectoriesExist();
    }

    /// <summary>
    /// Store test metrics for historical analysis
    /// </summary>
    public void StoreTestMetrics(List<TestMetric> metrics, string runId)
    {
        if (!metrics.Any()) return;

        try
        {
            var historicalRun = new HistoricalTestRun
            {
                RunId = runId,
                Timestamp = DateTime.UtcNow,
                TestMetrics = metrics,
                Browser = metrics.FirstOrDefault()?.Browser ?? "unknown",
                SiteId = metrics.FirstOrDefault()?.SiteId ?? "unknown",
                CommitSha = metrics.FirstOrDefault()?.CommitSha ?? "unknown"
            };

            var fileName = $"run_{runId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
            var filePath = Path.Combine(_baseDirectory, "runs", fileName);

            lock (_lockObject)
            {
                var json = JsonConvert.SerializeObject(historicalRun, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }

            Console.WriteLine($"[HistoricalMetrics] Stored {metrics.Count} test metrics to {fileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HistoricalMetrics] Failed to store metrics: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieve historical test results for sliding window analysis
    /// </summary>
    public List<HistoricalTestRun> GetRecentRuns(int maxRuns)
    {
        try
        {
            var runsDir = Path.Combine(_baseDirectory, "runs");
            if (!Directory.Exists(runsDir)) return new List<HistoricalTestRun>();

            var files = Directory.GetFiles(runsDir, "run_*.json")
                .OrderByDescending(f => new FileInfo(f).CreationTimeUtc)
                .Take(maxRuns)
                .ToList();

            var runs = new List<HistoricalTestRun>();
            foreach (var file in files)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var run = JsonConvert.DeserializeObject<HistoricalTestRun>(json);
                    if (run != null && IsWithinMaxAge(run.Timestamp))
                    {
                        runs.Add(run);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[HistoricalMetrics] Failed to parse {file}: {ex.Message}");
                }
            }

            return runs;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HistoricalMetrics] Failed to retrieve runs: {ex.Message}");
            return new List<HistoricalTestRun>();
        }
    }

    /// <summary>
    /// Get test execution history for a specific test name
    /// </summary>
    public List<TestExecutionHistory> GetTestHistory(string testName, string? browser = null, string? siteId = null)
    {
        var recentRuns = GetRecentRuns(_config.SlidingWindowSize);
        var history = new List<TestExecutionHistory>();

        foreach (var run in recentRuns)
        {
            var matchingMetrics = run.TestMetrics.Where(m => 
                m.Name.Equals(testName, StringComparison.OrdinalIgnoreCase) &&
                (browser == null || m.Browser.Equals(browser, StringComparison.OrdinalIgnoreCase)) &&
                (siteId == null || m.SiteId.Equals(siteId, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            foreach (var metric in matchingMetrics)
            {
                history.Add(new TestExecutionHistory
                {
                    TestName = metric.Name,
                    Browser = metric.Browser,
                    SiteId = metric.SiteId,
                    Status = metric.Status,
                    DurationMs = metric.DurationMs,
                    ExecutionTimestamp = metric.TimestampUtc,
                    RunId = run.RunId,
                    CommitSha = metric.CommitSha,
                    ErrorMessage = metric.ErrorMessage,
                    Retries = metric.Retries
                });
            }
        }

        return history.OrderByDescending(h => h.ExecutionTimestamp).ToList();
    }

    /// <summary>
    /// Store flaky analysis results
    /// </summary>
    public void StoreFlakyAnalysis(List<FlakyAnalysisResult> analysisResults)
    {
        if (!analysisResults.Any()) return;

        try
        {
            var fileName = $"flaky_analysis_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
            var filePath = Path.Combine(_baseDirectory, "analysis", fileName);

            lock (_lockObject)
            {
                var json = JsonConvert.SerializeObject(analysisResults, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }

            // Also update the latest analysis file
            var latestPath = Path.Combine(_baseDirectory, "analysis", "latest.json");
            File.WriteAllText(latestPath, JsonConvert.SerializeObject(analysisResults, Formatting.Indented));

            Console.WriteLine($"[HistoricalMetrics] Stored flaky analysis: {analysisResults.Count} results");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HistoricalMetrics] Failed to store flaky analysis: {ex.Message}");
        }
    }

    /// <summary>
    /// Get the latest flaky analysis results
    /// </summary>
    public List<FlakyAnalysisResult> GetLatestFlakyAnalysis()
    {
        try
        {
            var latestPath = Path.Combine(_baseDirectory, "analysis", "latest.json");
            if (!File.Exists(latestPath)) return new List<FlakyAnalysisResult>();

            var json = File.ReadAllText(latestPath);
            return JsonConvert.DeserializeObject<List<FlakyAnalysisResult>>(json) ?? new List<FlakyAnalysisResult>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HistoricalMetrics] Failed to retrieve latest analysis: {ex.Message}");
            return new List<FlakyAnalysisResult>();
        }
    }

    /// <summary>
    /// Clean up old historical data beyond retention period
    /// </summary>
    public void CleanupOldData()
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-_config.MaxHistoryAgeDays);
            var runsDir = Path.Combine(_baseDirectory, "runs");
            
            if (Directory.Exists(runsDir))
            {
                var oldFiles = Directory.GetFiles(runsDir, "run_*.json")
                    .Where(f => new FileInfo(f).CreationTimeUtc < cutoffDate)
                    .ToList();

                foreach (var file in oldFiles)
                {
                    File.Delete(file);
                }

                if (oldFiles.Any())
                {
                    Console.WriteLine($"[HistoricalMetrics] Cleaned up {oldFiles.Count} old data files");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HistoricalMetrics] Failed to cleanup old data: {ex.Message}");
        }
    }

    private void EnsureDirectoriesExist()
    {
        var directories = new[]
        {
            Path.Combine(_baseDirectory, "runs"),
            Path.Combine(_baseDirectory, "analysis"),
            Path.Combine(_baseDirectory, "quarantine")
        };

        foreach (var dir in directories)
        {
            Directory.CreateDirectory(dir);
        }
    }

    private bool IsWithinMaxAge(DateTime timestamp)
    {
        return timestamp >= DateTime.UtcNow.AddDays(-_config.MaxHistoryAgeDays);
    }
}

/// <summary>
/// Represents a complete test run with all metrics
/// </summary>
public class HistoricalTestRun
{
    [JsonProperty("runId")]
    public string RunId { get; set; } = string.Empty;

    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonProperty("testMetrics")]
    public List<TestMetric> TestMetrics { get; set; } = new();

    [JsonProperty("browser")]
    public string Browser { get; set; } = string.Empty;

    [JsonProperty("siteId")]
    public string SiteId { get; set; } = string.Empty;

    [JsonProperty("commitSha")]
    public string CommitSha { get; set; } = string.Empty;
}

/// <summary>
/// Represents execution history for a specific test
/// </summary>
public class TestExecutionHistory
{
    [JsonProperty("testName")]
    public string TestName { get; set; } = string.Empty;

    [JsonProperty("browser")]
    public string Browser { get; set; } = string.Empty;

    [JsonProperty("siteId")]
    public string SiteId { get; set; } = string.Empty;

    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;

    [JsonProperty("durationMs")]
    public long DurationMs { get; set; }

    [JsonProperty("executionTimestamp")]
    public DateTime ExecutionTimestamp { get; set; }

    [JsonProperty("runId")]
    public string RunId { get; set; } = string.Empty;

    [JsonProperty("commitSha")]
    public string CommitSha { get; set; } = string.Empty;

    [JsonProperty("errorMessage")]
    public string? ErrorMessage { get; set; }

    [JsonProperty("retries")]
    public int Retries { get; set; }
}