using Allure.NUnit.Attributes;
using NUnit.Framework;
using Microsoft.Playwright;
using System.IO;
using System.Threading.Tasks
using Allure.Commons;

public sealed class TestPageContext : IAsyncDisposable
{
    public IPlaywright PW { get; }
    public IBrowser Browser { get; }
    public IBrowserContext Ctx { get; }
    public IPage Page { get; }

    private TestPageContext(IPlaywright pw, IBrowser browser, IBrowserContext ctx, IPage page)
        => (PW, Browser, Ctx, Page) = (pw, browser, ctx, page);

    public static async Task<TestPageContext> CreateAsync(string browserName, BrowserNewContextOptions? opts = null)
    {
        var pw = await Playwright.CreateAsync();
        var launch = new BrowserTypeLaunchOptions { Headless = true };
        IBrowser browser = browserName switch
        {
            "firefox"  => await pw.Firefox.LaunchAsync(launch),
            "webkit"   => await pw.Webkit.LaunchAsync(launch),
            _          => await pw.Chromium.LaunchAsync(launch),
        };
        opts ??= new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1366, Height = 900 },
            IgnoreHTTPSErrors = true,
        };
        var ctx = await browser.NewContextAsync(opts);
        var page = await ctx.NewPageAsync();
        return new TestPageContext(pw, browser, ctx, page);
    }

    public async ValueTask DisposeAsync()
    {
        await Ctx.CloseAsync();
        await Browser.CloseAsync();
        PW.Dispose();
    }
}

[AllureNUnit]
[Parallelizable(ParallelScope.All)]
public abstract class BaseUiTest
{
    protected TestPageContext Ctx = null!;
    protected IPage Page => Ctx.Page;

    [SetUp]
    public async Task SetUp()
    {
        var browser = TestContext.Parameters.Get("Browser", "chromium");
        Ctx = await TestPageContext.CreateAsync(browser);
        await Ctx.Ctx.Tracing.StartAsync(new() { Screenshots = true, Snapshots = true, Sources = true });

        RunMetricsCollector.StartTest(TestContext.CurrentContext.Test.FullName, browser);
    }

    [TearDown]
    public async Task TearDown()
    {
        var testName = Sanitize(TestContext.CurrentContext.Test.FullName);
        Directory.CreateDirectory("artifacts");
        var screenshot = Path.Combine("artifacts", $"{testName}.png");
        var traceZip   = Path.Combine("artifacts", $"{testName}.trace.zip");

        var passed = TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Passed;
        if (!passed)
        {
            await Page.ScreenshotAsync(new() { Path = screenshot, FullPage = true });
        }
        await Ctx.Ctx.Tracing.StopAsync(new() { Path = traceZip });

        // Adjuntos a Allure solo si falla
        if (!passed)
        {
            Allure.Net.Commons.AllureApi.AddAttachment($"{testName}-screenshot", "image/png", screenshot);
            Allure.Net.Commons.AllureApi.AddAttachment($"{testName}-trace", "application/zip", traceZip);
        }

        RunMetricsCollector.EndTest(TestContext.CurrentContext.Test.FullName, passed, retries: 0);
        await Ctx.DisposeAsync();
if (!passed)
{
    // screenshot/trace como ya tengas
    var (safeHtml, safeConsole) = SanitizedAttachmentsHelper.WriteSanitizedAndAttach(
        page: _page,                          // tu IPage
        consoleLogText: _consoleLog.ToString(),// tu StringBuilder/log
        artifactsDir: artifactsDir            // p.ej. "artifacts"
    );
    }

    [OneTimeTearDown]
    public void OneTimeTearDown() => RunMetricsCollector.WriteSummary();

    private static string Sanitize(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
        return name.Replace(' ', '_');
    }
}