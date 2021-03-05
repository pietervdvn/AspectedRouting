using System.Collections.Generic;
using AspectedRouting.IO.LuaSkeleton;
using AspectedRouting.Language;

namespace AspectedRouting.IO.LuaSnippets
{
    /// <summary>
    ///     The member-of snippet breaks the abstraction _completely_
    ///     It is tied heavily to the preprocessor and use the function name to determine which tag to check
    /// </summary>
    public class MemberOfSnippet : LuaSnippet
    {
        public MemberOfSnippet() : base(Funcs.MemberOf) { }

        public override string Convert(LuaSkeleton.LuaSkeleton lua, string assignTo, List<IExpression> args)
        {
            // Note how we totally throw away args[0]
            var tagsToken = args[1];

            var tags = "";
            var result = "";
            if (tagsToken is LuaLiteral lit) {
                tags = lit.Lua;
            }
            else {
                tags = lua.FreeVar("tags");
                result += "local " + tags + "\n";
                result += Snippets.Convert(lua, tags, tagsToken);
            }

            var r = lua.FreeVar("relationValue");
            result += "local " + r + " = " + tags + "[\"_relation:" + lua.Context.AspectName.Replace(".", "_") + "\"]\n";
            result += assignTo + " = " + r + " == \"yes\"";
            return result;
        }
    }
}