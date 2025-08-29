using Microsoft.Playwright;
using ShopWeb.E2E.Tests.Config;

namespace ShopWeb.E2E.Tests.Pages.SiteA;

public class SiteA_HomePage : IHomePage
{
    private readonly IPage _page;
    private readonly TestSettings _settings;

    public SiteA_HomePage(IPage page)
    {
        _page = page;
        _settings = ConfigurationManager.TestSettings;
    }

    private ILocator HomeLink => _page.Locator("a.navbar-brand");
    private ILocator ProductsNav => _page.Locator("a[onclick*='byCat']").First;
    private ILocator CartNav => _page.Locator("#cartur");
    private ILocator LoginNav => _page.Locator("#login2");
    private ILocator SignUpNav => _page.Locator("#signin2");

    public async Task NavigateAsync()
    {
        await _page.GotoAsync(_settings.BaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });
    }

    public async Task<bool> IsLoadedAsync()
    {
        try
        {
            await HomeLink.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = _settings.Timeouts.Default
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IProductListPage> GoToProductsAsync()
    {
        // For demoblaze, products are on the home page, so we just return the product list implementation
        return new SiteA_ProductListPage(_page);
    }

    public async Task<ICartPage> GoToCartAsync()
    {
        await CartNav.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        return new SiteA_CartPage(_page);
    }

    public async Task<ILoginPage> GoToLoginAsync()
    {
        await LoginNav.ClickAsync();
        // Wait for login modal to appear
        await _page.WaitForSelectorAsync("#logInModal", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible
        });
        return new SiteA_LoginPage(_page);
    }

    public async Task<string> GetPageTitleAsync()
    {
        return await _page.TitleAsync();
    }
}