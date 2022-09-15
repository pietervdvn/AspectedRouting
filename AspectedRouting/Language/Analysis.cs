using System;
using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Functions;
using AspectedRouting.Language.Typ;
using Type = AspectedRouting.Language.Typ.Type;

namespace AspectedRouting.Language
{
    public static class Analysis
    {
        public static Dictionary<string, (List<Type> Types, string inFunction)> UsedParameters(
            this ProfileMetaData profile, Context context)
        {
            var parameters = new Dictionary<string, (List<Type> Types, string usageLocation)>();


            void AddParams(IExpression e, string inFunction)
            {
                var parms = e.UsedParameters();
                foreach (var param in parms) {
                    if (parameters.TryGetValue(param.ParamName, out var typesOldUsage)) {
                        var (types, oldUsage) = typesOldUsage;
                        var unified = types.SpecializeTo(param.Types);
                        if (unified == null) {
                            throw new ArgumentException("Inconsistent parameter usage: the paremeter " +
                                                        param.ParamName + " is used\n" +
                                                        $"   in {oldUsage} as {string.Join(",", types)}\n" +
                                                        $"   in {inFunction} as {string.Join(",", param.Types)}\n" +
                                                        "which can not be unified");
                        }
                    }
                    else {
                        parameters[param.ParamName] = (param.Types.ToList(), inFunction);
                    }
                }
            }


            AddParams(profile.Access, "profile definition for " + profile.Name + ".access");
            AddParams(profile.Oneway, "profile definition for " + profile.Name + ".oneway");
            AddParams(profile.Speed, "profile definition for " + profile.Name + ".speed");

            foreach (var (key, expr) in profile.Priority) {
                AddParams(new Parameter(key), profile.Name + ".priority.lefthand");
                AddParams(expr, profile.Name + ".priority");
            }

            var calledFunctions = profile.CalledFunctionsRecursive(context).Values
                .SelectMany(ls => ls).ToHashSet();
            foreach (var calledFunction in calledFunctions) {
                var func = context.GetFunction(calledFunction);
                if (func is AspectMetadata meta && meta.ProfileInternal) {
                    continue;
                }

                AddParams(func, "function " + calledFunction);
            }


            return parameters;
        }


        public static HashSet<Parameter> UsedParameters(this IExpression e)
        {
            var result = new HashSet<Parameter>();
            e.Visit(expr => {
                if (expr is Parameter p) {
                    result.Add(p);
                }

                return true;
            });
            return result;
        }


        public static Dictionary<string, List<string>> CalledFunctionsRecursive(this ProfileMetaData profile,
            Context c)
        {
            // Read as: this function calls the value-function
            var result = new Dictionary<string, List<string>>();


            var calledFunctions = new Queue<string>();

            void ScanExpression(IExpression e, string inFunction)
            {
                if (!result.ContainsKey(inFunction)) {
                    result.Add(inFunction, new List<string>());
                }

                e.Visit(x => {
                    if (x is FunctionCall fc) {
                        result[inFunction].Add(fc.CalledFunctionName);
                        if (!result.ContainsKey(fc.CalledFunctionName)) {
                            calledFunctions.Enqueue(fc.CalledFunctionName);
                        }
                    }

                    return true;
                });
            }


            ScanExpression(profile.Access, profile.Name + ".access");
            ScanExpression(profile.Oneway, profile.Name + ".oneway");
            ScanExpression(profile.Speed, profile.Name + ".speed");

            foreach (var (key, expr) in profile.Priority) {
                ScanExpression(new Parameter(key), $"{profile.Name}.priority.{key}.lefthand");
                ScanExpression(expr, $"{profile.Name}.priority.{key}");
            }

            while (calledFunctions.TryDequeue(out var calledFunction)) {
                var func = c.GetFunction(calledFunction);
                ScanExpression(func, calledFunction);
            }


            return result;
        }

        public static (HashSet<string> parameterName, HashSet<string> calledFunctionNames) DirectlyAndInderectlyCalled(
            this List<IExpression> exprs, Context ctx)
        {
            var parameters = new HashSet<string>();
            var dependencies = new HashSet<string>();

            var queue = new Queue<IExpression>();
            exprs.ForEach(queue.Enqueue);

            while (queue.TryDequeue(out var next)) {
                var (p, deps) = next.DirectlyCalled();
                parameters.UnionWith(p);
                var toCheck = deps.Except(dependencies);
                dependencies.UnionWith(deps);

                foreach (var fName in toCheck) {
                    queue.Enqueue(ctx.GetFunction(fName));
                }
            }

            return (parameters, dependencies);
        }


        /// <summary>
        ///     Generates an overview of the dependencies of the expression, both which parameters it needs and what other
        ///     functions (builtin or defined) it needs.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static (HashSet<string> parameterName, HashSet<string> calledFunctionNames) DirectlyCalled(
            this IExpression expr)
        {
            var parameters = new HashSet<string>();
            var dependencies = new HashSet<string>();

            expr.Visit(e => {
                if (e is FunctionCall fc) {
                    dependencies.Add(fc.CalledFunctionName);
                }

                if (e is Parameter p) {
                    parameters.Add(p.ParamName);
                }

                return true;
            });


            return (parameters, dependencies);
        }

        public static string TypeBreakdown(this IExpression e)
        {
            return e + " : " + string.Join(" ; ", e.Types);
        }

        public static void SanityCheckProfile(this ProfileMetaData pmd, Context context)
        {
            var defaultParameters = pmd.DefaultParameters.Keys
                .Select(k => k.TrimStart('#')).ToList();


            var usedMetadata = pmd.UsedParameters(context);

            string MetaList(IEnumerable<string> paramNames)
            {
                var metaInfo = "";
                foreach (var paramName in paramNames) {
                    var _ = usedMetadata.TryGetValue(paramName, out var inFunction) ||
                            usedMetadata.TryGetValue('#' + paramName, out inFunction);
                    metaInfo += $"\n - {paramName} (used in {inFunction.inFunction})";
                }

                return metaInfo;
            }

            var usedParameters = usedMetadata.Keys.Select(key => key.TrimStart('#')).ToList();

            var diff = usedParameters.ToHashSet().Except(defaultParameters).ToList();
            if (diff.Any()) {
                throw new ArgumentException("No default value set for parameter: " + MetaList(diff));
            }
            
            var unused = defaultParameters.Except(usedParameters);
            if (unused.Any()) {
                Console.WriteLine("[WARNING] A default value is set for parameter, but it is unused: " +
                                  string.Join(", ", unused));
            }

            var paramsUsedInBehaviour = new HashSet<string>();

            foreach (var (behaviourName, behaviourParams) in pmd.Behaviours) {
                var sum = 0.0;
                var explanation = "";
                paramsUsedInBehaviour.UnionWith(behaviourParams.Keys.Select(k => k.Trim('#')));
                foreach (var (paramName, _) in pmd.Priority) {
                    if (!pmd.DefaultParameters.ContainsKey(paramName)) {
                        throw new ArgumentException(
                            $"The behaviour {behaviourName} uses a parameter for which no default is set: {paramName}");
                    }

                    if (!behaviourParams.TryGetValue(paramName, out var weight)) {
                        explanation += $"\n - {paramName} = default (not set)";
                        continue;
                    }

                    var weightObj = weight.Evaluate(context);

                    if (!(weightObj is double d)) {
                        throw new ArgumentException(
                            $"The parameter {paramName} is not a numeric value in profile {behaviourName}");
                    }

                    sum += Math.Abs(d);
                    explanation += $"\n - {paramName} = {d}";
                }

                if (Math.Abs(sum) < 0.0001) {
                    throw new ArgumentException("Profile " + behaviourName +
                                                ": the summed parameters to calculate the weight are zero or very low:" +
                                                explanation);
                }
            }


            var defaultOnly = defaultParameters.Except(paramsUsedInBehaviour).ToList();
            if (defaultOnly.Any()) {
                Console.WriteLine(
                    $"[{pmd.Name}] WARNING: Some parameters only have a default value: {string.Join(", ", defaultOnly)}");
            }
            
        }

        public static void SanityCheck(this IExpression e)
        {
            e.Visit(expr => {
                var order = new List<IExpression>();
                var mapping = new List<IExpression>();
                if (Deconstruct.UnApply(
                        Deconstruct.UnApply(Deconstruct.IsFunc(Funcs.FirstOf), Deconstruct.Assign(order)),
                        Deconstruct.Assign(mapping)
                    ).Invoke(expr)) {
                    var expectedKeys = ((IEnumerable<object>)order.First().Evaluate(null)).Select(o => {
                            if (o is IExpression x) {
                                return (string)x.Evaluate(null);
                            }

                            return (string)o;
                        })
                        .ToHashSet();
                    var actualKeys = mapping.First().PossibleTags().Keys;
                    var missingInOrder = actualKeys.Where(key => !expectedKeys.Contains(key)).ToList();
                    var missingInMapping = expectedKeys.Where(key => !actualKeys.Contains(key)).ToList();
                    if (missingInOrder.Any() || missingInMapping.Any()) {
                        var missingInOrderMsg = "";
                        if (missingInOrder.Any()) {
                            missingInOrderMsg = $"The order misses keys {string.Join(",", missingInOrder)}\n";
                        }

                        var missingInMappingMsg = "";
                        if (missingInMapping.Any()) {
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


        /**
         * Returns all possible tags which are used in the given expression.
         *
         * If a tag might match a wildcard, an explicit '*' will be added to the collection.
         * This is different from the _other_ possibleTags, which will return an empty set.
         */
        public static Dictionary<string, HashSet<string>> PossibleTags(this IEnumerable<IExpression> exprs)
        {
            var usedTags = new Dictionary<string, HashSet<string>>();
            foreach (var expr in exprs) {
                var possible = expr.PossibleTags();
                if (possible == null) {
                    continue;
                }

                foreach (var (key, values) in possible) {
                    if (!usedTags.TryGetValue(key, out var collection)) {
                        // This is the first time we see this collection
                        collection = new HashSet<string>();
                        usedTags[key] = collection;
                    }

                    foreach (var v in values) {
                        collection.Add(v);
                    }

                    if (values.Count == 0) {
                        collection.Add("*");
                    }
                }
            }

            return usedTags;
        }

        public static Dictionary<string, HashSet<string>> PossibleTagsRecursive(this IEnumerable<IExpression> exprs, Context c) 
        {  var usedTags = new Dictionary<string, HashSet<string>>();
            foreach (var e in exprs) {
                var possibleTags = e.PossibleTagsRecursive(c);

                if (possibleTags != null) {
                    foreach (var tag in possibleTags) {
                        usedTags[tag.Key] = tag.Value;
                    }
                }
            } 
            return usedTags;
        }
        
        public static Dictionary<string, HashSet<string>> PossibleTagsRecursive(this IExpression e, Context c)
        {
            var allExpr = new List<IExpression>();
            var queue = new Queue<IExpression>();
            queue.Enqueue(e);
            do {
                var next = queue.Dequeue();
                allExpr.Add(next);
                next.Visit(expression => {
                    if (expression is FunctionCall fc) {
                        var called = c.GetFunction(fc.CalledFunctionName);
                        queue.Enqueue(called);
                    }

                    return true;
                });
            } while (queue.Any());

            var result = new Dictionary<string, HashSet<string>>();

            foreach (var expression in allExpr) {
                var subTags = expression.PossibleTags();
                if (subTags == null) {
                    continue;
                }

                foreach (var kv in subTags) {
                    if (!result.ContainsKey(kv.Key)) {
                        result[kv.Key] = new HashSet<string>();
                    }

                    foreach (var val in kv.Value) {
                        result[kv.Key].Add(val);
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///     Returns which tags are used in this calculation
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A dictionary containing all possible values. An entry with an empty list indicates a wildcard</returns>
        public static Dictionary<string, HashSet<string>> PossibleTags(this IExpression e)
        {
            var mappings = new List<Mapping>();
            e.Visit(x => {
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

                if (x is Mapping m) {
                    mappings.Add(m);
                }

                return true;
            });

            if (mappings.Count == 0) {
                return null;
            }

            // Visit will have the main mapping at the first position
            var rootMapping = mappings[0];
            var result = new Dictionary<string, HashSet<string>>();

            foreach (var (key, expr) in rootMapping.StringToResultFunctions) {
                var values = new List<string>();
                expr.Visit(x => {
                    if (x is Mapping m) {
                        values.AddRange(m.StringToResultFunctions.Keys);
                    }

                    return true;
                });
                result[key] = values.ToHashSet();
            }

            return result;
        }

        public static Dictionary<string, IExpression> MembershipMappingsFor(ProfileMetaData profile, Context context)
        {
            var calledFunctions = profile.Priority.Values.ToHashSet();
            calledFunctions.Add(profile.Speed);
            calledFunctions.Add(profile.Access);
            calledFunctions.Add(profile.Oneway);


            var calledFunctionQueue = new Queue<string>();
            var alreadyAnalysedFunctions = new HashSet<string>();
            var memberships = new Dictionary<string, IExpression>();

            void HandleExpression(IExpression e, string calledIn)
            {
                e.Visit(f => {
                    var mapping = new List<IExpression>();
                    if (Deconstruct.UnApply(Deconstruct.IsFunc(Funcs.MemberOf),
                            Deconstruct.Assign(mapping)
                        ).Invoke(f)) {
                        memberships.Add(calledIn, mapping.First());
                        return false;
                    }

                    if (f is FunctionCall fc) {
                        calledFunctionQueue.Enqueue(fc.CalledFunctionName);
                    }

                    return true;
                });
            }

            foreach (var e in calledFunctions) {
                HandleExpression(e, "profile_root");
            }

            while (calledFunctionQueue.TryDequeue(out var functionName)) {
                if (alreadyAnalysedFunctions.Contains(functionName)) {
                    continue;
                }

                alreadyAnalysedFunctions.Add(functionName);

                var functionImplementation = context.GetFunction(functionName);
                HandleExpression(functionImplementation, functionName);
            }


            return memberships;
        }
    }
}