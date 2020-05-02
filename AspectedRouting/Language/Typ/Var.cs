using System;
using System.Collections.Generic;

namespace AspectedRouting.Language.Typ
{
    public class Var : Type
    {
        public Var(string name) : base("$" + name, false)
        {
        }


        public static Type Fresh(Type tp)
        {
            return Fresh(tp.UsedVariables());
        }

        public static Type Fresh(HashSet<string> blacklist)
        {
            foreach (var str in AllStrings())
            {
                if (!blacklist.Contains("$" + str))
                {
                    return new Var(str);
                }
            }

            throw new Exception("Fallen out of the infinte loop");
        }

        private static IEnumerable<string> AllStrings()
        {
            var l = 1;
            while (true)
            {
                foreach (var str in AllStringOfLength(l))
                {
                    yield return str;
                }

                l++;
            }
            // ReSharper disable once IteratorNeverReturns
        }

        private static IEnumerable<string> AllStringOfLength(int stringLength = 1)
        {
            while (true)
            {
                if (stringLength == 0)
                {
                    yield return "";
                }

                if (stringLength == 1)
                {
                    foreach (var chr in abc)
                    {
                        yield return chr;
                    }
                }
                else
                {
                    foreach (var chr in abc)
                    {
                        foreach (var postfix in AllStringOfLength(stringLength - 1))
                        {
                            yield return chr + postfix;
                        }
                    }
                }
            }

            // ReSharper disable once IteratorNeverReturns
        }

        private static List<string> abc = new List<string>
        {
            "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u",
            "v", "w", "x", "y", "z"
        };
    }
}