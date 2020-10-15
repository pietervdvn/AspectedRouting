using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Tests;

namespace AspectedRouting.IO.itinero1
{
    public class LuaTestPrinter
    {
        private LuaSkeleton.LuaSkeleton _skeleton;

        public LuaTestPrinter(LuaSkeleton.LuaSkeleton skeleton, List<string> unitTestRunners)
        {
            _skeleton = skeleton;
            unitTestRunners.ForEach(_skeleton.AddDep);
        }
        
        
        public string GenerateFullTestSuite(List<BehaviourTestSuite> profileTests, List<AspectTestSuite> aspectTests)
        {
            
            _skeleton.AddDep("inv");
            _skeleton.AddDep("double_compare");


            var aspectTestSuite =
                string.Join("\n\n", aspectTests
                    .Where(x => x != null)
                    .Select(
                        GenerateAspectTestSuite
                    ));

            var profileTestSuite =
                string.Join("\n\n", profileTests
                    .Where(x => x != null)
                    .Select(
                        GenerateProfileTestSuite
                    ));

            return string.Join("\n\n\n", new List<string>
            {
                "function test_all()",
                "    " + aspectTestSuite.Indent(),
                "  -- Behaviour tests --",
                "    " + profileTestSuite.Indent(),
                "end"
            });
        }


        private string GenerateProfileTestSuite(BehaviourTestSuite testSuite)
        {
            return string.Join("\n",
                testSuite.Tests.Select((test, i) => GenerateProfileUnitTestCall(testSuite, i, test.Item1, test.tags))
                    .ToList());
        }

        private string GenerateProfileUnitTestCall(BehaviourTestSuite testSuite, int index, ProfileResult expected,
            Dictionary<string, string> tags)
        {
            _skeleton.AddDep("debug_table");
            var parameters = new Dictionary<string, string>();


            var keysToCheck = new List<string>();
            foreach (var (key, value) in tags)
            {
                if (key.StartsWith("#"))
                {
                    parameters[key.TrimStart('#')] = value;
                }

                if (key.StartsWith("_relation:"))
                {
                    keysToCheck.Add(key);
                }
            }


            foreach (var key in keysToCheck)
            {
                var newKey = key.Replace(".", "_");
                tags[newKey] = tags[key];
                tags.Remove(key);
            }

            foreach (var (paramName, _) in parameters)
            {
                tags.Remove("#" + paramName);
            }

            // Generates something like:
            // function unit_test_profile(profile_function, profile_name, index, expected, tags)
            return
                $"unit_test_profile(behaviour_{testSuite.Profile.Name.AsLuaIdentifier()}_{testSuite.BehaviourName.AsLuaIdentifier()}, " +
                $"\"{testSuite.BehaviourName}\", " +
                $"{index}, " +
                $"{{access = \"{D(expected.Access)}\", speed = {expected.Speed}, oneway = \"{D(expected.Oneway)}\", priority = {expected.Priority} }}, " +
                tags.ToLuaTable() +
                ")";
        }


        private string GenerateAspectTestSuite(AspectTestSuite testSuite)
        {
            var fName = testSuite.FunctionToApply.Name;

            if (!_skeleton.ContainsFunction(fName))
            {
                return "";
            }
            
            var tests =
                testSuite.Tests
                    .Select((test, i) => GenerateAspectUnitTestCall(fName, i, test.expected, test.tags))
                    .ToList();
            return string.Join("\n", tests);
        }

        /// <summary>
        /// Generate a unit test call
        /// </summary>
        private string GenerateAspectUnitTestCall(string functionToApplyName, int index, string expected,
            Dictionary<string, string> tags)
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

            _skeleton.AddDep("unitTest");
            _skeleton.AddDep("debug_table");
            return
                $"unit_test({functionToApplyName.AsLuaIdentifier()}, \"{functionToApplyName}\", {index}, \"{expected}\", {parameters.ToLuaTable()}, {tags.ToLuaTable()})";
        }


        private string D(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return "0";
            }

            return s;
        }
    }
}