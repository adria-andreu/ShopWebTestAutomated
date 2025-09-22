using Microsoft.Playwright;

namespace ShopWeb.E2E.Tests.Pages.SiteB;

public class SiteB_HomePage : IHomePage
{
    private readonly IPage _page;

    public SiteB_HomePage(IPage page)
    {
        _page = page;
    }

    private ILocator ProductsPageTitle => _page.Locator(".title").Filter(new LocatorFilterOptions { HasText = "Products" });
    private ILocator ShoppingCartLink => _page.Locator(".shopping_cart_link");
    private ILocator MenuButton => _page.Locator("#react-burger-menu-btn");
    private ILocator LogoutLink => _page.Locator("#logout_sidebar_link");

    public async Task NavigateAsync()
    {
        await _page.GotoAsync("https://www.saucedemo.com/inventory.html");
    }

    public async Task<bool> IsLoadedAsync()
    {
        try
        {
            await ProductsPageTitle.WaitForAsync(new LocatorWaitForOptions
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

    public async Task<IProductListPage> GoToProductsAsync()
    {
        // In SauceDemo, the home page IS the products page
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        return new SiteB_ProductListPage(_page);
    }

    public async Task<ICartPage> GoToCartAsync()
    {
        await ShoppingCartLink.ClickAsync();
        await _page.WaitForURLAsync("**/cart.html");
        return new SiteB_CartPage(_page);
    }

    public async Task<ILoginPage> GoToLoginAsync()
    {
        // Logout first to get to login page
        await MenuButton.ClickAsync();
        await LogoutLink.ClickAsync();
        await _page.WaitForURLAsync("**/index.html");
        return new SiteB_LoginPage(_page);
    }

    public async Task<string> GetPageTitleAsync()
    {
        return await ProductsPageTitle.TextContentAsync() ?? "Products";
    }
}