using System.Collections.Generic;
using System.Linq;
using AspectedRouting.IO.itinero1;
using AspectedRouting.Language;
using AspectedRouting.Language.Expression;

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

            var possibleTags = meta.PossibleTags() ?? new Dictionary<string, List<string>>();
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

            var impl = string.Join("\n",
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
                "    return " + ToLua(meta.ExpressionImplementation),
                "end"
            );

            _functionImplementations.Add(impl);
        }
    }
}