// Ejemplo mínimo. Adapta namespaces a tu proyecto.
// Requiere Microsoft.Playwright y NUnit.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;

namespace DocsExamples
{
    [Parallelizable(ParallelScope.Self)]
    public abstract class BaseTestExample
    {
        protected IBrowser Browser => _browser!;
        protected IBrowserContext Context => _context!;
        protected IPage Page => _page!;

        private IBrowser? _browser;
        private IBrowserContext? _context;
        private IPage? _page;

        private readonly StringBuilder _consoleLog = new();
        private Stopwatch _sw = new();
        private string _artifactsPath = "";

        private sealed class Settings
        {
            public TestSettings TestSettings { get; set; } = new();
        }
        private sealed class TestSettings
        {
            public string BaseUrl { get; set; } = "";
            public string Browser { get; set; } = "chromium";
            public bool Headed { get; set; } = false;
            public Timeouts Timeouts { get; set; } = new();
            public Artifacts Artifacts { get; set; } = new();
            public Ctx Context { get; set; } = new();
        }
        private sealed class Timeouts { public int Default { get; set; } = 8000; public int Navigation { get; set; } = 15000; }
        private sealed class Artifacts { public bool DumpHtmlOnFail { get; set; } = true; public bool LogConsoleOnFail { get; set; } = true; public string TraceMode { get; set; } = "OnFailure"; }
        private sealed class Ctx { public int ViewportWidth { get; set; } = 1366; public int ViewportHeight { get; set; } = 768; public string Locale { get; set; } = "en-US"; public string UserAgent { get; set; } = "ShopWebTestAutomated/1.0"; }

        private Settings _cfg = new();

        [SetUp]
        public async Task SetUpAsync()
        {
            _sw.Restart();

            // Carga de config (simplificada para ejemplo)
            var cfgPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Config", "appsettings.tests.json");
            if (File.Exists(cfgPath))
                _cfg = JsonSerializer.Deserialize<Settings>(File.ReadAllText(cfgPath)) ?? new Settings();

            var playwright = await Playwright.CreateAsync();
            var browserType = _cfg.TestSettings.Browser.ToLowerInvariant() switch
            {
                "firefox" => playwright.Firefox,
                "webkit"  => playwright.Webkit,
                _         => playwright.Chromium
            };

            _browser = await browserType.LaunchAsync(new()
            {
                Headless = !_cfg.TestSettings.Headed
            });

            _context = await _browser.NewContextAsync(new()
            {
                ViewportSize = new() { Width = _cfg.TestSettings.Context.ViewportWidth, Height = _cfg.TestSettings.Context.ViewportHeight },
                Locale = _cfg.TestSettings.Context.Locale,
                UserAgent = _cfg.TestSettings.Context.UserAgent
            });

            _context.SetDefaultTimeout(_cfg.TestSettings.Timeouts.Default);
            _context.SetDefaultNavigationTimeout(_cfg.TestSettings.Timeouts.Navigation);

            // Tracing según TraceMode
            var traceMode = (_cfg.TestSettings.Artifacts.TraceMode ?? "OnFailure").ToLowerInvariant();
            var startTrace = traceMode == "on" || traceMode == "onfailure";
            if (startTrace)
            {
                await _context.Tracing.StartAsync(new()
                {
                    Screenshots = true,
                    Snapshots = true,
                    Sources = true
                });
            }

            _page = await _context.NewPageAsync();

            _page.Console += (_, msg) => _consoleLog.AppendLine($"[{msg.Type}] {msg.Text}");

            // carpeta de artefactos
            var testNameSafe = MakeSafe(TestContext.CurrentContext.Test.Name);
            _artifactsPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "artifacts", $"{testNameSafe}_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}");
            Directory.CreateDirectory(_artifactsPath);
        }

        [TearDown]
        public async Task TearDownAsync()
        {
            try
            {
                var status = TestContext.CurrentContext.Result.Outcome.Status.ToString();
                var browserName = _cfg.TestSettings.Browser;
                var siteId = Environment.GetEnvironmentVariable("SITE_ID") ?? "A";
                var commitSha = Environment.GetEnvironmentVariable("GIT_SHA") ?? "";
                var pipelineId = Environment.GetEnvironmentVariable("PIPELINE_ID") ?? "";

                // Always stop tracing if it was started
                try
                {
                    var tracePath = Path.Combine(_artifactsPath, "trace.zip");
                    await _context.Tracing.StopAsync(new() { Path = tracePath });
                    TestContext.AddTestAttachment(tracePath, "Playwright trace");
                }
                catch { /* ignore if not started */ }

                if (status == "Failed" && _page is not null)
                {
                    var screenshotPath = Path.Combine(_artifactsPath, "screenshot.png");
                    await _page.ScreenshotAsync(new() { Path = screenshotPath, FullPage = true });
                    TestContext.AddTestAttachment(screenshotPath, "Screenshot on failure");

                    if (_cfg.TestSettings.Artifacts.DumpHtmlOnFail)
                    {
                        var htmlPath = Path.Combine(_artifactsPath, "page.html");
                        var html = await _page.ContentAsync();
                        await File.WriteAllTextAsync(htmlPath, html);
                        TestContext.AddTestAttachment(htmlPath, "Page HTML");
                    }

                    if (_cfg.TestSettings.Artifacts.LogConsoleOnFail && _consoleLog.Length > 0)
                    {
                        var consolePath = Path.Combine(_artifactsPath, "console.log.txt");
                        await File.WriteAllTextAsync(consolePath, _consoleLog.ToString());
                        TestContext.AddTestAttachment(consolePath, "Console log");
                    }
                }

                // Métricas por test
                DocsExamples.RunMetricsCollectorExample.RecordTest(new DocsExamples.RunMetricsCollectorExample.TestLine
                {
                    name = TestContext.CurrentContext.Test.Name,
                    status = status,
                    durationMs = (long)_sw.Elapsed.TotalMilliseconds,
                    artifactsPath = RelPath(_artifactsPath),
                    timestampUtc = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                    browser = browserName,
                    siteId = siteId,
                    commitSha = commitSha,
                    retries = TestContext.CurrentContext.CurrentRepeatCount // si usas Retry
                });

                // Allure (opcional)
                DocsExamples.AllureIntegrationExample.AttachArtifactsAndLabels(_artifactsPath, browserName, siteId, commitSha, pipelineId);
            }
            finally
            {
                try { if (_page != null) await _page.CloseAsync(); } catch { }
                try { if (_context != null) await _context.CloseAsync(); } catch { }
                try { if (_browser != null) await _browser.CloseAsync(); } catch { }
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            var startedAt = Environment.GetEnvironmentVariable("RUN_STARTED_AT_UTC");
            DocsExamples.RunMetricsCollectorExample.RecordRunSummary(
                commitSha: Environment.GetEnvironmentVariable("GIT_SHA"),
                startedAtUtc: startedAt,
                finishedAtUtc: DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)
            );
        }

        private static string MakeSafe(string s)
        {
            foreach (var c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
            return s;
        }
        private static string RelPath(string abs) => abs.Replace(Path.Combine(AppContext.BaseDirectory, "..", "..", "..") + Path.DirectorySeparatorChar, "").Replace('\\','/');
    }
}
