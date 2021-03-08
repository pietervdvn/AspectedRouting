using System.Collections.Generic;
using System.Linq;
using AspectedRouting.IO.itinero1;
using AspectedRouting.IO.LuaSnippets;
using AspectedRouting.Language;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Typ;

namespace AspectedRouting.IO.LuaSkeleton
{
    public partial class LuaSkeleton
    {
        public void AddFunction(AspectMetadata meta)
        {
            if (_alreadyAddedFunctions.Contains(meta.Name)) {
                // already added
                return;
            }

            _alreadyAddedFunctions.Add(meta.Name);

            var possibleTags = meta.PossibleTags() ?? new Dictionary<string, HashSet<string>>();
            int numberOfCombinations;
            numberOfCombinations = possibleTags.Values.Select(lst => 1 + lst.Count).Multiply();

            var usedParams = meta.UsedParameters();

            var funcNameDeclaration = "";

            meta.Visit(e => {
                if (e is Function f && f.Name.Equals(Funcs.MemberOf.Name)) {
                    funcNameDeclaration = $"\n    local funcName = \"{meta.Name.AsLuaIdentifier()}\"";
                }

                return true;
            });

            var expression = meta.ExpressionImplementation;

            var ctx = Context;
            _context = _context.WithAspectName(meta.Name);

            var body = "";
            if (_useSnippets) {
                if (expression.Types.First() is Curry c) {
                    expression = expression.Apply(new LuaLiteral(Typs.Tags, "tags"));
                }

                body = Utils.Lines(
                    "    local r = nil",
                    "    " + Snippets.Convert(this, "r", expression).Indent(),
                    "    return r"
                );
            }
            else {
                body = "    return " + ToLua(expression);
            }


            var impl = Utils.Lines(
                "--[[",
                meta.Description,
                "",
                "Unit: " + meta.Unit,
                "Created by " + meta.Author,
                "Originally defined in " + meta.Filepath,
                "Uses tags: " + string.Join(", ", possibleTags.Keys),
                "Used parameters: " + string.Join(", ", usedParams),
                "Number of combintations: " + numberOfCombinations,
                "Returns values: ",
                "]]",
                "function " + meta.Name.AsLuaIdentifier() + "(parameters, tags, result)" + funcNameDeclaration,
                body,
                "end"
            );

            _context = ctx;
            _functionImplementations.Add(impl);
        }
    }
}