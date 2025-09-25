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

    private ILocator HomeLink => _page.Locator(".login_logo");
    private ILocator ProductsNav => _page.Locator(".inventory_list");
    private ILocator CartNav => _page.Locator(".shopping_cart_link");
    private ILocator LoginNav => _page.Locator("#user-name"); // Login form is directly on page
    private ILocator SignUpNav => _page.Locator("#user-name"); // SauceDemo doesn't have signup

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

    public Task<IProductListPage> GoToProductsAsync()
    {
        // For demoblaze, products are on the home page, so we just return the product list implementation
        return Task.FromResult<IProductListPage>(new SiteA_ProductListPage(_page));
    }

    public async Task<ICartPage> GoToCartAsync()
    {
        await CartNav.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        return new SiteA_CartPage(_page);
    }

    public async Task<ILoginPage> GoToLoginAsync()
    {
        // For SauceDemo, the login form is already on the homepage - no need to click anything
        // Just wait for the login form to be visible
        await _page.WaitForSelectorAsync("#user-name", new PageWaitForSelectorOptions
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