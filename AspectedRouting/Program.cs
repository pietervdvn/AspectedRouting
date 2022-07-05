using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AspectedRouting.IO;
using AspectedRouting.IO.itinero1;
using AspectedRouting.IO.itinero2;
using AspectedRouting.IO.jsonParser;
using AspectedRouting.IO.md;
using AspectedRouting.Language;
using AspectedRouting.Language.Expression;
using AspectedRouting.Tests;

namespace AspectedRouting
{
    public static class Program
    {
        public static List<(AspectMetadata aspect, AspectTestSuite tests)> ParseAspects(
            this IEnumerable<string> jsonFileNames, List<string> testFileNames, Context context)
        {
            var aspects = new List<(AspectMetadata aspect, AspectTestSuite tests)>();
            foreach (var file in jsonFileNames) {
                var fi = new FileInfo(file);

                var aspect = JsonParser.AspectFromJson(context, File.ReadAllText(file), fi.Name);
                if (aspect == null) {
                    continue;
                }


                var testName = aspect.Name + ".test.csv";
                var testPath = testFileNames.FindTest(testName);
                AspectTestSuite tests = null;
                if (!string.IsNullOrEmpty(testPath) && File.Exists(testPath)) {
                    tests = AspectTestSuite.FromString(aspect, File.ReadAllText(testPath));
                }

                aspects.Add((aspect, tests));
            }

            return aspects;
        }

        private static string FindTest(this IEnumerable<string> testFileNames, string testName)
        {
            var testPaths = testFileNames.Where(nm => nm.EndsWith(testName)).ToList();
            if (testPaths.Count > 1) {
                Console.WriteLine("[WARNING] Multiple tests found for " + testName + ", using only one arbitrarily");
            }

            if (testPaths.Count > 0) {
                return testPaths.First();
            }

            return null;
        }


        private static List<(ProfileMetaData profile, List<BehaviourTestSuite> profileTests)> ParseProfiles(
            IEnumerable<string> jsonFiles, IReadOnlyCollection<string> testFiles, Context context, DateTime lastChange)
        {
            var result = new List<(ProfileMetaData profile, List<BehaviourTestSuite> profileTests)>();
            foreach (var jsonFile in jsonFiles) {
                try {
                    var profile =
                        JsonParser.ProfileFromJson(context, File.ReadAllText(jsonFile), new FileInfo(jsonFile),
                            lastChange);
                    if (profile == null) {
                        continue;
                    }

                    profile.SanityCheckProfile(context);

                    var profileTests = new List<BehaviourTestSuite>();
                    foreach (var behaviourName in profile.Behaviours.Keys) {
                        var path = testFiles.FindTest($"{profile.Name}.{behaviourName}.behaviour_test.csv");
                        if (path != null && File.Exists(path)) {
                            var test = BehaviourTestSuite.FromString(context, profile, behaviourName,
                                File.ReadAllText(path));
                            profileTests.Add(test);
                        }
                        else {
                            Console.WriteLine($"[{profile.Name}] WARNING: no test found for behaviour {behaviourName}");
                        }
                    }

                    result.Add((profile, profileTests));
                }
                catch (Exception e) {
                    // PrintError(jsonFile, e);
                    throw new Exception("In the file " + jsonFile, e);
                }
            }

            return result;
        }

        private static void Repl(Context c, Dictionary<string, ProfileMetaData> profiles)
        {
            var profile = profiles["emergency_vehicle"];
            var behaviour = profile.Behaviours.Keys.First();
            do {
                Console.Write(profile.Name + "." + behaviour + " > ");
                var read = Console.ReadLine();
                if (read == null) {
                    return; // End of stream has been reached
                }

                if (read == "") {
                    Console.WriteLine("looƆ sᴉ dɐWʇǝǝɹʇSuǝdO");
                    continue;
                }

                if (read.Equals("quit")) {
                    return;
                }

                if (read.Equals("help")) {
                    Console.WriteLine(
                        Utils.Lines(
                            "select <behaviourName> to change behaviour or <vehicle.behaviourName> to change vehicle",
                            ""));
                    continue;
                }

                if (read.Equals("clear")) {
                    for (var i = 0; i < 80; i++) {
                        Console.WriteLine();
                    }

                    continue;
                }

                if (read.StartsWith("select")) {
                    var beh = read.Substring("select".Length + 1).Trim();

                    if (beh.Contains(".")) {
                        var profileName = beh.Split(".")[0];
                        if (!profiles.TryGetValue(profileName, out var newProfile)) {
                            Console.Error.WriteLine("Profile " + profileName + " not found, ignoring");
                            continue;
                        }

                        profile = newProfile;
                        beh = beh.Substring(beh.IndexOf(".") + 1);
                    }

                    if (profile.Behaviours.ContainsKey(beh)) {
                        behaviour = beh;
                        Console.WriteLine("Switched to " + beh);
                    }
                    else {
                        Console.WriteLine("Behaviour not found. Known behaviours are:\n   " +
                                          string.Join("\n   ", profile.Behaviours.Keys));
                    }


                    continue;
                }

                var tagsRaw = read.Split(";").Select(s => s.Trim());
                var tags = new Dictionary<string, string>();
                foreach (var str in tagsRaw) {
                    if (str == "") {
                        continue;
                    }

                    var strSplit = str.Split("=");
                    var k = strSplit[0].Trim();
                    var v = strSplit[1].Trim();
                    tags[k] = v;
                }

                try {
                    var result = profile.Run(c, behaviour, tags);
                    Console.WriteLine(result);
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                    Console.WriteLine(e.Message);
                }
            } while (true);
        }

        private static void PrintError(string file, Exception exception)
        {
            var msg = exception.Message;
            while (exception.InnerException != null) {
                exception = exception.InnerException;
                msg += "\n    " + exception.Message;
            }

            Console.WriteLine($"Error in the file {file}:\n    {msg}");
        }

        private static void PrintUsedTags(ProfileMetaData profile, Context context)
        {
            Console.WriteLine("\n\n\n---------- " + profile.Name +" : used tags and corresponding values --------------");
            foreach (var (key, values) in profile.AllExpressions(context).PossibleTags()) {
                var vs = "*";
                if (values.Any()) {
                    vs = string.Join(", ", values);
                }

                Console.WriteLine(key + ": " + vs);
            }

            Console.WriteLine("\n\n\n------------------------");
        }

        private static void Main(string[] args)
        {
            var errMessage = MainWithError(args);
            if (errMessage != null) {
                Console.WriteLine(errMessage);
            }
        }

        public static string MainWithError(string[] args)
        {
            if (args.Length < 2) {
                return "Usage: <directory where all aspects and profiles can be found> <outputdirectory>";
            }

            var inputDir = args[0];
            var outputDir = args[1];

            if (!Directory.Exists(outputDir)) {
                Directory.CreateDirectory(outputDir);
            }

            MdPrinter.GenerateHelpText(outputDir + "helpText.md");
            Console.WriteLine("Written helptext to "+outputDir);


            var files = Directory.EnumerateFiles(inputDir, "*.json", SearchOption.AllDirectories)
                .ToList();

            var tests = Directory.EnumerateFiles(inputDir, "*.csv", SearchOption.AllDirectories)
                .ToList();
            tests.Sort();

            foreach (var test in tests) {
                if (test.EndsWith(".test.csv") || test.EndsWith(".behaviour_test.csv")) {
                    continue;
                }

                throw new ArgumentException(
                    $"Invalid name for csv file ${test}, should end with either '.behaviour_test.csv' or '.test.csv'");
            }

            var context = new Context();

            var aspects = ParseAspects(files, tests, context);

            foreach (var (aspect, _) in aspects) {
                context.AddFunction(aspect.Name, aspect);
            }

            var lastChange = DateTime.UnixEpoch;
            foreach (var file in files) {
                var time = new FileInfo(file).LastWriteTimeUtc;
                if (lastChange < time) {
                    lastChange = time;
                }
            }

            var profiles = ParseProfiles(files, tests, context, lastChange);


            // With everything parsed and typechecked, time for tests
            var testsOk = true;
            foreach (var (aspect, t) in aspects) {
                if (t == null) {
                    Console.WriteLine($"[{aspect.Name}] WARNING: no tests found: please add {aspect.Name}.test.csv");
                }
                else {
                    testsOk &= t.Run();
                }
            }


            foreach (var (profile, profileTests) in profiles)
            foreach (var test in profileTests) {
                testsOk &= test.Run(context);
            }

            if (!testsOk) {
                return "Some tests failed, quitting now without generating output";
            }

            if (!Directory.Exists($"{outputDir}/profile-documentation/")) {
                Directory.CreateDirectory($"{outputDir}/profile-documentation/");
            }

            if (!Directory.Exists($"{outputDir}/itinero2/")) {
                Directory.CreateDirectory($"{outputDir}/itinero2/");
            }

            foreach (var (profile, profileTests) in profiles) {
                PrintUsedTags(profile, context);

                var aspectTests = aspects.Select(a => a.tests).ToList();
                var luaProfile = new LuaPrinter1(profile, context,
                    aspectTests,
                    profileTests
                ).ToLua();
                File.WriteAllText(outputDir + "/" + profile.Name + ".lua", luaProfile);

                var profileMd = new MarkDownSection();
                profileMd.AddTitle(profile.Name, 1);

                profileMd.Add(profile.Description);
                profileMd.AddTitle("Default parameters", 4);
                profileMd.Add("| name | value | ", "| ---- | ---- | ",
                    string.Join("\n",
                      profile.DefaultParameters.Select(delegate(KeyValuePair<string, IExpression> kv) {
                          var v = kv.Value.Evaluate(context);
                          if (!(v is string || v is int || v is double)) {
                              v = "_special value_";
                          }
                          return $" | {kv.Key} | {v} |";
                      })) 
                );

                foreach (var (behaviourName, vars) in profile.Behaviours) {
                    var lua2behaviour = new LuaPrinter2(
                        profile,
                        behaviourName,
                        context,
                        aspectTests,
                        profileTests.Where(testsSuite => testsSuite.BehaviourName == behaviourName),
                        lastChange
                    ).ToLua();

                    File.WriteAllText(
                        $"{outputDir}/itinero2/{profile.Name}.{behaviourName}.lua",
                        lua2behaviour);

                    var behaviourMd = new ProfileToMD(profile, behaviourName, context);

                    File.WriteAllText(
                        $"{outputDir}/profile-documentation/{profile.Name}.{behaviourName}.md",
                        behaviourMd.ToString());
                    profileMd.AddTitle($"[{behaviourName}](./{profile.Name}.{behaviourName}.md)", 2);
                    profileMd.Add(vars["description"].Evaluate(context).ToString());
                    profileMd.Add(behaviourMd.MainFormula());
                }

                File.WriteAllText(
                    $"{outputDir}/profile-documentation/{profile.Name}.md",
                    profileMd.ToString());
            }

            File.WriteAllText($"{outputDir}/ProfileMetadata.json",
                Utils.GenerateExplanationJson(profiles.Select(p => p.profile), context)
            );
            File.WriteAllText($"{outputDir}/UsedTags.json",
                Utils.GenerateTagsOverview(profiles.Select(p => p.profile), context)
            );


            if (!args.Contains("--no-repl")) {
                Repl(context, profiles
                    .Select(p => p.profile)
                    .ToDictionary(p => p.Name, p => p));
            }
            else {
                Console.WriteLine("Not starting REPL as --no-repl is specified");
            }

            return null;
        }
    }
}