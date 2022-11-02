using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace AspectedRouting.IO.itinero1
{
    public static class LuaStringExtensions
    {

        public static string ToLuaTable(this Dictionary<string, string> tags)
        {
            var contents = tags.Select(kv =>
            {
                var (key, value) = kv;
                var left = "[\"" + key .ToString(CultureInfo.InvariantCulture)+ "\"]";

                if (Regex.IsMatch(key, "^[a-zA-Z][_a-zA-Z-9]*$"))
                {
                    left = key;
                }

                return $"{left.ToString(CultureInfo.InvariantCulture)} = \"{value.ToString(CultureInfo.InvariantCulture)}\"";
            });
            return "{" + string.Join(", ", contents) + "}";
        }

        public static string AsLuaIdentifier(this string s)
        {
            return s.Replace("$", "")
                .Replace("#", "")
                .Replace(" ", "_")
                .Replace(".", "_")
                .Replace("-", "_");
        }
    }
}