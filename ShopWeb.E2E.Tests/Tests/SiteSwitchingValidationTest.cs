using NUnit.Framework;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using ShopWeb.E2E.Tests.Pages;
using ShopWeb.E2E.Tests.Config;

namespace ShopWeb.E2E.Tests.Tests;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
[AllureNUnit]
[AllureSuite("Site Switching Validation")]
public class SiteSwitchingValidationTest : BaseTest
{
    [Test]
    [Category("Validation")]
    [AllureFeature("Multi-Site Portability")]
    [AllureStory("Site A vs Site B Switching")]
    [Description("Validates that the framework can switch between Site A and Site B via configuration")]
    public async Task SiteSwitching_WhenChangingConfig_ShouldLoadCorrectSite()
    {
        // Arrange - Test current site configuration
        var currentSiteConfig = Settings.GetCurrentSite();
        var homePage = PageFactory.CreateHomePage(Page);

        // Act - Navigate to home page
        await homePage.NavigateAsync();
        
        // Assert - Verify page loads correctly for current site
        var isLoaded = await homePage.IsLoadedAsync();
        var pageTitle = await homePage.GetPageTitleAsync();
        
        Assert.That(isLoaded, Is.True, $"Home page should load successfully for Site {Settings.SiteId}");
        Assert.That(pageTitle, Is.Not.Empty, "Page title should not be empty");
        
        // Log site information for validation
        TestContext.WriteLine($"✅ Site {Settings.SiteId} ({currentSiteConfig.Name}) loaded successfully");
        TestContext.WriteLine($"📍 URL: {currentSiteConfig.BaseUrl}");
        TestContext.WriteLine($"📄 Title: {pageTitle}");
    }

    [Test]
    [Category("Validation")]
    [AllureFeature("Multi-Site Portability")]
    [AllureStory("Page Factory Pattern")]
    [Description("Validates that PageFactory creates correct page implementations based on site configuration")]
    public async Task PageFactory_WhenCreatingPages_ShouldReturnCorrectImplementations()
    {
        // Arrange
        var siteId = Settings.SiteId;
        
        // Act - Create pages using PageFactory
        var homePage = PageFactory.CreateHomePage(Page);
        var productListPage = PageFactory.CreateProductListPage(Page);
        var cartPage = PageFactory.CreateCartPage(Page);
        var loginPage = PageFactory.CreateLoginPage(Page);
        
        // Assert - Verify correct implementations are returned
        var expectedPrefix = $"Site{siteId.ToUpper()}";
        
        Assert.That(homePage.GetType().Name, Does.StartWith(expectedPrefix), 
            $"HomePage should be {expectedPrefix} implementation");
        Assert.That(productListPage.GetType().Name, Does.StartWith(expectedPrefix), 
            $"ProductListPage should be {expectedPrefix} implementation");
        Assert.That(cartPage.GetType().Name, Does.StartWith(expectedPrefix), 
            $"CartPage should be {expectedPrefix} implementation");
        Assert.That(loginPage.GetType().Name, Does.StartWith(expectedPrefix), 
            $"LoginPage should be {expectedPrefix} implementation");
            
        TestContext.WriteLine($"✅ All pages correctly implement {expectedPrefix} pattern");
    }

    [Test]
    [Category("Integration")]
    [AllureFeature("Multi-Site Portability")]
    [AllureStory("Basic Navigation Flow")]
    [Description("Validates basic navigation flow works on current site configuration")]
    public async Task BasicFlow_WhenNavigatingThroughSite_ShouldWorkCorrectly()
    {
        // Arrange
        var homePage = PageFactory.CreateHomePage(Page);
        
        // Act & Assert - Navigate through basic flow
        await homePage.NavigateAsync();
        Assert.That(await homePage.IsLoadedAsync(), Is.True, "Home page should load");
        
        var productListPage = await homePage.GoToProductsAsync();
        Assert.That(await productListPage.IsLoadedAsync(), Is.True, "Product list should load");
        
        var productCount = await productListPage.GetProductCountAsync();
        Assert.That(productCount, Is.GreaterThan(0), "Should have products available");
        
        TestContext.WriteLine($"✅ Basic flow validated - Found {productCount} products on Site {Settings.SiteId}");
    }
}