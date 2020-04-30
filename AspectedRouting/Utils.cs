using System.Collections.Generic;

namespace AspectedRouting
{
    public static class Utils
    {
        public static string Indent(this string s)
        {
            return s.Replace("\n", "\n    ");
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