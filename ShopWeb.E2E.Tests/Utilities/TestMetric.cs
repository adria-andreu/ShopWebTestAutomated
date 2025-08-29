using Newtonsoft.Json;

namespace ShopWeb.E2E.Tests.Utilities;

public class TestMetric
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;

    [JsonProperty("durationMs")]
    public long DurationMs { get; set; }

    [JsonProperty("artifactsPath")]
    public string ArtifactsPath { get; set; } = string.Empty;

    [JsonProperty("timestampUtc")]
    public DateTime TimestampUtc { get; set; }

    [JsonProperty("browser")]
    public string Browser { get; set; } = string.Empty;

    [JsonProperty("siteId")]
    public string SiteId { get; set; } = string.Empty;

    [JsonProperty("commitSha")]
    public string CommitSha { get; set; } = string.Empty;

    [JsonProperty("retries")]
    public int Retries { get; set; }

    [JsonProperty("errorMessage")]
    public string? ErrorMessage { get; set; }
}

public class RunMetric
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
    public double PassRate => Total > 0 ? (double)Passed / Total : 0.0;

    [JsonProperty("passRateEffective")]
    public double PassRateEffective => Total > 0 ? (double)Passed / (Passed + Failed) : 0.0;

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