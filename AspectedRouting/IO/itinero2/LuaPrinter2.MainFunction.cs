using System;
using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Language;
using AspectedRouting.Language.Typ;

namespace AspectedRouting.IO.itinero2
{
    public partial class LuaPrinter2
    {
        private string GenerateFactorFunction()
        {
            var parameters = new Dictionary<string, IExpression>();
            foreach (var (name, value) in _profile.DefaultParameters)
            {
                parameters[name] = value;
            }

            foreach (var (name, value) in _profile.Behaviours[_behaviourName])
            {
                parameters[name] = value;
            }


            var aspects = new List<string>();

            foreach (var (paramName, expr) in _profile.Priority)
            {
                var paramExpr = parameters[paramName].Evaluate(_context);
                if (!(paramExpr is double weight))
                {
                    continue;
                }

                if (weight == 0)
                {
                    continue;
                }

                var expression = _profile.Priority[paramName];
                var exprInLua = _skeleton.ToLua(expression);
                var subs = new Curry(Typs.Tags, new Var(("a"))).UnificationTable(expression.Types.First());
                if (subs != null && subs.TryGetValue("$a", out var resultType) &&
                    (resultType.Equals(Typs.Bool) || resultType.Equals(Typs.String)))
                {
                    _skeleton.AddDep("parse");
                    exprInLua = "parse(" + exprInLua + ")";
                }

                aspects.Add(weight + " * " + exprInLua);
            }

            Console.WriteLine(aspects.Lined());
            var code = new List<string>()
            {
                "--[[",
                "Generates the factor according to the priorities and the parameters for this behaviour",
                "Note: 'result' is not actually used",
                "]]",
                "function calculate_priority(parameters, tags, result, access, oneway, speed)",
                "    local distance = 1",
                "    local priority = \n        " + string.Join(" +\n       ", aspects),
                "    return priority",
                "end"
            };
            return code.Lined();
        }

        private string GenerateMainFunction()
        {
            var parameters = _profile.Behaviours[_behaviourName];

            _skeleton.AddDependenciesFor(_profile.Access);
            _skeleton.AddDependenciesFor(_profile.Oneway);
            _skeleton.AddDependenciesFor(_profile.Speed);

            _skeleton.AddDep("eq");
            var code = new List<string>
            {
                "--[[",
                _profile.Behaviours[_behaviourName]["description"].Evaluate(_context).ToString(),
                "]]",
                "function factor(tags, result)",
                "    ",
                "    -- initialize the result table on the default values",
                "    result.forward_speed = 0",
                "    result.backward_speed = 0",
                "    result.forward = 0",
                "    result.backward = 0",
                "    result.canstop = true",
                "    result.attributes_to_keep = {} -- not actually used anymore, but the code generation still uses this",

                "",
                "",
                "    local parameters = default_parameters()",
                _parameterPrinter.DeclareParametersFor(parameters),
                "",
                "    local oneway = " + _skeleton.ToLua(_profile.Oneway),
                "    tags.oneway = oneway",
                "    -- forward calculation",
                "    tags[\"_direction\"] = \"with\"",
                "    local access_forward = " + _skeleton.ToLua(_profile.Access),
                "    if(oneway == \"against\") then",
                "        access_forward = \"no\"",
                "    end",

                "    if(access_forward ~= nil and access_forward ~= \"no\") then",
                "        tags.access = access_forward",
                "        result.forward_speed = " + _skeleton.ToLua(_profile.Speed).Indent(),
                "        tags.speed = result.forward_speed",
                "        local priority = calculate_priority(parameters, tags, result, access_forward, oneway, result.forward_speed)",
                "        if (priority <= 0) then",
                "            result.forward_speed = 0",
                "        else",
                "            result.forward = 1 / priority",
                "         end",
                "    end",
                "",
                "    -- backward calculation",
                "    tags[\"_direction\"] = \"against\"",
                "    local access_backward = " + _skeleton.ToLua(_profile.Access),
                "",
                "    if(oneway == \"with\") then",
                "        access_backward = \"no\"",
                "    end",
                "",
                "    if(access_backward ~= nil and access_backward ~= \"no\") then",
                "        tags.access = access_backward" +
                "        result.backward_speed = " + _skeleton.ToLua(_profile.Speed).Indent(),
                "        tags.speed = result.backward_speed",
                "        local priority = calculate_priority(parameters, tags, result, access_backward, oneway, result.backward_speed)",
                "        if (priority <= 0) then",
                "            result.backward_speed = 0",
                "        else",
                "            result.backward = 1 / priority",
                "         end",
                "    end",

                "end"
            };

            return code.Lined();
        }
    }
}