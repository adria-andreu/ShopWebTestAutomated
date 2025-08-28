using Microsoft.Playwright;

namespace ShopWeb.E2E.Tests.Pages;

public interface ILoginPage
{
    Task<bool> IsLoadedAsync();
    Task<IHomePage> LoginAsync(string username, string password);
    Task<string> GetErrorMessageAsync();
    Task<ISignUpPage> GoToSignUpAsync();
}

public interface ISignUpPage
{
    Task<bool> IsLoadedAsync();
    Task<IHomePage> SignUpAsync(string username, string password);
    Task<string> GetErrorMessageAsync();
    Task<ILoginPage> GoToLoginAsync();
}