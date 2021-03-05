using System.Collections.Generic;
using System.Linq;
using AspectedRouting.IO.LuaSkeleton;
using AspectedRouting.Language;
using AspectedRouting.Language.Functions;
using AspectedRouting.Language.Typ;
using static AspectedRouting.Language.Deconstruct;

namespace AspectedRouting.IO.LuaSnippets
{
    public class HeadSnippet : LuaSnippet
    {
        public HeadSnippet() : base(Funcs.Head) { }

        public override string Convert(LuaSkeleton.LuaSkeleton lua, string assignTo, List<IExpression> args)
        {
            var actualArgs = new List<IExpression>();
            var mappings = new List<Mapping>();
            if (UnApply(
                UnApply(IsFunc(Funcs.StringStringToTags),
                    IsMapping(mappings)),
                Assign(actualArgs)
            ).Invoke(args[0])) {
                var actualArg = actualArgs.First();
                var mapping = mappings.First();

                if (mapping.StringToResultFunctions.Count != 1) {
                    return null;
                }

                var (key, func) = mapping.StringToResultFunctions.ToList().First();
                var result = "";
                var tags = "";
                if (actualArg is LuaLiteral l) {
                    tags = l.Lua;
                }
                else {
                    tags = lua.FreeVar("tags");
                    result += "local " + tags+"\n";
                    result += Snippets.Convert(lua, tags, actualArg);
                }
                
                
                
                var v = lua.FreeVar("value");
                result += "local " + v + " = " + tags + "[\"" + key + "\"]\n";
                result += Snippets.Convert(lua, assignTo, func.Apply(new LuaLiteral(Typs.String, v)));
                return result;

            }

            return null;
        }
    }
}