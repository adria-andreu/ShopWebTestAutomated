// docs/examples/GateCheckExample.cs
// Quality gate sencillo: lee artifacts/run-metrics.json y hace fail si no se cumple el umbral.

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

        using var s = File.OpenRead(path);
        var doc = JsonDocument.Parse(s);
        var passRate = doc.RootElement.GetProperty("passRate").GetDouble();
        var p95 = doc.RootElement.GetProperty("p95").GetDouble();

        var minPassRate = 0.90;     // 90%
        var maxP95Ms = 12 * 60 * 1000; // 12 minutos en ms

        var ok = passRate >= minPassRate && p95 <= maxP95Ms;
        Console.WriteLine($"[GateCheck] passRate={passRate:P2} p95={p95}ms → {(ok ? "OK" : "FAIL")}");
        return ok ? 0 : 2;
    }
}
