using Newtonsoft.Json;

namespace ShopWeb.E2E.Tests.Utilities;

/// <summary>
/// Comprehensive quarantine workflow system for intelligent test lifecycle management
/// Orchestrates quarantine, monitoring, recovery, and notification workflows
/// </summary>
public class QuarantineWorkflowConfig
{
    [JsonProperty("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonProperty("quarantineRules")]
    public QuarantineRules QuarantineRules { get; set; } = new();

    [JsonProperty("recoveryRules")]
    public RecoveryRules RecoveryRules { get; set; } = new();

    [JsonProperty("notifications")]
    public NotificationConfig Notifications { get; set; } = new();

    [JsonProperty("workflow")]
    public WorkflowConfig Workflow { get; set; } = new();

    [JsonProperty("retentionPolicy")]
    public RetentionPolicy RetentionPolicy { get; set; } = new();
}

public class QuarantineRules
{
    [JsonProperty("autoQuarantineEnabled")]
    public bool AutoQuarantineEnabled { get; set; } = true;

    [JsonProperty("failureRateThreshold")]
    public double FailureRateThreshold { get; set; } = 0.3; // 30%

    [JsonProperty("consecutiveFailuresThreshold")]
    public int ConsecutiveFailuresThreshold { get; set; } = 3;

    [JsonProperty("minimumRunsRequired")]
    public int MinimumRunsRequired { get; set; } = 5;

    [JsonProperty("performanceRegressionThreshold")]
    public double PerformanceRegressionThreshold { get; set; } = 25.0; // 25%

    [JsonProperty("immediateQuarantineTriggers")]
    public List<ImmediateQuarantineTrigger> ImmediateQuarantineTriggers { get; set; } = new();
}

public class RecoveryRules
{
    [JsonProperty("autoRecoveryEnabled")]
    public bool AutoRecoveryEnabled { get; set; } = true;

    [JsonProperty("consecutiveSuccessesRequired")]
    public int ConsecutiveSuccessesRequired { get; set; } = 5;

    [JsonProperty("stabilityPeriodDays")]
    public int StabilityPeriodDays { get; set; } = 3;

    [JsonProperty("performanceStabilityThreshold")]
    public double PerformanceStabilityThreshold { get; set; } = 0.15; // 15% variation

    [JsonProperty("manualOverrideEnabled")]
    public bool ManualOverrideEnabled { get; set; } = true;
}

public class NotificationConfig
{
    [JsonProperty("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonProperty("channels")]
    public List<NotificationChannel> Channels { get; set; } = new();

    [JsonProperty("severityLevels")]
    public List<NotificationSeverity> SeverityLevels { get; set; } = new()
    {
        NotificationSeverity.Critical,
        NotificationSeverity.Warning
    };

    [JsonProperty("throttling")]
    public NotificationThrottling Throttling { get; set; } = new();
}

public class WorkflowConfig
{
    [JsonProperty("stateTransitionTimeoutMinutes")]
    public int StateTransitionTimeoutMinutes { get; set; } = 30;

    [JsonProperty("maxRetryAttempts")]
    public int MaxRetryAttempts { get; set; } = 3;

    [JsonProperty("enableWorkflowLogging")]
    public bool EnableWorkflowLogging { get; set; } = true;

    [JsonProperty("enableStateHistory")]
    public bool EnableStateHistory { get; set; } = true;
}

public class RetentionPolicy
{
    [JsonProperty("quarantineRecordsRetentionDays")]
    public int QuarantineRecordsRetentionDays { get; set; } = 90;

    [JsonProperty("workflowLogsRetentionDays")]
    public int WorkflowLogsRetentionDays { get; set; } = 30;

    [JsonProperty("enableAutomaticCleanup")]
    public bool EnableAutomaticCleanup { get; set; } = true;
}

/// <summary>
/// Quarantine record representing a test in quarantine state
/// </summary>
public class QuarantineRecord
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("testIdentifier")]
    public TestIdentifier TestIdentifier { get; set; } = new();

    [JsonProperty("status")]
    public QuarantineStatus Status { get; set; }

    [JsonProperty("reason")]
    public QuarantineReason Reason { get; set; } = new();

    [JsonProperty("quarantinedAt")]
    public DateTime QuarantinedAt { get; set; }

    [JsonProperty("lastStatusUpdate")]
    public DateTime LastStatusUpdate { get; set; }

    [JsonProperty("recoveryAttempts")]
    public int RecoveryAttempts { get; set; }

    [JsonProperty("evidence")]
    public QuarantineEvidence Evidence { get; set; } = new();

    [JsonProperty("workflowHistory")]
    public List<WorkflowStateChange> WorkflowHistory { get; set; } = new();

    [JsonProperty("notifications")]
    public List<NotificationRecord> Notifications { get; set; } = new();

    [JsonProperty("recoveryConditions")]
    public RecoveryConditions RecoveryConditions { get; set; } = new();

    [JsonProperty("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class TestIdentifier
{
    [JsonProperty("testName")]
    public string TestName { get; set; } = string.Empty;

    [JsonProperty("browser")]
    public string Browser { get; set; } = string.Empty;

    [JsonProperty("siteId")]
    public string SiteId { get; set; } = string.Empty;

    [JsonProperty("fullIdentifier")]
    public string FullIdentifier => $"{TestName}_{Browser}_{SiteId}";
}

public class QuarantineReason
{
    [JsonProperty("primaryReason")]
    public QuarantineTrigger PrimaryReason { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("triggerValues")]
    public Dictionary<string, object> TriggerValues { get; set; } = new();

    [JsonProperty("relatedAnalysis")]
    public List<string> RelatedAnalysis { get; set; } = new();
}

public class QuarantineEvidence
{
    [JsonProperty("flakyAnalysisResult")]
    public FlakyAnalysisResult? FlakyAnalysisResult { get; set; }

    [JsonProperty("performanceTrendingResult")]
    public PerformanceTrendingResult? PerformanceTrendingResult { get; set; }

    [JsonProperty("recentFailures")]
    public List<TestExecutionHistory> RecentFailures { get; set; } = new();

    [JsonProperty("historicalMetrics")]
    public Dictionary<string, object> HistoricalMetrics { get; set; } = new();
}

public class RecoveryConditions
{
    [JsonProperty("consecutiveSuccessesAchieved")]
    public int ConsecutiveSuccessesAchieved { get; set; }

    [JsonProperty("stabilityPeriodStart")]
    public DateTime? StabilityPeriodStart { get; set; }

    [JsonProperty("lastFailureDate")]
    public DateTime? LastFailureDate { get; set; }

    [JsonProperty("performanceStabilized")]
    public bool PerformanceStabilized { get; set; }

    [JsonProperty("manualOverrideApproved")]
    public bool ManualOverrideApproved { get; set; }
}

public class WorkflowStateChange
{
    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonProperty("fromStatus")]
    public QuarantineStatus FromStatus { get; set; }

    [JsonProperty("toStatus")]
    public QuarantineStatus ToStatus { get; set; }

    [JsonProperty("trigger")]
    public string Trigger { get; set; } = string.Empty;

    [JsonProperty("reason")]
    public string Reason { get; set; } = string.Empty;

    [JsonProperty("automaticTransition")]
    public bool AutomaticTransition { get; set; }

    [JsonProperty("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class NotificationRecord
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonProperty("channel")]
    public NotificationChannel Channel { get; set; }

    [JsonProperty("severity")]
    public NotificationSeverity Severity { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; } = string.Empty;

    [JsonProperty("delivered")]
    public bool Delivered { get; set; }

    [JsonProperty("deliveryAttempts")]
    public int DeliveryAttempts { get; set; }
}

public class ImmediateQuarantineTrigger
{
    [JsonProperty("trigger")]
    public QuarantineTrigger Trigger { get; set; }

    [JsonProperty("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonProperty("parameters")]
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class NotificationThrottling
{
    [JsonProperty("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonProperty("maxNotificationsPerHour")]
    public int MaxNotificationsPerHour { get; set; } = 10;

    [JsonProperty("cooldownPeriodMinutes")]
    public int CooldownPeriodMinutes { get; set; } = 30;
}

/// <summary>
/// Workflow orchestration result with detailed execution information
/// </summary>
public class WorkflowExecutionResult
{
    [JsonProperty("executionId")]
    public string ExecutionId { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonProperty("workflowType")]
    public WorkflowType WorkflowType { get; set; }

    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("actionsExecuted")]
    public List<WorkflowAction> ActionsExecuted { get; set; } = new();

    [JsonProperty("quarantineChanges")]
    public List<QuarantineStatusChange> QuarantineChanges { get; set; } = new();

    [JsonProperty("notificationsSent")]
    public List<NotificationRecord> NotificationsSent { get; set; } = new();

    [JsonProperty("errors")]
    public List<string> Errors { get; set; } = new();

    [JsonProperty("summary")]
    public string Summary { get; set; } = string.Empty;
}

public class WorkflowAction
{
    [JsonProperty("actionType")]
    public WorkflowActionType ActionType { get; set; }

    [JsonProperty("testIdentifier")]
    public TestIdentifier TestIdentifier { get; set; } = new();

    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("details")]
    public string Details { get; set; } = string.Empty;

    [JsonProperty("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class QuarantineStatusChange
{
    [JsonProperty("quarantineId")]
    public string QuarantineId { get; set; } = string.Empty;

    [JsonProperty("testIdentifier")]
    public TestIdentifier TestIdentifier { get; set; } = new();

    [JsonProperty("previousStatus")]
    public QuarantineStatus PreviousStatus { get; set; }

    [JsonProperty("newStatus")]
    public QuarantineStatus NewStatus { get; set; }

    [JsonProperty("reason")]
    public string Reason { get; set; } = string.Empty;

    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Enumerations for workflow system
/// </summary>
public enum QuarantineStatus
{
    None,
    UnderObservation,
    Quarantined,
    RecoveryCandidate,
    RecoveryInProgress,
    Recovered,
    PermanentlyDisabled
}

public enum QuarantineTrigger
{
    HighFailureRate,
    ConsecutiveFailures,
    PerformanceRegression,
    ManualQuarantine,
    CriticalError,
    InfrastructureIssue
}

public enum NotificationChannel
{
    Console,
    Email,
    Slack,
    Teams,
    Webhook,
    LogFile
}

public enum NotificationSeverity
{
    Info,
    Warning,
    Critical
}

public enum WorkflowType
{
    QuarantineEvaluation,
    RecoveryEvaluation,
    PeriodicMaintenance,
    ManualIntervention
}

public enum WorkflowActionType
{
    QuarantineTest,
    RecoverTest,
    UpdateStatus,
    SendNotification,
    CreateAlert,
    UpdateMetadata,
    CleanupRecord
}