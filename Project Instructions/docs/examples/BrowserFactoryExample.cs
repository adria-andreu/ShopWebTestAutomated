// docs/examples/BrowserFactoryExample.cs
// BrowserFactory thread-safe para Playwright con paralelización segura por test.

using Microsoft.Playwright;
using System;
using System.Threading;
using System.Threading.Tasks;

public static class BrowserFactory
{
    private static readonly AsyncLocal<IBrowser?> _browser = new();
    private static readonly AsyncLocal<IPlaywright?> _pw = new();

    public static async Task<IPage> CreatePageAsync(string browserName = "chromium", bool headless = true)
    {
        if (_pw.Value is null)
        {
            _pw.Value = await Playwright.CreateAsync();
        }
        if (_browser.Value is null || _browser.Value.IsClosed)
        {
            var type = browserName.ToLowerInvariant() switch
            {
                "firefox" => _pw.Value!.Firefox,
                "webkit" => _pw.Value!.Webkit,
                _ => _pw.Value!.Chromium
            };
            _browser.Value = await type.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = headless
            });
        }
        var ctx = await _browser.Value!.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1280, Height = 800 },
            IgnoreHTTPSErrors = true
        });
        return await ctx.NewPageAsync();
    }

    public static async Task DisposeAsync()
    {
        if (_browser.Value is not null && !_browser.Value.IsClosed)
            await _browser.Value.CloseAsync();
        _browser.Value = null;

        if (_pw.Value is not null)
            _pw.Value.Dispose();
        _pw.Value = null;
    }
}
