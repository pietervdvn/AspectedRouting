using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AspectedRouting.Functions;
using AspectedRouting.Typ;
using static AspectedRouting.Deconstruct;

namespace AspectedRouting.IO
{
    public class LuaPrinter
    {
        public static Dictionary<string, string> BasicFunctions = _basicFunctions();


        private readonly HashSet<string> _deps = new HashSet<string>();
        private readonly List<string> _code = new List<string>();
        private readonly HashSet<string> _neededKeys = new HashSet<string>();

        /// <summary>
        /// A dictionary containing the implementation of basic functions
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, string> _basicFunctions()
        {
            var imps = new Dictionary<string, string>();

            var functionsToFetch = Funcs.BuiltinNames.ToList();
            // These functions should be loaded from disk, but are not necessarily included
            functionsToFetch.Add("table_to_list");
            functionsToFetch.Add("debug_table");
            functionsToFetch.Add("unitTest");
            functionsToFetch.Add("unitTestProfile");
            functionsToFetch.Add("double_compare");
            
            


            foreach (var name in functionsToFetch)
            {
                var path = $"IO/lua/{name}.lua";
                if (File.Exists(path))
                {
                    imps[name] = File.ReadAllText(path);
                }
            }

            return imps;
        }

        public LuaPrinter()
        {
            _deps.Add("debug_table");
        }


        private string ToLua(IExpression bare, Context context, string key = "nil")
        {
            var collectedMapping = new List<IExpression>();
            var order = new List<IExpression>();


            if (UnApply(
                UnApply(
                    IsFunc(Funcs.FirstOf),
                    Assign(order))
                , UnApply(
                    IsFunc(Funcs.StringStringToTags),
                    Assign(collectedMapping))
            ).Invoke(bare))
            {
                _deps.Add(Funcs.FirstOf.Name);
                return "first_match_of(tags, result, \n" +
                       "        " + ToLua(order.First(), context, key) + "," +
                       ("\n" + MappingToLua((Mapping) collectedMapping.First(), context)).Indent().Indent() +
                       ")";
            }

            if (UnApply(
                UnApply(
                    IsFunc(Funcs.MustMatch),
                    Assign(order))
                , UnApply(
                    IsFunc(Funcs.StringStringToTags),
                    Assign(collectedMapping))
            ).Invoke(bare))
            {
                _deps.Add(Funcs.MustMatch.Name);
                return "must_match(tags, result, \n" +
                       "        " + ToLua(order.First(), context, key) + "," +
                       ("\n" + MappingToLua((Mapping) collectedMapping.First(), context)).Indent().Indent() +
                       ")";
            }

            var collectedList = new List<IExpression>();
            var func = new List<IExpression>();


            if (
                UnApply(
                    UnApply(IsFunc(Funcs.Dot), Assign(func)),
                    UnApply(IsFunc(Funcs.ListDot),
                        Assign(collectedList))).Invoke(bare))
            {
                var exprs = (IEnumerable<IExpression>) ((Constant) collectedList.First()).Evaluate(context);
                var luaExprs = new List<string>();
                var funcName = func.First().ToString().TrimStart('$');
                _deps.Add(funcName);
                foreach (var expr in exprs)
                {
                    var c = new List<IExpression>();
                    if (UnApply(IsFunc(Funcs.Const), Assign(c)).Invoke(expr))
                    {
                        luaExprs.Add(ToLua(c.First(), context, key));
                        continue;
                    }

                    if (expr.Types.First() is Curry curry
                        && curry.ArgType.Equals(Typs.Tags))
                    {
                        var lua = ToLua(expr, context, key);
                        luaExprs.Add(lua);
                    }
                }

                return "\n        " + funcName + "({\n         " + string.Join(",\n         ", luaExprs) +
                       "\n        })";
            }


            collectedMapping.Clear();
            var dottedFunction = new List<IExpression>();


            dottedFunction.Clear();

            if (UnApply(
                    UnApply(
                        IsFunc(Funcs.Dot),
                        Assign(dottedFunction)
                    ),
                    UnApply(
                        IsFunc(Funcs.StringStringToTags),
                        Assign(collectedMapping))).Invoke(bare)
            )
            {
                var mapping = (Mapping) collectedMapping.First();
                var baseFunc = (Function) dottedFunction.First();
                _deps.Add(baseFunc.Name);
                _deps.Add("table_to_list");

                return baseFunc.Name +
                       "(table_to_list(tags, result, " +
                       ("\n" + MappingToLua(mapping, context)).Indent().Indent() +
                       "))";
            }

            // The expression might be a function which still expects a string (the value from the tag) as argument
            if (!(bare is Mapping) &&
                bare.Types.First() is Curry curr &&
                curr.ArgType.Equals(Typs.String))
            {
                var applied = new Apply(bare, new Constant(curr.ArgType, ("tags", "\"" + key + "\"")));
                return ToLua(applied.Optimize(), context, key);
            }


            // The expression might consist of multiple nested functions
            var fArgs = bare.DeconstructApply();
            if (fArgs != null)
            {
                var (f, args) = fArgs.Value;
                var baseFunc = (Function) f;
                _deps.Add(baseFunc.Name);

                return baseFunc.Name + "(" + string.Join(", ", args.Select(arg => ToLua(arg, context, key))) + ")";
            }


            var collected = new List<IExpression>();
            switch (bare)
            {
                case FunctionCall fc:
                    var called = context.DefinedFunctions[fc.CalledFunctionName];
                    if (called.ProfileInternal)
                    {
                        return called.Name;
                    }

                    AddFunction(called, context);
                    return $"{fc.CalledFunctionName.FunctionName()}(parameters, tags, result)";
                case Constant c:
                    return ConstantToLua(c, context);
                case Mapping m:
                    return MappingToLua(m, context).Indent();
                case Function f:
                    var fName = f.Name.TrimStart('$');

                    if (Funcs.Builtins.ContainsKey(fName))
                    {
                        _deps.Add(f.Name);
                    }
                    else
                    {
                        var definedFunc = context.DefinedFunctions[fName];
                        if (definedFunc.ProfileInternal)
                        {
                            return f.Name;
                        }

                        AddFunction(definedFunc, context);
                    }

                    return f.Name;
                case Apply a when UnApply(IsFunc(Funcs.Const), Assign(collected)).Invoke(a):
                    return ToLua(collected.First(), context, key);

                case Parameter p:
                    return $"parameters[\"{p.ParamName.FunctionName()}\"]";
                default:
                    throw new Exception("Could not convert " + bare + " to a lua expression");
            }
        }


        public static string ConstantToLua(Constant c, Context context)
        {
            var o = c.Evaluate(context);
            switch (o)
            {
                case IExpression e:
                    return ConstantToLua(new Constant(e.Types.First(), e.Evaluate(null)), context);
                case int i:
                    return "" + i;
                case double d:
                    return "" + d;
                case string s:
                    return '"' + s.Replace("\"", "\\\"") + '"';
                case ValueTuple<string, string> unpack:
                    return unpack.Item1 + "[" + unpack.Item2 + "]";
                case IEnumerable<object> ls:
                    var t = (c.Types.First() as ListType).InnerType;
                    return "{" + string.Join(", ", ls.Select(obj =>
                    {
                        var objInConstant = new Constant(t, obj);
                        if (obj is Constant asConstant)
                        {
                            objInConstant = asConstant;
                        }

                        return ConstantToLua(objInConstant, context);
                    })) + "}";
                default:
                    return o.ToString();
            }
        }

        public string MappingToLua(Mapping m, Context context)
        {
            var contents = m.StringToResultFunctions.Select(kv =>
                {
                    var (key, expr) = kv;
                    var left = "[\"" + key + "\"]";

                    if (Regex.IsMatch(key, "^[a-zA-Z][_a-zA-Z-9]*$"))
                    {
                        left = key;
                    }

                    return left + " = " + ToLua(expr, context, key);
                }
            );
            return
                "{\n    " +
                string.Join(",\n    ", contents) +
                "\n}";
        }


        private HashSet<string> addFunctions = new HashSet<string>();

        public void AddFunction(AspectMetadata meta, Context context)
        {
            if (addFunctions.Contains(meta.Name))
            {
                // already added
                return;
            }

            addFunctions.Add(meta.Name);

            var possibleTags = meta.PossibleTags();
            var usedParams = meta.UsedParameters();
            var numberOfCombinations = possibleTags.Values.Select(lst => 1 + lst.Count).Multiply();
            var impl = string.Join("\n",
                "--[[",
                meta.Description,
                "",
                "Unit: " + meta.Unit,
                "Created by " + meta.Author,
                "Originally defined in " + meta.Filepath,
                "Uses tags: " + string.Join(", ", possibleTags.Keys),
                "Used parameters: " + string.Join(", ", usedParams),
                "Number of combintations: " + numberOfCombinations,
                "Returns values: ",
                "]]",
                "function " + meta.Name.FunctionName() + "(parameters, tags, result)",
                "    return " + ToLua(meta.ExpressionImplementation, context),
                "end"
            );

            _code.Add(impl);
            foreach (var k in possibleTags.Keys)
            {
                _neededKeys.Add(k); // To generate a whitelist of OSM-keys that should be kept
            }
        }

        private string CreateMembershipPreprocessor()
        {

            return "";


        }

        /// <summary>
        /// Adds the necessary called functions and the profile main entry point
        /// </summary>
        public void CreateProfile(ProfileMetaData profile, Context context)
        {
            var defaultParameters = "\n";
            foreach (var (name, (types, inFunction)) in profile.UsedParameters(context))
            {
                defaultParameters += $"{name}: {string.Join(", ", types)}\n" +
                                     $"    Used in {inFunction}\n";
            }


            var impl = string.Join("\n",
                "",
                "",
                $"name = \"{profile.Name}\"",
                "normalize = false",
                "vehicle_type = {" + string.Join(", ", profile.VehicleTyps.Select(s => "\"" + s + "\"")) + "}",
                "meta_whitelist = {" + string.Join(", ", profile.Metadata.Select(s => "\"" + s + "\"")) + "}",
                "",
                "",
                "",
                "--[[",
                profile.Name,
                "This is the main function called to calculate the access, oneway and speed.",
                "Comfort is calculated as well, based on the parameters which are padded in",
                "",
                "Created by " + profile.Author,
                "Originally defined in " + profile.Filename,
                "Used parameters: " + defaultParameters.Indent(),
                "]]",
                "function " + profile.Name + "(parameters, tags, result)",
                "",
                "    -- initialize the result table on the default values",
                "    result.access = 0",
                "    result.speed = 0",
                "    result.factor = 1",
                "    result.direction = 0",
                "    result.canstop = true",
                "    result.attributes_to_keep = {}",
                "",
                "    local access = " + ToLua(profile.Access, context),
                "    if (access == nil or access == \"no\") then",
                "         return",
                "    end",
                "    local oneway = " + ToLua(profile.Oneway, context),
                "    local speed = " + ToLua(profile.Speed, context),
                "    local distance = 1 -- the weight per meter for distance travelled is, well, 1m/m",
                "");

            impl +=
                "\n    local weight = \n        ";

            var weightParts = new List<string>();
            foreach (var (parameterName, expression) in profile.Weights)
            {
                var weightPart = ToLua(new Parameter(parameterName), context) + " * ";

                var subs = new Curry(Typs.Tags, new Var(("a"))).UnificationTable(expression.Types.First());
                if (subs != null && subs.TryGetValue("$a", out var resultType) &&
                    (resultType.Equals(Typs.Bool) || resultType.Equals(Typs.String)))
                {
                    weightPart += "parse(" + ToLua(expression, context) + ")";
                }
                else
                {
                    weightPart += ToLua(expression, context);
                }

                weightParts.Add(weightPart);
            }

            impl += string.Join(" + \n        ", weightParts);

            impl += string.Join("\n",
                "",
                "",
                "",
                "    -- put all the values into the result-table, as needed for itinero",
                "    result.access = 1",
                "    result.speed = speed",
                "    result.factor = 1/weight",
                "",
                "    if (oneway == \"both\") then",
                "        result.oneway = 0",
                "    elseif (oneway == \"with\") then",
                "        result.oneway = 1",
                "    else",
                "         result.oneway = 2",
                "    end",
                "",
                "end",
                "",
                "",
                "function default_parameters()",
                "    return " + profile.DefaultParameters.ToLuaTable(),
                "end",
                "",
                ""
            );

            impl += "\n\n" + CreateMembershipPreprocessor() + "\n\n";


            var profiles = new List<string>();
            foreach (var (name, subParams) in profile.Profiles)
            {
                var functionName = profile.Name + "_" + name;

                subParams.TryGetValue("description", out var description);
                profiles.Add(
                    string.Join(",\n    ",
                        $"    name = \"{name}\"",
                        "    function_name = \"profile_" + functionName + "\"",
                        "    metric = \"custom\""
                    )
                );

                impl += string.Join("\n",
                    "",
                    "--[[",
                    description,
                    "]]",
                    "function profile_" + functionName + "(tags, result)",
                    "    local parameters = default_parameters()",
                    ""
                );

                foreach (var (paramName, value) in subParams)
                {
                    impl += $"    parameters.{paramName.TrimStart('#').FunctionName()} = {value.Pretty()}\n";
                }

                impl += "    " + profile.Name + "(parameters, tags, result)\n";
                impl += "end\n";
            }

            impl += "\n\n\n";
            impl += "profiles = {\n    {\n" +
                    string.Join("\n    },\n    {\n    ", profiles) + "\n    }\n}";

            _code.Add(impl);
        }

        public string ToLua()
        {
            var deps = _deps.ToList();
            deps.Add("unitTest");
            deps.Add("unitTestProfile");
            deps.Add("inv");
            deps.Add("double_compare");
            deps.Sort();
            var code = deps.Select(d => BasicFunctions[d]).ToList();

            var keys = _neededKeys.Select(key => "\"" + key + "\"");
            code.Add("\n\nprofile_whitelist = {" + string.Join(", ", keys) + "}");

            code.AddRange(_code);


            return string.Join("\n\n\n", code);
        }
    }


    public static class StringExtensions
    {
        public static string ToLuaTable(this Dictionary<string, string> tags)
        {
            var contents = tags.Select(kv =>
            {
                var (key, value) = kv;
                var left = "[\"" + key + "\"]";

                if (Regex.IsMatch(key, "^[a-zA-Z][_a-zA-Z-9]*$"))
                {
                    left = key;
                }

                return $"{left} =  \"{value}\"";
            });
            return "{" + string.Join(", ", contents) + "}";
        }

        public static string ToLuaTable(this Dictionary<string, object> tags)
        {
            var contents = tags.Select(kv =>
            {
                var (key, value) = kv;
                var left = "[\"" + key + "\"]";

                if (Regex.IsMatch(key, "^[a-zA-Z][_a-zA-Z-9]*$"))
                {
                    left = key;
                }

                return $"{left} =  {value.Pretty()}";
            });
            return "{" + string.Join(", ", contents) + "}";
        }

        public static string FunctionName(this string s)
        {
            return s.Replace(" ", "_").Replace(".", "_").Replace("-", "_");
        }
    }
}