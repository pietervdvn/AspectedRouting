using System;
using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Functions;

namespace AspectedRouting.IO
{
    public class FunctionTestSuite
    {
        private readonly AspectMetadata _functionToApply;
        private readonly IEnumerable<(string expected, Dictionary<string, string> tags)> _tests;

        public static FunctionTestSuite FromString(AspectMetadata function, string csvContents)
        {
            var all = csvContents.Split("\n").ToList();
            var keys = all[0].Split(",").ToList();
            keys = keys.GetRange(1, keys.Count - 1);

            var tests = new List<(string, Dictionary<string, string>)>();

            foreach (var test in all.GetRange(1, all.Count - 1))
            {
                if (string.IsNullOrEmpty(test.Trim()))
                {
                    continue;
                }

                var testData = test.Split(",").ToList();
                var expected = testData[0];
                var vals = testData.GetRange(1, testData.Count - 1);
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

            return new FunctionTestSuite(function, tests);
        }

        public FunctionTestSuite(
            AspectMetadata functionToApply,
            IEnumerable<(string expected, Dictionary<string, string> tags)> tests)
        {
            _functionToApply = functionToApply;
            _tests = tests;
        }


        public string ToLua()
        {


            var tests = string.Join("\n", _tests.Select((test, i) => ToLua(i, test.expected, test.tags)));
            return "\n" + tests + "\n";
        }

        private string ToLua(int index, string expected, Dictionary<string, string> tags)
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

            var funcName = _functionToApply.Name.Replace(" ", "_").Replace(".", "_");
            return
                $"unit_test({funcName}, \"{_functionToApply.Name}\", {index}, \"{expected}\", {parameters.ToLuaTable()}, {tags.ToLuaTable()})";
        }

        public void Run()
        {
            var failed = false;
            var testCase = 0;
            foreach (var test in _tests)
            {
                testCase++;
                var context = new Context();
                foreach (var (key, value) in test.tags)
                {
                    if (key.StartsWith("#"))
                    {
                        context.AddParameter(key, value);
                    }
                }

                var actual = _functionToApply.Evaluate(context, new Constant(test.tags));
                if (!actual.ToString().Equals(test.expected) &&
                    !(actual is double actualD && Math.Abs(double.Parse(test.expected) - actualD) < 0.0001)
                )
                {
                    failed = true;
                    Console.WriteLine(
                        $"[{_functionToApply.Name}] Testcase {testCase} failed:\n   Expected: {test.expected}\n   actual: {actual}\n   tags: {test.tags.Pretty()}");
                }
            }

            if (failed)
            {
                throw new ArgumentException("Some test failed");
            }

            Console.WriteLine($"[{_functionToApply.Name}] {testCase} tests successful");
        }
    }
}