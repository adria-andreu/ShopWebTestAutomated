using NUnit.Framework;

namespace ShopWeb.E2E.Tests.Utilities;

/// <summary>
/// Step logging infrastructure for structured test reporting
/// Implements Setup → Step → SubStep → Confirm pattern from E2E_Policy.md
/// Provides formatted output for test execution traceability
/// </summary>
public static class StepLogger
{
    private static int _setupCounter = 0;
    private static int _stepCounter = 0;
    private static int _subStepCounter = 0;
    private static int _confirmCounter = 0;

    /// <summary>
    /// Reset counters for new test
    /// </summary>
    public static void ResetCounters()
    {
        _setupCounter = 0;
        _stepCounter = 0;
        _subStepCounter = 0;
        _confirmCounter = 0;
    }

    /// <summary>
    /// Log setup precondition step
    /// </summary>
    /// <param name="description">Description of setup action</param>
    public static void Setup(string description)
    {
        _setupCounter++;
        var message = $"🔧 SETUP {_setupCounter:D2}: {description}";
        Console.WriteLine(message);
        TestContext.WriteLine(message);
    }

    /// <summary>
    /// Log main test step
    /// </summary>
    /// <param name="description">Description of test step</param>
    public static void Step(string description)
    {
        _stepCounter++;
        _subStepCounter = 0; // Reset substep counter for new step
        var message = $"🚀 STEP {_stepCounter:D2}: {description}";
        Console.WriteLine(message);
        TestContext.WriteLine(message);
    }

    /// <summary>
    /// Log substep within a main step
    /// </summary>
    /// <param name="description">Description of substep</param>
    public static void SubStep(string description)
    {
        _subStepCounter++;
        var message = $"   └─ SubStep {_stepCounter:D2}.{_subStepCounter}: {description}";
        Console.WriteLine(message);
        TestContext.WriteLine(message);
    }

    /// <summary>
    /// Log confirmation/verification step
    /// </summary>
    /// <param name="description">Description of confirmation</param>
    public static void Confirm(string description)
    {
        _confirmCounter++;
        var message = $"✅ CONFIRM {_confirmCounter:D2}: {description}";
        Console.WriteLine(message);
        TestContext.WriteLine(message);
    }

    /// <summary>
    /// Log generic information step
    /// </summary>
    /// <param name="description">Information to log</param>
    public static void Info(string description)
    {
        var message = $"ℹ️  INFO: {description}";
        Console.WriteLine(message);
        TestContext.WriteLine(message);
    }

    /// <summary>
    /// Log warning step
    /// </summary>
    /// <param name="description">Warning to log</param>
    public static void Warning(string description)
    {
        var message = $"⚠️  WARNING: {description}";
        Console.WriteLine(message);
        TestContext.WriteLine(message);
    }

    /// <summary>
    /// Log error step
    /// </summary>
    /// <param name="description">Error to log</param>
    public static void Error(string description)
    {
        var message = $"❌ ERROR: {description}";
        Console.WriteLine(message);
        TestContext.WriteLine(message);
    }

    /// <summary>
    /// Log test completion summary
    /// </summary>
    public static void TestSummary()
    {
        var message = $"📊 TEST SUMMARY: {_setupCounter} setups, {_stepCounter} steps, {_confirmCounter} confirms completed";
        Console.WriteLine(message);
        TestContext.WriteLine(message);
    }
}