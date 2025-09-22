using Microsoft.Playwright;
using NUnit.Framework;
// Allure integration temporarily disabled to unblock IT03 completion - complex context management issues in CI/CD
// using Allure.NUnit;
// using Allure.NUnit.Attributes;
// using Allure.Net.Commons;
using ShopWeb.E2E.Tests.Browsers;
using ShopWeb.E2E.Tests.Config;
using ShopWeb.E2E.Tests.Utilities;
using ShopWeb.E2E.Tests.Exceptions;
using System.Diagnostics;

namespace ShopWeb.E2E.Tests.Tests;

[Parallelizable(ParallelScope.All)]
// [AllureNUnit] // Temporarily disabled to unblock IT03 - complex context management issues in CI/CD
public abstract class BaseTest
{
    private IBrowserFactory? _browserFactory;
    private IBrowserContext? _context;
    private IPage? _page;
    private string? _testName;
    private string? _artifactsDir;
    private Stopwatch? _testStopwatch;
    private bool _testFailed = false;
    private readonly List<string> _consoleLogs = new();

    protected IPage Page => _page ?? throw new InvalidOperationException("Page not initialized. Ensure SetUp has run.");
    protected IBrowserContext Context => _context ?? throw new InvalidOperationException("Context not initialized. Ensure SetUp has run.");
    protected TestSettings Settings => ConfigurationManager.TestSettings;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Allure context initialization disabled to unblock IT03
        // AllureContextManager.Initialize();
        
        _browserFactory = new BrowserFactory();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        // Dispose instance-level browser factory to satisfy NUnit analyzer
        _browserFactory?.Dispose();
        // Dispose static browser factory resources only at the very end
        BrowserFactory.DisposeStatic();
        MetricsCollector.GenerateRunMetrics();
        
        // Allure context cleanup disabled to unblock IT03
        // AllureContextManager.Cleanup();
    }

    [SetUp]
    public async Task SetUpAsync()
    {
        _testStopwatch = Stopwatch.StartNew();
        _testName = TestContext.CurrentContext.Test.Name.Replace(" ", "_");
        _artifactsDir = CreateArtifactsDirectory();
        _testFailed = false;

        await InitializeBrowserAsync();
        await StartTracingAsync();
        ConfigureTimeouts();
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        _testStopwatch?.Stop();
        var durationMs = _testStopwatch?.ElapsedMilliseconds ?? 0;

        _testFailed = TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed;

        await StopTracingAsync();
        await CaptureArtifactsAsync();
        await CleanupBrowserAsync();

        RecordTestMetrics(durationMs);
    }

    private async Task InitializeBrowserAsync()
    {
        var browser = GetBrowserFromTestParameters() ?? Settings.Browser;
        var contextOptions = CreateContextOptions();
        
        _context = await _browserFactory!.CreateContextAsync(browser, contextOptions);
        _page = await _context.NewPageAsync();

        // Configure console logging
        _page.Console += OnConsoleMessage;

        // Configure page-level settings
        _page.SetDefaultNavigationTimeout(Settings.Timeouts.Navigation);
        _page.SetDefaultTimeout(Settings.Timeouts.Default);
    }

    private Dictionary<string, object> CreateContextOptions()
    {
        return new Dictionary<string, object>
        {
            ["viewport"] = new Dictionary<string, object>
            {
                ["width"] = 1280,
                ["height"] = 720
            },
            ["userAgent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
            ["locale"] = "en-US"
        };
    }

    private async Task StartTracingAsync()
    {
        var traceMode = Environment.GetEnvironmentVariable("TRACE_MODE") ?? Settings.Artifacts.TraceMode;
        
        if (traceMode.Equals("On", StringComparison.OrdinalIgnoreCase) ||
            traceMode.Equals("OnFailure", StringComparison.OrdinalIgnoreCase))
        {
            await _context!.Tracing.StartAsync(new TracingStartOptions
            {
                Screenshots = true,
                Snapshots = true,
                Sources = true
            });
        }
    }

    private async Task StopTracingAsync()
    {
        var traceMode = Environment.GetEnvironmentVariable("TRACE_MODE") ?? Settings.Artifacts.TraceMode;
        
        if (traceMode.Equals("On", StringComparison.OrdinalIgnoreCase) ||
            (traceMode.Equals("OnFailure", StringComparison.OrdinalIgnoreCase) && _testFailed))
        {
            var tracePath = Path.Combine(_artifactsDir!, "trace.zip");
            await _context!.Tracing.StopAsync(new TracingStopOptions
            {
                Path = tracePath
            });

            // Attach to NUnit only for now
            TestContext.AddTestAttachment(tracePath, "Playwright Trace");
        }
    }

    private async Task CaptureArtifactsAsync()
    {
        // Screenshot on failure
        if (_testFailed)
        {
            var screenshotPath = Path.Combine(_artifactsDir!, "screenshot.png");
            await _page!.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath });
            
            TestContext.AddTestAttachment(screenshotPath, "Failure Screenshot");
        }

        // HTML dump on failure (if configured)
        if (_testFailed && Settings.Artifacts.DumpHtmlOnFail)
        {
            var htmlPath = Path.Combine(_artifactsDir!, "page.html");
            var content = await _page!.ContentAsync();
            await File.WriteAllTextAsync(htmlPath, content);
            
            TestContext.AddTestAttachment(htmlPath, "Page HTML");
        }

        // Console logs on failure (if configured)
        if (_testFailed && Settings.Artifacts.LogConsoleOnFail && _consoleLogs.Any())
        {
            var logPath = Path.Combine(_artifactsDir!, "console.log.txt");
            var logContent = string.Join(Environment.NewLine, _consoleLogs);
            await File.WriteAllTextAsync(logPath, logContent);
            
            TestContext.AddTestAttachment(logPath, "Console Logs");
        }
    }

    private void ConfigureTimeouts()
    {
        // Set global timeouts
        _page?.SetDefaultTimeout(Settings.Timeouts.Default);
        _page?.SetDefaultNavigationTimeout(Settings.Timeouts.Navigation);
    }

    private string CreateArtifactsDirectory()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmssfff");
        var dirName = $"{_testName}_{timestamp}";
        var artifactsPath = Path.Combine(Directory.GetCurrentDirectory(), "artifacts", dirName);
        
        Directory.CreateDirectory(artifactsPath);
        return artifactsPath;
    }

    private string? GetBrowserFromTestParameters()
    {
        return TestContext.Parameters.Get("Browser") ?? Environment.GetEnvironmentVariable("BROWSER");
    }

    private string GetSiteIdFromTestParameters()
    {
        return TestContext.Parameters.Get("SiteId") ?? Environment.GetEnvironmentVariable("SITE_ID") ?? Settings.SiteId;
    }

    private void OnConsoleMessage(object? sender, IConsoleMessage e)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var logEntry = $"[{timestamp}] {e.Type.ToUpper()}: {e.Text}";
        _consoleLogs.Add(logEntry);

        // Also log to test output for immediate visibility
        if (e.Type == "error" || e.Type == "warning")
        {
            TestContext.WriteLine($"CONSOLE {e.Type.ToUpper()}: {e.Text}");
        }
    }

    private async Task CleanupBrowserAsync()
    {
        if (_page != null)
        {
            _page.Console -= OnConsoleMessage;
            await _page.CloseAsync();
            _page = null;
        }

        if (_context != null)
        {
            await _context.CloseAsync();
            _context = null;
        }
    }

    private void RecordTestMetrics(long durationMs)
    {
        var testResult = TestContext.CurrentContext.Result;
        var status = testResult.Outcome.Status switch
        {
            NUnit.Framework.Interfaces.TestStatus.Passed => "Passed",
            NUnit.Framework.Interfaces.TestStatus.Failed => "Failed",
            NUnit.Framework.Interfaces.TestStatus.Skipped => "Skipped",
            _ => "Unknown"
        };

        var metric = new TestMetric
        {
            Name = _testName ?? "Unknown",
            Status = status,
            DurationMs = durationMs,
            ArtifactsPath = _artifactsDir ?? string.Empty,
            TimestampUtc = DateTime.UtcNow,
            Browser = GetBrowserFromTestParameters() ?? Settings.Browser,
            SiteId = GetSiteIdFromTestParameters(),
            CommitSha = Environment.GetEnvironmentVariable("GIT_SHA") ?? Environment.GetEnvironmentVariable("GITHUB_SHA") ?? "unknown",
            Retries = 0, // RetryCount not available in this NUnit version
            ErrorMessage = status == "Failed" ? testResult.Message : null
        };

        MetricsCollector.RecordTestMetric(metric);

        // Allure labels disabled to unblock IT03
        // AllureContextManager.SafeAddLabel("browser", metric.Browser);
        // AllureContextManager.SafeAddLabel("siteId", metric.SiteId);
        // AllureContextManager.SafeAddLabel("duration", $"{metric.DurationMs}ms");
        // AllureContextManager.SafeAddLabel("status", metric.Status);
        
        // Add artifacts as Allure attachments if test failed
        if (metric.Status == "Failed" && !string.IsNullOrEmpty(metric.ArtifactsPath))
        {
            var screenshotPath = Path.Combine(metric.ArtifactsPath, "screenshot.png");
            if (File.Exists(screenshotPath))
            {
                // AllureContextManager.SafeAddAttachment("Failure Screenshot", screenshotPath, "image/png");
            }
        }
    }

    // Helper method for navigation with retry logic
    protected async Task NavigateAsync(string url, int retries = 3)
    {
        await RetryPolicy.ExecuteAsync(
            async () =>
            {
                await Page.GotoAsync(url, new PageGotoOptions 
                { 
                    WaitUntil = WaitUntilState.NetworkIdle 
                });
            },
            operationName: $"Navigate to {url}",
            maxRetries: retries,
            baseDelay: TimeSpan.FromSeconds(1),
            useExponentialBackoff: true,
            retryableExceptions: new[] { typeof(Microsoft.Playwright.PlaywrightException) }
        );
    }
}