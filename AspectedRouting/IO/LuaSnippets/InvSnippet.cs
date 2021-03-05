using System.Collections.Generic;
using AspectedRouting.Language;

namespace AspectedRouting.IO.LuaSnippets
{
    public class InvSnippet : LuaSnippet
    {
        public InvSnippet() : base(Funcs.Inv) { }
        public override string Convert(LuaSkeleton.LuaSkeleton lua, string assignTo, List<IExpression> args)
        {

            var result = Snippets.Convert(lua, assignTo, args[0]);
            result += "    "+ assignTo +" = 1 / " + assignTo;
            return result;
        }
    }
}