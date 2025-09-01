using NUnit.Framework;
// Allure integration disabled to unblock IT03 - complex context management issues in CI/CD
// using Allure.NUnit;
// using Allure.NUnit.Attributes;
using ShopWeb.E2E.Tests.Flows;
using ShopWeb.E2E.Tests.Pages;

namespace ShopWeb.E2E.Tests.Tests;

[TestFixture]
[Parallelizable(ParallelScope.All)]
// [AllureNUnit] // Disabled to unblock IT03 - complex context management issues in CI/CD
// [AllureSuite("Shopping Tests")]
public class ShoppingTests : BaseTest
{
    private ShoppingFlow? _shoppingFlow;

    [SetUp]
    public void SetUpShoppingFlow()
    {
        _shoppingFlow = new ShoppingFlow(Page);
    }

    [TearDown]
    public async Task CleanupShoppingFlow()
    {
        if (_shoppingFlow != null)
        {
            await _shoppingFlow.CleanupAsync();
        }
    }

    [Test]
    [Category("Smoke")]
    // [AllureFeature("Shopping Cart")] // Temporarily disabled - TD-14
    // [AllureStory("Add Product to Cart")]
        [Description("Verify that a user can successfully add a product to the shopping cart")]
    public async Task AddToCart_WhenSelectingProduct_ShouldAddProductSuccessfully()
    {
        // Arrange & Act
        var productName = await _shoppingFlow!.AddProductToCartAsync(productIndex: 0);
        
        // Assert
        var cartItems = await _shoppingFlow.GetCartItemsAsync();
        
        Assert.That(cartItems, Is.Not.Empty, "Cart should contain items after adding a product");
        Assert.That(cartItems.Any(item => item.Name.Contains(productName, StringComparison.OrdinalIgnoreCase)), 
            Is.True, $"Cart should contain the added product: {productName}");
        
        var total = await _shoppingFlow.GetCartTotalAsync();
        Assert.That(total, Is.GreaterThan(0), "Cart total should be greater than 0");
    }

    [Test]
    [Category("Smoke")]
    // [AllureFeature("Shopping Cart")] // Temporarily disabled - TD-14
    // [AllureStory("Remove Product from Cart")]
        [Description("Verify that a user can remove products from the shopping cart")]
    public async Task RemoveFromCart_WhenProductInCart_ShouldRemoveProductSuccessfully()
    {
        // Arrange
        var productName = await _shoppingFlow!.AddProductToCartAsync(productIndex: 0);
        var initialItems = await _shoppingFlow.GetCartItemsAsync();
        var initialCount = initialItems.Count;
        
        // Act
        await _shoppingFlow.RemoveProductFromCartAsync(productName);
        
        // Assert
        var updatedItems = await _shoppingFlow.GetCartItemsAsync();
        Assert.That(updatedItems.Count, Is.EqualTo(initialCount - 1), 
            "Cart should have one less item after removal");
        Assert.That(updatedItems.Any(item => item.Name.Contains(productName, StringComparison.OrdinalIgnoreCase)), 
            Is.False, "Removed product should no longer be in cart");
    }

    [Test]
    [Category("Regression")]
    // [AllureFeature("Product Browsing")] // Temporarily disabled - TD-14
    // [AllureStory("Filter Products by Category")]
        [Description("Verify that products can be filtered by category")]
    public async Task BrowseProducts_WhenFilteringByCategory_ShouldShowRelevantProducts()
    {
        // Act
        var phoneProducts = await _shoppingFlow!.BrowseProductsByCategoryAsync("phones");
        
        // Assert
        Assert.That(phoneProducts, Is.Not.Empty, "Should find phone products");
        Assert.That(phoneProducts.Count, Is.GreaterThan(0), "Phone category should contain products");
        
        // Verify that products are actually phones (basic check)
        var hasPhoneKeywords = phoneProducts.Any(product => 
            product.ToLowerInvariant().Contains("phone") || 
            product.ToLowerInvariant().Contains("samsung") || 
            product.ToLowerInvariant().Contains("iphone") ||
            product.ToLowerInvariant().Contains("nexus"));
        
        Assert.That(hasPhoneKeywords, Is.True, "Products should be phone-related");
    }

    [Test]
    [Category("Regression")]
    // [AllureFeature("Shopping Cart")] // Temporarily disabled - TD-14
    // [AllureStory("Multiple Products in Cart")]
        [Description("Verify that multiple products can be added to cart and total is calculated correctly")]
    public async Task AddMultipleProducts_WhenAddingTwoDifferentProducts_ShouldCalculateCorrectTotal()
    {
        // Arrange & Act
        var product1Name = await _shoppingFlow!.AddProductToCartAsync(productIndex: 0);
        var product2Name = await _shoppingFlow.AddProductToCartAsync(productIndex: 1);
        
        // Assert
        var cartItems = await _shoppingFlow.GetCartItemsAsync();
        Assert.That(cartItems.Count, Is.EqualTo(2), "Cart should contain 2 products");
        
        var containsProduct1 = cartItems.Any(item => 
            item.Name.Contains(product1Name, StringComparison.OrdinalIgnoreCase));
        var containsProduct2 = cartItems.Any(item => 
            item.Name.Contains(product2Name, StringComparison.OrdinalIgnoreCase));
        
        Assert.That(containsProduct1, Is.True, $"Cart should contain first product: {product1Name}");
        Assert.That(containsProduct2, Is.True, $"Cart should contain second product: {product2Name}");
        
        var total = await _shoppingFlow.GetCartTotalAsync();
        var expectedTotal = cartItems.Sum(item => item.Price);
        
        Assert.That(total, Is.EqualTo(expectedTotal), 
            "Cart total should equal sum of individual product prices");
    }

    [Test]
    [Category("Edge")]
    // [AllureFeature("Shopping Cart")] // Temporarily disabled - TD-14
    // [AllureStory("Empty Cart Handling")]
        [Description("Verify behavior when working with an empty cart")]
    public async Task EmptyCart_WhenCheckingCartContents_ShouldHandleGracefully()
    {
        // Arrange
        await _shoppingFlow!.ClearCartAsync();
        
        // Act & Assert
        var cartItems = await _shoppingFlow.GetCartItemsAsync();
        Assert.That(cartItems, Is.Empty, "Empty cart should contain no items");
        
        var total = await _shoppingFlow.GetCartTotalAsync();
        Assert.That(total, Is.EqualTo(0), "Empty cart total should be 0");
    }

    [Test]
    [Category("Smoke")]
    // [AllureFeature("E-commerce Checkout")] // Temporarily disabled - TD-14
    // [AllureStory("Complete Purchase Flow")]
        [Description("Verify complete purchase flow from adding product to order completion")]
    public async Task CompletePurchase_WhenCheckingOut_ShouldCompleteOrderSuccessfully()
    {
        // Arrange
        await _shoppingFlow!.AddProductToCartAsync(productIndex: 0);
        
        var shippingInfo = new ShippingInfo
        {
            Name = "John Doe",
            Country = "USA",
            City = "New York",
            CreditCard = "1234567812345678",
            Month = "12",
            Year = "2025"
        };
        
        // Act
        var orderId = await _shoppingFlow.PlaceOrderAsync(shippingInfo);
        
        // Assert
        Assert.That(orderId, Is.Not.Empty, "Order ID should be generated");
        Assert.That(orderId, Is.Not.Null, "Order ID should not be null");
        
        // Verify cart is empty after successful order
        var cartItems = await _shoppingFlow.GetCartItemsAsync();
        Assert.That(cartItems, Is.Empty, "Cart should be empty after completing purchase");
    }
}