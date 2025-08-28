## `/docs/examples/builders-examples.cs`

```csharp
// Builders para datos de prueba deterministas y únicos.
// Uso: var user = new UserBuilder().WithEmailPrefix("smk").Build();

using System;
using System.Globalization;

public sealed class User
{
    public string Id { get; init; } = "";
    public string Email { get; init; } = "";
    public string Password { get; init; } = "";
}

public sealed class Product
{
    public string Id { get; init; } = "";
    public string Sku { get; init; } = "";
    public string Name { get; init; } = "";
    public decimal ExpectedPrice { get; init; }
}

public static class TestDataId
{
    public static string New(string prefix)
        => $"{prefix}_{DateTime.UtcNow:yyyyMMdd_HHmm}_{Guid.NewGuid().ToString()[..8]}";
}

public sealed class UserBuilder
{
    private string _prefix = "usr";
    private string _domain = "example.com";
    private string _password = Environment.GetEnvironmentVariable("TEST_USER_PASSWORD") ?? "Passw0rd!";
    private string? _fixedId;

    public UserBuilder WithEmailPrefix(string prefix) { _prefix = prefix; return this; }
    public UserBuilder WithDomain(string domain) { _domain = domain; return this; }
    public UserBuilder WithPassword(string password) { _password = password; return this; }
    public UserBuilder WithFixedId(string id) { _fixedId = id; return this; }

    public User Build()
    {
        var id = _fixedId ?? TestDataId.New(_prefix);
        var email = $"{_prefix}+{id.Split('_')[^1]}@{_domain}".ToLowerInvariant();
        return new User { Id = id, Email = email, Password = _password };
    }
}

public sealed class ProductBuilder
{
    private string _id = "prod_tee_basic_black";
    private string _sku = "TEE-BLK-001";
    private string _name = "Basic Tee Black";
    private decimal _price = 9.99m;

    public ProductBuilder WithId(string id) { _id = id; return this; }
    public ProductBuilder WithSku(string sku) { _sku = sku; return this; }
    public ProductBuilder WithName(string name) { _name = name; return this; }
    public ProductBuilder WithExpectedPrice(decimal price) { _price = price; return this; }

    public Product Build() => new Product { Id = _id, Sku = _sku, Name = _name, ExpectedPrice = _price };
}

/* Ejemplo de uso en un test:
var user = new UserBuilder().WithEmailPrefix("smk").Build();
var product = new ProductBuilder().WithId("prod_cap_red").WithExpectedPrice(14.50m).Build();
*/
```
