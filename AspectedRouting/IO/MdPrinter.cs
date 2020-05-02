using System;
using System.IO;
using System.Linq;
using AspectedRouting.Language;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Typ;

namespace AspectedRouting.IO
{
    /// <summary>
    /// Prints function signatures and profile explanations to file
    /// </summary>
    public static class MdPrinter
    {
        public static void GenerateHelpText(string saveTo = null)
        {
            var helpText = TypeOverview +
                           FunctionOverview;

            if (saveTo == null)
            {
                Console.WriteLine(helpText);
            }
            else
            {
                File.WriteAllText(saveTo, helpText);
            }
        }

        public static string FunctionOverview
        {
            get
            {
                var txt = "## Builtin functions\n\n";
                foreach (var biFunc in Funcs.BuiltinNames)
                {
                    txt += "- " + biFunc + "\n";
                }

                txt += "\n\n";
                txt += "### Function overview\n\n";

                foreach (var (name, func) in Funcs.Builtins)
                {
                    txt += "#### " + name + "\n\n";
                    txt += func.ArgTableHorizontal();

                    txt += "\n\n" + func.Description + "\n\n";
                    try
                    {
                        var lua = File.ReadAllText("IO/lua/" + func.Name + ".lua");
                        txt += $"\n\nLua implementation:\n\n````lua\n{lua}\n````\n\n\n";
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Could not load lua for {func.Name}: {e.Message}");
                    }
                }


                return txt;
            }
        }

        private static string ArgTableHorizontal(this Function f)
        {
            var header = string.Join(" | ", f.ArgNames) + " | returns |";
            var headerLine = string.Join(" | ", f.ArgNames.Select(_ => "---")) + " | --- |";

            var types =
                string.Join("\n",
                    f.Types.Select(t =>
                        string.Join(" | ", t.Uncurry()) + " |"
                    ));

            return string.Join("\n", header, headerLine, types);
        }

        private static string ArgTableVerticalTypes(this Function f)
        {
            try
            {
                var args = f.ArgBreakdown();
                var header = "Argument name | ";
                var line = "--------------- |   ";
                for (int i = 0; i < f.Types.Count(); i++)
                {
                    header += "  |";
                    line += " -- | ";
                }

                var lines = "";
                foreach (var n in f.ArgNames)
                {
                    lines += $"**{n}** | ";
                    foreach (var t in args[n])
                    {
                        lines += (t?.ToString() ?? "_none_") + "\t | ";
                    }

                    lines += "\n";
                }

                lines += $"_return type_ | ";
                foreach (var t in args[""])
                {
                    lines += (t.ToString() ?? "_none_") + "\t | ";
                }

                lines += "\n";

                return $"{header}\n{line}\n{lines}";
            }
            catch (Exception e)
            {
                Console.WriteLine(f.Name + ": " + e.Message);
                return string.Join("\n", f.Types.Select(t => "- " + t));
            }
        }


        public static string TypeOverview
        {
            get
            {
                var txt = "## Types\n\n";
                foreach (var biType in Typs.BuiltinTypes)
                {
                    txt += "- " + biType.Name + "\n";
                }

                return txt;
            }
        }
    }
}