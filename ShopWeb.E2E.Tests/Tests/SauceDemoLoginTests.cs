using NUnit.Framework;
using ShopWeb.E2E.Tests.Flows;
using ShopWeb.E2E.Tests.Config;
using ShopWeb.E2E.Tests.Pages;
using ShopWeb.E2E.Tests.Utilities;

namespace ShopWeb.E2E.Tests.Tests;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class SauceDemoLoginTests : BaseTest
{
    private AuthenticationFlow? _authFlow;

    [SetUp]
    public void SetUpAuthFlow()
    {
        _authFlow = new AuthenticationFlow(Page);
    }

    [TearDown]
    public async Task CleanupAuthFlow()
    {
        if (_authFlow != null)
        {
            await _authFlow.CleanupAsync();
        }
    }

    [Test]
    [Category(AutoStatus.Automated), Category(Tag.Smoke), Category(Tag.CriticalPath)]
    [Description("Ensure that a standard user can login successfully to SauceDemo with valid credentials")]
    public async Task Login_StandardUser_Success()
    {
        // Setup: Prepare test data for standard user login
        var (username, password) = DataFactory.SauceDemo.Users.StandardUser;
        var siteProfile = SiteRegistry.GetProfile("B");

        // Setup: Navigate to SauceDemo login page
        await NavigateAsync(siteProfile.BaseUrl);
        var loginPage = PageFactory.CreateLoginPage(Page);

        // Setup: Verify login page is loaded correctly
        var isLoginPageLoaded = await loginPage.IsLoadedAsync();
        Verify.VerifyTrue(isLoginPageLoaded, "Login page is loaded successfully");

        // Step: Enter valid credentials for standard user
        var homePage = await loginPage.LoginAsync(username, password);

        // Step: Verify navigation to inventory page after successful login
        var isHomePageLoaded = await homePage.IsLoadedAsync();

        // Verify: User is successfully logged in and redirected to inventory page
        Verify.VerifyTrue(isHomePageLoaded, "User is successfully logged in and inventory page is displayed");

        // Verify: Page title shows "Products" indicating successful login
        var pageTitle = await homePage.GetPageTitleAsync();
        Verify.VerifyEquals("Products", pageTitle, "Products page title is displayed correctly after login");
    }

    [Test]
    [Category(AutoStatus.Automated), Category(Tag.Negative)]
    [Description("Ensure that login fails gracefully with invalid credentials")]
    public async Task Login_InvalidCredentials_Failure()
    {
        // Setup: Prepare test data for invalid user credentials
        var (username, password) = DataFactory.SauceDemo.Users.InvalidUser;
        var siteProfile = SiteRegistry.GetProfile("B");

        // Setup: Navigate to SauceDemo login page
        await NavigateAsync(siteProfile.BaseUrl);
        var loginPage = PageFactory.CreateLoginPage(Page);

        // Step: Attempt login with invalid credentials
        try
        {
            await loginPage.LoginAsync(username, password);
            Verify.VerifyFail("Login should have failed with invalid credentials");
        }
        catch (Exception ex)
        {
            // Verify: Error message is displayed for invalid credentials
            var errorMessage = await loginPage.GetErrorMessageAsync();
            Verify.VerifyTrue(!string.IsNullOrEmpty(errorMessage), "Error message is displayed for invalid credentials");

            // Verify: User remains on login page after failed login attempt
            var isStillOnLoginPage = await loginPage.IsLoadedAsync();
            Verify.VerifyTrue(isStillOnLoginPage, "User remains on login page after invalid login attempt");
        }
    }

    [Test]
    [Category(AutoStatus.Automated), Category(Tag.Negative)]
    [Description("Ensure that locked out user cannot login and receives appropriate error message")]
    public async Task Login_LockedOutUser_Failure()
    {
        // Setup: Prepare test data for locked out user
        var (username, password) = DataFactory.SauceDemo.Users.LockedUser;
        var siteProfile = SiteRegistry.GetProfile("B");

        // Setup: Navigate to SauceDemo login page
        await NavigateAsync(siteProfile.BaseUrl);
        var loginPage = PageFactory.CreateLoginPage(Page);

        // Step: Attempt login with locked out user credentials
        try
        {
            await loginPage.LoginAsync(username, password);
            Verify.VerifyFail("Login should have failed for locked out user");
        }
        catch (Exception ex)
        {
            // Verify: Error message is displayed for locked out user
            var errorMessage = await loginPage.GetErrorMessageAsync();
            Verify.VerifyTrue(!string.IsNullOrEmpty(errorMessage), "Error message is displayed for locked out user");

            // Verify: User remains on login page after locked out login attempt
            var isStillOnLoginPage = await loginPage.IsLoadedAsync();
            Verify.VerifyTrue(isStillOnLoginPage, "User remains on login page after locked out login attempt");
        }
    }

    [Test]
    [Category(AutoStatus.Automated), Category(Tag.Edge)]
    [Description("Ensure that login fails gracefully with empty credentials")]
    public async Task Login_EmptyCredentials_Failure()
    {
        // Setup: Prepare empty credentials test data
        var (username, password) = DataFactory.SauceDemo.Users.EmptyCredentials;
        var siteProfile = SiteRegistry.GetProfile("B");

        // Setup: Navigate to SauceDemo login page
        await NavigateAsync(siteProfile.BaseUrl);
        var loginPage = PageFactory.CreateLoginPage(Page);

        // Step: Attempt login with empty credentials
        try
        {
            await loginPage.LoginAsync(username, password);
            Verify.VerifyFail("Login should have failed with empty credentials");
        }
        catch (Exception ex)
        {
            // Verify: Error message is displayed for empty credentials
            var errorMessage = await loginPage.GetErrorMessageAsync();
            Verify.VerifyTrue(!string.IsNullOrEmpty(errorMessage), "Error message is displayed for empty credentials");

            // Verify: User remains on login page after empty credentials login attempt
            var isStillOnLoginPage = await loginPage.IsLoadedAsync();
            Verify.VerifyTrue(isStillOnLoginPage, "User remains on login page after empty credentials login attempt");
        }
    }

    [Test]
    [Category(AutoStatus.Automated), Category(Tag.Regression)]
    [Description("Ensure that direct navigation to SauceDemo login page loads successfully")]
    public async Task LoginPage_DirectNavigation_Success()
    {
        // Setup: Prepare site profile for direct navigation
        var siteProfile = SiteRegistry.GetProfile("B");

        // Step: Navigate directly to SauceDemo login page
        await NavigateAsync(siteProfile.BaseUrl);

        // Step: Create login page object and verify loading
        var loginPage = PageFactory.CreateLoginPage(Page);
        var isLoaded = await loginPage.IsLoadedAsync();

        // Verify: SauceDemo login page loads successfully
        Verify.VerifyTrue(isLoaded, "SauceDemo login page loads successfully on direct navigation");

        // Verify: Login form elements are visible and accessible
        var usernameFieldVisible = await Page.Locator("[data-test='username']").IsVisibleAsync();
        var passwordFieldVisible = await Page.Locator("[data-test='password']").IsVisibleAsync();
        var loginButtonVisible = await Page.Locator("[data-test='login-button']").IsVisibleAsync();

        Verify.VerifyTrue(usernameFieldVisible, "Username field is visible on login page");
        Verify.VerifyTrue(passwordFieldVisible, "Password field is visible on login page");
        Verify.VerifyTrue(loginButtonVisible, "Login button is visible on login page");
    }
}