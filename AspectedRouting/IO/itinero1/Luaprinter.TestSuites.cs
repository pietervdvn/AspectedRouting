using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Tests;

namespace AspectedRouting.IO.itinero1
{
    public partial class LuaPrinter
    {
        public void AddTestSuite(ProfileTestSuite testSuite)
        {
            var tests = string.Join("\n",
                testSuite.Tests.Select((test, i) => ToLua(testSuite, i, test.Item1, test.tags)));
            _tests.Add(tests);
        }

        private string ToLua(ProfileTestSuite testSuite, int index, ProfileResult expected, Dictionary<string, string> tags)
        {
            AddDep("debug_table");
            var parameters = new Dictionary<string, string>();


            var keysToCheck = new List<string>();
            foreach (var (key, value) in tags)
            {
                if (key.StartsWith("#"))
                {
                    parameters[key.TrimStart('#')] = value;
                }
                if(key.StartsWith("_relation:"))
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
            // function unit_test_profile(profile_function, profile_name, index, expected, tags)

            return $"unit_test_profile(behaviour_{testSuite.Profile.Name.FunctionName()}_{testSuite.BehaviourName.FunctionName()}, " +
                   $"\"{testSuite.BehaviourName}\", " +
                   $"{index}, " +
                   $"{{access = \"{D(expected.Access)}\", speed = {expected.Speed}, oneway = \"{D(expected.Oneway)}\", weight = {expected.Priority} }}, " +
                   tags.ToLuaTable() +
                   ")";
        }

        private string D(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return "0";
            }

            return s;
        }


        public void AddTestSuite(AspectTestSuite testSuite)
        {
            var fName = testSuite.FunctionToApply.Name;
            var tests = string.Join("\n",
                testSuite.Tests.Select((test, i) => ToLua(fName, i, test.expected, test.tags)));
            _tests.Add(tests);
        }

        private string ToLua(string functionToApplyName, int index, string expected, Dictionary<string, string> tags)
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

            AddDep("unitTest");
            AddDep("debug_table");
            var funcName = functionToApplyName.Replace(" ", "_").Replace(".", "_");
            return
                $"unit_test({funcName}, \"{functionToApplyName}\", {index}, \"{expected}\", {parameters.ToLuaTable()}, {tags.ToLuaTable()})";
        }
    }
}