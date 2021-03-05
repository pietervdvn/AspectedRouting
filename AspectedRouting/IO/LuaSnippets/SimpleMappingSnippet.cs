using System.Collections.Generic;
using System.Linq;
using AspectedRouting.IO.LuaSkeleton;
using AspectedRouting.Language;
using AspectedRouting.Language.Functions;
using AspectedRouting.Language.Typ;

namespace AspectedRouting.IO.LuaSnippets
{
    /// <summary>
    /// Applies a one-on-one-mapping (thus: a function converting one value into another one)
    /// </summary>
    public class SimpleMappingSnippet : LuaSnippet
    {
        private readonly Mapping _mapping;

        public SimpleMappingSnippet(Mapping mapping) : base(null)
        {
            _mapping = mapping;
        }

        public override string Convert(LuaSkeleton.LuaSkeleton lua, string assignTo, List<IExpression> args)
        {
            var arg = args[0];
            var v = lua.FreeVar("v");
            var vLua = new LuaLiteral(Typs.String, v);

            var mappings = new List<string>();
            foreach (var kv in _mapping.StringToResultFunctions) {
                var f = kv.Value;
                if (f.Types.First() is Curry) {
                    f = f.Apply(vLua);
                }
                mappings.Add("if (" + v + " == \"" + kv.Key + "\") then\n    " + assignTo + " = " + lua.ToLua(f));
            }


            return Utils.Lines(
                "local " + v,
                Snippets.Convert(lua, v, arg),
                string.Join("\nelse", mappings),
                "end"
            );
        }
    }
}