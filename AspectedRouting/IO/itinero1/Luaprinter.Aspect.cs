using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Language;
using AspectedRouting.Language.Expression;

namespace AspectedRouting.IO.itinero1
{
    public partial class LuaPrinter
    {
        private readonly HashSet<string> _alreadyAddedFunctions = new HashSet<string>();

        public void AddFunction(AspectMetadata meta)
        {
            if (_alreadyAddedFunctions.Contains(meta.Name))
            {
                // already added
                return;
            }

            _alreadyAddedFunctions.Add(meta.Name);

            var possibleTags = meta.PossibleTags() ?? new Dictionary<string, List<string>>();
            var numberOfCombinations = possibleTags.Values.Select(lst => 1 + lst.Count).Multiply();

            var usedParams = meta.UsedParameters();

            var funcNameDeclaration = "";

            meta.Visit(e =>
            {
                if (e is Function f && f.Name.Equals(Funcs.MemberOf.Name))
                {
                    funcNameDeclaration = $"\n    local funcName = \"{meta.Name.FunctionName()}\"";
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
                "function " + meta.Name.FunctionName() + "(parameters, tags, result)" + funcNameDeclaration,
                "    return " + ToLua(meta.ExpressionImplementation),
                "end"
            );

            _code.Add(impl);
            foreach (var k in possibleTags.Keys)
            {
                _neededKeys.Add(k); // To generate a whitelist of OSM-keys that should be kept
            }
        }
    }
}