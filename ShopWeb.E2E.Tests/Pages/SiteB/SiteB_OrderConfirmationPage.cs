using Microsoft.Playwright;

namespace ShopWeb.E2E.Tests.Pages.SiteB;

public class SiteB_OrderConfirmationPage : IOrderConfirmationPage
{
    private readonly IPage _page;

    public SiteB_OrderConfirmationPage(IPage page)
    {
        _page = page;
    }

    private ILocator CompleteHeader => _page.Locator(".complete-header");
    private ILocator BackHomeButton => _page.Locator("[data-test='back-to-products']");

    public async Task<bool> IsLoadedAsync()
    {
        try
        {
            await CompleteHeader.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 10000
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetOrderIdAsync()
    {
        // SauceDemo doesn't provide order IDs, return a mock value
        return $"SAUCE{DateTime.UtcNow.Ticks}";
    }

    public async Task<decimal> GetOrderTotalAsync()
    {
        // Order total is not shown on confirmation page in SauceDemo
        return 0m;
    }

    public async Task<IHomePage> ReturnToHomeAsync()
    {
        await BackHomeButton.ClickAsync();
        await _page.WaitForURLAsync("**/inventory.html");
        return new SiteB_HomePage(_page);
    }
}