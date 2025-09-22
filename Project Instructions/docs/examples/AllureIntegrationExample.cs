// docs/examples/AllureIntegrationExample.cs
// Integración Allure para NUnit + adjuntos (screenshots, traces)

// Requiere paquete Allure.Commons + adaptador NUnit (o equivalente)
// dotnet add package Allure.NUnit --version <latest>
// dotnet add package Allure.Commons --version <latest>

using System.IO;
using Allure.Commons;
using NUnit.Framework;

namespace DocsExamples
{
    /// <summary>
    /// Ejemplo de cómo adjuntar artefactos y labels mínimos a Allure.
    /// Llamar desde TearDown del BaseTest.
    /// </summary>
    public static class AllureIntegrationExample
    {
        public static void AttachArtifactsAndLabels(string artifactsPath, string browser, string siteId, string commitSha, string pipelineId)
        {
            var lifecycle = AllureLifecycle.Instance;

            // Attachments mínimos
            var trace = Path.Combine(artifactsPath, "trace.zip");
            if (File.Exists(trace))
                lifecycle.AddAttachment("Playwright trace", "application/zip", trace, "zip");

            var screenshot = Path.Combine(artifactsPath, "screenshot.png");
            if (File.Exists(screenshot))
                lifecycle.AddAttachment("Screenshot on failure", "image/png", screenshot, "png");

            var html = Path.Combine(artifactsPath, "page.html");
            if (File.Exists(html))
                lifecycle.AddAttachment("Page HTML", "text/html", html, "html");

            var console = Path.Combine(artifactsPath, "console.log.txt");
            if (File.Exists(console))
                lifecycle.AddAttachment("Console log", "text/plain", console, "txt");

            // Labels obligatorios (browser, siteId, commit, pipeline)
            lifecycle.UpdateTestCase(test =>
            {
                test.labels.Add(Label.Feature(siteId));              // o Label.Owner/Layer/etc según convención
                test.labels.Add(Label.Host(browser));                // reutilizamos Host para browser si no tienes label custom
                test.labels.Add(Label.Build(pipelineId));
                test.labels.Add(new Label { name = "commitSha", value = commitSha }); // label custom
		test.labels.Add(new Label { name = "type", value = "e2e" });
            });
        }
    }
}
