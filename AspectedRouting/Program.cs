using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AspectedRouting.IO;
using AspectedRouting.IO.itinero1;
using AspectedRouting.IO.jsonParser;
using AspectedRouting.Language;
using AspectedRouting.Language.Expression;
using AspectedRouting.Tests;

namespace AspectedRouting
{
    static class Program
    {
        public static List<(AspectMetadata aspect, FunctionTestSuite tests)> ParseAspects(
            this List<string> jsonFileNames, Context context)
        {
            var aspects = new List<(AspectMetadata aspect, FunctionTestSuite tests)>();
            foreach (var file in jsonFileNames)
            {
                var fi = new FileInfo(file);
                var aspect = JsonParser.AspectFromJson(context, File.ReadAllText(file), fi.Name);
                if (aspect != null)
                {
                    var testPath = fi.DirectoryName + "/" + aspect.Name + ".test.csv";
                    FunctionTestSuite tests = null;
                    if (File.Exists(testPath))
                    {
                        tests = FunctionTestSuite.FromString(aspect, File.ReadAllText(testPath));
                    }

                    aspects.Add((aspect, tests));
                }
            }

            return aspects;
        }

        private static LuaPrinter GenerateLua(Context context,
            List<(AspectMetadata aspect, FunctionTestSuite tests)> aspects,
            ProfileMetaData profile, List<ProfileTestSuite> profileTests)
        {
            var luaPrinter = new LuaPrinter(context);
            foreach (var (aspect, tests) in aspects)
            {
                luaPrinter.AddFunction(aspect);
                if (tests != null)
                {
                    luaPrinter.AddTestSuite(tests);
                }
            }

            luaPrinter.AddProfile(profile);


            foreach (var testSuite in profileTests)
            {
                luaPrinter.AddTestSuite(testSuite);
            }


            return luaPrinter;
        }

        private static (ProfileMetaData profile, List<ProfileTestSuite> profileTests) ParseProfile(string profilePath,
            Context context)
        {
            var profile = JsonParser.ProfileFromJson(context, File.ReadAllText(profilePath), new FileInfo(profilePath));
            profile.SanityCheckProfile(context);

            var profileFi = new FileInfo(profilePath);
            var profileTests = new List<ProfileTestSuite>();
            foreach (var behaviourName in profile.Behaviours.Keys)
            {
                var testPath = profileFi.DirectoryName + "/" + profile.Name + "." + behaviourName + ".csv";
                if (File.Exists(testPath))
                {
                    var test = ProfileTestSuite.FromString(context, profile, behaviourName, File.ReadAllText(testPath));
                    profileTests.Add(test);
                }
                else
                {
                    Console.WriteLine($"[{behaviourName}] WARNING: no test found for behaviour");
                }
            }

            return (profile, profileTests);
        }

        public static void Main(string[] args)
        {
            MdPrinter.GenerateHelpText("IO/md/helpText.md");


            var files = Directory.EnumerateFiles("Profiles", "*.json", SearchOption.AllDirectories)
                .ToList();


            var context = new Context();

            var aspects = ParseAspects(files, context);

            foreach (var (aspect, t) in aspects)
            {
                if (t == null)
                {
                    Console.WriteLine($"[{aspect.Name}] WARNING: no tests found: please add {aspect.Name}.test.csv");
                }
                else
                {
                    t.Run();
                }

                context.AddFunction(aspect.Name, aspect);
            }


            var profilePath = "Profiles/bicycle/bicycle.json";
            var (profile, profileTests) = ParseProfile(profilePath, context);

            foreach (var test in profileTests)
            {
                test.Run(context);
            }


            var luaPrinter = GenerateLua(context, aspects, profile, profileTests);

            File.WriteAllText("output.lua", luaPrinter.ToLua());
        }
    }
}