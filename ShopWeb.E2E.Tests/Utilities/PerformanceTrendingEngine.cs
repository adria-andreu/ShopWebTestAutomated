using Newtonsoft.Json;
using System.Text;

namespace ShopWeb.E2E.Tests.Utilities;

/// <summary>
/// Advanced performance trending engine with regression detection and P95 analysis
/// Provides sophisticated performance monitoring, alerting, and dashboard generation
/// </summary>
public class PerformanceTrendingEngine
{
    private readonly PerformanceTrendingConfig _config;
    private readonly HistoricalMetricsRepository _repository;
    private readonly string _reportsDirectory;

    public PerformanceTrendingEngine(PerformanceTrendingConfig? config = null)
    {
        _config = config ?? LoadDefaultConfig();
        _repository = new HistoricalMetricsRepository();
        _reportsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "artifacts", "performance-reports");
        Directory.CreateDirectory(_reportsDirectory);
    }

    /// <summary>
    /// Generate comprehensive performance trending dashboard
    /// </summary>
    public PerformanceDashboard GenerateDashboard()
    {
        try
        {
            if (!_config.Enabled)
            {
                Console.WriteLine("[PerformanceTrending] Performance trending is disabled");
                return new PerformanceDashboard { GeneratedAt = DateTime.UtcNow };
            }

            Console.WriteLine($"[PerformanceTrending] Generating dashboard for {_config.AnalysisWindowDays}-day window");

            var analysisWindow = CreateAnalysisWindow();
            var recentRuns = GetRecentRunsInWindow(analysisWindow);

            if (!recentRuns.Any())
            {
                Console.WriteLine("[PerformanceTrending] No data available for analysis");
                return new PerformanceDashboard 
                { 
                    GeneratedAt = DateTime.UtcNow,
                    AnalysisWindow = analysisWindow
                };
            }

            var testResults = AnalyzeAllTests(recentRuns);
            var dashboard = CreateDashboard(analysisWindow, testResults);

            if (_config.Reporting.GenerateJsonReport)
            {
                SaveDashboardAsJson(dashboard);
            }

            if (_config.Reporting.GenerateHtmlReport)
            {
                GenerateHtmlReport(dashboard);
            }

            GenerateConsoleReport(dashboard);
            return dashboard;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PerformanceTrending] Dashboard generation failed: {ex.Message}");
            return new PerformanceDashboard { GeneratedAt = DateTime.UtcNow };
        }
    }

    /// <summary>
    /// Analyze performance trends for all tests in the dataset
    /// </summary>
    private List<PerformanceTrendingResult> AnalyzeAllTests(List<HistoricalTestRun> runs)
    {
        var testCombinations = ExtractTestCombinations(runs);
        var results = new List<PerformanceTrendingResult>();

        foreach (var (testName, browser, siteId) in testCombinations)
        {
            var dataPoints = ExtractPerformanceDataPoints(runs, testName, browser, siteId);
            if (dataPoints.Count < _config.RegressionDetection.MinimumSamples) continue;

            var result = AnalyzePerformanceTrend(testName, browser, siteId, dataPoints);
            results.Add(result);
        }

        return results.OrderByDescending(r => r.AlertLevel).ThenBy(r => r.TestName).ToList();
    }

    /// <summary>
    /// Perform comprehensive performance trend analysis for a specific test
    /// </summary>
    private PerformanceTrendingResult AnalyzePerformanceTrend(string testName, string browser, string siteId, 
        List<PerformanceDataPoint> dataPoints)
    {
        // Remove outliers if enabled
        if (_config.Trending.OutlierDetectionEnabled)
        {
            dataPoints = DetectAndMarkOutliers(dataPoints);
        }

        var statistics = CalculateStatistics(dataPoints);
        var trend = AnalyzeTrend(dataPoints);
        var regressions = DetectRegressions(dataPoints, statistics);
        var alertLevel = DetermineAlertLevel(statistics, regressions);
        var recommendations = GenerateRecommendations(statistics, trend, regressions, alertLevel);

        return new PerformanceTrendingResult
        {
            TestName = testName,
            Browser = browser,
            SiteId = siteId,
            AnalysisTimestamp = DateTime.UtcNow,
            DataPoints = dataPoints,
            Statistics = statistics,
            Trend = trend,
            Regressions = regressions,
            AlertLevel = alertLevel,
            Recommendations = recommendations
        };
    }

    /// <summary>
    /// Detect and mark outliers using statistical analysis
    /// </summary>
    private List<PerformanceDataPoint> DetectAndMarkOutliers(List<PerformanceDataPoint> dataPoints)
    {
        if (dataPoints.Count < 5) return dataPoints;

        var durations = dataPoints.Select(dp => (double)dp.DurationMs).ToList();
        var mean = durations.Average();
        var stdDev = Math.Sqrt(durations.Average(v => Math.Pow(v - mean, 2)));
        var threshold = _config.Trending.OutlierThresholdStdDev * stdDev;

        foreach (var point in dataPoints)
        {
            point.IsOutlier = Math.Abs(point.DurationMs - mean) > threshold;
        }

        return dataPoints;
    }

    /// <summary>
    /// Calculate comprehensive performance statistics
    /// </summary>
    private PerformanceStatistics CalculateStatistics(List<PerformanceDataPoint> dataPoints)
    {
        var validPoints = dataPoints.Where(dp => !dp.IsOutlier).ToList();
        if (!validPoints.Any()) validPoints = dataPoints; // Fallback to all points

        var durations = validPoints.Select(dp => (double)dp.DurationMs).OrderBy(d => d).ToList();
        var mean = durations.Average();
        var stdDev = durations.Count > 1 
            ? Math.Sqrt(durations.Average(v => Math.Pow(v - mean, 2))) 
            : 0;

        return new PerformanceStatistics
        {
            TotalSamples = dataPoints.Count,
            AverageDurationMs = mean,
            MedianDurationMs = CalculatePercentile(durations, 50),
            P95DurationMs = CalculatePercentile(durations, 95),
            P99DurationMs = CalculatePercentile(durations, 99),
            MinDurationMs = (long)durations.Min(),
            MaxDurationMs = (long)durations.Max(),
            StandardDeviation = stdDev,
            CoefficientOfVariation = mean > 0 ? stdDev / mean : 0,
            OutlierCount = dataPoints.Count(dp => dp.IsOutlier)
        };
    }

    /// <summary>
    /// Analyze performance trend with smoothing and direction detection
    /// </summary>
    private TrendAnalysis AnalyzeTrend(List<PerformanceDataPoint> dataPoints)
    {
        var sortedPoints = dataPoints.OrderBy(dp => dp.Timestamp).ToList();
        var durations = sortedPoints.Select(dp => (double)dp.DurationMs).ToList();
        
        var smoothedValues = ApplyExponentialSmoothing(durations, _config.Trending.SmoothingFactor);
        var linearTrend = CalculateLinearTrend(durations);
        var direction = DetermineTrendDirection(linearTrend, smoothedValues);
        var magnitude = CalculateTrendMagnitude(smoothedValues);
        var confidence = CalculateTrendConfidence(linearTrend.RSquared);

        return new TrendAnalysis
        {
            Direction = direction,
            Magnitude = magnitude,
            Confidence = confidence,
            SmoothedValues = smoothedValues,
            TrendLine = linearTrend,
            SeasonalPatterns = DetectSeasonalPatterns(dataPoints)
        };
    }

    /// <summary>
    /// Detect performance regressions using multiple algorithms
    /// </summary>
    private List<RegressionAlert> DetectRegressions(List<PerformanceDataPoint> dataPoints, PerformanceStatistics statistics)
    {
        var alerts = new List<RegressionAlert>();

        if (!_config.RegressionDetection.Enabled || dataPoints.Count < _config.RegressionDetection.MinimumSamples)
            return alerts;

        var baselinePeriod = DateTime.UtcNow.AddDays(-_config.RegressionDetection.CompareToBaselinePeriodDays);
        var baselinePoints = dataPoints.Where(dp => dp.Timestamp <= baselinePeriod).ToList();
        var recentPoints = dataPoints.Where(dp => dp.Timestamp > baselinePeriod).ToList();

        if (!baselinePoints.Any() || !recentPoints.Any()) return alerts;

        var baselineP95 = CalculatePercentile(baselinePoints.Select(dp => (double)dp.DurationMs).ToList(), 95);
        var recentP95 = CalculatePercentile(recentPoints.Select(dp => (double)dp.DurationMs).ToList(), 95);

        var regressionPercentage = (recentP95 - baselineP95) / baselineP95 * 100;
        
        if (Math.Abs(regressionPercentage) >= _config.RegressionDetection.ThresholdPercentage)
        {
            var severity = DetermineRegressionSeverity(regressionPercentage);
            
            alerts.Add(new RegressionAlert
            {
                DetectedAt = DateTime.UtcNow,
                RegressionType = RegressionType.P95Duration,
                SeverityLevel = severity,
                CurrentValue = recentP95,
                BaselineValue = baselineP95,
                ChangePercentage = regressionPercentage,
                Description = GenerateRegressionDescription(RegressionType.P95Duration, regressionPercentage),
                AffectedCommits = ExtractRecentCommits(recentPoints)
            });
        }

        return alerts;
    }

    /// <summary>
    /// Apply exponential smoothing for trend analysis
    /// </summary>
    private List<double> ApplyExponentialSmoothing(List<double> values, double alpha)
    {
        if (!values.Any()) return new List<double>();

        var smoothed = new List<double> { values[0] };
        
        for (int i = 1; i < values.Count; i++)
        {
            var smoothedValue = alpha * values[i] + (1 - alpha) * smoothed[i - 1];
            smoothed.Add(smoothedValue);
        }

        return smoothed;
    }

    /// <summary>
    /// Calculate linear trend using least squares regression
    /// </summary>
    private LinearTrend CalculateLinearTrend(List<double> values)
    {
        if (values.Count < 2) return new LinearTrend();

        var n = values.Count;
        var x = Enumerable.Range(0, n).Select(i => (double)i).ToArray();
        var y = values.ToArray();

        var sumX = x.Sum();
        var sumY = y.Sum();
        var sumXY = x.Zip(y, (xi, yi) => xi * yi).Sum();
        var sumXX = x.Sum(xi => xi * xi);

        var slope = (n * sumXY - sumX * sumY) / (n * sumXX - sumX * sumX);
        var intercept = (sumY - slope * sumX) / n;

        // Calculate R-squared
        var yMean = y.Average();
        var ssTotal = y.Sum(yi => Math.Pow(yi - yMean, 2));
        var ssResidual = x.Zip(y, (xi, yi) => Math.Pow(yi - (slope * xi + intercept), 2)).Sum();
        var rSquared = ssTotal > 0 ? 1 - (ssResidual / ssTotal) : 0;

        return new LinearTrend
        {
            Slope = slope,
            Intercept = intercept,
            RSquared = Math.Max(0, Math.Min(1, rSquared))
        };
    }

    /// <summary>
    /// Generate actionable recommendations based on analysis
    /// </summary>
    private List<string> GenerateRecommendations(PerformanceStatistics statistics, TrendAnalysis trend, 
        List<RegressionAlert> regressions, AlertLevel alertLevel)
    {
        var recommendations = new List<string>();

        if (alertLevel >= AlertLevel.Critical)
        {
            recommendations.Add("🚨 CRITICAL: Immediate performance investigation required");
        }

        if (regressions.Any(r => r.SeverityLevel >= AlertLevel.Warning))
        {
            recommendations.Add("📈 Performance regression detected - review recent code changes");
        }

        if (trend.Direction == TrendDirection.Degrading)
        {
            recommendations.Add("📉 Performance declining over time - consider optimization review");
        }

        if (statistics.CoefficientOfVariation > 0.5)
        {
            recommendations.Add("📊 High performance variability - investigate test stability");
        }

        if (statistics.OutlierCount > statistics.TotalSamples * 0.1)
        {
            recommendations.Add("🎯 Frequent outliers detected - check for environmental factors");
        }

        if (!recommendations.Any())
        {
            recommendations.Add("✅ Performance appears stable within expected parameters");
        }

        return recommendations;
    }

    /// <summary>
    /// Generate console summary report
    /// </summary>
    private void GenerateConsoleReport(PerformanceDashboard dashboard)
    {
        Console.WriteLine("");
        Console.WriteLine("📊 PERFORMANCE TRENDING DASHBOARD");
        Console.WriteLine($"   Analysis Window: {dashboard.AnalysisWindow.StartDate:yyyy-MM-dd} to {dashboard.AnalysisWindow.EndDate:yyyy-MM-dd}");
        Console.WriteLine($"   Tests Analyzed: {dashboard.OverallSummary.TotalTestsAnalyzed}");
        Console.WriteLine($"   Overall Trend: {dashboard.OverallSummary.OverallTrendDirection}");

        var criticalAlerts = dashboard.Alerts.Where(a => a.SeverityLevel == AlertLevel.Critical).ToList();
        var warningAlerts = dashboard.Alerts.Where(a => a.SeverityLevel == AlertLevel.Warning).ToList();

        if (criticalAlerts.Any())
        {
            Console.WriteLine("");
            Console.WriteLine("🚨 CRITICAL PERFORMANCE REGRESSIONS:");
            foreach (var alert in criticalAlerts.Take(5))
            {
                Console.WriteLine($"   ⚠️  {alert.Description} ({alert.ChangePercentage:+0.0}%)");
            }
        }

        if (warningAlerts.Any())
        {
            Console.WriteLine("");
            Console.WriteLine("⚠️  PERFORMANCE WARNINGS:");
            foreach (var alert in warningAlerts.Take(3))
            {
                Console.WriteLine($"   • {alert.Description} ({alert.ChangePercentage:+0.0}%)");
            }
        }

        if (dashboard.TopRecommendations.Any())
        {
            Console.WriteLine("");
            Console.WriteLine("💡 TOP RECOMMENDATIONS:");
            foreach (var recommendation in dashboard.TopRecommendations.Take(3))
            {
                Console.WriteLine($"   {recommendation}");
            }
        }

        Console.WriteLine("");
    }

    #region Helper Methods

    private AnalysisWindow CreateAnalysisWindow()
    {
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddDays(-_config.AnalysisWindowDays);
        
        return new AnalysisWindow
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalDays = _config.AnalysisWindowDays
        };
    }

    private List<HistoricalTestRun> GetRecentRunsInWindow(AnalysisWindow window)
    {
        return _repository.GetRecentRuns(100) // Get more runs and filter by date
            .Where(run => run.Timestamp >= window.StartDate && run.Timestamp <= window.EndDate)
            .ToList();
    }

    private List<(string testName, string browser, string siteId)> ExtractTestCombinations(List<HistoricalTestRun> runs)
    {
        return runs.SelectMany(run => run.TestMetrics)
            .Select(metric => (metric.Name, metric.Browser, metric.SiteId))
            .Distinct()
            .ToList();
    }

    private List<PerformanceDataPoint> ExtractPerformanceDataPoints(List<HistoricalTestRun> runs, 
        string testName, string browser, string siteId)
    {
        return runs.SelectMany(run => run.TestMetrics
                .Where(m => m.Name == testName && m.Browser == browser && m.SiteId == siteId)
                .Select(m => new PerformanceDataPoint
                {
                    Timestamp = m.TimestampUtc,
                    TestName = m.Name,
                    Browser = m.Browser,
                    SiteId = m.SiteId,
                    DurationMs = m.DurationMs,
                    RunId = run.RunId,
                    CommitSha = m.CommitSha
                }))
            .OrderBy(dp => dp.Timestamp)
            .ToList();
    }

    private double CalculatePercentile(List<double> sortedValues, int percentile)
    {
        if (!sortedValues.Any()) return 0;
        
        var index = (double)(percentile / 100.0) * (sortedValues.Count - 1);
        var lower = (int)Math.Floor(index);
        var upper = (int)Math.Ceiling(index);
        var weight = index - lower;

        if (upper >= sortedValues.Count) return sortedValues.Last();
        if (lower < 0) return sortedValues.First();

        return sortedValues[lower] * (1 - weight) + sortedValues[upper] * weight;
    }

    private TrendDirection DetermineTrendDirection(LinearTrend trend, List<double> smoothedValues)
    {
        if (trend.RSquared < 0.1) return TrendDirection.Volatile;
        
        if (Math.Abs(trend.Slope) < 0.1) return TrendDirection.Stable;
        
        return trend.Slope > 0 ? TrendDirection.Degrading : TrendDirection.Improving;
    }

    private double CalculateTrendMagnitude(List<double> smoothedValues)
    {
        if (smoothedValues.Count < 2) return 0;
        
        var start = smoothedValues.First();
        var end = smoothedValues.Last();
        
        return Math.Abs((end - start) / start * 100);
    }

    private double CalculateTrendConfidence(double rSquared)
    {
        return Math.Max(0, Math.Min(1, rSquared)) * 100;
    }

    private List<SeasonalPattern> DetectSeasonalPatterns(List<PerformanceDataPoint> dataPoints)
    {
        // Simplified seasonal pattern detection
        return new List<SeasonalPattern>();
    }

    private AlertLevel DetermineAlertLevel(PerformanceStatistics statistics, List<RegressionAlert> regressions)
    {
        if (regressions.Any(r => r.SeverityLevel == AlertLevel.Critical) || 
            statistics.P95DurationMs > _config.AlertThresholds.P95DurationMs.Critical)
            return AlertLevel.Critical;

        if (regressions.Any(r => r.SeverityLevel == AlertLevel.Warning) ||
            statistics.P95DurationMs > _config.AlertThresholds.P95DurationMs.Warning)
            return AlertLevel.Warning;

        return AlertLevel.None;
    }

    private AlertLevel DetermineRegressionSeverity(double regressionPercentage)
    {
        var absPercentage = Math.Abs(regressionPercentage);
        
        if (absPercentage >= _config.AlertThresholds.RegressionPercentage.Critical)
            return AlertLevel.Critical;
        
        if (absPercentage >= _config.AlertThresholds.RegressionPercentage.Warning)
            return AlertLevel.Warning;
            
        return AlertLevel.Info;
    }

    private string GenerateRegressionDescription(RegressionType type, double changePercentage)
    {
        var direction = changePercentage > 0 ? "increased" : "decreased";
        return $"P95 duration has {direction} by {Math.Abs(changePercentage):F1}%";
    }

    private List<string> ExtractRecentCommits(List<PerformanceDataPoint> recentPoints)
    {
        return recentPoints.Select(dp => dp.CommitSha)
            .Where(sha => !string.IsNullOrEmpty(sha))
            .Distinct()
            .ToList();
    }

    private PerformanceDashboard CreateDashboard(AnalysisWindow window, List<PerformanceTrendingResult> results)
    {
        var allAlerts = results.SelectMany(r => r.Regressions).ToList();
        var topRecommendations = results.SelectMany(r => r.Recommendations)
            .GroupBy(rec => rec)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => g.Key)
            .ToList();

        return new PerformanceDashboard
        {
            GeneratedAt = DateTime.UtcNow,
            AnalysisWindow = window,
            OverallSummary = CreateOverallSummary(results),
            TestResults = results,
            Alerts = allAlerts,
            TopRecommendations = topRecommendations
        };
    }

    private OverallPerformanceSummary CreateOverallSummary(List<PerformanceTrendingResult> results)
    {
        var regressions = results.Count(r => r.Trend.Direction == TrendDirection.Degrading);
        var improvements = results.Count(r => r.Trend.Direction == TrendDirection.Improving);
        var avgP95 = results.Any() ? results.Average(r => r.Statistics.P95DurationMs) : 0;

        var alertCounts = new Dictionary<AlertLevel, int>
        {
            [AlertLevel.None] = results.Count(r => r.AlertLevel == AlertLevel.None),
            [AlertLevel.Info] = results.Count(r => r.AlertLevel == AlertLevel.Info),
            [AlertLevel.Warning] = results.Count(r => r.AlertLevel == AlertLevel.Warning),
            [AlertLevel.Critical] = results.Count(r => r.AlertLevel == AlertLevel.Critical)
        };

        return new OverallPerformanceSummary
        {
            TotalTestsAnalyzed = results.Count,
            AverageP95DurationMs = avgP95,
            TestsWithRegressions = regressions,
            TestsWithImprovements = improvements,
            OverallTrendDirection = DetermineOverallTrend(results),
            AlertCounts = alertCounts
        };
    }

    private TrendDirection DetermineOverallTrend(List<PerformanceTrendingResult> results)
    {
        if (!results.Any()) return TrendDirection.Unknown;

        var trends = results.Select(r => r.Trend.Direction).ToList();
        var degrading = trends.Count(t => t == TrendDirection.Degrading);
        var improving = trends.Count(t => t == TrendDirection.Improving);

        if (degrading > improving * 1.5) return TrendDirection.Degrading;
        if (improving > degrading * 1.5) return TrendDirection.Improving;
        
        return TrendDirection.Stable;
    }

    private void SaveDashboardAsJson(PerformanceDashboard dashboard)
    {
        try
        {
            var fileName = $"performance_dashboard_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
            var filePath = Path.Combine(_reportsDirectory, fileName);
            
            var json = JsonConvert.SerializeObject(dashboard, Formatting.Indented);
            File.WriteAllText(filePath, json);
            
            // Also save as latest
            var latestPath = Path.Combine(_reportsDirectory, "latest.json");
            File.WriteAllText(latestPath, json);

            Console.WriteLine($"[PerformanceTrending] Dashboard saved to {fileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PerformanceTrending] Failed to save JSON dashboard: {ex.Message}");
        }
    }

    private void GenerateHtmlReport(PerformanceDashboard dashboard)
    {
        try
        {
            var html = GenerateHtmlContent(dashboard);
            var fileName = $"performance_dashboard_{DateTime.UtcNow:yyyyMMdd_HHmmss}.html";
            var filePath = Path.Combine(_reportsDirectory, fileName);
            
            File.WriteAllText(filePath, html);
            
            // Also save as latest
            var latestPath = Path.Combine(_reportsDirectory, "latest.html");
            File.WriteAllText(latestPath, html);

            Console.WriteLine($"[PerformanceTrending] HTML report saved to {fileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PerformanceTrending] Failed to generate HTML report: {ex.Message}");
        }
    }

    private string GenerateHtmlContent(PerformanceDashboard dashboard)
    {
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html><head>");
        html.AppendLine("<title>ShopWeb Performance Trending Dashboard</title>");
        html.AppendLine("<style>");
        html.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; background-color: #f5f5f5; }");
        html.AppendLine(".header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; border-radius: 8px; margin-bottom: 20px; }");
        html.AppendLine(".card { background: white; padding: 15px; border-radius: 8px; margin-bottom: 15px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }");
        html.AppendLine(".alert-critical { border-left: 5px solid #e74c3c; background-color: #fdf2f2; }");
        html.AppendLine(".alert-warning { border-left: 5px solid #f39c12; background-color: #fefbf3; }");
        html.AppendLine(".trend-degrading { color: #e74c3c; }");
        html.AppendLine(".trend-improving { color: #27ae60; }");
        html.AppendLine(".trend-stable { color: #2ecc71; }");
        html.AppendLine("</style></head><body>");

        // Header
        html.AppendLine($"<div class='header'>");
        html.AppendLine($"<h1>🚀 ShopWeb Performance Trending Dashboard</h1>");
        html.AppendLine($"<p>Generated: {dashboard.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC</p>");
        html.AppendLine($"<p>Analysis Period: {dashboard.AnalysisWindow.StartDate:yyyy-MM-dd} to {dashboard.AnalysisWindow.EndDate:yyyy-MM-dd}</p>");
        html.AppendLine("</div>");

        // Overall Summary
        html.AppendLine("<div class='card'>");
        html.AppendLine("<h2>📊 Overall Performance Summary</h2>");
        html.AppendLine($"<p><strong>Tests Analyzed:</strong> {dashboard.OverallSummary.TotalTestsAnalyzed}</p>");
        html.AppendLine($"<p><strong>Average P95 Duration:</strong> {dashboard.OverallSummary.AverageP95DurationMs:F0}ms</p>");
        html.AppendLine($"<p><strong>Overall Trend:</strong> <span class='trend-{dashboard.OverallSummary.OverallTrendDirection.ToString().ToLower()}'>{dashboard.OverallSummary.OverallTrendDirection}</span></p>");
        html.AppendLine("</div>");

        // Alerts
        if (dashboard.Alerts.Any())
        {
            html.AppendLine("<div class='card'>");
            html.AppendLine("<h2>🚨 Performance Alerts</h2>");
            foreach (var alert in dashboard.Alerts.Take(10))
            {
                var cssClass = alert.SeverityLevel == AlertLevel.Critical ? "alert-critical" : "alert-warning";
                html.AppendLine($"<div class='{cssClass}' style='padding: 10px; margin: 5px 0;'>");
                html.AppendLine($"<strong>{alert.SeverityLevel}:</strong> {alert.Description}");
                html.AppendLine($"<br><small>Change: {alert.ChangePercentage:+0.0}% | Detected: {alert.DetectedAt:yyyy-MM-dd HH:mm}</small>");
                html.AppendLine("</div>");
            }
            html.AppendLine("</div>");
        }

        html.AppendLine("</body></html>");
        return html.ToString();
    }

    private PerformanceTrendingConfig LoadDefaultConfig()
    {
        try
        {
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "Config", "performanceTrending.json");
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                var rootConfig = JsonConvert.DeserializeObject<dynamic>(json);
                return JsonConvert.DeserializeObject<PerformanceTrendingConfig>(rootConfig?.performanceTrending?.ToString() ?? "{}") ?? new PerformanceTrendingConfig();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PerformanceTrending] Failed to load config: {ex.Message}");
        }
        
        return new PerformanceTrendingConfig();
    }

    #endregion
}