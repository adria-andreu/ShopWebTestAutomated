using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ShopWebTestAutomated.Utilities.Security
{
    public static class Redactor
    {
        // === Patrones comunes (compilados) ===
        static readonly Regex Email = new(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}", RegexOptions.Compiled);
        static readonly Regex Jwt = new(@"\beyJ[0-9A-Za-z_-]{10,}\.[0-9A-Za-z_-]{10,}\.[0-9A-Za-z_-]{10,}\b", RegexOptions.Compiled);
        static readonly Regex Bearer = new(@"(?i)\bBearer\s+[A-Za-z0-9._-]{20,}\b", RegexOptions.Compiled);
        static readonly Regex BasicAuthInUrl = new(@"(?i)\bhttps?://([^:@/\s]+):([^@/\s]+)@", RegexOptions.Compiled);
        static readonly Regex ApiKeyJson = new(@"(?i)(""|')?(api[_-]?key|client[_-]?secret|secret|authorization|token|access[_-]?token|refresh[_-]?token)(""|')?\s*:\s*(""|')[^""']+(""|')", RegexOptions.Compiled);
        static readonly Regex PasswordJson = new(@"(?i)(""|')?password(""|')?\s*:\s*(""|')[^""']+(""|')", RegexOptions.Compiled);
        static readonly Regex CookiePair = new(@"(?i)\b(Set-Cookie|Cookie):\s*([A-Za-z0-9_-]+)=([^;]+)", RegexOptions.Compiled);

        // Candidatos a CC: 13-19 dígitos con separadores, luego validamos por Luhn
        static readonly Regex CreditCardCandidate = new(@"\b(?:\d[ -]*?){13,19}\b", RegexOptions.Compiled);

        public static string Sanitize(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Emails
            var s = Email.Replace(input, "<redacted@email>");
            // JWT y Bearer
            s = Jwt.Replace(s, "<redacted-jwt>");
            s = Bearer.Replace(s, "Bearer <redacted-token>");
            // User:Pass en URL
            s = BasicAuthInUrl.Replace(s, m => m.Value.Replace(m.Groups[1].Value + ":" + m.Groups[2].Value, "<redacted>:<redacted>"));
            // Campos JSON sensibles
            s = ApiKeyJson.Replace(s, m => RedactJsonValue(m.Value));
            s = PasswordJson.Replace(s, m => RedactJsonValue(m.Value));
            // Cookies
            s = CookiePair.Replace(s, m => $"{m.Groups[1].Value}: {m.Groups[2].Value}=<redacted-cookie>");
            // Tarjetas de crédito (Luhn)
            s = CreditCardCandidate.Replace(s, m => IsLikelyCreditCard(m.Value) ? "<redacted-cc>" : m.Value);

            return s;
        }

        public static void SanitizeFile(string inputPath, string outputPath, long maxBytes = 5_000_000)
        {
            if (!File.Exists(inputPath)) return;

            var length = new FileInfo(inputPath).Length;
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

            // Ficheros pequeños → memoria; grandes → streaming línea a línea
            if (length <= maxBytes)
            {
                var text = File.ReadAllText(inputPath, Encoding.UTF8);
                File.WriteAllText(outputPath, Sanitize(text), Encoding.UTF8);
            }
            else
            {
                using var sr = new StreamReader(inputPath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
                using var sw = new StreamWriter(outputPath, false, Encoding.UTF8);
                string? line;
                while ((line = sr.ReadLine()) is not null)
                    sw.WriteLine(Sanitize(line));
            }
        }

        private static string RedactJsonValue(string pair)
        {
            // Reemplaza todo lo que está después de los dos puntos por "<redacted>"
            var idx = pair.IndexOf(':');
            if (idx < 0) return pair;
            return pair.Substring(0, idx + 1) + " \"<redacted>\"";
        }

        private static bool IsLikelyCreditCard(string digitsWithSep)
        {
            // Normaliza a dígitos
            var digits = new string(digitsWithSep.Where(char.IsDigit).ToArray());
            if (digits.Length < 13 || digits.Length > 19) return false;
            return PassesLuhn(digits);
        }

        private static bool PassesLuhn(string digits)
        {
            int sum = 0; bool alt = false;
            for (int i = digits.Length - 1; i >= 0; i--)
            {
                int n = digits[i] - '0';
                if (alt) { n *= 2; if (n > 9) n -= 9; }
                sum += n; alt = !alt;
            }
            return sum % 10 == 0;
        }
    }
}
