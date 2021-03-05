using System;
using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Language;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Functions;

namespace AspectedRouting.IO.LuaSnippets
{
    public class Snippets
    {
        private static readonly List<LuaSnippet> AllSnippets =
            new List<LuaSnippet> {
                new DefaultSnippet(),
                new FirstMatchOfSnippet(),
                new MultiplySnippet(),
                new SumSnippet(),
                new MaxSnippet(),
                new MinSnippet(),
                new IfThenElseDottedSnippet(),
                new InvSnippet(),
                new HeadSnippet(),
                new MemberOfSnippet()
            };

        private static readonly Dictionary<string, LuaSnippet> SnippetsIndex = AllSnippets.ToDictionary(
            snippet => snippet.ImplementsFunction.Name, snippet => snippet
        );

        public static string Convert(LuaSkeleton.LuaSkeleton lua, string assignTo, IExpression e)
        {

            var opt  = e.Optimize();
            if (!Equals(e.Types.First(), opt.Types.First())) {
                throw new Exception("Optimization went wrong!");
            }
            e = opt;
            var deconstructed = e.DeconstructApply();
            
            if (deconstructed != null){

                if (deconstructed.Value.f is Mapping m) {
                    return new SimpleMappingSnippet(m).Convert(lua, assignTo, deconstructed.Value.args);
                }
                
                if (deconstructed.Value.f is Function f
                    && SnippetsIndex.TryGetValue(f.Name, out var snippet)) {
                    var optimized = snippet.Convert(lua, assignTo, deconstructed.Value.args);
                    if (optimized != null) {
                        return optimized + "\n";
                    }
                }
            }

            try {


                return assignTo + " = " + lua.ToLua(e)+"\n";
            }
            catch (Exception err) {
                return "print(\"ERROR COMPILER BUG\");\n";
            }
        }
    }
}