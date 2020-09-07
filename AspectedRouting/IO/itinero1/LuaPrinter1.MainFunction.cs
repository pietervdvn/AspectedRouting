using System.Collections.Generic;
using System.Linq;
using AspectedRouting.IO.LuaSkeleton;
using AspectedRouting.Language;
using AspectedRouting.Language.Functions;
using AspectedRouting.Language.Typ;

namespace AspectedRouting.IO.itinero1
{
    public partial class LuaPrinter1
    {


        private string GenerateMainProfileFunction()
        {

            var access = _skeleton.ToLua(_profile.Access);
            var oneway = _skeleton.ToLua(_profile.Oneway);
            var speed = _skeleton.ToLua(_profile.Speed);
            
            var impl = string.Join("\n",
                "",
                "",
                "--[[",
                _profile.Name,
                "This is the main function called to calculate the access, oneway and speed.",
                "Comfort is calculated as well, based on the parameters which are padded in",
                "",
                "Created by " + _profile.Author,
                "Originally defined in " + _profile.Filename,
                "]]",
                "function " + _profile.Name + "(parameters, tags, result)",
                "",
                "    -- initialize the result table on the default values",
                "    result.access = 0",
                "    result.speed = 0",
                "    result.factor = 1",
                "    result.direction = 0",
                "    result.canstop = true",
                "    result.attributes_to_keep = {}",
                "",
                "    local access = " + access,
                "    if (access == nil or access == \"no\") then",
                "         return",
                "    end",
                "    tags.access = access",
                "    local oneway = " + oneway,
                "    tags.oneway = oneway",
                "    local speed = " + speed,
                "    tags.speed = speed",
                "    local distance = 1 -- the weight per meter for distance travelled is, well, 1m/m",
                "");

            impl +=
                "\n    local priority = 0\n        ";

            foreach (var (parameterName, expression) in _profile.Priority)
            {
                var paramInLua = _skeleton.ToLua(new Parameter(parameterName));


                var exprInLua = _skeleton.ToLua(expression.Optimize(), forceFirstArgInDot: true);
                var resultTypes = expression.Types.Select(t => t.Uncurry().Last());
                if (resultTypes.Any(t => t.Name.Equals(Typs.Bool.Name)))
                {
                   _skeleton. AddDep("parse");
                    exprInLua = "parse(" + exprInLua + ")";
                }

                impl += "\n    " + string.Join("\n    ",
                            $"if({paramInLua} ~= 0) then",
                            $"    priority = priority + {paramInLua} * {exprInLua}",
                            "end"
                        );
            }


            impl += string.Join("\n",
                "",
                "",
                "    if (priority <= 0) then",
                "        result.access = 0",
                "        return",
                "    end",
                "",
                "    result.access = 1",
                "    result.speed = speed",
                "    result.factor = 1 / priority",
                "",
                "    if (oneway == \"both\") then",
                "        result.direction = 0",
                "    elseif (oneway == \"with\") then",
                "        result.direction = 1",
                "    elseif (oneway == \"against\") then",
                "         result.direction = 2",
                "    else",
                "        error(\"Unexpected value for oneway: \"..oneway)",
                "    end",
                "",
                "end"
            );

           return impl;
        }
        
        
        private (string functionName, string implementation) GenerateBehaviourFunction(
            string behaviourName,
            Dictionary<string, IExpression> behaviourParameters)
        {
            var referenceName = _profile.Name + "_" + behaviourName;
            var functionName = referenceName.AsLuaIdentifier();
            behaviourParameters.TryGetValue("description", out var description);

            _skeleton.AddDep("remove_relation_prefix");
            var impl = string.Join("\n",
                "",
                "--[[",
                description,
                "]]",
                "function behaviour_" + functionName + "(tags, result)",
                $"    tags = remove_relation_prefix(tags, \"{behaviourName.AsLuaIdentifier()}\")",
                "    local parameters = default_parameters()",
                "    parameters.name = \"" + referenceName + "\"",
                ""
            );

            impl += _parameterPrinter.DeclareParametersFor(behaviourParameters);

            impl += "    " + _profile.Name + "(parameters, tags, result)\n";
            impl += "end\n";
            return (functionName, impl);
        }
    }
}