namespace ShopWeb.E2E.Tests.Utilities;

public static class DataFactory
{
    public static class SauceDemo
    {
        public static class Users
        {
            public static (string Username, string Password) StandardUser => ("standard_user", "secret_sauce");
            public static (string Username, string Password) LockedUser => ("locked_out_user", "secret_sauce");
            public static (string Username, string Password) ProblemUser => ("problem_user", "secret_sauce");
            public static (string Username, string Password) PerformanceGlitchUser => ("performance_glitch_user", "secret_sauce");
            public static (string Username, string Password) InvalidUser => ("invalid_user", "wrong_password");
            public static (string Username, string Password) EmptyCredentials => ("", "");
        }

        public static class Products
        {
            public static string BackpackName => "Sauce Labs Backpack";
            public static string BikeLightName => "Sauce Labs Bike Light";
            public static string BoltTShirtName => "Sauce Labs Bolt T-Shirt";
            public static string FleeceJacketName => "Sauce Labs Fleece Jacket";
            public static string OnesiteName => "Sauce Labs Onesie";
            public static string TShirtRedName => "Test.allTheThings() T-Shirt (Red)";
        }

        public static class ShippingInfo
        {
            public static (string FirstName, string LastName, string PostalCode) ValidInfo =>
                ("John", "Doe", "12345");

            public static (string FirstName, string LastName, string PostalCode) EmptyInfo =>
                ("", "", "");
        }
    }
}