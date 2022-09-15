using System;
using System.Collections.Generic;
using System.Linq;
using AspectedRouting.IO.LuaSkeleton;
using AspectedRouting.Language;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Functions;
using AspectedRouting.Language.Typ;
using static AspectedRouting.Language.Deconstruct;

namespace AspectedRouting.IO.LuaSnippets
{
    public abstract class ListFoldingSnippet : LuaSnippet
    {
        private readonly string _neutralValue;

        public ListFoldingSnippet(Function f, string neutralValue) : base(f)
        {
            _neutralValue = neutralValue;
        }

        public override string Convert(LuaSkeleton.LuaSkeleton lua, string assignTo, List<IExpression> args)
        {
            
            // Multiply multiplies a list of values - we thus have to handle _each_ arg
            // Note: we get a single argument which is an expression resulting in a list of values

            {
                var mappings = new List<Mapping>();
                var arg = new List<IExpression>();
                if (args.Count == 1 && UnApply(UnApply(
                            IsFunc(Funcs.StringStringToTags),
                            IsMapping(mappings)),
                        Assign(arg)
                    ).Invoke(args[0]))
                {
                    var mapping = mappings.First();

                    var result = assignTo + " = " + _neutralValue + "\n";
                    var mappingArg = arg.First();
                    if (!Equals(mappingArg.Types.First(), Typs.Tags))
                    {
                        return null;
                    }

                    string tags;
                    if (mappingArg is LuaLiteral literal)
                    {
                        tags = literal.Lua;
                    }
                    else
                    {
                        tags = lua.FreeVar("tags");
                        result += "local " + tags + " = nil\n";
                        result += Snippets.Convert(lua, tags, mappingArg);
                    }

                    var m = lua.FreeVar("m");
                    result += "    local " + m + " = nil\n";

                    foreach (var (key, func) in mapping.StringToResultFunctions)
                    {
                        result += "if (" + tags + "[\"" + key + "\"] ~= nil) then\n";
                        result += m + " = nil\n";
                        result += "    " +
                                  Snippets.Convert(lua, m,
                                      func.Apply(new LuaLiteral(Typs.String, tags + "[\"" + key + "\"]"))).Indent() +
                                  "\n";
                        result += "\n\n    if (" + m + " ~= nil) then\n        " +
                                  Combine(assignTo, m) +
                                  "\n    end\n";
                        result += "end\n";
                    }

                    return result;
                }
            }
            {
                // Print a 'listDot', assume 'tags' is the applied argument
                var arg = new List<IExpression>();
               
                var listDotArgs = new List<IExpression>();
                if (args.Count == 1 && UnApply(
                        UnApply(IsFunc(Funcs.ListDot),
                            Assign(listDotArgs)),
                        Assign(arg)
                    ).Invoke(args[0]))
                {
                    var listDotArg = arg.First();
                    if (!(listDotArgs.First().Evaluate(lua.Context) is List<IExpression> functionsToApply))
                    {
                        return null;
                    }

                    var result = "    " + assignTo + " = " + _neutralValue + "\n";
                    string tags;
                    if (listDotArg is LuaLiteral literal)
                    {
                        tags = literal.Lua;
                    }
                    else
                    {
                        tags = lua.FreeVar("tags");
                        result += "    local " + tags + "\n";
                        result += Snippets.Convert(lua, tags, listDotArg);
                    }

                    var m = lua.FreeVar("m");
                    result += "    local " + m + "\n";
                    foreach (var func in functionsToApply)
                    {
                        result += "    " + m + " = nil\n";
                        var subMapping = ExtractSubMapping(func);
                        if (subMapping != null)
                        {
                            var (key, f) = subMapping.Value;
                            var e = f.Apply(new LuaLiteral(Typs.String, tags + "[\"" + key + "\"]"));
                            e = e.Optimize(out _);
                            result += Snippets.Convert(lua, m, e).Indent();
                        }
                        else
                        {
                            result += Snippets.Convert(lua, m, func.Apply(new LuaLiteral(Typs.Tags, "tags")));
                        }


                        result += "\n\n    if (" + m + " ~= nil) then\n        " + Combine(assignTo, m) + "\n    end\n";
                    }

                    return result;
                }
            }
            
            
            
            
                   {
                       
               
                var constantArgs = new List<Constant>();
                if (args.Count == 1 && IsConstant(constantArgs).Invoke(args[0]))
                {
                    if (!(constantArgs.First().Get() is List<IExpression> listItems))
                    {
                        return null;
                    }

                    var result = "    " + assignTo + " = " + _neutralValue + "\n";


                    var m = lua.FreeVar("m");
                    result += "    local " + m + "\n";
                    foreach (var listItem in listItems)
                    {
                        result += "    " + m + " = nil\n";
                        result += Snippets.Convert(lua, m, listItem).Indent();
                        result += "\n\n    if (" + m + " ~= nil) then\n        " + Combine(assignTo, m) + "\n    end\n";
                    }

                    return result;
                }
            }

      

            Console.Error.WriteLine("ListFoldingSnippet encountered an unsupported expression");
            
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Combine both values in lua - both are not nil
        /// </summary>
        /// <param name="assignTo"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract string Combine(string assignTo, string value);

        private static (string Key, IExpression Value)? ExtractSubMapping(IExpression app)
        {
            var mappings = new List<Mapping>();
            // ($dot $head) (stringToTag (mapping (mapping with single function)
            if (UnApply(
                UnApply(
                    IsFunc(Funcs.Dot),
                    IsFunc(Funcs.Head)),
                UnApply(
                    IsFunc(Funcs.StringStringToTags),
                    IsMapping(mappings)
                )
            ).Invoke(app)) {
                var mapping = mappings.First();
                if (mapping.StringToResultFunctions.Count == 1) {
                    var kv = mapping.StringToResultFunctions.ToList().First();
                    return (kv.Key, kv.Value);
                }
            }

            return null;
        }
    }
}