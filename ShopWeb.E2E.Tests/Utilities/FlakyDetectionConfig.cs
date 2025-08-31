using Newtonsoft.Json;

namespace ShopWeb.E2E.Tests.Utilities;

/// <summary>
/// Configuration for advanced flaky test detection system
/// Supports sliding window analysis, auto-quarantine, and recovery workflows
/// </summary>
public class FlakyDetectionConfig
{
    /// <summary>
    /// Number of recent test runs to analyze (sliding window size)
    /// </summary>
    [JsonProperty("slidingWindowSize")]
    public int SlidingWindowSize { get; set; } = 20;

    /// <summary>
    /// Minimum number of runs required before flaky detection kicks in
    /// </summary>
    [JsonProperty("minimumRunsRequired")]
    public int MinimumRunsRequired { get; set; } = 5;

    /// <summary>
    /// Failure threshold for quarantine (e.g., 0.3 = 30% failure rate)
    /// </summary>
    [JsonProperty("quarantineFailureThreshold")]
    public double QuarantineFailureThreshold { get; set; } = 0.3;

    /// <summary>
    /// Number of failures in window to trigger quarantine
    /// </summary>
    [JsonProperty("quarantineFailureCount")]
    public int QuarantineFailureCount { get; set; } = 3;

    /// <summary>
    /// Number of consecutive successes required for recovery
    /// </summary>
    [JsonProperty("recoverySuccessCount")]
    public int RecoverySuccessCount { get; set; } = 5;

    /// <summary>
    /// Maximum age of historical data to consider (days)
    /// </summary>
    [JsonProperty("maxHistoryAgeDays")]
    public int MaxHistoryAgeDays { get; set; } = 30;

    /// <summary>
    /// Enable auto-quarantine workflow
    /// </summary>
    [JsonProperty("autoQuarantineEnabled")]
    public bool AutoQuarantineEnabled { get; set; } = true;

    /// <summary>
    /// Enable auto-recovery workflow
    /// </summary>
    [JsonProperty("autoRecoveryEnabled")]
    public bool AutoRecoveryEnabled { get; set; } = true;

    /// <summary>
    /// Pattern matching for test names (regex)
    /// </summary>
    [JsonProperty("testNamePattern")]
    public string TestNamePattern { get; set; } = ".*";

    /// <summary>
    /// Browsers to analyze (empty = all browsers)
    /// </summary>
    [JsonProperty("targetBrowsers")]
    public List<string> TargetBrowsers { get; set; } = new();

    /// <summary>
    /// Sites to analyze (empty = all sites)
    /// </summary>
    [JsonProperty("targetSites")]
    public List<string> TargetSites { get; set; } = new();

    /// <summary>
    /// Severity levels for different failure rates
    /// </summary>
    [JsonProperty("severityThresholds")]
    public FlakyDetectionSeverityThresholds SeverityThresholds { get; set; } = new();
}

/// <summary>
/// Thresholds for different severity levels of flaky tests
/// </summary>
public class FlakyDetectionSeverityThresholds
{
    [JsonProperty("lowSeverity")]
    public double LowSeverity { get; set; } = 0.1; // 10% failure rate

    [JsonProperty("mediumSeverity")]
    public double MediumSeverity { get; set; } = 0.2; // 20% failure rate

    [JsonProperty("highSeverity")]
    public double HighSeverity { get; set; } = 0.3; // 30% failure rate

    [JsonProperty("criticalSeverity")]
    public double CriticalSeverity { get; set; } = 0.5; // 50% failure rate
}

/// <summary>
/// Status of a test in the flaky detection system
/// </summary>
public enum FlakyTestStatus
{
    Stable,
    UnderObservation,
    Flaky,
    Quarantined,
    RecoveryCandidate,
    Recovered
}

/// <summary>
/// Severity level of a flaky test
/// </summary>
public enum FlakySeverity
{
    None,
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Historical analysis result for a specific test
/// </summary>
public class FlakyAnalysisResult
{
    [JsonProperty("testName")]
    public string TestName { get; set; } = string.Empty;

    [JsonProperty("browser")]
    public string Browser { get; set; } = string.Empty;

    [JsonProperty("siteId")]
    public string SiteId { get; set; } = string.Empty;

    [JsonProperty("status")]
    public FlakyTestStatus Status { get; set; }

    [JsonProperty("severity")]
    public FlakySeverity Severity { get; set; }

    [JsonProperty("failureRate")]
    public double FailureRate { get; set; }

    [JsonProperty("totalRuns")]
    public int TotalRuns { get; set; }

    [JsonProperty("failedRuns")]
    public int FailedRuns { get; set; }

    [JsonProperty("consecutiveFailures")]
    public int ConsecutiveFailures { get; set; }

    [JsonProperty("consecutiveSuccesses")]
    public int ConsecutiveSuccesses { get; set; }

    [JsonProperty("lastFailure")]
    public DateTime? LastFailure { get; set; }

    [JsonProperty("lastSuccess")]
    public DateTime? LastSuccess { get; set; }

    [JsonProperty("analysisTimestamp")]
    public DateTime AnalysisTimestamp { get; set; }

    [JsonProperty("recommendation")]
    public string Recommendation { get; set; } = string.Empty;

    [JsonProperty("quarantinedAt")]
    public DateTime? QuarantinedAt { get; set; }

    [JsonProperty("recoveredAt")]
    public DateTime? RecoveredAt { get; set; }
}