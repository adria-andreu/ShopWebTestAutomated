using Microsoft.Playwright;
using System.Collections.Concurrent;

namespace ShopWeb.E2E.Tests.Browsers;

public class BrowserFactory : IBrowserFactory, IDisposable
{
    private static readonly ConcurrentDictionary<string, IBrowser> _browsers = new();
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private static IPlaywright? _playwright;
    private static bool _disposed = false;

    public async Task<IBrowser> GetBrowserAsync(string browserType = "chromium")
    {
        if (_disposed) throw new ObjectDisposedException(nameof(BrowserFactory));

        if (_browsers.TryGetValue(browserType, out var existingBrowser))
            return existingBrowser;
        
        var browser = await CreateBrowserAsync(browserType);
        _browsers.TryAdd(browserType, browser);
        return browser;
    }

    public async Task<IBrowserContext> CreateContextAsync(string browserType = "chromium", Dictionary<string, object>? options = null)
    {
        var browser = await GetBrowserAsync(browserType);
        
        var contextOptions = new BrowserNewContextOptions();
        
        if (options != null)
        {
            if (options.ContainsKey("viewport"))
            {
                var viewport = options["viewport"] as Dictionary<string, object>;
                if (viewport != null)
                {
                    contextOptions.ViewportSize = new ViewportSize
                    {
                        Width = Convert.ToInt32(viewport["width"]),
                        Height = Convert.ToInt32(viewport["height"])
                    };
                }
            }

            if (options.ContainsKey("userAgent"))
                contextOptions.UserAgent = options["userAgent"]?.ToString();

            if (options.ContainsKey("locale"))
                contextOptions.Locale = options["locale"]?.ToString();
        }

        // Default viewport if not specified
        contextOptions.ViewportSize ??= new ViewportSize { Width = 1280, Height = 720 };

        return await browser.NewContextAsync(contextOptions);
    }

    private static async Task<IBrowser> CreateBrowserAsync(string browserType)
    {
        await _semaphore.WaitAsync();
        try
        {
            _playwright ??= await Playwright.CreateAsync();

            var launchOptions = new BrowserTypeLaunchOptions
            {
                Headless = !IsHeadedMode(),
                Args = new[] { "--disable-web-security", "--disable-features=VizDisplayCompositor" }
            };

            return browserType.ToLowerInvariant() switch
            {
                "chromium" => await _playwright.Chromium.LaunchAsync(launchOptions),
                "firefox" => await _playwright.Firefox.LaunchAsync(launchOptions),
                "webkit" => await _playwright.Webkit.LaunchAsync(launchOptions),
                _ => throw new ArgumentException($"Unsupported browser type: {browserType}")
            };
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private static bool IsHeadedMode()
    {
        var headed = Environment.GetEnvironmentVariable("HEADED");
        return headed == "1" || headed?.ToLowerInvariant() == "true";
    }

    public async Task CloseBrowserAsync(string browserType = "chromium")
    {
        if (_browsers.TryRemove(browserType, out var browser))
        {
            await browser.CloseAsync();
        }
    }

    public async Task CloseAllBrowsersAsync()
    {
        await CloseAllBrowsersStaticAsync();
    }

    public static async Task CloseAllBrowsersStaticAsync()
    {
        var browsers = _browsers.Values.ToList();
        _browsers.Clear();

        foreach (var browser in browsers)
        {
            try
            {
                await browser.CloseAsync();
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        if (_playwright != null)
        {
            _playwright.Dispose();
            _playwright = null;
        }
    }

    public void Dispose()
    {
        // Don't dispose static resources per instance - they should be managed globally
        // This prevents ObjectDisposedException during parallel test execution
        GC.SuppressFinalize(this);
    }

    public static void DisposeStatic()
    {
        if (!_disposed)
        {
            CloseAllBrowsersStaticAsync().GetAwaiter().GetResult();
            _disposed = true;
        }
    }
}