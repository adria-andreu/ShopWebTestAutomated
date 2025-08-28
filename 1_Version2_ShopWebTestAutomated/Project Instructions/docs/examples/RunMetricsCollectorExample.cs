// docs/examples/RunMetricsCollectorExample.cs
// Colector de métricas mínimo: escribe JSONL por test y un resumen JSON al final.
// Artefactos: artifacts/test-metrics.jsonl, artifacts/run-metrics.json

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DocsExamples
{
    /// <summary>
    /// Esqueleto de collector para:
    ///  - Escribir una línea por test en artifacts/test-metrics.jsonl
    ///  - Generar el resumen de ejecución en artifacts/run-metrics.json
    /// Es thread-safe via lock de archivo. Ajusta a tus necesidades.
    /// </summary>
    public static class RunMetricsCollectorExample
    {
        private static readonly string Root = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "artifacts");
        private static readonly string TestMetricsFile = Path.Combine(Root, "test-metrics.jsonl");
        private static readonly string RunMetricsFile = Path.Combine(Root, "run-metrics.json");
        private static readonly object FileLock = new();

        public sealed class TestLine
        {
            public string name { get; set; } = "";
            public string status { get; set; } = "Passed"; // Passed|Failed|Skipped
            public long durationMs { get; set; }
            public string artifactsPath { get; set; } = "";
            public string timestampUtc { get; set; } = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            public string browser { get; set; } = "chromium";
            public string siteId { get; set; } = "A";
            public string commitSha { get; set; } = "";
            public int retries { get; set; } = 0;
        }

        public static void RecordTest(TestLine line)
        {
            Directory.CreateDirectory(Root);
            var json = JsonSerializer.Serialize(line);
            lock (FileLock)
            {
                File.AppendAllText(TestMetricsFile, json + Environment.NewLine);
            }
        }

        /// <summary>
        /// Llama en OneTimeTearDown. Calcula passRate, passRateEffective, flakyRatio y p95.
        /// </summary>
        public static void RecordRunSummary(string? commitSha = null, string? startedAtUtc = null, string? finishedAtUtc = null)
        {
            if (!File.Exists(TestMetricsFile))
                throw new FileNotFoundException("test-metrics.jsonl not found", TestMetricsFile);

            var lines = File.ReadAllLines(TestMetricsFile);
            var tests = new List<TestLine>(capacity: lines.Length);
            foreach (var l in lines)
            {
                if (string.IsNullOrWhiteSpace(l)) continue;
                try { tests.Add(JsonSerializer.Deserialize<TestLine>(l)!); } catch { /* ignore malformed */ }
            }

            int total = tests.Count;
            int passed = tests.Count(t => t.status == "Passed");
            int failed = tests.Count(t => t.status == "Failed");
            int skipped = tests.Count(t => t.status == "Skipped");

            // p95 durations
            var durations = tests.Select(t => t.durationMs).OrderBy(x => x).ToArray();
            long p95 = durations.Length == 0 ? 0 : durations[(int)Math.Ceiling(durations.Length * 0.95) - 1];

            // pass rate
            double passRate = total == 0 ? 0 : (double)passed / total;

            // efectivos: no contar pasados-por-retry como "passed sin retry"
            // (Si necesitas exactitud total, marca un flag en el test line cuando haya pasado tras retry)
            int passedEffective = tests.Count(t => t.status == "Passed" && t.retries == 0);
            double passRateEffective = total == 0 ? 0 : (double)passedEffective / total;

            // flaky de este run: tests que han requerido retry (>0) o con historial externo (no incluido aquí)
            // Para historia de 10 runs, necesitarías un storage histórico (no implementado en ejemplo)
            int flakyInRun = tests.Count(t => t.retries > 0);
            double flakyRatio = total == 0 ? 0 : (double)flakyInRun / total;

            var run = new
            {
                schemaVersion = 1,
                total, passed, failed, skipped,
                passRate,
                passRateEffective,
                flakyRatio,
                p95DurationMs = p95,
                startedAtUtc = startedAtUtc ?? DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                finishedAtUtc = finishedAtUtc ?? DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                commit = commitSha ?? ""
            };

            lock (FileLock)
            {
                File.WriteAllText(RunMetricsFile, JsonSerializer.Serialize(run, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
    }
}
