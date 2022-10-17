using System;
using System.Collections.Generic;
using AspectedRouting.Language;

namespace AspectedRouting.IO.LuaSnippets
{
    public class DefaultSnippet : LuaSnippet
    {
        public DefaultSnippet() : base(Funcs.Default) { }

        public override string Convert(LuaSkeleton.LuaSkeleton lua, string assignTo, List<IExpression> args)
        {
            var defaultValue = args[0];
            var func = args[1];
            var funcArg = args[2];

            return Snippets.Convert(lua, assignTo, func.Apply(funcArg))
                   + "\n"
                   + "if (" + assignTo + " == nil) then\n"
                   + "    " + assignTo + " = " + lua.ToLua(defaultValue) + "\n"
                   + "end";

        }
    }
}