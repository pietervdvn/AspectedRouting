using System;
using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Language;
using AspectedRouting.Language.Typ;
using static AspectedRouting.Language.Deconstruct;

namespace AspectedRouting.IO.LuaSnippets
{
    public class IfThenElseSnippet : LuaSnippet
    {
        public IfThenElseSnippet() : base(Funcs.If) { }

        public override string Convert(LuaSkeleton.LuaSkeleton lua, string assignTo, List<IExpression> args)
        {
            var cond = args[0].Optimize(out _);
            var ifTrue = args[1];
            IExpression ifElse = null;
            if (args.Count == 3)
            {
                ifElse = args[2];
            }


            {
                var fa = new List<IExpression>();
                if (UnApply(
                        IsFunc(Funcs.IsNull),
                        Assign(fa)
                        ).Invoke(cond))
                {

                    if (fa.First().ToString() == ifElse.ToString())
                    {
                        var result = "";

                        // We calculate the value that we need
                        result += Snippets.Convert(lua, assignTo, ifElse) + "\n";
                        result += "if (" + assignTo + " == nil) then\n";
                        result += "    " + Snippets.Convert(lua, assignTo, ifTrue).Indent();
                        result += "end\n";
                        return result;

                    }
                    throw new Exception("TODO optimize with default");
                }
            }

            {
                var c = lua.FreeVar("cond");
                var result = "";
                result += "local " + c + "\n";

                var isString = cond.Types.First().Equals(Typs.String);
                result += Snippets.Convert(lua, c, cond) + "\n";
                result += $"if ( {c} == true or {c} == \"yes\" ) then \n";
                result += "    " + Snippets.Convert(lua, assignTo, ifTrue).Indent();

                if (ifElse != null)
                {
                    result += "else\n";
                    result += "    " + Snippets.Convert(lua, assignTo, ifElse).Indent();
                }

                result += "end\n";
                return result;
            }
        }
    }
}