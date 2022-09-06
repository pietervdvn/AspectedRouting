using System;
using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Language;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Functions;
using AspectedRouting.Language.Typ;

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
                new IfThenElseSnippet(),
                new IfThenElseDottedSnippet(),
                new InvSnippet(),
                new HeadSnippet(),
                new MemberOfSnippet(),
            //    new MustMatchSnippet()
            };

        private static readonly Dictionary<string, LuaSnippet> SnippetsIndex = AllSnippets.ToDictionary(
            snippet => snippet.ImplementsFunction.Name, snippet => snippet
        );

        public static string Convert(LuaSkeleton.LuaSkeleton lua, string assignTo, IExpression e)
        {

            var opt  = e.Optimize(out _);
            // Note that optimization might optimize to a _subtype_ of the original expresion - which is fine!
            var origType = e.Types.First();
            var optType = opt.Types.First();
            if (!origType.Equals(optType) && !origType.IsSuperSet(optType)) {
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