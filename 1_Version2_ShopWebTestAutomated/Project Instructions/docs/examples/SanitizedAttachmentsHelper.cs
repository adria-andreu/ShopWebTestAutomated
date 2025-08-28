// docs/examples/SanitizedAttachmentsHelper.cs.txt
// Helper para generar artefactos saneados (HTML/console) y adjuntarlos a Allure.
// Uso típico en TearDown cuando el test falla:
//
//   var (safeHtml, safeConsole, screenshot, trace) = await SanitizedAttachmentsHelper.WriteSanitizedAndAttachAsync(
//       page: _page,
//       consoleLogText: _consoleLog.ToString(),
//       artifactsDir: artifactsDir,
//       captureScreenshot: true,
//       attachTrace: true,
//       traceZipPath: traceZipPath,          // si ya lo tienes generado
//       sanitize: true                       // o controla con env var SANITIZE_ATTACHMENTS
//   );
//
// Requisitos:
// - Clase Redactor en namespace ShopWebTestAutomated.Utilities.Security.
// - Paquetes Allure: Allure.NUnit / Allure.Commons
// - Microsoft.Playwright para IPage

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Allure.Commons;
using Microsoft.Playwright;
using ShopWebTestAutomated.Utilities.Security;

namespace ShopWebTestAutomated.Utilities.Attachments
{
    public static class SanitizedAttachmentsHelper
    {
        /// <summary>
        /// Escribe page.html y console.log.txt RAW, genera copias saneadas y adjunta las saneadas a Allure.
        /// Opcionalmente captura screenshot y adjunta trace si se indica.
        /// Devuelve rutas de los ficheros generados (saneados) y, si procede, screenshot y trace.
        /// </summary>
        public static async Task<(string safeHtml, string safeConsole, string? screenshot, string? trace)>
            WriteSanitizedAndAttachAsync(
                IPage page,
                string consoleLogText,
                string artifactsDir,
                bool captureScreenshot = true,
                bool attachTrace = false,
                string? traceZipPath = null,
                bool sanitize = true)
        {
            Directory.CreateDirectory(artifactsDir);

            // Rutas base
            var rawHtmlPath      = Path.Combine(artifactsDir, "page.html");
            var rawConsolePath   = Path.Combine(artifactsDir, "console.log.txt");
            var safeHtmlPath     = Path.Combine(artifactsDir, "page.sanitized.html");
            var safeConsolePath  = Path.Combine(artifactsDir, "console.sanitized.log.txt");
            string? screenshot   = null;
            string? tracePath    = null;

            // 1) Generar artefactos RAW (no adjuntar estos fuera del dominio)
            try
            {
                // HTML de la página
                try
                {
                    var html = await page.ContentAsync();
                    File.WriteAllText(rawHtmlPath, html, Encoding.UTF8);
                }
                catch { /* si la página está cerrada o falla, seguimos */ }

                // Consola (pasar el StringBuilder a string antes)
                try
                {
                    File.WriteAllText(rawConsolePath, consoleLogText ?? string.Empty, Encoding.UTF8);
                }
                catch { /* ignorar errores de disco */ }
            }
            catch { /* nunca tirar el teardown por esto */ }

            // 2) Generar artefactos SANEADOS
            var doSanitize = sanitize;
            try
            {
                // Permite controlar desde entorno: SANITIZE_ATTACHMENTS=true|false
                var envToggle = Environment.GetEnvironmentVariable("SANITIZE_ATTACHMENTS");
                if (!string.IsNullOrWhiteSpace(envToggle))
                    doSanitize = envToggle.Equals("true", StringComparison.OrdinalIgnoreCase);
            }
            catch { /* ignore */ }

            try
            {
                if (doSanitize)
                {
                    Redactor.SanitizeFile(rawHtmlPath,    safeHtmlPath);
                    Redactor.SanitizeFile(rawConsolePath, safeConsolePath);
                }
                else
                {
                    // Si se desactiva la sanitización, copiamos RAW → SAFE (pero mantenemos el sufijo para dejar claro qué adjuntamos)
                    TryCopy(rawHtmlPath,    safeHtmlPath);
                    TryCopy(rawConsolePath, safeConsolePath);
                }
            }
            catch { /* ignore */ }

            // 3) Screenshot opcional
            if (captureScreenshot)
            {
                try
                {
                    screenshot = Path.Combine(artifactsDir, "screenshot.png");
                    await page.ScreenshotAsync(new() { Path = screenshot, FullPage = true });
                }
                catch { /* ignore */ }
            }

            // 4) Trace opcional (NO se sanitiza: adjuntar solo en CI privado o controlado)
            if (attachTrace && !string.IsNullOrWhiteSpace(traceZipPath) && File.Exists(traceZipPath))
            {
                tracePath = traceZipPath;
            }

            // 5) Adjuntar a Allure (solo *SANEADOS* + binarios permitidos)
            try
            {
                var lifecycle = AllureLifecycle.Instance;

                if (File.Exists(safeHtmlPath))
                    lifecycle.AddAttachment("Page HTML (sanitized)", "text/html", safeHtmlPath, "html");

                if (File.Exists(safeConsolePath))
                    lifecycle.AddAttachment("Console log (sanitized)", "text/plain", safeConsolePath, "txt");

                if (!string.IsNullOrWhiteSpace(screenshot) && File.Exists(screenshot))
                    lifecycle.AddAttachment("Screenshot on failure", "image/png", screenshot, "png");

                // Control opcional por env var para adjuntar trace
                var attachTraceEnv = Environment.GetEnvironmentVariable("ATTACH_TRACE");
                var allowTrace = string.IsNullOrWhiteSpace(attachTraceEnv)
                                 ? attachTrace
                                 : attachTraceEnv.Equals("true", StringComparison.OrdinalIgnoreCase);

                if (allowTrace && !string.IsNullOrWhiteSpace(tracePath) && File.Exists(tracePath))
                    lifecycle.AddAttachment("Playwright trace", "application/zip", tracePath, "zip");
            }
            catch { /* nunca romper teardown por adjuntos */ }

            return (safeHtmlPath, safeConsolePath, screenshot, tracePath);
        }

        private static void TryCopy(string src, string dst)
        {
            try
            {
                if (File.Exists(src))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(dst)!);
                    File.Copy(src, dst, overwrite: true);
                }
            }
            catch { /* ignore */ }
        }
    }
}
