using Microsoft.Playwright;

namespace ShopWeb.E2E.Tests.Pages;

public interface IHomePage
{
    Task NavigateAsync();
    Task<bool> IsLoadedAsync();
    Task<IProductListPage> GoToProductsAsync();
    Task<ICartPage> GoToCartAsync();
    Task<ILoginPage> GoToLoginAsync();
    Task<string> GetPageTitleAsync();
}