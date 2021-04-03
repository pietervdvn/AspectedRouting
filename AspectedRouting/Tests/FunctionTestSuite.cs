using System;
using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Language;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Functions;

namespace AspectedRouting.Tests
{
    public class AspectTestSuite
    {
        public readonly AspectMetadata FunctionToApply;
        public readonly IEnumerable<(string expected, Dictionary<string, string> tags)> Tests;

        public AspectTestSuite(
            AspectMetadata functionToApply,
            IEnumerable<(string expected, Dictionary<string, string> tags)> tests)
        {
            if (functionToApply == null) {
                throw new NullReferenceException("functionToApply is null");
            }

            FunctionToApply = functionToApply;
            Tests = tests;
        }

        public static AspectTestSuite FromString(AspectMetadata function, string csvContents)
        {
            var all = csvContents.Split("\n").ToList();
            var keys = all[0].Split(",").ToList();
            keys = keys.GetRange(1, keys.Count - 1);

            var tests = new List<(string, Dictionary<string, string>)>();

            foreach (var test in all.GetRange(1, all.Count - 1)) {
                if (string.IsNullOrEmpty(test.Trim())) {
                    continue;
                }

                var testData = test.Split(",").ToList();
                var expected = testData[0];
                var vals = testData.GetRange(1, testData.Count - 1);
                var tags = new Dictionary<string, string>();
                for (var i = 0; i < keys.Count; i++) {
                    if (i < vals.Count && !string.IsNullOrEmpty(vals[i])) {
                        tags[keys[i]] = vals[i].Trim(new []{'"'}).Replace("\"","\\\"");
                    }
                }

                tests.Add((expected, tags));
            }

            return new AspectTestSuite(function, tests);
        }


        public bool Run()
        {
            var failed = false;
            var testCase = 0;
            foreach (var test in Tests) {
                testCase++;
                var context = new Context();
                foreach (var (key, value) in test.tags) {
                    if (key.StartsWith("#")) {
                        context.AddParameter(key, value);
                    }
                }

                try {
                    var actual = FunctionToApply.Evaluate(context, new Constant(test.tags));

                    if (string.IsNullOrWhiteSpace(test.expected)) {
                        failed = true;
                        Console.WriteLine(
                            $"[{FunctionToApply.Name}] Line {testCase + 1} failed:\n   The expected value is not defined or only whitespace. Do you want null? Write null in your test as expected value\n");
                        continue;
                    }

                    if (test.expected == "null" && actual == null) {
                        // Test ok
                        continue;
                    }

                    if (actual == null) {
                        Console.WriteLine(
                            $"[{FunctionToApply.Name}] Line {testCase + 1} failed:\n   Expected: {test.expected}\n   actual value is not defined (null)\n   tags: {test.tags.Pretty()}\n");
                        failed = true;
                        continue;
                    }

                    if (!actual.ToString().Equals(test.expected) &&
                        !(actual is double actualD && Math.Abs(double.Parse(test.expected) - actualD) < 0.0001)
                    ) {
                        failed = true;
                        Console.WriteLine(
                            $"[{FunctionToApply.Name}] Line {testCase + 1} failed:\n   Expected: {test.expected}\n   actual: {actual}\n   tags: {test.tags.Pretty()}\n");
                    }
                }
                catch (Exception e) {
                    Console.WriteLine(
                        $"[{FunctionToApply.Name}] Line {testCase + 1} ERROR:\n   Expected: {test.expected}\n   error message: {e.Message}\n   tags: {test.tags.Pretty()}\n");
                    Console.WriteLine(e);
                    failed = true;
                }
            }


            Console.WriteLine($"[{FunctionToApply.Name}] {testCase} tests " + (failed ? "failed" : "successful"));
            return !failed;
        }
    }
}