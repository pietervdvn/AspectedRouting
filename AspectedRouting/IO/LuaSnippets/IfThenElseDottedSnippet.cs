using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Language;
using AspectedRouting.Language.Typ;

namespace AspectedRouting.IO.LuaSnippets
{
    public class IfThenElseDottedSnippet : LuaSnippet
    {
        public IfThenElseDottedSnippet() : base(Funcs.IfDotted) { }

        public override string Convert(LuaSkeleton.LuaSkeleton lua, string assignTo, List<IExpression> args)
        {
            var fCond = args[0].Optimize();
            var fValue = args[1];
            IExpression fElse = null;
            var arg = args[2];
            if (args.Count == 4) {
                arg = args[3];
                fElse = args[2];
            }

            var c = lua.FreeVar("cond");
            var result = "";
            result += "local " + c+"\n";
            var condApplied = fCond.Apply(arg);
            var isString = condApplied.Types.First().Equals(Typs.String);
            result += Snippets.Convert(lua, c, condApplied)+"\n";
            result += $"if ( {c} or {c} == \"yes\" ) then \n";
            result += "    " + Snippets.Convert(lua, assignTo, fValue.Apply(arg)).Indent() ;

            if (fElse != null) {
                result += "else\n";
                result += "    " + Snippets.Convert(lua, assignTo, fElse.Apply(arg)).Indent();
            }

            result += "end\n";
            return result;
        }
    }
}