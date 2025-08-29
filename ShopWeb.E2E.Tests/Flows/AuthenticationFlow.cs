using Microsoft.Playwright;
using ShopWeb.E2E.Tests.Pages;
using ShopWeb.E2E.Tests.Pages.SiteA;
using ShopWeb.E2E.Tests.Config;

namespace ShopWeb.E2E.Tests.Flows;

public class AuthenticationFlow
{
    private readonly IPage _page;
    private readonly TestSettings _settings;

    public AuthenticationFlow(IPage page)
    {
        _page = page;
        _settings = ConfigurationManager.TestSettings;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            var homePage = GetHomePage();
            await homePage.NavigateAsync();
            
            if (!await homePage.IsLoadedAsync())
                throw new Exception("Home page failed to load");

            var loginPage = await homePage.GoToLoginAsync();
            
            if (!await loginPage.IsLoadedAsync())
                throw new Exception("Login page failed to load");

            await loginPage.LoginAsync(username, password);
            
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SignUpAsync(string username, string password)
    {
        try
        {
            var homePage = GetHomePage();
            await homePage.NavigateAsync();
            
            if (!await homePage.IsLoadedAsync())
                throw new Exception("Home page failed to load");

            var loginPage = await homePage.GoToLoginAsync();
            
            if (!await loginPage.IsLoadedAsync())
                throw new Exception("Login page failed to load");

            var signUpPage = await loginPage.GoToSignUpAsync();
            
            if (!await signUpPage.IsLoadedAsync())
                throw new Exception("Sign up page failed to load");

            await signUpPage.SignUpAsync(username, password);
            
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> TryLoginWithInvalidCredentialsAsync(string username, string password)
    {
        try
        {
            var homePage = GetHomePage();
            await homePage.NavigateAsync();
            
            var loginPage = await homePage.GoToLoginAsync();
            
            if (!await loginPage.IsLoadedAsync())
                return false;

            // This should fail for invalid credentials
            await loginPage.LoginAsync(username, password);
            
            return false; // If we reach here, login unexpectedly succeeded
        }
        catch
        {
            return true; // Expected failure for invalid credentials
        }
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
        // Authentication flow doesn't typically need cleanup
        // but this method is here for consistency with other flows
        await Task.CompletedTask;
    }
}