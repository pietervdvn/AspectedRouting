using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Language;
using AspectedRouting.Language.Expression;

namespace AspectedRouting.IO.itinero1
{
    public partial class LuaPrinter1
    {
        
        private (string implementation, HashSet<string> extraKeys) GenerateMembershipPreprocessor()
        {
            // Extra keys are the names of introduced tag-keys, e.g. '_relation:bicycle_fastest:cycle_highway'
            var extraKeys = new HashSet<string>();
            var memberships = Analysis.MembershipMappingsFor(_profile, _context);

            foreach (var (calledInFunction, membership) in memberships)
            {
                var funcMetaData = new AspectMetadata(
                    membership,
                    "relation_preprocessing_for_" + calledInFunction.AsLuaIdentifier(),
                    "Function preprocessing needed for aspect " + calledInFunction +
                    ", called by the relation preprocessor",
                    "Generator", "", "NA"
                );


                _skeleton.AddFunction(funcMetaData);
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
                "    result.attributes_to_keep = {}",
                "    ",
                "    -- Legacy to add colours to the bike networks",
                "    legacy_relation_preprocessor(relation_tags, result)"
            };
            _skeleton.AddDep("legacy");
            
            foreach (var (calledInFunction, expr) in memberships)
            {
                func.Add($"\n\n  -- {calledInFunction} ---");

                var usedParameters = expr.UsedParameters().Select(param => param.ParamName.TrimStart('#')).ToHashSet();

                // First, we calculate the value for the default parameters
                var preProcName = "relation_preprocessing_for_" + calledInFunction.AsLuaIdentifier();
                func.Add("");
                func.Add("");
                func.Add("    subresult.attributes_to_keep = {}");
                func.Add("    parameters = default_parameters()");
                func.Add($"    matched = {preProcName}(parameters, relation_tags, subresult)");
                func.Add("    if (matched) then");
                var tagKey = "_relation:" + calledInFunction.AsLuaIdentifier();
                extraKeys.Add(tagKey);
                func.Add(
                    "    -- " + tagKey +
                    " is the default value, which will be overwritten in 'remove_relation_prefix' for behaviours having a different parameter settign");
                func.Add($"        result.attributes_to_keep[\"{tagKey}\"] = \"yes\"");
                func.Add("    end");


                if (!usedParameters.Any())
                {
                    // Every behaviour uses the default parameters for this one
                    func.Add("    -- No parameter dependence for aspect " + calledInFunction);
                    continue;
                }

                foreach (var (behaviourName, parameters) in _profile.Behaviours)
                {
                    if (usedParameters.Except(parameters.Keys.ToHashSet()).Any())
                    {
                        // The parameters where the membership depends on, are not used here
                        // This is thus the same as the default. We don't have to calculate it
                        continue;
                    }

                    func.Add("");
                    func.Add("");
                    func.Add("    subresult.attributes_to_keep = {}");
                    func.Add("    parameters = default_parameters()");
                    func.Add(_parameterPrinter.DeclareParametersFor(parameters.Where(kv => usedParameters.Contains(kv.Key))
                        .ToDictionary(kv => kv.Key, kv => kv.Value)));
                    func.Add($"    matched = {preProcName}(parameters, relation_tags, subresult)");
                    func.Add("    if (matched) then");
                    tagKey = "_relation:" + behaviourName.AsLuaIdentifier() + ":" + calledInFunction.AsLuaIdentifier();
                    extraKeys.Add(tagKey);
                    func.Add($"        result.attributes_to_keep[\"{tagKey}\"] = \"yes\"");
                    func.Add("    end");
                }
            }

            func.Add("end");

            return (string.Join("\n", func), extraKeys);
        }

    }
}