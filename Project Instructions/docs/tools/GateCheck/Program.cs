// tools/GateCheck/Program.cs
// Quality gate que lee artifacts/run-metrics.json y devuelve código de salida !=0 si falla.

using System;
using System.IO;
using System.Text.Json;

public static class GateCheck
{
    public static int Main(string[] args)
    {
        var path = args.Length > 0 ? args[0] : Path.Combine("artifacts", "run-metrics.json");
        if (!File.Exists(path))
        {
            Console.Error.WriteLine($"[GateCheck] Summary not found: {path}");
            return 1;
        }

        double MinPass() => double.TryParse(Environment.GetEnvironmentVariable("GATECHECK_MIN_PASSRATE"), out var v) ? v : 0.90;
        double MaxP95 () => double.TryParse(Environment.GetEnvironmentVariable("GATECHECK_MAX_P95_MS"), out var v) ? v : 12 * 60 * 1000;

        using var s = File.OpenRead(path);
        var root = JsonDocument.Parse(s).RootElement;
        var total = root.TryGetProperty("total", out var t) ? t.GetInt32() : 0;
        var passed = root.TryGetProperty("passed", out var p) ? p.GetInt32() : 0;
        var quarantined = root.TryGetProperty("quarantined", out var q) ? q.GetInt32() : 0;
        
        // Fallback: aceptar 'p95' o 'p95DurationMs'
        var p95 = root.TryGetProperty("p95", out var pEl)
                    ? pEl.GetDouble()
                    : (root.TryGetProperty("p95DurationMs", out var pAlt) ? pAlt.GetDouble() : 0);
        
        var effectiveTotal = Math.Max(1, total - quarantined);
        var passRate = effectiveTotal == 0 ? 1.0 : (double)passed / effectiveTotal;
        var p95 = doc.TryGetProperty("p95", out var p95El) ? p95El.GetDouble() : 0;

        Console.WriteLine($"[GateCheck] thresholds → passRate≥{MinPass():P0}  p95≤{MaxP95()}ms");
        Console.WriteLine($"[GateCheck] measured   → total={total} quarantined={quarantined} passRate={passRate:P2} p95={p95}ms");
        
        var ok = passRate >= MinPass() && p95 <= MaxP95();
        Console.WriteLine($"[GateCheck] result     → {(ok ? "OK" : "FAIL")}");
        return ok ? 0 : 2;
    }
}