using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Functions;

namespace AspectedRouting.IO
{
    public static class JsonPrinter
    {
        private static string Objs(params (string k, string v)[] stuff)
        {
            return "{\n" + string.Join("\n", stuff.Where(kv => kv.v != null).Select(kv => kv.k.Dq() + ": " + kv.v)).Indent() + "\n}";
        }

        private static string Arr(IEnumerable<string> strs)
        {
            return "[" + string.Join(", ", strs) + "]";
        }

        private static string Dq(this string s)
        {
            if (s == null)
            {
                return null;
            }

            return '"' + s + '"';
        }

        public static string Print(AspectMetadata meta)
        {
            return Objs(
                ("name", meta.Name.Dq()),
                ("description", meta.Description.Dq()),
                ("unit", meta.Unit.Dq()),
                ("author", meta.Author.Dq()),
                ("file", meta.Filepath.Dq()),
                ("type", Arr(meta.Types.Select(tp => tp.ToString().Dq()))),
                ("value", meta.ExpressionImplementation.ToString())
            );
        }
    }
}