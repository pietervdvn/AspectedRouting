using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;
using AspectedRouting.IO.itinero1;
using AspectedRouting.Language;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Functions;
using AspectedRouting.Language.Typ;
using static AspectedRouting.Language.Deconstruct;

namespace AspectedRouting.IO.LuaSkeleton
{
    public partial class LuaSkeleton
    {

        internal string ToLuaWithTags(IExpression bare)
        {
            var opt = bare.Apply(new LuaLiteral(Typs.Tags, "tags")).SpecializeToSmallestType().Optimize(out _);
            return this.ToLua(opt);
        }
        internal string ToLua(IExpression bare, string key = "nil", bool forceFirstArgInDot = false)
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
            ).Invoke(bare)) {
                AddDep(Funcs.FirstOf.Name);
                return "first_match_of(\n" +
                       "        " + ToLua(order.First(), key) + ",\n" +
                       ("\n" + MappingToLua((Mapping) collectedMapping.First())).Indent().Indent() +
                       ",\n        tags, result)";
            }

            if (UnApply(
                UnApply(
                    IsFunc(Funcs.MustMatch),
                    Assign(order))
                , UnApply(
                    IsFunc(Funcs.StringStringToTags),
                    Assign(collectedMapping))
            ).Invoke(bare)) {
                AddDep(Funcs.MustMatch.Name);
                return "must_match(" +
                       "        " + ToLua(order.First(), key) + "," +
                       ("\n" + MappingToLua((Mapping) collectedMapping.First())).Indent().Indent() +
                       ",\n        tags, result)";
            }

            if (UnApply(
                IsFunc(Funcs.MemberOf),
                Any
            ).Invoke(bare)) {
                AddDep("memberOf");
                return "memberOf(funcName, parameters, tags, result)";
            }

            {
                var name = new List<string>();
                var arg = new List<IExpression>();
                if (UnApply(
                        IsFunctionCall(name),
                        Assign(arg)
                    ).Invoke(bare))
                {
                    var called = _context.DefinedFunctions[name.First()];
                    if (called.ProfileInternal) {
                        return called.Name;
                    }

                    AddDependenciesFor(called);
                    AddFunction(called);
                    var usesParams = called.ExpressionImplementation.UsedParameters().Any();
                    if (usesParams)
                    {
                        return $"{name.First().Replace(".","_")}({ToLua(arg.First())}, parameters)";
                    }
                    return $"{name.First().Replace(".","_")}({ToLua(arg.First())})";
                }
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
            ) {
                var mapping = (Mapping) collectedMapping.First();
                var baseFunc = (Function) dottedFunction.First();
                AddDep(baseFunc.Name);
                AddDep("table_to_list");

                return baseFunc.Name +
                       "(table_to_list(tags, result, " +
                       ("\n" + MappingToLua(mapping)).Indent().Indent() +
                       "))";
            }


            // The expression might be a function which still expects a string (the value from the tag) as argument
            if (!(bare is Mapping) &&
                bare.Types.First() is Curry curr &&
                curr.ArgType.Equals(Typs.String)) {
                var applied = new Apply(bare, new Constant(curr.ArgType, ("tags", "\"" + key + "\"")));
                return ToLua(applied.Optimize(out _), key);
            }


            // The expression might consist of multiple nested functions
            var fArgs = bare.DeconstructApply();
            if (fArgs != null) {
                var (f, args) = fArgs.Value;
                
                if(f is Constant constant) {
                    return ConstantToLua(constant);
                }
                
                if (!(f is Function baseFunc)) {
                    throw new ArgumentException("Not a function: " + f);
                }

                if (baseFunc.Name.Equals(Funcs.Id.Name)) {
                    // This is an ugly hack
                    return ToLua(args.First());
                }

                if (baseFunc.Name.Equals(Funcs.Dot.Name)) {
                    if (args.Count == 1 )
                    {
                        return ToLua(args[0]);
                    }
                    
                    if (forceFirstArgInDot)
                    {
                        return ToLua(args[0]);
                    }

                    var argsAsLua = args.Select(arg => ToLua(arg, key)).ToList();
                    var fName = argsAsLua[0];
                    var actualArgs =
                        string.Join(",", argsAsLua.GetRange(1, argsAsLua.Count - 1));
                    return $"{fName}({actualArgs})";
                }

                AddDep(baseFunc.Name);

                var argExpressions = new List<string>();
                foreach (var arg in args) {
                    argExpressions.Add(ToLua(arg, key));
                }

                return baseFunc.Name + "(" + string.Join(", ", argExpressions) + ")";
            }


            var collected = new List<IExpression>();
            switch (bare) {
                case LuaLiteral lua:
                    return lua.Lua;
                case FunctionCall fc:
                    var called = _context.DefinedFunctions[fc.CalledFunctionName];
                    if (called.ProfileInternal) {
                        return called.Name;
                    }

                    AddDependenciesFor(called);
                    AddFunction(called);
                    return $"{fc.CalledFunctionName.AsLuaIdentifier()}(tags, parameters)";
                case Constant c:
                    return ConstantToLua(c);
                case Mapping m:
                    return MappingToLua(m).Indent();
                case Function f:
                    var fName = f.Name.TrimStart('$');
                    if (Funcs.Builtins.ContainsKey(fName)) {
                        AddDep(f.Name);
                    }
                    else {
                        var definedFunc = _context.DefinedFunctions[fName];
                        if (definedFunc.ProfileInternal) {
                            return f.Name;
                        }

                        AddFunction(definedFunc);
                    }

                    return f.Name;
                case Apply a when UnApply(IsFunc(Funcs.Const), Assign(collected))
                    .Invoke(a):
                    return ToLua(collected.First(), key);

                case Parameter p:
                    return $"parameters[\"{p.ParamName.AsLuaIdentifier()}\"]";
                default:
                    throw new Exception("Could not convert " + bare + " to a lua expression");
            }
        }

        public string MappingToLua(Mapping m)
        {
            var isConstant = true;
            var contents = m.StringToResultFunctions.Select(kv => {
                    var (key, expr) = kv;
                    var left = "[\"" + key + "\"]";

                    if (Regex.IsMatch(key, "^[a-zA-Z][_a-zA-Z-9]*$")) {
                        left = key;
                    }

                    var luaExpr = ToLua(expr, key);
                    if (luaExpr.Contains("tags")) {
                        isConstant = false;
                    }

                    return left + " = " + luaExpr;
                }
            );
            var mapping =
                "{\n    " +
                string.Join(",\n    ", contents) +
                "\n}";
            if (_staticTables && isConstant) {
                return AddConstant(mapping);
            }

            return mapping;
        }

        /// <summary>
        ///     Neatly creates a value expression in lua, based on a constant
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private string ConstantToLua(Constant c)
        {
            var o = c.Get();
            switch (o) {
                case LuaLiteral lua:
                    return lua.Lua;
                case IExpression e:
                    return ToLua(e);
                case int i:
                    return "" + i;
                case double d:
                    return "" + d;
                case string s:
                    return '"' + s.Replace("\"", "\\\"") + '"';
                case null:
                    return "nil";
                case ValueTuple<string, string> unpack:
                    return unpack.Item1 + "[" + unpack.Item2 + "]";
                case IEnumerable<object> ls:
                    var t = ((ListType) c.Types.First()).InnerType;
                    return "{" + string.Join(", ", ls.Select(obj => {
                        var objInConstant = new Constant(t, obj);
                        if (obj is Constant asConstant) {
                            objInConstant = asConstant;
                        }

                        return ConstantToLua(objInConstant);
                    })) + "}";
                default:
                    return o.ToString();
            }
        }
    }
}