using Newtonsoft.Json;

namespace ShopWeb.E2E.Tests.Utilities;

/// <summary>
/// Configuration for performance trending and regression detection
/// </summary>
public class PerformanceTrendingConfig
{
    [JsonProperty("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonProperty("analysisWindowDays")]
    public int AnalysisWindowDays { get; set; } = 14;

    [JsonProperty("regressionDetection")]
    public RegressionDetectionConfig RegressionDetection { get; set; } = new();

    [JsonProperty("alertThresholds")]
    public AlertThresholdsConfig AlertThresholds { get; set; } = new();

    [JsonProperty("trending")]
    public TrendingConfig Trending { get; set; } = new();

    [JsonProperty("reporting")]
    public ReportingConfig Reporting { get; set; } = new();
}

public class RegressionDetectionConfig
{
    [JsonProperty("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonProperty("thresholdPercentage")]
    public double ThresholdPercentage { get; set; } = 20.0;

    [JsonProperty("minimumSamples")]
    public int MinimumSamples { get; set; } = 5;

    [JsonProperty("compareToBaselinePeriodDays")]
    public int CompareToBaselinePeriodDays { get; set; } = 7;
}

public class AlertThresholdsConfig
{
    [JsonProperty("p95DurationMs")]
    public ThresholdValues P95DurationMs { get; set; } = new() { Warning = 300000, Critical = 600000 };

    [JsonProperty("averageDurationMs")]
    public ThresholdValues AverageDurationMs { get; set; } = new() { Warning = 180000, Critical = 360000 };

    [JsonProperty("regressionPercentage")]
    public ThresholdValues RegressionPercentage { get; set; } = new() { Warning = 15.0, Critical = 25.0 };
}

public class ThresholdValues
{
    [JsonProperty("warning")]
    public double Warning { get; set; }

    [JsonProperty("critical")]
    public double Critical { get; set; }
}

public class TrendingConfig
{
    [JsonProperty("smoothingFactor")]
    public double SmoothingFactor { get; set; } = 0.3;

    [JsonProperty("outlierDetectionEnabled")]
    public bool OutlierDetectionEnabled { get; set; } = true;

    [JsonProperty("outlierThresholdStdDev")]
    public double OutlierThresholdStdDev { get; set; } = 2.0;
}

public class ReportingConfig
{
    [JsonProperty("generateHtmlReport")]
    public bool GenerateHtmlReport { get; set; } = true;

    [JsonProperty("generateJsonReport")]
    public bool GenerateJsonReport { get; set; } = true;

    [JsonProperty("retentionDays")]
    public int RetentionDays { get; set; } = 90;
}

/// <summary>
/// Performance data point for trending analysis
/// </summary>
public class PerformanceDataPoint
{
    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonProperty("testName")]
    public string TestName { get; set; } = string.Empty;

    [JsonProperty("browser")]
    public string Browser { get; set; } = string.Empty;

    [JsonProperty("siteId")]
    public string SiteId { get; set; } = string.Empty;

    [JsonProperty("durationMs")]
    public long DurationMs { get; set; }

    [JsonProperty("runId")]
    public string RunId { get; set; } = string.Empty;

    [JsonProperty("commitSha")]
    public string CommitSha { get; set; } = string.Empty;

    [JsonProperty("isOutlier")]
    public bool IsOutlier { get; set; }
}

/// <summary>
/// Trending analysis result for a specific test
/// </summary>
public class PerformanceTrendingResult
{
    [JsonProperty("testName")]
    public string TestName { get; set; } = string.Empty;

    [JsonProperty("browser")]
    public string Browser { get; set; } = string.Empty;

    [JsonProperty("siteId")]
    public string SiteId { get; set; } = string.Empty;

    [JsonProperty("analysisTimestamp")]
    public DateTime AnalysisTimestamp { get; set; }

    [JsonProperty("dataPoints")]
    public List<PerformanceDataPoint> DataPoints { get; set; } = new();

    [JsonProperty("statistics")]
    public PerformanceStatistics Statistics { get; set; } = new();

    [JsonProperty("trend")]
    public TrendAnalysis Trend { get; set; } = new();

    [JsonProperty("regressions")]
    public List<RegressionAlert> Regressions { get; set; } = new();

    [JsonProperty("alertLevel")]
    public AlertLevel AlertLevel { get; set; }

    [JsonProperty("recommendations")]
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// Statistical analysis of performance data
/// </summary>
public class PerformanceStatistics
{
    [JsonProperty("totalSamples")]
    public int TotalSamples { get; set; }

    [JsonProperty("averageDurationMs")]
    public double AverageDurationMs { get; set; }

    [JsonProperty("medianDurationMs")]
    public double MedianDurationMs { get; set; }

    [JsonProperty("p95DurationMs")]
    public double P95DurationMs { get; set; }

    [JsonProperty("p99DurationMs")]
    public double P99DurationMs { get; set; }

    [JsonProperty("minDurationMs")]
    public long MinDurationMs { get; set; }

    [JsonProperty("maxDurationMs")]
    public long MaxDurationMs { get; set; }

    [JsonProperty("standardDeviation")]
    public double StandardDeviation { get; set; }

    [JsonProperty("coefficientOfVariation")]
    public double CoefficientOfVariation { get; set; }

    [JsonProperty("outlierCount")]
    public int OutlierCount { get; set; }
}

/// <summary>
/// Trend analysis with smoothed values and direction
/// </summary>
public class TrendAnalysis
{
    [JsonProperty("direction")]
    public TrendDirection Direction { get; set; }

    [JsonProperty("magnitude")]
    public double Magnitude { get; set; }

    [JsonProperty("confidence")]
    public double Confidence { get; set; }

    [JsonProperty("smoothedValues")]
    public List<double> SmoothedValues { get; set; } = new();

    [JsonProperty("trendLine")]
    public LinearTrend TrendLine { get; set; } = new();

    [JsonProperty("seasonalPatterns")]
    public List<SeasonalPattern> SeasonalPatterns { get; set; } = new();
}

public class LinearTrend
{
    [JsonProperty("slope")]
    public double Slope { get; set; }

    [JsonProperty("intercept")]
    public double Intercept { get; set; }

    [JsonProperty("rSquared")]
    public double RSquared { get; set; }
}

public class SeasonalPattern
{
    [JsonProperty("pattern")]
    public string Pattern { get; set; } = string.Empty;

    [JsonProperty("strength")]
    public double Strength { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Performance regression alert
/// </summary>
public class RegressionAlert
{
    [JsonProperty("detectedAt")]
    public DateTime DetectedAt { get; set; }

    [JsonProperty("regressionType")]
    public RegressionType RegressionType { get; set; }

    [JsonProperty("severityLevel")]
    public AlertLevel SeverityLevel { get; set; }

    [JsonProperty("currentValue")]
    public double CurrentValue { get; set; }

    [JsonProperty("baselineValue")]
    public double BaselineValue { get; set; }

    [JsonProperty("changePercentage")]
    public double ChangePercentage { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("affectedCommits")]
    public List<string> AffectedCommits { get; set; } = new();
}

/// <summary>
/// Enumerations for trending analysis
/// </summary>
public enum TrendDirection
{
    Unknown,
    Improving,
    Stable,
    Degrading,
    Volatile
}

public enum RegressionType
{
    P95Duration,
    AverageDuration,
    MedianDuration,
    MaxDuration,
    Variability
}

public enum AlertLevel
{
    None,
    Info,
    Warning,
    Critical
}

/// <summary>
/// Performance dashboard summary for reporting
/// </summary>
public class PerformanceDashboard
{
    [JsonProperty("generatedAt")]
    public DateTime GeneratedAt { get; set; }

    [JsonProperty("analysisWindow")]
    public AnalysisWindow AnalysisWindow { get; set; } = new();

    [JsonProperty("overallSummary")]
    public OverallPerformanceSummary OverallSummary { get; set; } = new();

    [JsonProperty("testResults")]
    public List<PerformanceTrendingResult> TestResults { get; set; } = new();

    [JsonProperty("alerts")]
    public List<RegressionAlert> Alerts { get; set; } = new();

    [JsonProperty("topRecommendations")]
    public List<string> TopRecommendations { get; set; } = new();
}

public class AnalysisWindow
{
    [JsonProperty("startDate")]
    public DateTime StartDate { get; set; }

    [JsonProperty("endDate")]
    public DateTime EndDate { get; set; }

    [JsonProperty("totalDays")]
    public int TotalDays { get; set; }
}

public class OverallPerformanceSummary
{
    [JsonProperty("totalTestsAnalyzed")]
    public int TotalTestsAnalyzed { get; set; }

    [JsonProperty("averageP95DurationMs")]
    public double AverageP95DurationMs { get; set; }

    [JsonProperty("testsWithRegressions")]
    public int TestsWithRegressions { get; set; }

    [JsonProperty("testsWithImprovements")]
    public int TestsWithImprovements { get; set; }

    [JsonProperty("overallTrendDirection")]
    public TrendDirection OverallTrendDirection { get; set; }

    [JsonProperty("alertCounts")]
    public Dictionary<AlertLevel, int> AlertCounts { get; set; } = new();
}