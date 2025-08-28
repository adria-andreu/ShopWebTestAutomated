using System;
using System.IO;
using System.Text.Json;

namespace ShopWebTestAutomated.Utilities
{
    /// <summary>
    /// Ejemplo de collector para escribir métricas JSONL por test
    /// y agregadas por suite.
    /// </summary>
    public static class MetricsCollector
    {
        private static readonly string Root = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "artifacts");
        private static readonly string TestMetricsFile = Path.Combine(Root, "test-metrics.jsonl");
        private static readonly string RunMetricsFile  = Path.Combine(Root, "run-metrics.json");

        private static readonly object FileLock = new();

        public static void RecordTest(string name, string status, long durationMs, string artifactsPath)
        {
            var line = JsonSerializer.Serialize(new {
                name,
                status,
                durationMs,
                artifactsPath,
                timestampUtc = DateTime.UtcNow
            });

            lock (FileLock)
            {
                Directory.CreateDirectory(Root);
                File.AppendAllText(TestMetricsFile, line + Environment.NewLine);
            }
        }

        // pseudocódigo → ejecutar en OneTimeTearDown
        public static void RecordRunSummary()
        {
            // leer todas las líneas del test-metrics.jsonl
            var tests = File.ReadAllLines(TestMetricsFile);
            int total = tests.Length;
            int passed = 0, failed = 0, skipped = 0;
            long[] durations = new long[total];

            // parsear cada línea y acumular (pseudo)
            // foreach (line in tests) { ... }

            var run = new {
                total,
                passed,
                failed,
                skipped,
                passRate = (double)passed / total,
                p95DurationMs = CalculateP95(durations),
                startedAtUtc = "...",
                finishedAtUtc = DateTime.UtcNow
            };

            lock (FileLock)
            {
                File.WriteAllText(RunMetricsFile, JsonSerializer.Serialize(run, new JsonSerializerOptions { WriteIndented = true }));
            }
        }

        private static long CalculateP95(long[] values)
        {
            Array.Sort(values);
            int index = (int)Math.Ceiling(0.95 * values.Length) - 1;
            return values.Length > 0 ? values[index] : 0;
        }
    }
}