using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using ShopWeb.E2E.Tests.Utilities;

namespace ShopWeb.UnitTests.Utilities;

[TestFixture]
[Category("Unit")]
public class FlakyDetectionConfigTests
{
    [Test]
    public void FlakyDetectionConfig_DefaultValues_ShouldBeCorrectlySet()
    {
        // Act
        var config = new FlakyDetectionConfig();

        // Assert
        config.SlidingWindowSize.Should().Be(20);
        config.MinimumRunsRequired.Should().Be(5);
        config.QuarantineFailureThreshold.Should().Be(0.3);
        config.QuarantineFailureCount.Should().Be(3);
        config.RecoverySuccessCount.Should().Be(5);
        config.MaxHistoryAgeDays.Should().Be(30);
        config.AutoQuarantineEnabled.Should().BeTrue();
        config.AutoRecoveryEnabled.Should().BeTrue();
        config.TestNamePattern.Should().Be(".*");
        config.TargetBrowsers.Should().NotBeNull().And.BeEmpty();
        config.TargetSites.Should().NotBeNull().And.BeEmpty();
        config.SeverityThresholds.Should().NotBeNull();
    }

    [Test]
    public void FlakyDetectionConfig_CustomValues_ShouldBeSettable()
    {
        // Arrange
        var config = new FlakyDetectionConfig();

        // Act
        config.SlidingWindowSize = 50;
        config.MinimumRunsRequired = 10;
        config.QuarantineFailureThreshold = 0.5;
        config.QuarantineFailureCount = 5;
        config.RecoverySuccessCount = 10;
        config.MaxHistoryAgeDays = 60;
        config.AutoQuarantineEnabled = false;
        config.AutoRecoveryEnabled = false;
        config.TestNamePattern = "Test.*";
        config.TargetBrowsers = new List<string> { "chromium", "firefox" };
        config.TargetSites = new List<string> { "A", "B" };

        // Assert
        config.SlidingWindowSize.Should().Be(50);
        config.MinimumRunsRequired.Should().Be(10);
        config.QuarantineFailureThreshold.Should().Be(0.5);
        config.QuarantineFailureCount.Should().Be(5);
        config.RecoverySuccessCount.Should().Be(10);
        config.MaxHistoryAgeDays.Should().Be(60);
        config.AutoQuarantineEnabled.Should().BeFalse();
        config.AutoRecoveryEnabled.Should().BeFalse();
        config.TestNamePattern.Should().Be("Test.*");
        config.TargetBrowsers.Should().BeEquivalentTo(new[] { "chromium", "firefox" });
        config.TargetSites.Should().BeEquivalentTo(new[] { "A", "B" });
    }

    [Test]
    public void FlakyDetectionConfig_JsonSerialization_ShouldPreserveAllProperties()
    {
        // Arrange
        var originalConfig = new FlakyDetectionConfig
        {
            SlidingWindowSize = 15,
            MinimumRunsRequired = 3,
            QuarantineFailureThreshold = 0.25,
            QuarantineFailureCount = 2,
            RecoverySuccessCount = 7,
            MaxHistoryAgeDays = 45,
            AutoQuarantineEnabled = false,
            AutoRecoveryEnabled = true,
            TestNamePattern = "Integration.*",
            TargetBrowsers = new List<string> { "webkit" },
            TargetSites = new List<string> { "A" }
        };

        // Act
        var json = JsonConvert.SerializeObject(originalConfig, Formatting.Indented);
        var deserializedConfig = JsonConvert.DeserializeObject<FlakyDetectionConfig>(json);

        // Assert
        deserializedConfig.Should().NotBeNull();
        deserializedConfig!.SlidingWindowSize.Should().Be(originalConfig.SlidingWindowSize);
        deserializedConfig.MinimumRunsRequired.Should().Be(originalConfig.MinimumRunsRequired);
        deserializedConfig.QuarantineFailureThreshold.Should().Be(originalConfig.QuarantineFailureThreshold);
        deserializedConfig.QuarantineFailureCount.Should().Be(originalConfig.QuarantineFailureCount);
        deserializedConfig.RecoverySuccessCount.Should().Be(originalConfig.RecoverySuccessCount);
        deserializedConfig.MaxHistoryAgeDays.Should().Be(originalConfig.MaxHistoryAgeDays);
        deserializedConfig.AutoQuarantineEnabled.Should().Be(originalConfig.AutoQuarantineEnabled);
        deserializedConfig.AutoRecoveryEnabled.Should().Be(originalConfig.AutoRecoveryEnabled);
        deserializedConfig.TestNamePattern.Should().Be(originalConfig.TestNamePattern);
        deserializedConfig.TargetBrowsers.Should().BeEquivalentTo(originalConfig.TargetBrowsers);
        deserializedConfig.TargetSites.Should().BeEquivalentTo(originalConfig.TargetSites);
        deserializedConfig.SeverityThresholds.Should().NotBeNull();
    }
}

[TestFixture]
[Category("Unit")]
public class FlakyDetectionSeverityThresholdsTests
{
    [Test]
    public void FlakyDetectionSeverityThresholds_DefaultValues_ShouldBeCorrectlySet()
    {
        // Act
        var thresholds = new FlakyDetectionSeverityThresholds();

        // Assert
        thresholds.LowSeverity.Should().Be(0.1);
        thresholds.MediumSeverity.Should().Be(0.2);
        thresholds.HighSeverity.Should().Be(0.3);
        thresholds.CriticalSeverity.Should().Be(0.5);
    }

    [Test]
    public void FlakyDetectionSeverityThresholds_CustomValues_ShouldBeSettable()
    {
        // Arrange
        var thresholds = new FlakyDetectionSeverityThresholds();

        // Act
        thresholds.LowSeverity = 0.05;
        thresholds.MediumSeverity = 0.15;
        thresholds.HighSeverity = 0.25;
        thresholds.CriticalSeverity = 0.4;

        // Assert
        thresholds.LowSeverity.Should().Be(0.05);
        thresholds.MediumSeverity.Should().Be(0.15);
        thresholds.HighSeverity.Should().Be(0.25);
        thresholds.CriticalSeverity.Should().Be(0.4);
    }

    [Test]
    public void FlakyDetectionSeverityThresholds_ShouldBeProgressive()
    {
        // Arrange
        var thresholds = new FlakyDetectionSeverityThresholds();

        // Assert - Default values should be progressive
        thresholds.LowSeverity.Should().BeLessThan(thresholds.MediumSeverity);
        thresholds.MediumSeverity.Should().BeLessThan(thresholds.HighSeverity);
        thresholds.HighSeverity.Should().BeLessThan(thresholds.CriticalSeverity);
    }

    [Test]
    public void FlakyDetectionSeverityThresholds_JsonSerialization_ShouldPreserveAllProperties()
    {
        // Arrange
        var originalThresholds = new FlakyDetectionSeverityThresholds
        {
            LowSeverity = 0.08,
            MediumSeverity = 0.18,
            HighSeverity = 0.28,
            CriticalSeverity = 0.45
        };

        // Act
        var json = JsonConvert.SerializeObject(originalThresholds, Formatting.Indented);
        var deserializedThresholds = JsonConvert.DeserializeObject<FlakyDetectionSeverityThresholds>(json);

        // Assert
        deserializedThresholds.Should().NotBeNull();
        deserializedThresholds!.LowSeverity.Should().Be(originalThresholds.LowSeverity);
        deserializedThresholds.MediumSeverity.Should().Be(originalThresholds.MediumSeverity);
        deserializedThresholds.HighSeverity.Should().Be(originalThresholds.HighSeverity);
        deserializedThresholds.CriticalSeverity.Should().Be(originalThresholds.CriticalSeverity);
    }
}

[TestFixture]
[Category("Unit")]
public class FlakyAnalysisResultTests
{
    [Test]
    public void FlakyAnalysisResult_DefaultValues_ShouldBeCorrectlySet()
    {
        // Act
        var result = new FlakyAnalysisResult();

        // Assert
        result.TestName.Should().BeEmpty();
        result.Browser.Should().BeEmpty();
        result.SiteId.Should().BeEmpty();
        result.Status.Should().Be(FlakyTestStatus.Stable);
        result.Severity.Should().Be(FlakySeverity.None);
        result.FailureRate.Should().Be(0.0);
        result.TotalRuns.Should().Be(0);
        result.FailedRuns.Should().Be(0);
        result.ConsecutiveFailures.Should().Be(0);
        result.ConsecutiveSuccesses.Should().Be(0);
        result.LastFailure.Should().BeNull();
        result.LastSuccess.Should().BeNull();
        result.AnalysisTimestamp.Should().Be(default(DateTime));
        result.Recommendation.Should().BeEmpty();
        result.QuarantinedAt.Should().BeNull();
        result.RecoveredAt.Should().BeNull();
    }

    [Test]
    public void FlakyAnalysisResult_AllProperties_ShouldBeSettable()
    {
        // Arrange
        var result = new FlakyAnalysisResult();
        var lastFailure = DateTime.UtcNow.AddHours(-2);
        var lastSuccess = DateTime.UtcNow.AddHours(-1);
        var analysisTimestamp = DateTime.UtcNow;
        var quarantinedAt = DateTime.UtcNow.AddDays(-1);
        var recoveredAt = DateTime.UtcNow;

        // Act
        result.TestName = "SampleTest";
        result.Browser = "chromium";
        result.SiteId = "A";
        result.Status = FlakyTestStatus.Quarantined;
        result.Severity = FlakySeverity.High;
        result.FailureRate = 0.35;
        result.TotalRuns = 20;
        result.FailedRuns = 7;
        result.ConsecutiveFailures = 3;
        result.ConsecutiveSuccesses = 2;
        result.LastFailure = lastFailure;
        result.LastSuccess = lastSuccess;
        result.AnalysisTimestamp = analysisTimestamp;
        result.Recommendation = "Review test stability";
        result.QuarantinedAt = quarantinedAt;
        result.RecoveredAt = recoveredAt;

        // Assert
        result.TestName.Should().Be("SampleTest");
        result.Browser.Should().Be("chromium");
        result.SiteId.Should().Be("A");
        result.Status.Should().Be(FlakyTestStatus.Quarantined);
        result.Severity.Should().Be(FlakySeverity.High);
        result.FailureRate.Should().Be(0.35);
        result.TotalRuns.Should().Be(20);
        result.FailedRuns.Should().Be(7);
        result.ConsecutiveFailures.Should().Be(3);
        result.ConsecutiveSuccesses.Should().Be(2);
        result.LastFailure.Should().Be(lastFailure);
        result.LastSuccess.Should().Be(lastSuccess);
        result.AnalysisTimestamp.Should().Be(analysisTimestamp);
        result.Recommendation.Should().Be("Review test stability");
        result.QuarantinedAt.Should().Be(quarantinedAt);
        result.RecoveredAt.Should().Be(recoveredAt);
    }

    [Test]
    public void FlakyAnalysisResult_JsonSerialization_ShouldPreserveAllProperties()
    {
        // Arrange
        var originalResult = new FlakyAnalysisResult
        {
            TestName = "ComplexTest",
            Browser = "firefox",
            SiteId = "B",
            Status = FlakyTestStatus.RecoveryCandidate,
            Severity = FlakySeverity.Medium,
            FailureRate = 0.25,
            TotalRuns = 40,
            FailedRuns = 10,
            ConsecutiveFailures = 1,
            ConsecutiveSuccesses = 5,
            LastFailure = DateTime.UtcNow.AddDays(-3),
            LastSuccess = DateTime.UtcNow.AddHours(-6),
            AnalysisTimestamp = DateTime.UtcNow,
            Recommendation = "Monitor closely",
            QuarantinedAt = DateTime.UtcNow.AddDays(-7),
            RecoveredAt = null
        };

        // Act
        var json = JsonConvert.SerializeObject(originalResult, Formatting.Indented);
        var deserializedResult = JsonConvert.DeserializeObject<FlakyAnalysisResult>(json);

        // Assert
        deserializedResult.Should().NotBeNull();
        deserializedResult!.TestName.Should().Be(originalResult.TestName);
        deserializedResult.Browser.Should().Be(originalResult.Browser);
        deserializedResult.SiteId.Should().Be(originalResult.SiteId);
        deserializedResult.Status.Should().Be(originalResult.Status);
        deserializedResult.Severity.Should().Be(originalResult.Severity);
        deserializedResult.FailureRate.Should().Be(originalResult.FailureRate);
        deserializedResult.TotalRuns.Should().Be(originalResult.TotalRuns);
        deserializedResult.FailedRuns.Should().Be(originalResult.FailedRuns);
        deserializedResult.ConsecutiveFailures.Should().Be(originalResult.ConsecutiveFailures);
        deserializedResult.ConsecutiveSuccesses.Should().Be(originalResult.ConsecutiveSuccesses);
        deserializedResult.LastFailure.Should().BeCloseTo(originalResult.LastFailure!.Value, precision: TimeSpan.FromSeconds(1));
        deserializedResult.LastSuccess.Should().BeCloseTo(originalResult.LastSuccess!.Value, precision: TimeSpan.FromSeconds(1));
        deserializedResult.AnalysisTimestamp.Should().BeCloseTo(originalResult.AnalysisTimestamp, precision: TimeSpan.FromSeconds(1));
        deserializedResult.Recommendation.Should().Be(originalResult.Recommendation);
        deserializedResult.QuarantinedAt.Should().BeCloseTo(originalResult.QuarantinedAt!.Value, precision: TimeSpan.FromSeconds(1));
        deserializedResult.RecoveredAt.Should().Be(originalResult.RecoveredAt);
    }

    [Test]
    public void FlakyTestStatus_EnumValues_ShouldHaveExpectedNames()
    {
        // Assert
        Enum.GetName(typeof(FlakyTestStatus), FlakyTestStatus.Stable).Should().Be("Stable");
        Enum.GetName(typeof(FlakyTestStatus), FlakyTestStatus.UnderObservation).Should().Be("UnderObservation");
        Enum.GetName(typeof(FlakyTestStatus), FlakyTestStatus.Flaky).Should().Be("Flaky");
        Enum.GetName(typeof(FlakyTestStatus), FlakyTestStatus.Quarantined).Should().Be("Quarantined");
        Enum.GetName(typeof(FlakyTestStatus), FlakyTestStatus.RecoveryCandidate).Should().Be("RecoveryCandidate");
        Enum.GetName(typeof(FlakyTestStatus), FlakyTestStatus.Recovered).Should().Be("Recovered");
    }

    [Test]
    public void FlakySeverity_EnumValues_ShouldHaveExpectedNames()
    {
        // Assert
        Enum.GetName(typeof(FlakySeverity), FlakySeverity.None).Should().Be("None");
        Enum.GetName(typeof(FlakySeverity), FlakySeverity.Low).Should().Be("Low");
        Enum.GetName(typeof(FlakySeverity), FlakySeverity.Medium).Should().Be("Medium");
        Enum.GetName(typeof(FlakySeverity), FlakySeverity.High).Should().Be("High");
        Enum.GetName(typeof(FlakySeverity), FlakySeverity.Critical).Should().Be("Critical");
    }
}