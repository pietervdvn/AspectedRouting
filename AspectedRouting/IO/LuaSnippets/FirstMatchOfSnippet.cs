using System;
using System.Collections.Generic;
using System.Linq;
using AspectedRouting.IO.LuaSkeleton;
using AspectedRouting.Language;
using AspectedRouting.Language.Functions;
using AspectedRouting.Language.Typ;
using static AspectedRouting.Language.Deconstruct;
namespace AspectedRouting.IO.LuaSnippets
{
    public class FirstMatchOfSnippet : LuaSnippet
    {
        public FirstMatchOfSnippet() : base(Funcs.FirstOf) { }
        public override string Convert(LuaSkeleton.LuaSkeleton lua, string assignTo, List<IExpression> args)
        {
            var c = lua.Context;

            if (!(args[0].Evaluate(c) is List<IExpression> order)) {
                return null;
            }

            List<Mapping> mappings = new List<Mapping>();
            if (!UnApply(
                IsFunc(Funcs.StringStringToTags),
                IsMapping(mappings)
            ).Invoke(args[1])) {
                return null;
            }

            if (mappings.Count != 1) {
                throw new Exception("Multiple possible implementations at this point - should not happen");
            }

            if (mappings.Count == 0) {
                
            }
            var mapping = mappings.First();
            var tags = args[2];

            var varName = "tags";

            var result = "";
            if (tags is LuaLiteral literal) {
                varName = literal.Lua;
            }
            else {
                result += Snippets.Convert(lua, "tags", tags);
            }

            // We _reverse_ the order, so that the _most_ important one is at the _bottom_
            // The most important one will then _overwrite_ the result value
            order.Reverse();
            foreach (var t in order) {
                if (!(t.Evaluate(c) is string key)) {
                    return null;
                }

                var func = mapping.StringToResultFunctions[key];
                    
                result += "if (" + varName + "[\"" + key + "\"] ~= nil) then\n";
                result += "    "+Snippets.Convert(lua, assignTo, func.Apply(new LuaLiteral(Typs.String, "tags[\""+key+"\"]"))).Indent();
                result += "\n";
                result += "end\n";
                // note: we do not do an 'elseif' as we have to fallthrough
                if (result.Contains("tags[\"nil\"]")) {
                    Console.WriteLine("EUHM");
                }
            }

            return result;

        }
    }
}