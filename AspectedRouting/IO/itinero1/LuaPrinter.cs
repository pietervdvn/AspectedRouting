using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AspectedRouting.IO.itinero1
{
    public partial class LuaPrinter
    {
        private readonly HashSet<string> _dependencies = new HashSet<string>();
        private readonly HashSet<string> _neededKeys = new HashSet<string>();

        private readonly List<string> _code = new List<string>();
        private readonly List<string> _tests = new List<string>();

        /// <summary>
        /// A dictionary containing the implementation of basic functions
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<string> LoadFunctions(List<string> names)
        {
            var imps = new List<string>();

            foreach (var name in names)
            {
                var path = $"IO/lua/{name}.lua";
                if (File.Exists(path))
                {
                    imps.Add(File.ReadAllText(path));
                }
                else
                {
                    throw new FileNotFoundException(path);
                }
            }

            return imps;
        }

        private void AddDep(string name)
        {
            _dependencies.Add(name);
        }


        public string ToLua()
        {
            var deps = _dependencies.ToList();
            deps.Add("unitTestProfile");
            deps.Add("inv");
            deps.Add("double_compare");

            var code = new List<string>();

            code.Add($"-- Itinero 1.0-profile, generated on {DateTime.Now:s}");
            code.Add("\n\n----------------------------- UTILS ---------------------------");
            code.AddRange(LoadFunctions(deps).ToList());

            code.Add("\n\n----------------------------- PROFILE ---------------------------");
            var keys = _neededKeys.Select(key => "\"" + key + "\"");
            code.Add("\n\nprofile_whitelist = {\n    " + string.Join("\n    , ", keys) + "}");

            code.AddRange(_code);

            code.Add("\n\n ------------------------------- TESTS -------------------------");

            code.Add("function test_all()");
            code.Add(string.Join("\n",_tests).Indent());
            code.Add("end");

            var compatibility = string.Join("\n",
                
                
                "",
                "if (itinero == nil) then",
                "    itinero = {}",
                "    itinero.log = print",
                "",
                "    -- Itinero is not defined -> we are running from a lua interpreter -> the tests are intended",
                "    runTests = true",
                "",
                "",
                "else",
                "    print = itinero.log",
                "end",
                "",
                "test_all()",
                "if (not failed_tests and not failed_profile_tests) then",
                "    print(\"Tests OK\")",
                "end"
            );
            code.Add(compatibility);

            return string.Join("\n\n\n", code);
        }
    }
}