using Microsoft.Playwright;

namespace ShopWeb.E2E.Tests.Pages;

public interface ICartPage
{
    Task<bool> IsLoadedAsync();
    Task<List<CartItem>> GetCartItemsAsync();
    Task<decimal> GetTotalPriceAsync();
    Task RemoveItemAsync(string productName);
    Task<ICheckoutPage> ProceedToCheckoutAsync();
    Task ClearCartAsync();
    Task<bool> IsEmptyAsync();
}

public class CartItem
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public interface ICheckoutPage
{
    Task<bool> IsLoadedAsync();
    Task FillShippingInfoAsync(ShippingInfo shippingInfo);
    Task<IOrderConfirmationPage> PlaceOrderAsync();
    Task<decimal> GetOrderTotalAsync();
}

public class ShippingInfo
{
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string CreditCard { get; set; } = string.Empty;
    public string Month { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
}

public interface IOrderConfirmationPage
{
    Task<bool> IsLoadedAsync();
    Task<string> GetOrderIdAsync();
    Task<decimal> GetOrderTotalAsync();
    Task<IHomePage> ReturnToHomeAsync();
}