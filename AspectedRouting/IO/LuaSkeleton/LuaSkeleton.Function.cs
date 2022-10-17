using System;
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
            if (_alreadyAddedFunctions.Contains(meta.Name))
            {
                // already added
                return;
            }

            _alreadyAddedFunctions.Add(meta.Name);

            var possibleTags = meta.PossibleTags() ?? new Dictionary<string, HashSet<string>>();
            int numberOfCombinations;
            numberOfCombinations = possibleTags.Values.Select(lst => 1 + lst.Count).Multiply();

            var usedParams = meta.UsedParameters();

            var funcNameDeclaration = "";

            meta.Visit(e =>
            {
                if (e is Function f && f.Name.Equals(Funcs.MemberOf.Name))
                {
                    funcNameDeclaration = $"\n    local funcName = \"{meta.Name.AsLuaIdentifier()}\"";
                }

                return true;
            });

            var expression = Funcs.Either(Funcs.Id, Funcs.Const, meta.ExpressionImplementation)
                .Apply(new LuaLiteral(Typs.Tags, "tags"))
                .PruneTypes(t => !(t is Curry))
                .SpecializeToSmallestType()
                .Optimize(out _);
            if (!expression.Types.Any())
            {
                throw new Exception("Could not optimize expression with applied tags");
            }

            var ctx = Context;
            _context = _context.WithAspectName(meta.Name);

            var body = "";
            if (_useSnippets)
            {
                body = Utils.Lines(
                    "    local r = nil",
                    "    " + Snippets.Convert(this, "r", expression).Indent(),
                    "    return r"
                );
            }
            else
            {
                body = "    return " + ToLua(expression);
            }

            var impl = Utils.Lines(
                "--[[",
                meta.Description,
                "",
                "Unit: " + meta.Unit,
                "Created by " + meta.Author,
                "Uses tags: " + string.Join(", ", possibleTags.Keys),
                "Used parameters: " + string.Join(", ", usedParams),
                "Number of combintations: " + numberOfCombinations,
                "Returns values: ",
                "]]",
                "function " + meta.Name.AsLuaIdentifier() + "(tags, parameters)" + funcNameDeclaration,
                body,
                "end"
            );

            _context = ctx;
            _functionImplementations.Add(impl);
        }
    }
}