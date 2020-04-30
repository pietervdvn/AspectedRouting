using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using AspectedRouting.Functions;
using AspectedRouting.Typ;
using static AspectedRouting.Functions.Funcs;
using Type = AspectedRouting.Typ.Type;

namespace AspectedRouting.IO
{
    public static class JsonParser
    {
        public static AspectMetadata AspectFromJson(Context c, string json, string fileName)
        {
            try
            {
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("defaults", out _))
                {
                    // this is a profile
                    return null;
                }

                return doc.RootElement.ParseAspect(fileName, c);
            }
            catch (Exception e)
            {
                throw new Exception("In the file " + fileName, e);
            }
        }

        public static ProfileMetaData ProfileFromJson(Context c, string json, FileInfo f)
        {
            try
            {
                var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("defaults", out _))
                {
                    return null;
                    // this is an aspect
                }

                return ParseProfile(doc.RootElement, c, f);
            }
            catch (Exception e)
            {
                throw new Exception("In the file " + f, e);
            }
        }


        private static readonly IExpression _mconst =
            new Apply(new Apply(Funcs.EitherFunc, Funcs.Id), Funcs.Const);

        private static readonly IExpression _mappingWrapper =
            new Apply(new Apply(Funcs.EitherFunc, Funcs.Id), StringStringToTags);

        private static IExpression ParseMapping(IEnumerable<JsonProperty> allArgs, Context context)
        {
            var keys = new List<string>();
            var exprs = new List<IExpression>();

            foreach (var prop in allArgs)
            {
                if (prop.Name.Equals("#"))
                {
                    continue;
                }

                keys.Add(prop.Name);
                var argExpr = ParseExpression(prop.Value, context);
                IExpression mappingWithOptArg;
                if (argExpr.Types.Count() == 1 && argExpr.Types.First().Equals(Typs.String))
                {
                    mappingWithOptArg =
                        Either(Funcs.Id, Funcs.Eq, argExpr);
                }
                else
                {
                    mappingWithOptArg = new Apply(_mconst, argExpr);
                }

                exprs.Add(mappingWithOptArg);
            }

            try
            {
                var simpleMapping = new Mapping(keys, exprs);
                return new Apply(_mappingWrapper, simpleMapping);
            }
            catch (Exception e)
            {
                throw new Exception("While constructing a mapping with members " + string.Join(", ", exprs) +
                                    ": " + e.Message, e);
            }
        }

        private static IExpression ParseExpression(this JsonElement e, Context context)
        {
            if (e.ValueKind == JsonValueKind.Object)
            {
                // Parse an actual function
                var funcCall = e.EnumerateObject().Where(v => v.Name.StartsWith("$")).ToList();
                var allArgs = e.EnumerateObject().Where(v => !v.Name.StartsWith("$")).ToList();

                if (funcCall.Count > 2)
                {
                    throw new ArgumentException("Multiple calls defined in object " + e);
                }

                if (funcCall.Count == 1)
                {
                    return ParseFunctionCall(context, funcCall, allArgs);
                }

                // Funccall has no elements: this is a mapping of strings or tags onto a value

                return ParseMapping(allArgs, context);
            }

            if (e.ValueKind == JsonValueKind.Array)
            {
                var exprs = e.EnumerateArray().Select(json =>
                    Either(Funcs.Id, Funcs.Const, json.ParseExpression(context)));
                var list = new Constant(exprs);
                return Either(Funcs.Id, Funcs.ListDot, list);
            }

            if (e.ValueKind == JsonValueKind.Number)
            {
                if (e.TryGetDouble(out var d))
                {
                    return new Constant(d);
                }

                if (e.TryGetInt32(out var i))
                {
                    return new Constant(i);
                }
            }

            if (e.ValueKind == JsonValueKind.True)
            {
                return new Constant(Typs.Bool, "yes");
            }

            if (e.ValueKind == JsonValueKind.False)
            {
                return new Constant(Typs.Bool, "no");
            }

            if (e.ValueKind == JsonValueKind.String)
            {
                var s = e.GetString();
                if (s.StartsWith("$"))
                {
                    var bi = BuiltinByName(s);

                    if (bi != null)
                    {
                        return Either(Funcs.Dot, Funcs.Id, bi);
                    }

                    var definedFunc = context.GetFunction(s);
                    return Either(Funcs.Dot, Funcs.Id, new FunctionCall(s, definedFunc.Types));
                }

                if (s.StartsWith("#"))
                {
                    // This is a parameter, the type of it is free
                    return new Parameter(s);
                }

                return new Constant(s);
            }


            throw new Exception("Could not parse " + e);
        }

        private static IExpression ParseFunctionCall(Context context, IReadOnlyCollection<JsonProperty> funcCall,
            IEnumerable<JsonProperty> allArgs)
        {
            var funcName = funcCall.First().Name;

            var func = BuiltinByName(funcName);

            // The list where all the arguments are collected
            var args = new List<IExpression>();


            // First argument of the function is the value of this property, e.g.
            // { "$f": "xxx", "a2":"yyy", "a3":"zzz" }
            var firstArgument = ParseExpression(funcCall.First().Value, context);


            // Cheat for the very special case 'mustMatch'
            if (func.Equals(Funcs.MustMatch))
            {
                // It gets an extra argument injected
                var neededKeys = firstArgument.PossibleTags().Keys.ToList();
                var neededKeysArg = new Constant(new ListType(Typs.String), neededKeys);
                args.Add(neededKeysArg);
                args.Add(firstArgument);
                return func.Apply(args);
            }

            args.Add(firstArgument);

            var allExprs = allArgs
                .Where(kv => !kv.NameEquals("#")) // Leave out comments
                .ToDictionary(kv => kv.Name, kv => kv.Value.ParseExpression(context));


            if (allExprs.Count > 1)
            {
                if (func.ArgNames == null || func.ArgNames.Count < 2)
                    throw new ArgumentException("{funcName} does not specify argument names");

                foreach (var argName in func.ArgNames)
                {
                    args.Add(allExprs[argName]);
                }
            }
            else if (allExprs.Count == 1)
            {
                args.Add(allExprs.Single().Value);
            }

            return Either(Funcs.Id, Funcs.Dot, func).Apply(args);
        }

        private static IExpression GetTopLevelExpression(this JsonElement root, Context context)
        {
            IExpression mapping = null;
            if (root.TryGetProperty("value", out var j))
            {
                // The expression is placed in the default 'value' location
                mapping = j.ParseExpression(context);
            }


            // We search for the function call with '$'
            foreach (var prop in root.EnumerateObject())
            {
                if (!prop.Name.StartsWith("$")) continue;


                var f = (IExpression) BuiltinByName(prop.Name);
                if (f == null)
                {
                    throw new KeyNotFoundException("The builtin function " + f + " was not found");
                }

                var fArg = prop.Value.ParseExpression(context);

                if (fArg == null)
                {
                    throw new ArgumentException("Could not type expression " + prop);
                }


                if (mapping != null)
                {
                    // This is probably a firstOrderedVersion, a default, or some other function that should be applied
                    return
                        new Apply(
                            Either(Funcs.Id, Funcs.Dot, new Apply(f, fArg)), mapping
                        );
                }

                // Cheat for the very special case 'mustMatch'
                if (f.Equals(Funcs.MustMatch))
                {
                    // It gets an extra argument injected
                    var neededKeys = fArg.PossibleTags().Keys.ToList();
                    var neededKeysArg = new Constant(new ListType(Typs.String), neededKeys);
                    f = f.Apply(new[] {neededKeysArg});
                }

                var appliedDot = new Apply(new Apply(Funcs.Dot, f), fArg);
                var appliedDirect = new Apply(f, fArg);

                if (!appliedDot.Types.Any())
                {
                    // Applied dot doesn't work out, so we return the other one
                    return appliedDirect;
                }

                if (!appliedDirect.Types.Any())
                {
                    return appliedDot;
                }

                var eithered = new Apply(new Apply(Funcs.EitherFunc, appliedDot), appliedDirect);


                // We apply the builtin function through a dot
                return eithered;
            }


            throw new Exception(
                "No top level reducer found. Did you forget the '$' in the reducing function? Did your forget 'value' to add the mapping?");
        }

        private static IExpression ParseProfileProperty(JsonElement e, Context c, string property)
        {
            try
            {
                var prop = e.GetProperty(property);
                return ParseExpression(prop, c)
                    .Specialize(new Curry(Typs.Tags, new Var("a")))
                    .Optimize();
            }
            catch (Exception exc)
            {
                throw new Exception("While parsing the property " + property, exc);
            }
        }

        private static Dictionary<string, object> ParseParameters(this JsonElement e)
        {
            var ps = new Dictionary<string, object>();
            foreach (var obj in e.EnumerateObject())
            {
                var nm = obj.Name.TrimStart('#');
                switch (obj.Value.ValueKind)
                {
                    case JsonValueKind.String:
                        ps[nm] = obj.Value.ToString();
                        break;
                    case JsonValueKind.Number:
                        ps[nm] = obj.Value.GetDouble();
                        break;
                    case JsonValueKind.True:
                        ps[nm] = "yes";
                        break;
                    case JsonValueKind.False:
                        ps[nm] = "no";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return ps;
        }

        public static ProfileMetaData ParseProfile(this JsonElement e, Context context, FileInfo filepath)
        {
            var name = e.Get("name");
            var author = e.TryGet("author");
            if (filepath != null && !(name + ".json").ToLower().Equals(filepath.Name.ToLower()))
            {
                throw new ArgumentException($"Filename does not match the defined name: " +
                                            $"filename is {filepath.Name}, declared name is {name}");
            }

            var vehicleTypes = e.GetProperty("vehicletypes").EnumerateArray().Select(
                el => el.GetString()).ToList();
            var metadata = e.GetProperty("metadata").EnumerateArray().Select(
                el => el.GetString()).ToList();


            var access = ParseProfileProperty(e, context, "access").Finalize();
            var oneway = ParseProfileProperty(e, context, "oneway").Finalize();
            var speed = ParseProfileProperty(e, context, "speed").Finalize();


            IExpression TagsApplied(IExpression x)
            {
                return new Apply(x, new Constant(new Dictionary<string, string>()));
            }

            context.AddFunction("speed",
                new AspectMetadata(TagsApplied(speed), "speed", "The speed of this profile", author, "", filepath.Name,
                    true));
            context.AddFunction("access",
                new AspectMetadata(TagsApplied(access), "access", "The access of this profile", author, "",
                    filepath.Name,
                    true));
            context.AddFunction("oneway",
                new AspectMetadata(TagsApplied(oneway), "oneway", "The oneway of this profile", author, "",
                    filepath.Name,
                    true));
            context.AddFunction("distance",
                new AspectMetadata(new Constant(1), "distance", "The distance travelled of this profile", author, "",
                    filepath.Name,
                    true));


            var weights = new Dictionary<string, IExpression>();
            var weightProperty = e.GetProperty("weight");
            foreach (var prop in weightProperty.EnumerateObject())
            {
                var parameter = prop.Name.TrimStart('#');
                var factor = ParseExpression(prop.Value, context).Finalize();
                weights[parameter] = factor;
            }

            var profiles = new Dictionary<string, Dictionary<string, object>>();

            foreach (var profile in e.GetProperty("profiles").EnumerateObject())
            {
                profiles[profile.Name] = ParseParameters(profile.Value);
            }

            return new ProfileMetaData(
                name,
                e.Get("description"),
                author,
                filepath?.DirectoryName ?? "unknown",
                vehicleTypes,
                e.GetProperty("defaults").ParseParameters(),
                profiles,
                access,
                oneway,
                speed,
                weights,
                metadata
            );
        }

        private static AspectMetadata ParseAspect(this JsonElement e, string filepath, Context context)
        {
            var expr = GetTopLevelExpression(e, context);


            var targetTypes = new List<Type>();
            foreach (var t in expr.Types)
            {
                var a = Var.Fresh(t);
                var b = Var.Fresh(new Curry(a, t));

                if (t.Unify(new Curry(Typs.Tags, a)) != null &&
                    t.Unify(new Curry(Typs.Tags, new Curry(a, b))) == null
                ) // Second should not  match
                {
                    // The target type is 'Tags -> a', where a is NOT a curry
                    targetTypes.Add(t);
                }
            }

            if (targetTypes.Count == 0)
            {
                throw new ArgumentException("The top level expression has types:\n" +
                                            string.Join("\n    ", expr.Types) +
                                            "\nwhich can not be specialized into a form suiting `tags -> a`\n" + expr);
            }

            var exprSpec = expr.Specialize(targetTypes);
            if (expr == null)
            {
                throw new Exception("Could not specialize the expression " + expr + " to one of the target types " +
                                    string.Join(", ", targetTypes));
            }

            expr = exprSpec.Finalize();

            if (expr.Finalize() == null)
            {
                throw new NullReferenceException("The finalized form of expression `" + exprSpec + "` is null");
            }

            var name = e.Get("name");
            if (expr.Types.Count() > 1)
            {
                throw new ArgumentException("The aspect " + name + " is ambigous, it matches multiple types: " +
                                            string.Join(", ", expr.Types));
            }

            if (filepath != null && !(name + ".json").ToLower().Equals(filepath.ToLower()))
            {
                throw new ArgumentException($"Filename does not match the defined name: " +
                                            $"filename is {filepath}, declared name is {name}");
            }

            return new AspectMetadata(
                expr,
                name,
                e.Get("description"),
                e.TryGet("author"),
                e.TryGet("unit"),
                filepath ?? "unknown"
            );
        }


        private static string Get(this JsonElement json, string key)
        {
            if (json.TryGetProperty(key, out var p))
            {
                return p.GetString();
            }

            throw new ArgumentException($"The obligated property {key} is missing");
        }

        private static string TryGet(this JsonElement json, string key)
        {
            if (json.TryGetProperty(key, out var p))
            {
                return p.GetString();
            }

            return null;
        }
    }
}