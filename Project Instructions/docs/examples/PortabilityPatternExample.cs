// docs/examples/PortabilityPatternExample.cs
// Patrón de portabilidad: ISiteProfile + POM por sitio + flows neutrales.

using Microsoft.Playwright;
using System.Threading.Tasks;

public interface ISiteProfile
{
    string SiteId { get; }
    string BaseUrl { get; }
    string CurrencySymbol { get; }
    // Extensible: rutas conocidas, formato precios, etc.
}

public class SiteAProfile : ISiteProfile
{
    public string SiteId => "A";
    public string BaseUrl => "https://shopA.example.com";
    public string CurrencySymbol => "€";
}

public class SiteBProfile : ISiteProfile
{
    public string SiteId => "B";
    public string BaseUrl => "https://shopB.example.com";
    public string CurrencySymbol => "$";
}

// Contratos POM (no exponen selectores)
public interface IHomePage { Task GoAsync(); Task SearchAsync(string text); }
public interface ICatalogPage { Task OpenProductAsync(string productId); }
public interface IProductPage { Task<decimal> GetPriceAsync(); Task AddToCartAsync(); }
public interface ICartPage { Task<int> GetItemsCountAsync(); }

// Implementación Site A (los selectores *dentro* de las clases)
public class SiteA_HomePage : IHomePage
{
    private readonly IPage _page; private readonly ISiteProfile _p;
    public SiteA_HomePage(IPage page, ISiteProfile profile) { _page = page; _p = profile; }
    public async Task GoAsync() => await _page.GotoAsync(_p.BaseUrl);
    public async Task SearchAsync(string text)
    {
        await _page.GetByPlaceholder("Search products").FillAsync(text);
        await _page.PressAsync("input[placeholder='Search products']", "Enter");
    }
}

public class SiteA_ProductPage : IProductPage
{
    private readonly IPage _page;
    public SiteA_ProductPage(IPage page) { _page = page; }
    public async Task<decimal> GetPriceAsync()
    {
        var priceText = await _page.Locator(".price").InnerTextAsync();
        return PriceParser.ToDecimal(priceText);
    }
    public async Task AddToCartAsync() => await _page.GetByRole(AriaRole.Button, new() { Name = "Add to cart" }).ClickAsync();
}

public class SiteA_CartPage : ICartPage
{
    private readonly IPage _page;
    public SiteA_CartPage(IPage page) { _page = page; }
    public async Task<int> GetItemsCountAsync()
        => int.Parse(await _page.Locator(".cart-count").InnerTextAsync());
}

// Implementación Site B (otros selectores/rutas)
public class SiteB_HomePage : IHomePage
{
    private readonly IPage _page; private readonly ISiteProfile _p;
    public SiteB_HomePage(IPage page, ISiteProfile profile) { _page = page; _p = profile; }
    public async Task GoAsync() => await _page.GotoAsync(_p.BaseUrl + "/home");
    public async Task SearchAsync(string text)
    {
        await _page.FillAsync("#q", text);
        await _page.ClickAsync("#searchBtn");
    }
}
public class SiteB_ProductPage : IProductPage
{
    private readonly IPage _page;
    public SiteB_ProductPage(IPage page) { _page = page; }
    public async Task<decimal> GetPriceAsync()
    {
        var priceText = await _page.InnerTextAsync("span[data-testid='price']");
        return PriceParser.ToDecimal(priceText);
    }
    public async Task AddToCartAsync() => await _page.ClickAsync("button[data-action='add']");
}
public class SiteB_CartPage : ICartPage
{
    private readonly IPage _page;
    public SiteB_CartPage(IPage page) { _page = page; }
    public async Task<int> GetItemsCountAsync()
        => int.Parse(await _page.InnerTextAsync("[data-testid='cart-count']"));
}

// Factory simple por SiteId (puedes reemplazar por DI)
public static class SiteFactory
{
    public static ISiteProfile CreateProfile(string siteId) => siteId switch
    {
        "B" => new SiteBProfile(),
        _ => new SiteAProfile()
    };

    public static (IHomePage home, IProductPage product, ICartPage cart) CreatePages(string siteId, IPage page)
    {
        if (siteId == "B")
            return (new SiteB_HomePage(page, new SiteBProfile()), new SiteB_ProductPage(page), new SiteB_CartPage(page));
        return (new SiteA_HomePage(page, new SiteAProfile()), new SiteA_ProductPage(page), new SiteA_CartPage(page));
    }
}

// Normalizador de precios sencillo
public static class PriceParser
{
    public static decimal ToDecimal(string raw)
    {
        // Elimina símbolos, usa InvariantCulture; adapta si hay formatos locales complejos.
        var clean = new string(raw.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());
        clean = clean.Replace(",", ".");
        return decimal.TryParse(clean, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d)
            ? d : 0m;
    }
}

// Flow neutral (los tests llaman a esto)
public static class ShoppingFlows
{
    public static async Task<int> AddProductAndCountAsync(string siteId, IPage page, string productIdOrQuery)
    {
        var (home, product, cart) = SiteFactory.CreatePages(siteId, page);
        await home.GoAsync();
        await home.SearchAsync(productIdOrQuery);
        // (En un ejemplo largo real, abriríamos el catálogo y el detalle usando el productId)
        await product.AddToCartAsync();
        return await cart.GetItemsCountAsync();
    }
}
