+namespace ShopWebTestAutomated.Portability;
+
+public interface ISiteProfile
+{
+    string SiteId { get; }
+    string BaseUrl { get; }
+    string CurrencySymbol { get; }
+}
diff --git a/ShopWebTestAutomated/Portability/SiteRegistry.cs b/ShopWebTestAutomated/Portability/SiteRegistry.cs
new file mode 100644
--- /dev/null
+++ b/ShopWebTestAutomated/Portability/SiteRegistry.cs
@@ -0,0 +1,27 @@
+using System;
+using System.Collections.Generic;
+
+namespace ShopWebTestAutomated.Portability;
+
+public static class SiteRegistry
+{
+    private static readonly IReadOnlyDictionary<string, Func<ISiteProfile>> Map =
+        new Dictionary<string, Func<ISiteProfile>>(StringComparer.OrdinalIgnoreCase)
+        {
+            ["A"] = () => new SiteA(),
+            ["B"] = () => new SiteB()
+        };
+
+    public static ISiteProfile Resolve(string siteId) =>
+        Map.TryGetValue(siteId, out var f) ? f() : throw new ArgumentException($"Unknown site: {siteId}");
+}
+
+file sealed class SiteA : ISiteProfile
+{
+    public string SiteId => "A";
+    public string BaseUrl => "https://a.example/";
+    public string CurrencySymbol => "€";
+}
+
+file sealed class SiteB : ISiteProfile
+{
+    public string SiteId => "B";
+    public string BaseUrl => "https://b.example/";
+    public string CurrencySymbol => "$";
+}
