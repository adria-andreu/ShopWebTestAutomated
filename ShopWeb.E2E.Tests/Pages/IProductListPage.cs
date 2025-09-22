using Microsoft.Playwright;

namespace ShopWeb.E2E.Tests.Pages;

public interface IProductListPage
{
    Task<bool> IsLoadedAsync();
    Task<List<string>> GetProductNamesAsync();
    Task<IProductDetailPage> SelectProductAsync(string productName);
    Task<IProductDetailPage> SelectProductByIndexAsync(int index);
    Task FilterByCategoryAsync(string category);
    Task<int> GetProductCountAsync();
}

public interface IProductDetailPage
{
    Task<bool> IsLoadedAsync();
    Task<string> GetProductNameAsync();
    Task<decimal> GetProductPriceAsync();
    Task<string> GetProductDescriptionAsync();
    Task<ICartPage> AddToCartAsync();
    Task<IProductListPage> GoBackToProductsAsync();
}