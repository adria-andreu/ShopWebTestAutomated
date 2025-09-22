+using Microsoft.Playwright;
+using System.Threading.Tasks;
+
+namespace ShopWebTestAutomated.Utilities;
+
+public static class Waits
+{
+    public static Task Visible(ILocator locator) =>
+        locator.WaitForAsync(new() { State = WaitForSelectorState.Visible });
+
+    public static Task Hidden(ILocator locator) =>
+        locator.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
+
+    public static Task Attached(ILocator locator) =>
+        locator.WaitForAsync(new() { State = WaitForSelectorState.Attached });
+}
