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
            this IEnumerable<string> jsonFileNames, List<string> testFileNames, Context context)
        {
            var aspects = new List<(AspectMetadata aspect, AspectTestSuite tests)>();
            foreach (var file in jsonFileNames)
            {
                var fi = new FileInfo(file);
                Console.WriteLine("Parsing " + file);
                var aspect = JsonParser.AspectFromJson(context, File.ReadAllText(file), fi.Name);
                if (aspect == null) continue;


                var testName = aspect.Name + ".test.csv";
                var testPath = testFileNames.FindTest(testName);
                AspectTestSuite tests = null;
                if (!string.IsNullOrEmpty(testPath) && File.Exists(testPath))
                {
                    tests = AspectTestSuite.FromString(aspect, File.ReadAllText(testPath));
                }

                aspects.Add((aspect, tests));
            }

            return aspects;
        }

        private static string FindTest(this IEnumerable<string> testFileNames, string testName)
        {
            var testPaths = testFileNames.Where(nm => nm.EndsWith(testName)).ToList();
            if (testPaths.Count > 1)
            {
                Console.WriteLine("[WARNING] Multiple tests found for " + testName + ", using only one arbitrarily");
            }

            if (testPaths.Count > 0)
            {
                return testPaths.First();
            }

            return null;
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
            IEnumerable<string> jsonFiles, IReadOnlyCollection<string> testFiles, Context context)
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

                    var profileTests = new List<ProfileTestSuite>();
                    foreach (var behaviourName in profile.Behaviours.Keys)
                    {
                        var path = testFiles.FindTest($"{profile.Name}.{behaviourName}.behaviour_test.csv");
                        if (path != null && File.Exists(path))
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

        private static void Repl(Context c, ProfileMetaData profile)
        {
            var behaviour = profile.Behaviours.Keys.First();
            do
            {
                Console.Write(profile.Name + "." + behaviour + " > ");
                var read = Console.ReadLine();
                if (read == null)
                {
                    return; // End of stream has been reached
                }

                if (read.Equals("quit"))
                {
                    return;
                }

                if (read.StartsWith("select"))
                {
                    var beh = read.Substring("select".Length + 1).Trim();
                    if (profile.Behaviours.ContainsKey(beh))
                    {
                        behaviour = beh;
                        Console.WriteLine("Switched to " + beh);
                    }
                    else
                    {
                        Console.WriteLine("Behaviour not found. Known behaviours are:\n   " +
                                          string.Join("\n   ", profile.Behaviours.Keys));
                    }


                    continue;
                }

                var tagsRaw = read.Split(";").Select(s => s.Trim());
                var tags = new Dictionary<string, string>();
                foreach (var str in tagsRaw)
                {
                    var strSplit = str.Split("=");
                    var k = strSplit[0].Trim();
                    var v = strSplit[1].Trim();
                    tags[k] = v;
                }

                try
                {
                    var result = profile.Run(c, behaviour, tags);
                    Console.WriteLine(result);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            } while (true);
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

            var tests = Directory.EnumerateFiles(inputDir, "*test.csv", SearchOption.AllDirectories)
                .ToList();

            var context = new Context();

            var aspects = ParseAspects(files, tests, context);

            foreach (var (aspect, _) in aspects)
            {
                context.AddFunction(aspect.Name, aspect);
            }

            var profiles = ParseProfiles(files, tests, context);


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

            Repl(context,
                profiles.First(p => p.profile.Name.Equals("bicycle")).profile);
        }
    }
}