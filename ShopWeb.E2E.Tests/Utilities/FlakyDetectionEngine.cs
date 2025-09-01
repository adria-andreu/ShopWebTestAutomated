using System.Text.RegularExpressions;

namespace ShopWeb.E2E.Tests.Utilities;

/// <summary>
/// Core engine for flaky test detection using sliding window analysis
/// Implements sophisticated algorithms for pattern recognition and auto-quarantine workflows
/// </summary>
public class FlakyDetectionEngine
{
    private readonly FlakyDetectionConfig _config;
    private readonly HistoricalMetricsRepository _repository;
    private static readonly object _lockObject = new();

    public FlakyDetectionEngine(FlakyDetectionConfig? config = null)
    {
        _config = config ?? LoadDefaultConfig();
        _repository = new HistoricalMetricsRepository(_config);
    }

    /// <summary>
    /// Analyze all tests in the sliding window for flaky behavior
    /// Returns comprehensive analysis results with recommendations
    /// </summary>
    public List<FlakyAnalysisResult> AnalyzeFlakyTests()
    {
        try
        {
            Console.WriteLine($"[FlakyDetection] Starting analysis with window size: {_config.SlidingWindowSize}");
            
            var recentRuns = _repository.GetRecentRuns(_config.SlidingWindowSize);
            if (recentRuns.Count < _config.MinimumRunsRequired)
            {
                Console.WriteLine($"[FlakyDetection] Insufficient data: {recentRuns.Count} runs < {_config.MinimumRunsRequired} required");
                return new List<FlakyAnalysisResult>();
            }

            var allTestNames = ExtractUniqueTestNames(recentRuns);
            var analysisResults = new List<FlakyAnalysisResult>();

            foreach (var testName in allTestNames)
            {
                if (!IsTestInScope(testName)) continue;

                var testResults = AnalyzeTestBehavior(testName, recentRuns);
                analysisResults.AddRange(testResults);
            }

            // Store results for historical tracking
            _repository.StoreFlakyAnalysis(analysisResults);
            
            // Generate summary report
            GenerateSummaryReport(analysisResults);
            
            return analysisResults;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FlakyDetection] Analysis failed: {ex.Message}");
            return new List<FlakyAnalysisResult>();
        }
    }

    /// <summary>
    /// Analyze specific test behavior across different browser/site combinations
    /// </summary>
    private List<FlakyAnalysisResult> AnalyzeTestBehavior(string testName, List<HistoricalTestRun> runs)
    {
        var results = new List<FlakyAnalysisResult>();
        
        // Get all browser/site combinations for this test
        var combinations = ExtractBrowserSiteCombinations(testName, runs);
        
        foreach (var (browser, siteId) in combinations)
        {
            var history = _repository.GetTestHistory(testName, browser, siteId);
            if (history.Count < _config.MinimumRunsRequired) continue;

            var analysis = PerformSlidingWindowAnalysis(testName, browser, siteId, history);
            results.Add(analysis);
        }

        return results;
    }

    /// <summary>
    /// Core sliding window analysis algorithm
    /// </summary>
    private FlakyAnalysisResult PerformSlidingWindowAnalysis(string testName, string browser, string siteId, 
        List<TestExecutionHistory> history)
    {
        var windowHistory = history.Take(_config.SlidingWindowSize).ToList();
        
        var totalRuns = windowHistory.Count;
        var failedRuns = windowHistory.Count(h => h.Status == "Failed");
        var failureRate = totalRuns > 0 ? (double)failedRuns / totalRuns : 0.0;

        var consecutiveFailures = CalculateConsecutiveFailures(windowHistory);
        var consecutiveSuccesses = CalculateConsecutiveSuccesses(windowHistory);

        var lastFailure = windowHistory.Where(h => h.Status == "Failed")
            .OrderByDescending(h => h.ExecutionTimestamp)
            .FirstOrDefault()?.ExecutionTimestamp;

        var lastSuccess = windowHistory.Where(h => h.Status == "Passed")
            .OrderByDescending(h => h.ExecutionTimestamp)
            .FirstOrDefault()?.ExecutionTimestamp;

        var severity = CalculateSeverity(failureRate);
        var status = DetermineTestStatus(failureRate, consecutiveFailures, consecutiveSuccesses, severity);
        var recommendation = GenerateRecommendation(status, severity, failureRate, consecutiveFailures);

        return new FlakyAnalysisResult
        {
            TestName = testName,
            Browser = browser,
            SiteId = siteId,
            Status = status,
            Severity = severity,
            FailureRate = failureRate,
            TotalRuns = totalRuns,
            FailedRuns = failedRuns,
            ConsecutiveFailures = consecutiveFailures,
            ConsecutiveSuccesses = consecutiveSuccesses,
            LastFailure = lastFailure,
            LastSuccess = lastSuccess,
            AnalysisTimestamp = DateTime.UtcNow,
            Recommendation = recommendation
        };
    }

    /// <summary>
    /// Calculate consecutive failures from the most recent executions
    /// </summary>
    private int CalculateConsecutiveFailures(List<TestExecutionHistory> history)
    {
        var consecutive = 0;
        foreach (var execution in history.OrderByDescending(h => h.ExecutionTimestamp))
        {
            if (execution.Status == "Failed")
                consecutive++;
            else
                break;
        }
        return consecutive;
    }

    /// <summary>
    /// Calculate consecutive successes from the most recent executions
    /// </summary>
    private int CalculateConsecutiveSuccesses(List<TestExecutionHistory> history)
    {
        var consecutive = 0;
        foreach (var execution in history.OrderByDescending(h => h.ExecutionTimestamp))
        {
            if (execution.Status == "Passed")
                consecutive++;
            else
                break;
        }
        return consecutive;
    }

    /// <summary>
    /// Determine severity level based on failure rate
    /// </summary>
    private FlakySeverity CalculateSeverity(double failureRate)
    {
        if (failureRate >= _config.SeverityThresholds.CriticalSeverity) return FlakySeverity.Critical;
        if (failureRate >= _config.SeverityThresholds.HighSeverity) return FlakySeverity.High;
        if (failureRate >= _config.SeverityThresholds.MediumSeverity) return FlakySeverity.Medium;
        if (failureRate >= _config.SeverityThresholds.LowSeverity) return FlakySeverity.Low;
        return FlakySeverity.None;
    }

    /// <summary>
    /// Determine test status for quarantine/recovery workflow
    /// </summary>
    private FlakyTestStatus DetermineTestStatus(double failureRate, int consecutiveFailures, 
        int consecutiveSuccesses, FlakySeverity severity)
    {
        // Auto-quarantine logic
        if (_config.AutoQuarantineEnabled && 
            (failureRate >= _config.QuarantineFailureThreshold || 
             consecutiveFailures >= _config.QuarantineFailureCount))
        {
            return FlakyTestStatus.Quarantined;
        }

        // Recovery logic
        if (_config.AutoRecoveryEnabled && 
            consecutiveSuccesses >= _config.RecoverySuccessCount && 
            failureRate < _config.QuarantineFailureThreshold)
        {
            return FlakyTestStatus.RecoveryCandidate;
        }

        // Classification logic
        if (severity >= FlakySeverity.Medium)
        {
            return FlakyTestStatus.Flaky;
        }
        else if (severity == FlakySeverity.Low)
        {
            return FlakyTestStatus.UnderObservation;
        }

        return FlakyTestStatus.Stable;
    }

    /// <summary>
    /// Generate actionable recommendations based on analysis
    /// </summary>
    private string GenerateRecommendation(FlakyTestStatus status, FlakySeverity severity, 
        double failureRate, int consecutiveFailures)
    {
        return status switch
        {
            FlakyTestStatus.Quarantined => 
                $"AUTO-QUARANTINE: {failureRate:P1} failure rate. Investigate test stability issues.",
            
            FlakyTestStatus.Flaky => 
                $"FLAKY DETECTED: {failureRate:P1} failure rate ({severity} severity). Review test implementation.",
            
            FlakyTestStatus.UnderObservation => 
                "MONITORING: Showing intermittent failures. Continue observation.",
            
            FlakyTestStatus.RecoveryCandidate => 
                "RECOVERY READY: Test showing stable behavior. Consider removing from quarantine.",
            
            FlakyTestStatus.Stable => 
                "STABLE: No flaky behavior detected in current window.",
            
            _ => "Status analysis complete."
        };
    }

    /// <summary>
    /// Extract unique test names from all runs
    /// </summary>
    private List<string> ExtractUniqueTestNames(List<HistoricalTestRun> runs)
    {
        return runs.SelectMany(run => run.TestMetrics)
            .Select(metric => metric.Name)
            .Distinct()
            .OrderBy(name => name)
            .ToList();
    }

    /// <summary>
    /// Extract browser/site combinations for a specific test
    /// </summary>
    private List<(string browser, string siteId)> ExtractBrowserSiteCombinations(string testName, List<HistoricalTestRun> runs)
    {
        return runs.SelectMany(run => run.TestMetrics)
            .Where(metric => metric.Name.Equals(testName, StringComparison.OrdinalIgnoreCase))
            .Select(metric => (metric.Browser, metric.SiteId))
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Check if test is in analysis scope based on configuration
    /// </summary>
    private bool IsTestInScope(string testName)
    {
        if (!string.IsNullOrEmpty(_config.TestNamePattern))
        {
            try
            {
                return Regex.IsMatch(testName, _config.TestNamePattern);
            }
            catch
            {
                return true; // Default to include if pattern is invalid
            }
        }
        return true;
    }

    /// <summary>
    /// Generate summary report of flaky detection results
    /// </summary>
    private void GenerateSummaryReport(List<FlakyAnalysisResult> results)
    {
        var quarantined = results.Count(r => r.Status == FlakyTestStatus.Quarantined);
        var flaky = results.Count(r => r.Status == FlakyTestStatus.Flaky);
        var underObservation = results.Count(r => r.Status == FlakyTestStatus.UnderObservation);
        var recoveryReady = results.Count(r => r.Status == FlakyTestStatus.RecoveryCandidate);

        Console.WriteLine($"[FlakyDetection] Analysis Summary:");
        Console.WriteLine($"  - Total Tests Analyzed: {results.Count}");
        Console.WriteLine($"  - Quarantined: {quarantined}");
        Console.WriteLine($"  - Flaky: {flaky}");
        Console.WriteLine($"  - Under Observation: {underObservation}");
        Console.WriteLine($"  - Recovery Ready: {recoveryReady}");
        
        if (quarantined > 0)
        {
            Console.WriteLine($"[FlakyDetection] ⚠️  {quarantined} tests require immediate attention!");
        }
    }

    /// <summary>
    /// Load default configuration from file or create default
    /// </summary>
    private static FlakyDetectionConfig LoadDefaultConfig()
    {
        try
        {
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "Config", "flakyDetection.json");
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<FlakyDetectionConfig>(json) ?? new FlakyDetectionConfig();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FlakyDetection] Failed to load config: {ex.Message}");
        }
        
        return new FlakyDetectionConfig();
    }
}