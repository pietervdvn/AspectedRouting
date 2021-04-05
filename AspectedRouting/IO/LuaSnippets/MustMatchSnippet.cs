using System.Collections.Generic;
using AspectedRouting.IO.LuaSkeleton;
using AspectedRouting.Language;
using AspectedRouting.Language.Expression;

namespace AspectedRouting.IO.LuaSnippets
{
    public class MustMatchSnippet : LuaSnippet
    {
        public MustMatchSnippet() : base(Funcs.MustMatch) { }
        public override string Convert(LuaSkeleton.LuaSkeleton lua, string assignTo, List<IExpression> args)
        {
            var neededKeysExpr = args[0];
            var funcExpr = args[1];
            var tagsExpr = args[2];

            var result = "";
            var neededKeys = lua.FreeVar("neededKeys");
            var tags = "";
            if (tagsExpr is LuaLiteral literal) {
                tags = literal.Lua;
            }
            else {
               tags =  lua.FreeVar("tags");
               result += $"local {tags}";
               result += Snippets.Convert(lua, tags, tagsExpr);

            }
          
             
            result += $"local {neededKeys}\n";
            result += Snippets.Convert(lua, neededKeys, neededKeysExpr);
            var key = lua.FreeVar("key");
            var value = lua.FreeVar("value");
            result += $"for _, {key} in ipairs({neededKeys}) do\n";
            result += $"   local {value} = {tags}[{key}]\n";
            result += $"   if ({value} == nil) then\n";
            result += $"       -- The value is nil, so mustmatch probably fails...\n";
            
            throw new System.NotImplementedException();
        }
    }
}