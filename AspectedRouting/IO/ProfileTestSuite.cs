using System;
using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Functions;

namespace AspectedRouting.IO
{
    public struct Expected
    {
        public int Access, Oneway;
        public double Speed, Weight;

        public Expected(int access, int oneway, double speed, double weight)
        {
            Access = access;
            Oneway = oneway;
            Speed = speed;
            Weight = weight;
            if (Access == 0)
            {
            }
        }
    }

    public class ProfileTestSuite
    {
        private readonly ProfileMetaData _profile;
        private readonly string _profileName;
        private readonly IEnumerable<(Expected, Dictionary<string, string> tags)> _tests;

        public static ProfileTestSuite FromString(ProfileMetaData function, string profileName, string csvContents)
        {
            try
            {
                var all = csvContents.Split("\n").ToList();
                var keys = all[0].Split(",").ToList();
                keys = keys.GetRange(4, keys.Count - 4).Select(k => k.Trim()).ToList();

                var tests = new List<(Expected, Dictionary<string, string>)>();

                var line = 1;
                foreach (var test in all.GetRange(1, all.Count - 1))
                {
                    line++;
                    if (string.IsNullOrEmpty(test.Trim()))
                    {
                        continue;
                    }

                    try
                    {
                        var testData = test.Split(",").ToList();
                        var expected = new Expected(
                            int.Parse(testData[0]),
                            int.Parse(testData[1]),
                            double.Parse(testData[2]),
                            double.Parse(testData[3])
                        );
                        var vals = testData.GetRange(4, testData.Count - 4);
                        var tags = new Dictionary<string, string>();
                        for (int i = 0; i < keys.Count; i++)
                        {
                            if (i < vals.Count && !string.IsNullOrEmpty(vals[i]))
                            {
                                tags[keys[i]] = vals[i];
                            }
                        }

                        tests.Add((expected, tags));
                    }
                    catch (Exception e)
                    {
                        throw new Exception("On line " + line, e);
                    }
                }

                return new ProfileTestSuite(function, profileName, tests);
            }
            catch (Exception e)
            {
                throw new Exception("In the profile test file for " + profileName, e);
            }
        }

        public ProfileTestSuite(
            ProfileMetaData profile,
            string profileName,
            IEnumerable<(Expected, Dictionary<string, string> tags)> tests)
        {
            _profile = profile;
            _profileName = profileName;
            _tests = tests;
        }


        public string ToLua()
        {
            var tests = string.Join("\n",
                _tests.Select((test, i) => ToLua(i, test.Item1, test.tags)));
            return tests + "\n";
        }

        private string ToLua(int index, Expected expected, Dictionary<string, string> tags)
        {
            var parameters = new Dictionary<string, string>();


            foreach (var (key, value) in tags)
            {
                if (key.StartsWith("#"))
                {
                    parameters[key.TrimStart('#')] = value;
                }
            }

            foreach (var (paramName, _) in parameters)
            {
                tags.Remove("#" + paramName);
            }
            // function unit_test_profile(profile_function, profile_name, index, expected, tags)

            return $"unit_test_profile(profile_bicycle_{_profileName.FunctionName()}, " +
                   $"\"{_profileName}\", " +
                   $"{index}, " +
                   $"{{access = {expected.Access}, speed = {expected.Speed}, oneway = {expected.Oneway}, weight = {expected.Weight} }}, " +
                   tags.ToLuaTable() +
                   ")";

        }

        public void Run()
        {
        }
    }
}