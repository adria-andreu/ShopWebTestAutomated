using Allure.Net.Commons;
using NUnit.Framework;
using System.Diagnostics;

namespace ShopWeb.E2E.Tests.Utilities;

/// <summary>
/// Manages Allure context lifecycle to prevent "No test context is active" errors
/// Implements minimal, safe initialization and cleanup for CI/CD compatibility
/// </summary>
public static class AllureContextManager
{
    private static readonly object _lock = new();
    private static bool _isInitialized = false;
    private static AllureLifecycle? _lifecycle;

    /// <summary>
    /// Initialize Allure context safely for the test run
    /// </summary>
    public static void Initialize()
    {
        lock (_lock)
        {
            if (_isInitialized) return;

            try
            {
                // Create lifecycle instance with safe defaults
                _lifecycle = AllureLifecycle.Instance;
                
                // Ensure results directory exists
                var resultsDir = Path.Combine(Directory.GetCurrentDirectory(), "allure-results");
                Directory.CreateDirectory(resultsDir);
                
                _isInitialized = true;
                TestContext.WriteLine("[Allure] Context initialized successfully");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"[Allure] Initialization failed: {ex.Message}");
                _isInitialized = false;
            }
        }
    }

    /// <summary>
    /// Check if Allure context is properly initialized
    /// </summary>
    public static bool IsContextActive()
    {
        lock (_lock)
        {
            return _isInitialized && _lifecycle != null;
        }
    }

    /// <summary>
    /// Safely add test metadata if context is active
    /// </summary>
    public static void SafeAddLabel(string name, string value)
    {
        if (!IsContextActive()) return;

        try
        {
            AllureApi.AddLabel(name, value);
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"[Allure] Failed to add label {name}={value}: {ex.Message}");
        }
    }

    /// <summary>
    /// Safely add attachment if context is active
    /// </summary>
    public static void SafeAddAttachment(string name, string path, string? type = null)
    {
        if (!IsContextActive()) return;

        try
        {
            if (File.Exists(path))
            {
                AllureApi.AddAttachment(name, type ?? "text/plain", path);
            }
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"[Allure] Failed to add attachment {name}: {ex.Message}");
        }
    }

    /// <summary>
    /// Safely start test step if context is active
    /// </summary>
    public static void SafeStep(string stepName, Action action)
    {
        if (!IsContextActive())
        {
            // Execute action without Allure context
            action.Invoke();
            return;
        }

        try
        {
            AllureApi.Step(stepName, action);
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"[Allure] Step '{stepName}' failed: {ex.Message}");
            // Still execute the action
            action.Invoke();
        }
    }

    /// <summary>
    /// Clean shutdown for test run completion
    /// </summary>
    public static void Cleanup()
    {
        lock (_lock)
        {
            if (_isInitialized)
            {
                try
                {
                    _lifecycle = null;
                    _isInitialized = false;
                    TestContext.WriteLine("[Allure] Context cleaned up successfully");
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"[Allure] Cleanup warning: {ex.Message}");
                }
            }
        }
    }
}