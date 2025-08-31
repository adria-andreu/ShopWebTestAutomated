using Microsoft.Playwright;

namespace ShopWeb.E2E.Tests.Browsers;

public interface IBrowserFactory : IDisposable
{
    Task<IBrowser> GetBrowserAsync(string browserType = "chromium");
    Task<IBrowserContext> CreateContextAsync(string browserType = "chromium", Dictionary<string, object>? options = null);
    Task CloseBrowserAsync(string browserType = "chromium");
    Task CloseAllBrowsersAsync();
}