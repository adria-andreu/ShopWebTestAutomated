using Microsoft.Playwright;

namespace ShopWeb.E2E.Tests.Pages.SiteB;

public class SiteB_ProductDetailPage : IProductDetailPage
{
    private readonly IPage _page;

    public SiteB_ProductDetailPage(IPage page)
    {
        _page = page;
    }

    private ILocator ProductName => _page.Locator(".inventory_details_name");
    private ILocator ProductPrice => _page.Locator(".inventory_details_price");
    private ILocator ProductDescription => _page.Locator(".inventory_details_desc");
    private ILocator AddToCartButton => _page.Locator("[data-test='add-to-cart']");
    private ILocator BackToProductsButton => _page.Locator("[data-test='back-to-products']");

    public async Task<bool> IsLoadedAsync()
    {
        try
        {
            await ProductName.WaitForAsync(new LocatorWaitForOptions
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

    public async Task<string> GetProductNameAsync()
    {
        return await ProductName.TextContentAsync() ?? "";
    }

    public async Task<decimal> GetProductPriceAsync()
    {
        var priceText = await ProductPrice.TextContentAsync() ?? "0";
        var cleanPrice = priceText.Replace("$", "").Trim();
        return decimal.TryParse(cleanPrice, out var price) ? price : 0m;
    }

    public async Task<string> GetProductDescriptionAsync()
    {
        return await ProductDescription.TextContentAsync() ?? "";
    }

    public async Task<ICartPage> AddToCartAsync()
    {
        await AddToCartButton.ClickAsync();

        // Navigate to cart page
        var cartLink = _page.Locator(".shopping_cart_link");
        await cartLink.ClickAsync();
        await _page.WaitForURLAsync("**/cart.html");

        return new SiteB_CartPage(_page);
    }

    public async Task<IProductListPage> GoBackToProductsAsync()
    {
        await BackToProductsButton.ClickAsync();
        await _page.WaitForURLAsync("**/inventory.html");
        return new SiteB_ProductListPage(_page);
    }
}