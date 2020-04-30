using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AspectedRouting.Functions;
using AspectedRouting.IO;

namespace AspectedRouting
{
    class Program
    {
        public static void Main(string[] args)
        {
            var files = Directory.EnumerateFiles("Profiles", "*.json", SearchOption.AllDirectories)
                .ToList();


            var context = new Context();


            MdPrinter.GenerateHelpText("IO/md/helpText.md");

            var testSuites = new List<FunctionTestSuite>();
            var aspects = new List<AspectMetadata>();
            foreach (var file in files)
            {
                var fi = new FileInfo(file);
                var aspect = JsonParser.AspectFromJson(context, File.ReadAllText(file), fi.Name);
                if (aspect != null)
                {
                    aspects.Add(aspect);

                    var testPath = fi.DirectoryName + "/" + aspect.Name + ".test.csv";
                    if (File.Exists(testPath))
                    {
                        var tests = FunctionTestSuite.FromString(aspect, File.ReadAllText(testPath));
                        testSuites.Add(tests);
                    }
                    else
                    {
                        Console.WriteLine($"[{aspect.Name}] No tests found: to directory" );
                    }
                }
            }

            foreach (var aspect in aspects)
            {
                context.AddFunction(aspect.Name, aspect);
            }


            var profilePath = "Profiles/bicycle/bicycle.json";
            var profile = JsonParser.ProfileFromJson(context, File.ReadAllText(profilePath), new FileInfo(profilePath));

            var profileFi = new FileInfo(profilePath);
            var profileTests = new List<ProfileTestSuite>();
            foreach (var profileName in profile.Profiles.Keys)
            {
                var testPath = profileFi.DirectoryName + "/" + profile.Name + "." + profileName + ".csv";
                if (File.Exists(testPath))
                {
                    var test = ProfileTestSuite.FromString(profile, profileName, File.ReadAllText(testPath));
                    profileTests.Add(test);
                }
            }

            profile.SanityCheckProfile();

            var luaPrinter = new LuaPrinter();
            luaPrinter.CreateProfile(profile, context);


            var testCode = "\n\n\n\n\n\n\n\n--------------------------- Test code -------------------------\n\n\n";


            foreach (var testSuite in testSuites)
            {
                testSuite.Run();
                testCode += testSuite.ToLua() + "\n";
            }

            foreach (var testSuite in profileTests)
            {
                testCode += testSuite.ToLua() + "\n";
            }


            // Compatibility code, itinero-transit doesn't know 'print'
            testCode += string.Join("\n",
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
                "if (not failed_tests and not failed_profile_tests) then",
                "    print(\"Tests OK\")",
                "end"
            );

            File.WriteAllText("output.lua", luaPrinter.ToLua() + testCode);
        }
    }
}