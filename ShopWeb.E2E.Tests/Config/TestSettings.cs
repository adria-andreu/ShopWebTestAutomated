namespace ShopWeb.E2E.Tests.Config;

public class TestSettings
{
    public string BaseUrl { get; set; } = "https://www.demoblaze.com/";
    public string Browser { get; set; } = "chromium";
    public bool Headed { get; set; } = false;
    public string SiteId { get; set; } = "A";
    public TimeoutSettings Timeouts { get; set; } = new();
    public ArtifactsSettings Artifacts { get; set; } = new();
    public ParallelizationSettings Parallelization { get; set; } = new();
    public QualityGatesSettings QualityGates { get; set; } = new();
}

public class TimeoutSettings
{
    public int Default { get; set; } = 8000;
    public int Navigation { get; set; } = 15000;
    public int Action { get; set; } = 5000;
}

public class ArtifactsSettings
{
    public bool DumpHtmlOnFail { get; set; } = true;
    public bool LogConsoleOnFail { get; set; } = true;
    public string TraceMode { get; set; } = "OnFailure";
    public int RetentionDays { get; set; } = 30;
}

public class ParallelizationSettings
{
    public int MaxWorkers { get; set; } = 4;
    public bool EnableParallelExecution { get; set; } = true;
}

public class QualityGatesSettings
{
    public GateThresholds PR { get; set; } = new();
    public GateThresholds Main { get; set; } = new();
}

public class GateThresholds
{
    public double MinPassRate { get; set; } = 0.90;
    public int MaxP95DurationMs { get; set; } = 720000;
    public double MaxFlakyRatio { get; set; } = 0.05;
}