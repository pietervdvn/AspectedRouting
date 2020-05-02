using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Language;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Functions;
using AspectedRouting.Language.Typ;

namespace AspectedRouting.IO.itinero1
{
    public partial class LuaPrinter
    {
        private readonly Context _context;

        public LuaPrinter(Context context)
        {
            _context = context;
        }
        
        private void CreateMembershipPreprocessor(ProfileMetaData profile)
        {
            var memberships = Analysis.MembershipMappingsFor(profile, _context);

            foreach (var (calledInFunction, membership) in memberships)
            {
                var funcMetaData = new AspectMetadata(
                    membership,
                    "relation_preprocessing_for_" + calledInFunction.FunctionName(),
                    "Function preprocessing needed for aspect " + calledInFunction +
                    ", called by the relation preprocessor",
                    "Generator", "", "NA"
                );


                AddFunction(funcMetaData);
            }


            var func = new List<string>
            {
                "",
                "",
                "-- Processes the relation. All tags which are added to result.attributes_to_keep will be copied to 'attributes' of each individual way",
                "function relation_tag_processor(relation_tags, result)",
                "    local parameters = {}",
                "    local subresult = {}",
                "    local matched = false",
                "    result.attributes_to_keep = {}"
            };

            foreach (var (calledInFunction, _) in memberships)
            {
                foreach (var (behaviourName, parameters) in profile.Behaviours)
                {
                    var preProcName = "relation_preprocessing_for_" + calledInFunction.FunctionName();

                    func.Add("");
                    func.Add("");
                    func.Add("    subresult.attributes_to_keep = {}");
                    func.Add("    parameters = default_parameters()");
                    func.Add(ParametersToLua(parameters));
                    func.Add($"    matched = {preProcName}(parameters, relation_tags, subresult)");
                    func.Add("    if (matched) then");
                    var tagKey = "_relation:" + behaviourName.FunctionName() + ":" + calledInFunction.FunctionName();
                    _neededKeys.Add("_relation:"+calledInFunction.FunctionName()); // Slightly different then tagkey!
                    func.Add($"        result.attributes_to_keep[\"{tagKey}\"] = \"yes\"");
                    func.Add("    end");
                }
            }

            func.Add("end");
            

            _code.Add(string.Join("\n", func));
        }



        /// <summary>
        /// Adds the necessary called functions and the profile main entry point
        /// </summary>
        public void AddProfile(ProfileMetaData profile)
        {
            var defaultParameters = "\n";
            foreach (var (name, (types, inFunction)) in profile.UsedParameters(_context))
            {
                defaultParameters += $"{name}: {string.Join(", ", types)}\n" +
                                     $"    Used in {inFunction}\n";
            }

            CreateMembershipPreprocessor(profile);


            var impl = string.Join("\n",
                "",
                "",
                $"name = \"{profile.Name}\"",
                "normalize = false",
                "vehicle_type = {" + string.Join(", ", profile.VehicleTyps.Select(s => "\"" + s + "\"")) + "}",
                "meta_whitelist = {" + string.Join(", ", profile.Metadata.Select(s => "\"" + s + "\"")) + "}",
                "",
                "",
                "",
                "--[[",
                profile.Name,
                "This is the main function called to calculate the access, oneway and speed.",
                "Comfort is calculated as well, based on the parameters which are padded in",
                "",
                "Created by " + profile.Author,
                "Originally defined in " + profile.Filename,
                "Used parameters: " + defaultParameters.Indent(),
                "]]",
                "function " + profile.Name + "(parameters, tags, result)",
                "",
                "    -- initialize the result table on the default values",
                "    result.access = 0",
                "    result.speed = 0",
                "    result.factor = 1",
                "    result.direction = 0",
                "    result.canstop = true",
                "    result.attributes_to_keep = {}",
                "",
                "    local access = " + ToLua(profile.Access),
                "    if (access == nil or access == \"no\") then",
                "         return",
                "    end",
                "    tags.access = access",
                "    local oneway = " + ToLua(profile.Oneway),
                "    tags.oneway = oneway",
                "    local speed = " + ToLua(profile.Speed),
                "    tags.speed = speed",
                "    local distance = 1 -- the weight per meter for distance travelled is, well, 1m/m",
                "");

            impl +=
                "\n    local priority = \n        ";

            var weightParts = new List<string>();
            foreach (var (parameterName, expression) in profile.Priority)
            {
                var priorityPart = ToLua(new Parameter(parameterName)) + " * ";

                var subs = new Curry(Typs.Tags, new Var(("a"))).UnificationTable(expression.Types.First());
                if (subs != null && subs.TryGetValue("$a", out var resultType) &&
                    (resultType.Equals(Typs.Bool) || resultType.Equals(Typs.String)))
                {
                    priorityPart += "parse(" + ToLua(expression) + ")";
                }
                else
                {
                    priorityPart += ToLua(expression);
                }

                weightParts.Add(priorityPart);
            }

            impl += string.Join(" + \n        ", weightParts);

            impl += string.Join("\n",
                "",
                "",
                "",
                "    -- put all the values into the result-table, as needed for itinero",
                "    result.access = 1",
                "    result.speed = speed",
                "    result.factor = priority",
                "",
                "    if (oneway == \"both\") then",
                "        result.oneway = 0",
                "    elseif (oneway == \"with\") then",
                "        result.oneway = 1",
                "    else",
                "         result.oneway = 2",
                "    end",
                "",
                "end",
                "",
                "",
                "function default_parameters()",
                "    local parameters = {}",
                ParametersToLua(profile.DefaultParameters),
                "    return parameters",
                "end",
                "",
                ""
            );


            var profiles = new List<string>();
            foreach (var (name, subParams) in profile.Behaviours)
            {
                impl += BehaviourFunction(profile, name, subParams, profiles);
            }

            impl += "\n\n\n";
            impl += "profiles = {\n    {\n" +
                    string.Join("\n    },\n    {\n    ", profiles) + "\n    }\n}";

            _code.Add(impl);
        }

        private string BehaviourFunction(ProfileMetaData profile,
            string name,
            Dictionary<string, IExpression> subParams, List<string> profiles)
        {
            var functionName = profile.Name + "_" + name;

            subParams.TryGetValue("description", out var description);
            profiles.Add(
                string.Join(",\n    ",
                    $"    name = \"{name}\"",
                    "    function_name = \"profile_" + functionName + "\"",
                    "    metric = \"custom\""
                )
            );

            AddDep("remove_relation_prefix");
            var impl = string.Join("\n",
                "",
                "--[[",
                description,
                "]]",
                "function profile_" + functionName + "(tags, result)",
                $"    tags = remove_relation_prefix(tags, \"{name.FunctionName()}\")",
                "    local parameters = default_parameters()",
                ""
            );

            impl += ParametersToLua(subParams);

            impl += "    " + profile.Name + "(parameters, tags, result)\n";
            impl += "end\n";
            return impl;
        }

        /// <summary>
        /// `local parameters = default_parameters()` must still be invoked by caller!
        /// </summary>
        /// <param name="subParams"></param>
        /// <returns></returns>
        private string ParametersToLua(Dictionary<string, IExpression> subParams)
        {
            
            var impl = "";
            foreach (var (paramName, value) in subParams)
            {
                if (paramName.Equals("description"))
                {
                    continue;
                }
                impl += $"    parameters.{paramName.TrimStart('#').FunctionName()} = {ToLua(value)}\n";
            }

            return impl;
        }
    }
}