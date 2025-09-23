using Microsoft.Extensions.Configuration;

namespace ShopWeb.E2E.Tests.Config;

public static class ConfigurationManager
{
    private static IConfiguration? _configuration;
    private static TestSettings? _testSettings;

    public static IConfiguration Configuration
    {
        get
        {
            if (_configuration == null)
                InitializeConfiguration();
            return _configuration!;
        }
    }

    public static TestSettings TestSettings
    {
        get
        {
            if (_testSettings == null)
                LoadTestSettings();
            return _testSettings!;
        }
    }

    private static void InitializeConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(GetConfigDirectory())
            .AddJsonFile("appsettings.tests.json", optional: false, reloadOnChange: true)
            .AddJsonFile("secrets.local.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        _configuration = builder.Build();
    }

    private static void LoadTestSettings()
    {
        _testSettings = new TestSettings();
        Configuration.GetSection("TestSettings").Bind(_testSettings);

        // Override with environment variables
        OverrideWithEnvironmentVariables();
    }

    private static void OverrideWithEnvironmentVariables()
    {
        if (_testSettings == null) return;

        var browser = Environment.GetEnvironmentVariable("BROWSER");
        if (!string.IsNullOrEmpty(browser))
            _testSettings.Browser = browser;

        var headed = Environment.GetEnvironmentVariable("HEADED");
        if (!string.IsNullOrEmpty(headed))
            _testSettings.Headed = headed == "1" || headed.ToLowerInvariant() == "true";


        var traceMode = Environment.GetEnvironmentVariable("TRACE_MODE");
        if (!string.IsNullOrEmpty(traceMode))
            _testSettings.Artifacts.TraceMode = traceMode;

        var baseUrl = Environment.GetEnvironmentVariable("BASE_URL");
        if (!string.IsNullOrEmpty(baseUrl))
            _testSettings.BaseUrl = baseUrl;
    }

    private static string GetConfigDirectory()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var configPath = Path.Combine(currentDirectory, "Config");
        
        if (Directory.Exists(configPath))
            return configPath;

        // Fallback to current directory
        return currentDirectory;
    }

    public static void Reset()
    {
        _configuration = null;
        _testSettings = null;
    }
}