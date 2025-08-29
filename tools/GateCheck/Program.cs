using Newtonsoft.Json;
using System.CommandLine;

namespace GateCheck;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Quality Gates Checker for ShopWeb E2E Tests");

        var metricsFileOption = new Option<string>(
            "--metrics-file",
            getDefaultValue: () => Path.Combine(Directory.GetCurrentDirectory(), "artifacts", "run-metrics.json"),
            "Path to the run metrics JSON file");

        var environmentOption = new Option<string>(
            "--environment",
            getDefaultValue: () => "PR",
            "Environment type (PR, Main, or Nightly)");

        var minPassRateOption = new Option<double?>(
            "--min-pass-rate",
            "Minimum pass rate threshold (overrides config)");

        var maxP95Option = new Option<long?>(
            "--max-p95-ms",
            "Maximum P95 duration in milliseconds (overrides config)");

        var maxFlakyRatioOption = new Option<double?>(
            "--max-flaky-ratio",
            "Maximum flaky ratio threshold (overrides config)");

        var verboseOption = new Option<bool>(
            "--verbose",
            "Enable verbose output");

        rootCommand.AddOption(metricsFileOption);
        rootCommand.AddOption(environmentOption);
        rootCommand.AddOption(minPassRateOption);
        rootCommand.AddOption(maxP95Option);
        rootCommand.AddOption(maxFlakyRatioOption);
        rootCommand.AddOption(verboseOption);

        rootCommand.SetHandler(async (metricsFile, environment, minPassRate, maxP95, maxFlakyRatio, verbose) =>
        {
            var exitCode = await CheckQualityGatesAsync(metricsFile, environment, minPassRate, maxP95, maxFlakyRatio, verbose);
            Environment.Exit(exitCode);
        }, metricsFileOption, environmentOption, minPassRateOption, maxP95Option, maxFlakyRatioOption, verboseOption);

        return await rootCommand.InvokeAsync(args);
    }

    private static async Task<int> CheckQualityGatesAsync(
        string metricsFile, 
        string environment, 
        double? minPassRate, 
        long? maxP95, 
        double? maxFlakyRatio, 
        bool verbose)
    {
        try
        {
            if (verbose)
                Console.WriteLine($"GateCheck starting - Environment: {environment}");

            if (!File.Exists(metricsFile))
            {
                Console.WriteLine($"❌ ERROR: Metrics file not found: {metricsFile}");
                return 1;
            }

            if (verbose)
                Console.WriteLine($"Reading metrics from: {metricsFile}");

            var json = await File.ReadAllTextAsync(metricsFile);
            var runMetrics = JsonConvert.DeserializeObject<RunMetrics>(json);

            if (runMetrics == null)
            {
                Console.WriteLine("❌ ERROR: Failed to parse run metrics JSON");
                return 1;
            }

            if (verbose)
            {
                Console.WriteLine($"Parsed metrics: {runMetrics.Total} tests, {runMetrics.Passed} passed, {runMetrics.Failed} failed");
            }

            var thresholds = GetThresholds(environment, minPassRate, maxP95, maxFlakyRatio);
            
            if (verbose)
            {
                Console.WriteLine($"Thresholds - MinPassRate: {thresholds.MinPassRate:P2}, MaxP95: {thresholds.MaxP95Ms}ms, MaxFlakyRatio: {thresholds.MaxFlakyRatio:P2}");
            }

            var violations = new List<string>();

            // Check pass rate
            if (runMetrics.PassRate < thresholds.MinPassRate)
            {
                violations.Add($"Pass rate {runMetrics.PassRate:P2} is below threshold {thresholds.MinPassRate:P2}");
            }

            // Check P95 duration
            if (runMetrics.P95DurationMs > thresholds.MaxP95Ms)
            {
                violations.Add($"P95 duration {runMetrics.P95DurationMs}ms exceeds threshold {thresholds.MaxP95Ms}ms");
            }

            // Check flaky ratio
            if (runMetrics.FlakyRatio > thresholds.MaxFlakyRatio)
            {
                violations.Add($"Flaky ratio {runMetrics.FlakyRatio:P2} exceeds threshold {thresholds.MaxFlakyRatio:P2}");
            }

            // Report results
            if (violations.Any())
            {
                Console.WriteLine($"❌ QUALITY GATES FAILED ({violations.Count} violations):");
                foreach (var violation in violations)
                {
                    Console.WriteLine($"   • {violation}");
                }
                
                Console.WriteLine();
                PrintMetricsSummary(runMetrics);
                return 1;
            }
            else
            {
                Console.WriteLine("✅ All quality gates passed!");
                if (verbose)
                {
                    PrintMetricsSummary(runMetrics);
                }
                return 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR: {ex.Message}");
            if (verbose)
            {
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            return 1;
        }
    }

    private static QualityThresholds GetThresholds(string environment, double? minPassRate, long? maxP95, double? maxFlakyRatio)
    {
        // Default thresholds based on environment
        var thresholds = environment.ToLowerInvariant() switch
        {
            "main" or "nightly" => new QualityThresholds
            {
                MinPassRate = 0.95,
                MaxP95Ms = 600000, // 10 minutes
                MaxFlakyRatio = 0.05
            },
            _ => new QualityThresholds  // Default to PR thresholds
            {
                MinPassRate = 0.90,
                MaxP95Ms = 720000, // 12 minutes
                MaxFlakyRatio = 0.05
            }
        };

        // Override with command line parameters
        if (minPassRate.HasValue)
            thresholds.MinPassRate = minPassRate.Value;
        
        if (maxP95.HasValue)
            thresholds.MaxP95Ms = maxP95.Value;
        
        if (maxFlakyRatio.HasValue)
            thresholds.MaxFlakyRatio = maxFlakyRatio.Value;

        // Override with environment variables
        var envMinPassRate = Environment.GetEnvironmentVariable("MIN_PASS_RATE");
        if (!string.IsNullOrEmpty(envMinPassRate) && double.TryParse(envMinPassRate, out var envMinPassRateValue))
            thresholds.MinPassRate = envMinPassRateValue;

        var envMaxP95 = Environment.GetEnvironmentVariable("MAX_P95_MS");
        if (!string.IsNullOrEmpty(envMaxP95) && long.TryParse(envMaxP95, out var envMaxP95Value))
            thresholds.MaxP95Ms = envMaxP95Value;

        var envMaxFlakyRatio = Environment.GetEnvironmentVariable("MAX_FLAKY_RATIO");
        if (!string.IsNullOrEmpty(envMaxFlakyRatio) && double.TryParse(envMaxFlakyRatio, out var envMaxFlakyRatioValue))
            thresholds.MaxFlakyRatio = envMaxFlakyRatioValue;

        return thresholds;
    }

    private static void PrintMetricsSummary(RunMetrics metrics)
    {
        Console.WriteLine("📊 Test Run Summary:");
        Console.WriteLine($"   Tests: {metrics.Total} total, {metrics.Passed} passed, {metrics.Failed} failed, {metrics.Skipped} skipped");
        Console.WriteLine($"   Pass Rate: {metrics.PassRate:P2} (effective: {metrics.PassRateEffective:P2})");
        Console.WriteLine($"   P95 Duration: {metrics.P95DurationMs}ms ({metrics.P95DurationMs / 1000.0:F1}s)");
        Console.WriteLine($"   Flaky Ratio: {metrics.FlakyRatio:P2}");
        Console.WriteLine($"   Runtime: {metrics.StartedAtUtc:yyyy-MM-dd HH:mm:ss} - {metrics.FinishedAtUtc:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"   Browser: {metrics.Browser}, Site: {metrics.SiteId}");
        Console.WriteLine($"   Commit: {metrics.CommitSha}, Pipeline: {metrics.PipelineId}");
    }
}

public class RunMetrics
{
    [JsonProperty("total")]
    public int Total { get; set; }

    [JsonProperty("passed")]
    public int Passed { get; set; }

    [JsonProperty("failed")]
    public int Failed { get; set; }

    [JsonProperty("skipped")]
    public int Skipped { get; set; }

    [JsonProperty("passRate")]
    public double PassRate { get; set; }

    [JsonProperty("passRateEffective")]
    public double PassRateEffective { get; set; }

    [JsonProperty("flakyRatio")]
    public double FlakyRatio { get; set; }

    [JsonProperty("p95DurationMs")]
    public long P95DurationMs { get; set; }

    [JsonProperty("startedAtUtc")]
    public DateTime StartedAtUtc { get; set; }

    [JsonProperty("finishedAtUtc")]
    public DateTime FinishedAtUtc { get; set; }

    [JsonProperty("browser")]
    public string Browser { get; set; } = string.Empty;

    [JsonProperty("siteId")]
    public string SiteId { get; set; } = string.Empty;

    [JsonProperty("commitSha")]
    public string CommitSha { get; set; } = string.Empty;

    [JsonProperty("pipelineId")]
    public string PipelineId { get; set; } = string.Empty;
}

public class QualityThresholds
{
    public double MinPassRate { get; set; }
    public long MaxP95Ms { get; set; }
    public double MaxFlakyRatio { get; set; }
}