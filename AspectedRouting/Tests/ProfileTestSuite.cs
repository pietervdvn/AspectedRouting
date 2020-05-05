using System;
using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Language;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Functions;
using AspectedRouting.Language.Typ;

namespace AspectedRouting.Tests
{
    public struct Expected
    {
        public readonly string Access;
        public readonly string Oneway;
        public readonly double Speed;
        public readonly double Weight;

        public Expected(string access, string oneway, double speed, double weight)
        {
            Access = access;
            Oneway = oneway;
            Speed = speed;
            Weight = weight;
        }
    }

    public class ProfileTestSuite
    {
        public readonly ProfileMetaData Profile;
        public readonly string BehaviourName;
        public readonly IEnumerable<(Expected, Dictionary<string, string> tags)> Tests;

        public static ProfileTestSuite FromString(Context c, ProfileMetaData function, string profileName,
            string csvContents)
        {
            try
            {
                var all = csvContents.Split("\n").ToList();
                var keys = all[0].Split(",").ToList();
                keys = keys.GetRange(4, keys.Count - 4).Select(k => k.Trim()).ToList();

                foreach (var k in keys)
                {
                    if (k.StartsWith("_relations:"))
                    {
                        throw new ArgumentException(
                            "To inject relation memberships, use '_relation:<aspect_name>', without S after relation");
                    }

                    if (k.StartsWith("_relation:"))
                    {
                        var aspectName = k.Substring("_relation:".Length);
                        if (aspectName.Contains(":"))
                        {
                            throw new ArgumentException(
                                "To inject relation memberships, use '_relation:<aspect_name>', don't add the behaviour name");
                        }

                        if (!c.DefinedFunctions.ContainsKey(aspectName))
                        {
                            throw new ArgumentException(
                                $"'_relation:<aspect_name>' detected, but the aspect {aspectName} wasn't found. Try one of: " +
                                string.Join(",", c.DefinedFunctions.Keys));
                        }
                    }
                }


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

                        var speed = 0.0;
                        if (!string.IsNullOrEmpty(testData[2]))
                        {
                            speed = double.Parse(testData[2]);
                        }

                        var weight = 0.0;
                        if (!string.IsNullOrEmpty(testData[3]))
                        {
                            weight = double.Parse(testData[3]);
                        }

                        var expected = new Expected(
                            testData[0],
                            testData[1],
                            speed,
                            weight
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
            Profile = profile;
            BehaviourName = profileName;
            Tests = tests;
        }


        private static bool Eq(Context c, string value, object result)
        {
            var v = Funcs.Eq.Apply(new Constant(value), new Constant(Typs.String, result));

            var o = v.Evaluate(c);
            return o is string s && s.Equals("yes");
        }

        public bool RunTest(Context c, int i, Expected expected, Dictionary<string, string> tags)
        {
            c = new Context(c);
            tags = new Dictionary<string, string>(tags);

            void Err(string message, object exp, object act)
            {
                Console.WriteLine(
                    $"[{Profile.Name}.{BehaviourName}]: Test on line {i + 1} failed: {message}; expected {exp} but got {act}\n    {{{tags.Pretty()}}}");
            }

            var success = true;
            var canAccess = Profile.Access.Run(c, tags);
            tags["access"] = "" + canAccess;

            if (!expected.Access.Equals(canAccess))
            {
                Err("access value incorrect", expected.Access, canAccess);
                success = false;
            }


            if (expected.Access.Equals("no"))
            {
                return success;
            }

            var oneway = Profile.Oneway.Run(c, tags);
            tags["oneway"] = "" + oneway;

            if (!Eq(c, expected.Oneway, oneway))
            {
                Err("oneway value incorrect", expected.Oneway, oneway);
                success = false;
            }

            var speed = (double) Profile.Speed.Run(c, tags);
            tags["speed"] = "" + speed;


            c.AddFunction("speed", new AspectMetadata(new Constant(Typs.Double, speed),
                "speed", "Actual speed of this function", "NA", "NA", "NA", true));
            c.AddFunction("oneway", new AspectMetadata(new Constant(Typs.String, oneway),
                "oneway", "Actual direction of this function", "NA", "NA", "NA", true));
            c.AddFunction("access", new AspectMetadata(new Constant(Typs.String, canAccess),
                "access", "Actual access of this function", "NA", "NA", "NA", true));

            if (Math.Abs(speed - expected.Speed) > 0.0001)
            {
                Err("speed value incorrect", expected.Speed, speed);
                success = false;
            }


            var priority = 0.0;
            var weightExplanation = new List<string>();
            foreach (var (paramName, expression) in Profile.Priority)
            {
                var aspectInfluence = (double) c.Parameters[paramName].Evaluate(c);
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (aspectInfluence == 0)
                {
                    continue;
                }


                var aspectWeightObj = new Apply(
                    Funcs.EitherFunc.Apply(Funcs.Id, Funcs.Const, expression)
                    , new Constant(tags)).Evaluate(c);

                double aspectWeight;
                switch (aspectWeightObj)
                {
                    case bool b:
                        aspectWeight = b ? 1.0 : 0.0;
                        break;
                    case double d:
                        aspectWeight = d;
                        break;
                    case int j:
                        aspectWeight = j;
                        break;
                    case string s:
                        if (s.Equals("yes"))
                        {
                            aspectWeight = 1.0;
                            break;
                        }
                        else if (s.Equals("no"))
                        {
                            aspectWeight = 0.0;
                            break;
                        }

                        throw new Exception($"Invalid value as result for {paramName}: got string {s}");
                    default:
                        throw new Exception($"Invalid value as result for {paramName}: got object {aspectWeightObj}");
                }

                weightExplanation.Add($"({paramName} = {aspectInfluence}) * {aspectWeight}");
                priority += aspectInfluence * aspectWeight;
            }

            if (Math.Abs(priority - expected.Weight) > 0.0001)
            {
                Err($"weight incorrect. Calculation is {string.Join(" + ", weightExplanation)}", expected.Weight,
                    priority);
                success = false;
            }

            if (!success)
            {
                Console.WriteLine();
            }

            return success;
        }

        public void Run(Context c)

        {
            var parameters = new Dictionary<string, IExpression>();

            foreach (var (k, v) in Profile.DefaultParameters)
            {
                parameters[k] = v;
            }

            foreach (var (k, v) in Profile.Behaviours[BehaviourName])
            {
                parameters[k] = v;
            }

            c = c.WithParameters(parameters);

            var allOk = true;
            var i = 1;
            foreach (var (expected, tags) in Tests)
            {
                try
                {
                    allOk &= RunTest(c, i, expected, tags);
                }
                catch (Exception e)
                {
                    throw new Exception("In a test for " + BehaviourName, e);
                }

                i++;
            }

            if (!allOk)
            {
                throw new ArgumentException("Some tests failed for " + BehaviourName);
            }
            else
            {
                Console.WriteLine($"[{Profile.Name}] {Tests.Count()} tests successful for behaviour {BehaviourName}");
            }
        }
    }
}