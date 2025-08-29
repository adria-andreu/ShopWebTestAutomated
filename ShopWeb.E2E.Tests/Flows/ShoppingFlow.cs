using Microsoft.Playwright;
using ShopWeb.E2E.Tests.Pages;
using ShopWeb.E2E.Tests.Pages.SiteA;
using ShopWeb.E2E.Tests.Config;

namespace ShopWeb.E2E.Tests.Flows;

public class ShoppingFlow
{
    private readonly IPage _page;
    private readonly TestSettings _settings;

    public ShoppingFlow(IPage page)
    {
        _page = page;
        _settings = ConfigurationManager.TestSettings;
    }

    public async Task<string> AddProductToCartAsync(string? productName = null, int? productIndex = null)
    {
        // Navigate to home page
        var homePage = GetHomePage();
        await homePage.NavigateAsync();
        
        if (!await homePage.IsLoadedAsync())
            throw new Exception("Home page failed to load");

        // Get product list
        var productListPage = await homePage.GoToProductsAsync();
        
        if (!await productListPage.IsLoadedAsync())
            throw new Exception("Product list page failed to load");

        // Select product
        IProductDetailPage productDetailPage;
        if (!string.IsNullOrEmpty(productName))
        {
            productDetailPage = await productListPage.SelectProductAsync(productName);
        }
        else if (productIndex.HasValue)
        {
            productDetailPage = await productListPage.SelectProductByIndexAsync(productIndex.Value);
        }
        else
        {
            // Select first product by default
            productDetailPage = await productListPage.SelectProductByIndexAsync(0);
        }

        if (!await productDetailPage.IsLoadedAsync())
            throw new Exception("Product detail page failed to load");

        // Get product name for return
        var selectedProductName = await productDetailPage.GetProductNameAsync();

        // Add to cart
        await productDetailPage.AddToCartAsync();

        return selectedProductName;
    }

    public async Task<decimal> GetCartTotalAsync()
    {
        var homePage = GetHomePage();
        var cartPage = await homePage.GoToCartAsync();
        
        if (!await cartPage.IsLoadedAsync())
            throw new Exception("Cart page failed to load");

        return await cartPage.GetTotalPriceAsync();
    }

    public async Task<List<CartItem>> GetCartItemsAsync()
    {
        var homePage = GetHomePage();
        var cartPage = await homePage.GoToCartAsync();
        
        if (!await cartPage.IsLoadedAsync())
            throw new Exception("Cart page failed to load");

        return await cartPage.GetCartItemsAsync();
    }

    public async Task RemoveProductFromCartAsync(string productName)
    {
        var homePage = GetHomePage();
        var cartPage = await homePage.GoToCartAsync();
        
        if (!await cartPage.IsLoadedAsync())
            throw new Exception("Cart page failed to load");

        await cartPage.RemoveItemAsync(productName);
    }

    public async Task ClearCartAsync()
    {
        var homePage = GetHomePage();
        var cartPage = await homePage.GoToCartAsync();
        
        if (!await cartPage.IsLoadedAsync())
            throw new Exception("Cart page failed to load");

        await cartPage.ClearCartAsync();
    }

    public async Task<string> PlaceOrderAsync(ShippingInfo shippingInfo)
    {
        var homePage = GetHomePage();
        var cartPage = await homePage.GoToCartAsync();
        
        if (!await cartPage.IsLoadedAsync())
            throw new Exception("Cart page failed to load");

        if (await cartPage.IsEmptyAsync())
            throw new Exception("Cannot place order - cart is empty");

        var checkoutPage = await cartPage.ProceedToCheckoutAsync();
        
        if (!await checkoutPage.IsLoadedAsync())
            throw new Exception("Checkout page failed to load");

        await checkoutPage.FillShippingInfoAsync(shippingInfo);
        
        var orderConfirmationPage = await checkoutPage.PlaceOrderAsync();
        
        if (!await orderConfirmationPage.IsLoadedAsync())
            throw new Exception("Order confirmation page failed to load");

        var orderId = await orderConfirmationPage.GetOrderIdAsync();
        
        // Return to home page
        await orderConfirmationPage.ReturnToHomeAsync();
        
        return orderId;
    }

    public async Task<List<string>> BrowseProductsByCategoryAsync(string category)
    {
        var homePage = GetHomePage();
        await homePage.NavigateAsync();
        
        var productListPage = await homePage.GoToProductsAsync();
        
        if (!await productListPage.IsLoadedAsync())
            throw new Exception("Product list page failed to load");

        await productListPage.FilterByCategoryAsync(category);
        
        return await productListPage.GetProductNamesAsync();
    }

    private IHomePage GetHomePage()
    {
        return _settings.SiteId.ToUpperInvariant() switch
        {
            "A" => new SiteA_HomePage(_page),
            "B" => throw new NotImplementedException("Site B implementation not available yet"),
            _ => throw new ArgumentException($"Unsupported SiteId: {_settings.SiteId}")
        };
    }

    public async Task CleanupAsync()
    {
        try
        {
            // Clear cart if needed
            await ClearCartAsync();
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}