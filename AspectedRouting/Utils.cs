using System.Collections.Generic;

namespace AspectedRouting
{
    public static class Utils
    {
        public static string Indent(this string s)
        {
            return s.Replace("\n", "\n    ");
        }

        public static List<T> InList<T>(this T t)
        {
            return new List<T> {t};
        }

        public static string Lined(this IEnumerable<string> lines)
        {
            return string.Join("\n", lines);
        }
        
        public static int Multiply(this IEnumerable<int> ints)
        {
            var factor = 1;
            foreach (var i in ints)
            {
                factor += i;
            }

            return factor;
        }


    }
}