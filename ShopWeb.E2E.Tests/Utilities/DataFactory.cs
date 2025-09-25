namespace ShopWeb.E2E.Tests.Utilities;

/// <summary>
/// Factory for creating deterministic test data
/// Complies with E2E_Policy.md section 4 - Test Data determinista
/// 🔹 Iteración 8 - T-076: Implementar DataFactory para datos deterministas
/// </summary>
public static class DataFactory
{
    /// <summary>
    /// Create a deterministic user for E2E tests
    /// </summary>
    public static TestUser CreateDefaultUser()
    {
        return new TestUser
        {
            Username = "User_E2E1",
            Email = "user.e2e1@example.com",
            Password = "Password123!"
        };
    }

    /// <summary>
    /// Create a deterministic invalid user for negative tests
    /// </summary>
    public static TestUser CreateInvalidUser()
    {
        return new TestUser
        {
            Username = "Invalid_User_E2E",
            Email = "invalid@example.com",
            Password = "WrongPassword!"
        };
    }

    /// <summary>
    /// Create deterministic empty credentials for edge case tests
    /// </summary>
    public static TestUser CreateEmptyCredentials()
    {
        return new TestUser
        {
            Username = "",
            Email = "",
            Password = ""
        };
    }

    /// <summary>
    /// Create a deterministic user with valid credentials for successful login
    /// These credentials match SauceDemo test environment
    /// </summary>
    public static TestUser CreateValidLoginUser()
    {
        return new TestUser
        {
            Username = "standard_user",
            Email = "standard_user@saucedemo.com",
            Password = "secret_sauce"
        };
    }

    /// <summary>
    /// Create a deterministic locked out user for testing locked scenarios
    /// </summary>
    public static TestUser CreateLockedOutUser()
    {
        return new TestUser
        {
            Username = "locked_out_user",
            Email = "locked_out_user@saucedemo.com",
            Password = "secret_sauce"
        };
    }

    /// <summary>
    /// Create a deterministic product for E2E tests
    /// </summary>
    public static TestProduct CreateDefaultProduct()
    {
        return new TestProduct
        {
            Name = "Product_E2E1",
            Price = 50.00m,
            Stock = 5
        };
    }
}

/// <summary>
/// Test user data model
/// </summary>
public class TestUser
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Test product data model
/// </summary>
public class TestProduct
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}