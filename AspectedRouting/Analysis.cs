using System;
using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Functions;
using AspectedRouting.Typ;
using static AspectedRouting.Deconstruct;

namespace AspectedRouting
{
    public static class Analysis
    {
        public static string GenerateFullOutputCsv(Context c, IExpression e)
        {
            var possibleTags = e.PossibleTags();

            var defaultValues = new List<string>
            {
                "0",
                "30",
                "50",
                "yes",
                "no",
                "SomeName"
            };

            Console.WriteLine(e);
            var keys = e.PossibleTags().Keys.ToList();

            var results = possibleTags.OnAllCombinations(
                tags =>
                {
                    Console.WriteLine(tags.Pretty());
                    return (new Apply(e, new Constant(tags)).Evaluate(c), tags);
                }, defaultValues).ToList();

            var csv = "result, " + string.Join(", ", keys) + "\n";

            foreach (var (result, tags) in results)
            {
                csv += result + ", " +
                       string.Join(", ",
                           keys.Select(key =>
                           {
                               if (tags.ContainsKey(key))
                               {
                                   return tags[key];
                               }
                               else
                               {
                                   return "";
                               }
                           }));
                csv += "\n";
            }

            return csv;
        }

        public static IEnumerable<T> OnAllCombinations<T>(this Dictionary<string, List<string>> possibleTags,
            Func<Dictionary<string, string>, T> f, List<string> defaultValues)
        {
            var newDict = new Dictionary<string, List<string>>();
            foreach (var (key, value) in possibleTags)
            {
                if (value.Count == 0)
                {
                    // This value is a list of possible values, e.g. a double
                    // We replace them with various other
                    newDict[key] = defaultValues;
                }
                else
                {
                    newDict[key] = value;
                }
            }

            possibleTags = newDict;

            var keys = possibleTags.Keys.ToList();
            var currentKeyIndex = new int[possibleTags.Count];
            for (int i = 0; i < currentKeyIndex.Length; i++)
            {
                currentKeyIndex[i] = -1;
            }

            bool SelectNext()
            {
                var j = 0;
                while (j < currentKeyIndex.Length)
                {
                    currentKeyIndex[j]++;
                    if (currentKeyIndex[j] ==
                        possibleTags[keys[j]].Count)
                    {
                        // This index rolls over
                        currentKeyIndex[j] = -1;
                        j++;
                    }
                    else
                    {
                        return true;
                    }
                }

                return false;
            }


            do
            {
                var tags = new Dictionary<string, string>();
                for (int i = 0; i < keys.Count(); i++)
                {
                    var key = keys[i];
                    var j = currentKeyIndex[i];
                    if (j >= 0)
                    {
                        var value = possibleTags[key][j];
                        tags.Add(key, value);
                    }
                }

                yield return f(tags);
            } while (SelectNext());
        }

        public static Dictionary<string, (IEnumerable<Typ.Type> Types, string inFunction)> UsedParameters(
            this ProfileMetaData profile, Context context)
        {
            var parameters = new Dictionary<string, (IEnumerable<Typ.Type> Types, string inFunction)>();

            void AddParams(IExpression e, string inFunction)
            {
                var parms = e.UsedParameters();
                foreach (var param in parms)
                {
                    if (parameters.TryGetValue(param.ParamName, out var typesOldUsage))
                    {
                        var (types, oldUsage) = typesOldUsage;
                        var unified = types.SpecializeTo(param.Types);
                        if (unified == null)
                        {
                            throw new ArgumentException("Inconsistent parameter usage: the paremeter " +
                                                        param.ParamName + " is used\n" +
                                                        $"   in function {oldUsage} as {string.Join(",", types)}\n" +
                                                        $"   in function {inFunction} as {string.Join(",", param.Types)}\n" +
                                                        $"which can not be unified");
                        }
                    }
                    else
                    {
                        parameters[param.ParamName] = (param.Types, inFunction);
                    }
                }
            }


            AddParams(profile.Access, profile.Name + ".access");
            AddParams(profile.Oneway, profile.Name + ".oneway");
            AddParams(profile.Speed, profile.Name + ".speed");

            foreach (var (name, expr) in context.DefinedFunctions)
            {
                AddParams(expr, name);
            }

            return parameters;
        }


        public static HashSet<Parameter> UsedParameters(this IExpression e)
        {
            var result = new HashSet<Parameter>();
            e.Visit(expr =>
            {
                if (expr is Parameter p)
                {
                    result.Add(p);
                }

                return true;
            });
            return result;
        }

        public static string TypeBreakdown(this IExpression e)
        {
            var text = "";
            e.Visit(x =>
            {
                text += $"\n\n{x}\n  : {string.Join("\n  : ", x.Types)}";
                return true;
            });
            return text;
        }

        public static void SanityCheckProfile(this ProfileMetaData pmd)
        {
            var defaultParameters = pmd.DefaultParameters.Keys;

            var usedParameters = pmd.UsedParameters(new Context()).Keys.Select(key => key.TrimStart('#'));

            var diff = usedParameters.ToHashSet().Except(defaultParameters).ToList();
            if (diff.Any())
            {
                throw new ArgumentException("No default value set for parameter " + string.Join(", ", diff));
            }

            foreach (var (profileName, profileParams) in pmd.Profiles)
            {
                var sum = 0.0;
                foreach (var (paramName, _) in pmd.Weights)
                {
                    if (!profileParams.TryGetValue(paramName, out var weight))
                    {
                        continue;
                    }

                    if (!(weight is double d))
                    {
                        continue;
                    }

                    sum += Math.Abs(d);
                }

                if (Math.Abs(sum) < 0.0001)
                {
                    throw new ArgumentException("Profile " + profileName +
                                                ": the summed parameters to calculate the weight are zero or very low");
                }
            }
        }

        public static void SanityCheck(this IExpression e)
        {
            e.Visit(expr =>
            {
                var order = new List<IExpression>();
                var mapping = new List<IExpression>();
                if (UnApply(
                    UnApply(IsFunc(Funcs.FirstOf), Assign(order)),
                    Assign(mapping)
                ).Invoke(expr))
                {
                    var expectedKeys = ((IEnumerable<object>) order.First().Evaluate(null)).Select(o =>
                        {
                            if (o is IExpression x)
                            {
                                return (string) x.Evaluate(null);
                            }

                            return (string) o;
                        })
                        .ToHashSet();
                    var actualKeys = mapping.First().PossibleTags().Keys;
                    var missingInOrder = actualKeys.Where(key => !expectedKeys.Contains(key)).ToList();
                    var missingInMapping = expectedKeys.Where(key => !actualKeys.Contains(key)).ToList();
                    if (missingInOrder.Any() || missingInMapping.Any())
                    {
                        var missingInOrderMsg = "";
                        if (missingInOrder.Any())
                        {
                            missingInOrderMsg = $"The order misses keys {string.Join(",", missingInOrder)}\n";
                        }

                        var missingInMappingMsg = "";
                        if (missingInMapping.Any())
                        {
                            missingInMappingMsg =
                                $"The mapping misses mappings for keys {string.Join(", ", missingInMapping)}\n";
                        }

                        throw new ArgumentException(
                            "Sanity check failed: the specified order of firstMatchOf contains to little or to much keys:\n" +
                            missingInOrderMsg + missingInMappingMsg
                        );
                    }
                }

                return true;
            });
        }

        /// <summary>
        /// Returns which tags are used in this calculation
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A dictionary containing all possible values. An entry with an empty list indicates a wildcard</returns>
        public static Dictionary<string, List<string>> PossibleTags(this IExpression e)
        {
            var result = new Dictionary<string, List<string>>();
            var mappings = new List<Mapping>();
            e.Visit(x =>
            {
                /*
                var networkMapping = new List<IExpression>();
                if (Deconstruct.UnApply(
                    IsFunc(Funcs.MustMatch),
                    Assign(networkMapping)
                ).Invoke(x))
                {
                    var possibleTags = x.PossibleTags();
                    result.
                    return false;
                }*/

                if (x is Mapping m)
                {
                    mappings.Add(m);
                }

                return true;
            });

            if (mappings.Count == 0)
            {
                return null;
            }

            // Visit will have the main mapping at the first position
            var rootMapping = mappings[0];

            foreach (var (key, expr) in rootMapping.StringToResultFunctions)
            {
                var values = new List<string>();
                expr.Visit(x =>
                {
                    if (x is Mapping m)
                    {
                        values.AddRange(m.StringToResultFunctions.Keys);
                    }

                    return true;
                });
                result[key] = values;
            }

            return result;
        }
    }
}