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
        public static IEnumerable<(AspectMetadata aspect, AspectTestSuite tests)> ParseAspects(
            this IEnumerable<string> jsonFileNames, Context context)
        {
            var aspects = new List<(AspectMetadata aspect, AspectTestSuite tests)>();
            foreach (var file in jsonFileNames)
            {
                var fi = new FileInfo(file);
                Console.WriteLine("Parsing " + file);
                var aspect = JsonParser.AspectFromJson(context, File.ReadAllText(file), fi.Name);
                if (aspect != null)
                {
                    var testPath = fi.DirectoryName + "/" + aspect.Name + ".test.csv";
                    AspectTestSuite tests = null;
                    if (File.Exists(testPath))
                    {
                        tests = AspectTestSuite.FromString(aspect, File.ReadAllText(testPath));
                    }

                    aspects.Add((aspect, tests));
                }
            }

            return aspects;
        }

        private static LuaPrinter GenerateLua(Context context,
            IEnumerable<(AspectMetadata aspect, AspectTestSuite tests)> aspects,
            ProfileMetaData profile, List<ProfileTestSuite> profileTests)
        {
            var luaPrinter = new LuaPrinter(context);

            var usedFunctions = profile.CalledFunctionsRecursive(context).Values.SelectMany(v => v).ToHashSet();
            
            foreach (var (aspect, tests) in aspects)
            {
                if (!usedFunctions.Contains(aspect.Name))
                {
                    continue;
                }
                
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

        private static IEnumerable<(ProfileMetaData profile, List<ProfileTestSuite> profileTests)> ParseProfiles(
            IEnumerable<string> jsonFiles, Context context)
        {
            var result = new List<(ProfileMetaData profile, List<ProfileTestSuite> profileTests)>();
            foreach (var jsonFile in jsonFiles)
            {
                try
                {
                    var profile =
                        JsonParser.ProfileFromJson(context, File.ReadAllText(jsonFile), new FileInfo(jsonFile));
                    if (profile == null)
                    {
                        continue;
                    }

                    profile.SanityCheckProfile(context);

                    var profileFi = new FileInfo(jsonFile);
                    var profileTests = new List<ProfileTestSuite>();
                    foreach (var behaviourName in profile.Behaviours.Keys)
                    {
                        var path = profileFi.DirectoryName + "/" + profile.Name + "." + behaviourName + ".behaviour_test.csv";
                        if (File.Exists(path))
                        {
                            var test = ProfileTestSuite.FromString(context, profile, behaviourName,
                                File.ReadAllText(path));
                            profileTests.Add(test);
                        }
                        else
                        {
                            Console.WriteLine($"[{profile.Name}] WARNING: no test found for behaviour {behaviourName}");
                        }
                    }

                    result.Add((profile, profileTests));
                }
                catch (Exception e)
                {
                    // PrintError(jsonFile, e);
                    throw new Exception("In the file " + jsonFile, e);
                }
            }

            return result;
        }

        private static void PrintError(string file, Exception exception)
        {
            var msg = exception.Message;
            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
                msg += "\n    " + exception.Message;
            }

            Console.WriteLine($"Error in the file {file}:\n    {msg}");
        }

        public static void Main(string[] args)
        {

            if (args.Length < 2)
            {
                Console.WriteLine("Usage: <directory where all aspects and profiles can be found> <outputdirectory>");
                return;
            }

            var inputDir = args[0];
            var outputDir = args[1];
            
            
            MdPrinter.GenerateHelpText(outputDir + "helpText.md");


            var files = Directory.EnumerateFiles(inputDir, "*.json", SearchOption.AllDirectories)
                .ToList();
            
            var context = new Context();

            var aspects = ParseAspects(files, context);

            foreach (var (aspect, _) in aspects)
            {
                context.AddFunction(aspect.Name, aspect);
            }

            var profiles = ParseProfiles(files, context);


            // With everything parsed and typechecked, time for tests
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
            }

            foreach (var (profile, profileTests) in profiles)
            {
                foreach (var test in profileTests)
                {
                    test.Run(context);
                }

                var luaPrinter = GenerateLua(context, aspects, profile, profileTests);
                File.WriteAllText(outputDir + "/" + profile.Name + ".lua", luaPrinter.ToLua());
            }
        }
    }
}