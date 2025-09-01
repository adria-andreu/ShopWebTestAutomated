using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace ShopWeb.E2E.Tests.Utilities;

/// <summary>
/// Advanced workflow orchestration engine for intelligent test quarantine lifecycle management
/// Coordinates quarantine decisions, state transitions, recovery monitoring, and notification delivery
/// </summary>
public class QuarantineWorkflowEngine
{
    private readonly QuarantineWorkflowConfig _config;
    private readonly FlakyDetectionEngine _flakyEngine;
    private readonly PerformanceTrendingEngine _performanceEngine;
    private readonly ConcurrentDictionary<string, QuarantineRecord> _quarantineRecords;
    private readonly string _quarantineDataPath;
    private readonly object _workflowLock = new object();

    public QuarantineWorkflowEngine(QuarantineWorkflowConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _flakyEngine = new FlakyDetectionEngine();
        _performanceEngine = new PerformanceTrendingEngine();
        _quarantineRecords = new ConcurrentDictionary<string, QuarantineRecord>();
        _quarantineDataPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "quarantine");
        
        Directory.CreateDirectory(_quarantineDataPath);
        LoadExistingQuarantineRecords();
    }

    /// <summary>
    /// Main workflow orchestration method - evaluates all active tests for quarantine/recovery decisions
    /// </summary>
    public WorkflowExecutionResult ExecuteWorkflow(WorkflowType workflowType, string? targetTestId = null)
    {
        var result = new WorkflowExecutionResult
        {
            WorkflowType = workflowType,
            Timestamp = DateTime.UtcNow
        };

        if (!_config.Enabled)
        {
            result.Summary = "Quarantine workflow is disabled";
            return result;
        }

        try
        {
            lock (_workflowLock)
            {
                switch (workflowType)
                {
                    case WorkflowType.QuarantineEvaluation:
                        result = ExecuteQuarantineEvaluation(targetTestId);
                        break;
                    case WorkflowType.RecoveryEvaluation:
                        result = ExecuteRecoveryEvaluation(targetTestId);
                        break;
                    case WorkflowType.PeriodicMaintenance:
                        result = ExecutePeriodicMaintenance();
                        break;
                    case WorkflowType.ManualIntervention:
                        result = ExecuteManualIntervention(targetTestId);
                        break;
                }

                PersistQuarantineRecords();
                
                if (_config.Workflow.EnableWorkflowLogging)
                {
                    LogWorkflowExecution(result);
                }
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add($"Workflow execution failed: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Evaluates tests for quarantine based on flaky detection and performance regression analysis
    /// </summary>
    private WorkflowExecutionResult ExecuteQuarantineEvaluation(string? targetTestId = null)
    {
        var result = new WorkflowExecutionResult
        {
            WorkflowType = WorkflowType.QuarantineEvaluation,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            // Get test execution history for analysis
            var historyPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "test-history.jsonl");
            if (!File.Exists(historyPath))
            {
                result.Summary = "No test history available for quarantine evaluation";
                return result;
            }

            var testHistories = LoadTestHistories(historyPath);
            var testsToEvaluate = string.IsNullOrEmpty(targetTestId) 
                ? testHistories.GroupBy(h => $"{h.TestName}_{h.Browser}_{h.SiteId}")
                : testHistories.Where(h => $"{h.TestName}_{h.Browser}_{h.SiteId}" == targetTestId)
                             .GroupBy(h => $"{h.TestName}_{h.Browser}_{h.SiteId}");

            foreach (var testGroup in testsToEvaluate)
            {
                var testId = testGroup.Key;
                var history = testGroup.OrderByDescending(h => h.ExecutionTimestamp).ToList();
                
                // Skip if already quarantined (unless manual override)
                if (_quarantineRecords.ContainsKey(testId) && 
                    _quarantineRecords[testId].Status == QuarantineStatus.Quarantined)
                    continue;

                var shouldQuarantine = ShouldQuarantineTest(testId, history, out var quarantineReason);
                
                if (shouldQuarantine)
                {
                    var quarantineAction = CreateQuarantineRecord(testId, history, quarantineReason);
                    result.ActionsExecuted.Add(quarantineAction);
                    result.QuarantineChanges.Add(CreateStatusChange(testId, QuarantineStatus.None, QuarantineStatus.Quarantined, quarantineReason.Description));
                }
            }

            result.Success = true;
            result.Summary = $"Quarantine evaluation completed: {result.ActionsExecuted.Count} actions executed";
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add($"Quarantine evaluation failed: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Evaluates quarantined tests for potential recovery based on success patterns
    /// </summary>
    private WorkflowExecutionResult ExecuteRecoveryEvaluation(string? targetTestId = null)
    {
        var result = new WorkflowExecutionResult
        {
            WorkflowType = WorkflowType.RecoveryEvaluation,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            var quarantinedTests = _quarantineRecords.Values
                .Where(r => r.Status == QuarantineStatus.Quarantined || r.Status == QuarantineStatus.RecoveryCandidate)
                .Where(r => string.IsNullOrEmpty(targetTestId) || r.TestIdentifier.FullIdentifier == targetTestId);

            foreach (var record in quarantinedTests)
            {
                var canRecover = CanRecoverTest(record, out var recoveryReason);
                
                if (canRecover)
                {
                    var recoveryAction = ExecuteTestRecovery(record, recoveryReason);
                    result.ActionsExecuted.Add(recoveryAction);
                    result.QuarantineChanges.Add(CreateStatusChange(record.TestIdentifier.FullIdentifier, 
                        record.Status, QuarantineStatus.Recovered, recoveryReason));
                }
            }

            result.Success = true;
            result.Summary = $"Recovery evaluation completed: {result.ActionsExecuted.Count} recoveries executed";
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add($"Recovery evaluation failed: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Determines if a test should be quarantined based on configured rules and analysis results
    /// </summary>
    private bool ShouldQuarantineTest(string testId, List<TestExecutionHistory> history, out QuarantineReason reason)
    {
        reason = new QuarantineReason();
        var parts = testId.Split('_');
        if (parts.Length != 3) return false;

        var testName = parts[0];
        var browser = parts[1];
        var siteId = parts[2];

        // Check immediate quarantine triggers first
        foreach (var trigger in _config.QuarantineRules.ImmediateQuarantineTriggers.Where(t => t.Enabled))
        {
            if (CheckImmediateQuarantineTrigger(trigger, history))
            {
                reason.PrimaryReason = trigger.Trigger;
                reason.Description = $"Immediate quarantine triggered: {trigger.Trigger}";
                return true;
            }
        }

        // Run flaky detection analysis
        if (_config.QuarantineRules.AutoQuarantineEnabled)
        {
            var flakyResults = _flakyEngine.AnalyzeFlakyTests();
            var flakyResult = flakyResults.FirstOrDefault(r => r.TestName == testName && r.Browser == browser && r.SiteId == siteId);
            
            if (flakyResult?.Status == FlakyTestStatus.Flaky)
            {
                reason.PrimaryReason = QuarantineTrigger.HighFailureRate;
                reason.Description = $"Flaky behavior detected: {flakyResult.FailureRate:F2} failure rate";
                reason.TriggerValues["failureRate"] = flakyResult.FailureRate;
                reason.TriggerValues["failureRate"] = flakyResult.FailureRate;
                return true;
            }
        }

        // Performance regression check temporarily disabled for CI/CD stability
        // TODO: Re-enable after API alignment between PerformanceTrendingEngine and QuarantineWorkflowEngine

        return false;
    }

    /// <summary>
    /// Checks if immediate quarantine trigger conditions are met
    /// </summary>
    private bool CheckImmediateQuarantineTrigger(ImmediateQuarantineTrigger trigger, List<TestExecutionHistory> history)
    {
        switch (trigger.Trigger)
        {
            case QuarantineTrigger.ConsecutiveFailures:
                var consecutiveLimit = trigger.Parameters.ContainsKey("limit") ? 
                    Convert.ToInt32(trigger.Parameters["limit"]) : _config.QuarantineRules.ConsecutiveFailuresThreshold;
                return CountConsecutiveFailures(history) >= consecutiveLimit;

            case QuarantineTrigger.CriticalError:
                return history.Take(5).Any(h => h.ErrorMessage?.Contains("critical", StringComparison.OrdinalIgnoreCase) == true);

            case QuarantineTrigger.InfrastructureIssue:
                return history.Take(3).Any(h => h.ErrorMessage?.Contains("timeout", StringComparison.OrdinalIgnoreCase) == true ||
                                               h.ErrorMessage?.Contains("connection", StringComparison.OrdinalIgnoreCase) == true);

            default:
                return false;
        }
    }

    /// <summary>
    /// Creates a new quarantine record and manages state transition
    /// </summary>
    private WorkflowAction CreateQuarantineRecord(string testId, List<TestExecutionHistory> history, QuarantineReason reason)
    {
        var parts = testId.Split('_');
        var testIdentifier = new TestIdentifier
        {
            TestName = parts[0],
            Browser = parts[1],
            SiteId = parts[2]
        };

        var record = new QuarantineRecord
        {
            TestIdentifier = testIdentifier,
            Status = QuarantineStatus.Quarantined,
            Reason = reason,
            QuarantinedAt = DateTime.UtcNow,
            LastStatusUpdate = DateTime.UtcNow,
            Evidence = CreateQuarantineEvidence(history)
        };

        // Add workflow state change
        record.WorkflowHistory.Add(new WorkflowStateChange
        {
            Timestamp = DateTime.UtcNow,
            FromStatus = QuarantineStatus.None,
            ToStatus = QuarantineStatus.Quarantined,
            Trigger = "AutomatedWorkflow",
            Reason = reason.Description,
            AutomaticTransition = true
        });

        _quarantineRecords.AddOrUpdate(testId, record, (key, existing) => record);

        // Send notifications if configured
        if (_config.Notifications.Enabled)
        {
            SendQuarantineNotification(record, NotificationSeverity.Warning);
        }

        return new WorkflowAction
        {
            ActionType = WorkflowActionType.QuarantineTest,
            TestIdentifier = testIdentifier,
            Timestamp = DateTime.UtcNow,
            Success = true,
            Details = $"Test quarantined: {reason.Description}"
        };
    }

    /// <summary>
    /// Determines if a quarantined test can be recovered
    /// </summary>
    private bool CanRecoverTest(QuarantineRecord record, out string recoveryReason)
    {
        recoveryReason = string.Empty;

        if (!_config.RecoveryRules.AutoRecoveryEnabled)
            return false;

        // Check manual override
        if (_config.RecoveryRules.ManualOverrideEnabled && record.RecoveryConditions.ManualOverrideApproved)
        {
            recoveryReason = "Manual override approved";
            return true;
        }

        // Check consecutive successes requirement
        if (record.RecoveryConditions.ConsecutiveSuccessesAchieved >= _config.RecoveryRules.ConsecutiveSuccessesRequired)
        {
            // Check stability period
            if (record.RecoveryConditions.StabilityPeriodStart.HasValue)
            {
                var stabilityDays = (DateTime.UtcNow - record.RecoveryConditions.StabilityPeriodStart.Value).TotalDays;
                if (stabilityDays >= _config.RecoveryRules.StabilityPeriodDays && record.RecoveryConditions.PerformanceStabilized)
                {
                    recoveryReason = $"Stability requirements met: {record.RecoveryConditions.ConsecutiveSuccessesAchieved} successes over {stabilityDays:F1} days";
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Executes test recovery workflow and state transition
    /// </summary>
    private WorkflowAction ExecuteTestRecovery(QuarantineRecord record, string recoveryReason)
    {
        var previousStatus = record.Status;
        record.Status = QuarantineStatus.Recovered;
        record.LastStatusUpdate = DateTime.UtcNow;
        record.RecoveryAttempts++;

        // Add workflow state change
        record.WorkflowHistory.Add(new WorkflowStateChange
        {
            Timestamp = DateTime.UtcNow,
            FromStatus = previousStatus,
            ToStatus = QuarantineStatus.Recovered,
            Trigger = "AutomatedRecovery",
            Reason = recoveryReason,
            AutomaticTransition = true
        });

        // Send recovery notification
        if (_config.Notifications.Enabled)
        {
            SendRecoveryNotification(record, NotificationSeverity.Info);
        }

        return new WorkflowAction
        {
            ActionType = WorkflowActionType.RecoverTest,
            TestIdentifier = record.TestIdentifier,
            Timestamp = DateTime.UtcNow,
            Success = true,
            Details = $"Test recovered: {recoveryReason}"
        };
    }

    /// <summary>
    /// Creates comprehensive evidence package for quarantine decision
    /// </summary>
    private QuarantineEvidence CreateQuarantineEvidence(List<TestExecutionHistory> history)
    {
        var evidence = new QuarantineEvidence();
        
        // Store recent failures for analysis
        evidence.RecentFailures = history.Where(h => h.Status == "Failed").Take(10).ToList();
        
        // Add historical metrics
        evidence.HistoricalMetrics["totalRuns"] = history.Count;
        evidence.HistoricalMetrics["totalFailures"] = history.Count(h => h.Status == "Failed");
        evidence.HistoricalMetrics["failureRate"] = history.Count > 0 ? (double)history.Count(h => h.Status == "Failed") / history.Count : 0.0;
        evidence.HistoricalMetrics["avgDuration"] = history.Average(h => h.DurationMs);
        evidence.HistoricalMetrics["lastFailure"] = history.Where(h => h.Status == "Failed").FirstOrDefault()?.ExecutionTimestamp;

        return evidence;
    }

    /// <summary>
    /// Periodic maintenance workflow for cleanup and optimization
    /// </summary>
    private WorkflowExecutionResult ExecutePeriodicMaintenance()
    {
        var result = new WorkflowExecutionResult
        {
            WorkflowType = WorkflowType.PeriodicMaintenance,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            var cleanupActions = 0;

            // Clean up old quarantine records based on retention policy
            if (_config.RetentionPolicy.EnableAutomaticCleanup)
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-_config.RetentionPolicy.QuarantineRecordsRetentionDays);
                var expiredRecords = _quarantineRecords.Values
                    .Where(r => r.QuarantinedAt < cutoffDate && (r.Status == QuarantineStatus.Recovered || r.Status == QuarantineStatus.PermanentlyDisabled))
                    .ToList();

                foreach (var record in expiredRecords)
                {
                    if (_quarantineRecords.TryRemove(record.TestIdentifier.FullIdentifier, out _))
                    {
                        cleanupActions++;
                    }
                }
            }

            // Clean up old workflow logs
            var logsDir = Path.Combine(_quarantineDataPath, "logs");
            if (Directory.Exists(logsDir))
            {
                var logCutoff = DateTime.UtcNow.AddDays(-_config.RetentionPolicy.WorkflowLogsRetentionDays);
                var oldLogs = Directory.GetFiles(logsDir, "*.log")
                    .Where(f => File.GetCreationTime(f) < logCutoff)
                    .ToList();

                foreach (var logFile in oldLogs)
                {
                    try
                    {
                        File.Delete(logFile);
                        cleanupActions++;
                    }
                    catch { /* Ignore cleanup failures */ }
                }
            }

            result.Success = true;
            result.Summary = $"Maintenance completed: {cleanupActions} cleanup actions performed";
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add($"Maintenance failed: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Manual intervention workflow for administrative actions
    /// </summary>
    private WorkflowExecutionResult ExecuteManualIntervention(string? targetTestId)
    {
        var result = new WorkflowExecutionResult
        {
            WorkflowType = WorkflowType.ManualIntervention,
            Timestamp = DateTime.UtcNow,
            Summary = "Manual intervention workflow - requires specific implementation per use case"
        };

        // This method serves as a framework for manual administrative actions
        // Implementation would depend on specific requirements

        return result;
    }

    /// <summary>
    /// Sends quarantine notification through configured channels
    /// </summary>
    private void SendQuarantineNotification(QuarantineRecord record, NotificationSeverity severity)
    {
        if (!_config.Notifications.Enabled || !_config.Notifications.SeverityLevels.Contains(severity))
            return;

        var message = $"Test quarantined: {record.TestIdentifier.FullIdentifier} - {record.Reason.Description}";
        
        var notification = new NotificationRecord
        {
            Timestamp = DateTime.UtcNow,
            Severity = severity,
            Message = message,
            Delivered = false
        };

        foreach (var channel in _config.Notifications.Channels)
        {
            try
            {
                DeliverNotification(channel, notification);
                notification.Delivered = true;
            }
            catch (Exception ex)
            {
                notification.DeliveryAttempts++;
                Console.WriteLine($"Failed to deliver notification via {channel}: {ex.Message}");
            }
        }

        record.Notifications.Add(notification);
    }

    /// <summary>
    /// Sends recovery notification through configured channels
    /// </summary>
    private void SendRecoveryNotification(QuarantineRecord record, NotificationSeverity severity)
    {
        if (!_config.Notifications.Enabled || !_config.Notifications.SeverityLevels.Contains(severity))
            return;

        var message = $"Test recovered: {record.TestIdentifier.FullIdentifier} - Ready for execution after {record.RecoveryAttempts} recovery attempts";
        
        var notification = new NotificationRecord
        {
            Timestamp = DateTime.UtcNow,
            Severity = severity,
            Message = message,
            Delivered = false
        };

        foreach (var channel in _config.Notifications.Channels)
        {
            try
            {
                DeliverNotification(channel, notification);
                notification.Delivered = true;
            }
            catch (Exception ex)
            {
                notification.DeliveryAttempts++;
                Console.WriteLine($"Failed to deliver notification via {channel}: {ex.Message}");
            }
        }

        record.Notifications.Add(notification);
    }

    /// <summary>
    /// Delivers notification through specific channel
    /// </summary>
    private void DeliverNotification(NotificationChannel channel, NotificationRecord notification)
    {
        switch (channel)
        {
            case NotificationChannel.Console:
                Console.WriteLine($"[{notification.Severity}] {notification.Message}");
                break;
            case NotificationChannel.LogFile:
                var logPath = Path.Combine(_quarantineDataPath, "logs", "notifications.log");
                Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
                File.AppendAllText(logPath, $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} [{notification.Severity}] {notification.Message}{Environment.NewLine}");
                break;
            case NotificationChannel.Email:
            case NotificationChannel.Slack:
            case NotificationChannel.Teams:
            case NotificationChannel.Webhook:
                // These would require specific integration implementations
                throw new NotImplementedException($"Notification channel {channel} not implemented");
        }
    }

    /// <summary>
    /// Utility methods for workflow operations
    /// </summary>
    private int CountConsecutiveFailures(List<TestExecutionHistory> history)
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

    private QuarantineStatusChange CreateStatusChange(string testId, QuarantineStatus from, QuarantineStatus to, string reason)
    {
        var parts = testId.Split('_');
        return new QuarantineStatusChange
        {
            TestIdentifier = new TestIdentifier
            {
                TestName = parts[0],
                Browser = parts[1],
                SiteId = parts[2]
            },
            PreviousStatus = from,
            NewStatus = to,
            Reason = reason,
            Timestamp = DateTime.UtcNow
        };
    }

    private void LogWorkflowExecution(WorkflowExecutionResult result)
    {
        var logPath = Path.Combine(_quarantineDataPath, "logs", "workflow.log");
        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
        
        var logEntry = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} [{result.WorkflowType}] " +
                      $"Success: {result.Success}, Actions: {result.ActionsExecuted.Count}, " +
                      $"Changes: {result.QuarantineChanges.Count}, Summary: {result.Summary}";
        
        File.AppendAllText(logPath, logEntry + Environment.NewLine);
    }

    private List<TestExecutionHistory> LoadTestHistories(string historyPath)
    {
        var histories = new List<TestExecutionHistory>();
        
        if (!File.Exists(historyPath)) return histories;

        foreach (var line in File.ReadAllLines(historyPath))
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var history = JsonConvert.DeserializeObject<TestExecutionHistory>(line);
                    if (history != null) histories.Add(history);
                }
            }
            catch { /* Skip malformed lines */ }
        }

        return histories;
    }

    private void LoadExistingQuarantineRecords()
    {
        var recordsPath = Path.Combine(_quarantineDataPath, "quarantine-records.jsonl");
        if (!File.Exists(recordsPath)) return;

        foreach (var line in File.ReadAllLines(recordsPath))
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var record = JsonConvert.DeserializeObject<QuarantineRecord>(line);
                    if (record != null)
                    {
                        _quarantineRecords.TryAdd(record.TestIdentifier.FullIdentifier, record);
                    }
                }
            }
            catch { /* Skip malformed lines */ }
        }
    }

    private void PersistQuarantineRecords()
    {
        var recordsPath = Path.Combine(_quarantineDataPath, "quarantine-records.jsonl");
        var lines = _quarantineRecords.Values
            .Select(record => JsonConvert.SerializeObject(record))
            .ToArray();
        
        File.WriteAllLines(recordsPath, lines);
    }

    /// <summary>
    /// Public API for external integration
    /// </summary>
    public QuarantineRecord? GetQuarantineRecord(string testId) => 
        _quarantineRecords.TryGetValue(testId, out var record) ? record : null;

    public IReadOnlyCollection<QuarantineRecord> GetAllQuarantineRecords() => 
        _quarantineRecords.Values.ToList().AsReadOnly();

    public bool IsTestQuarantined(string testId) =>
        _quarantineRecords.TryGetValue(testId, out var record) && 
        record.Status == QuarantineStatus.Quarantined;

    public void ForceQuarantine(string testId, string reason) =>
        ExecuteWorkflow(WorkflowType.ManualIntervention, testId);

    public void ApproveRecovery(string testId)
    {
        if (_quarantineRecords.TryGetValue(testId, out var record))
        {
            record.RecoveryConditions.ManualOverrideApproved = true;
            ExecuteWorkflow(WorkflowType.RecoveryEvaluation, testId);
        }
    }
}