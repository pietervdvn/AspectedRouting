using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Language;
using AspectedRouting.Language.Typ;

namespace AspectedRouting.IO.LuaSnippets
{
    public class IfThenElseSnippet : LuaSnippet
    {
        public IfThenElseSnippet() : base(Funcs.If) { }

        public override string Convert(LuaSkeleton.LuaSkeleton lua, string assignTo, List<IExpression> args)
        {
            var cond = args[0].Optimize();
            var ifTrue = args[1];
            IExpression ifElse = null;
            if (args.Count == 3) {
                ifElse = args[2];
            }

            var c = lua.FreeVar("cond");
            var result = "";
            result += "local " + c+"\n";
            
            var isString = cond.Types.First().Equals(Typs.String);
            result += Snippets.Convert(lua, c, cond)+"\n";
            result += $"if ( {c} or {c} == \"yes\" ) then \n";
            result += "    " + Snippets.Convert(lua, assignTo, ifTrue).Indent() ;

            if (ifElse != null) {
                result += "else\n";
                result += "    " + Snippets.Convert(lua, assignTo, ifElse).Indent();
            }

            result += "end\n";
            return result;
        }
    }
}