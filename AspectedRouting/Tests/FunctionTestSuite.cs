using System;
using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Language;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Functions;

namespace AspectedRouting.Tests
{
    public class FunctionTestSuite
    {
        public readonly AspectMetadata FunctionToApply;
        public readonly IEnumerable<(string expected, Dictionary<string, string> tags)> Tests;

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

            if (functionToApply == null)
            {
                throw new NullReferenceException("functionToApply is null");
            }
            FunctionToApply = functionToApply;
            Tests = tests;
        }


     

        public void Run()
        {
            var failed = false;
            var testCase = 0;
            foreach (var test in Tests)
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

                var actual = FunctionToApply.Evaluate(context, new Constant(test.tags));
                if (!actual.ToString().Equals(test.expected) &&
                    !(actual is double actualD && Math.Abs(double.Parse(test.expected) - actualD) < 0.0001)
                )
                {
                    failed = true;
                    Console.WriteLine(
                        $"[{FunctionToApply.Name}] Testcase {testCase} failed:\n   Expected: {test.expected}\n   actual: {actual}\n   tags: {test.tags.Pretty()}");
                }
            }

            if (failed)
            {
                throw new ArgumentException("Some test failed");
            }

            Console.WriteLine($"[{FunctionToApply.Name}] {testCase} tests successful");
        }
    }
}