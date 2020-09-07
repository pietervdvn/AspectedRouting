using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AspectedRouting.Language;
using AspectedRouting.Language.Functions;
using AspectedRouting.Language.Typ;

namespace AspectedRouting.IO.jsonParser
{
    public static partial class JsonParser
    {
        private static IExpression ParseProfileProperty(JsonElement e, Context c, string property)
        {
            if (!e.TryGetProperty(property, out var prop))
            {
                throw new ArgumentException("Not a valid profile: the declaration expression for '" + property +
                                            "' is missing");
            }

            try
            {
                var expr = ParseExpression(prop, c);
                if (!expr.Types.Any())
                {
                    throw new Exception($"Could not parse field {property}, no valid typing for expression found");
                }

                expr = expr.Optimize();
                expr = Funcs.Either(Funcs.Id, Funcs.Const, expr);

                var specialized = expr.Specialize(new Curry(Typs.Tags, new Var("a")));
                if (specialized == null)
                {
                    throw new ArgumentException("The expression for " + property +
                                                " hasn't the right type of 'Tags -> a'; it has types " +
                                                string.Join(",", expr.Types) + "\n" + expr.TypeBreakdown());
                }

                return specialized.Optimize();
            }
            catch (Exception exc)
            {
                throw new Exception("While parsing the property " + property, exc);
            }
        }

        private static Dictionary<string, IExpression> ParseParameters(this JsonElement e)
        {
            var ps = new Dictionary<string, IExpression>();
            foreach (var obj in e.EnumerateObject())
            {
                var nm = obj.Name.TrimStart('#');
                switch (obj.Value.ValueKind)
                {
                    case JsonValueKind.String:
                        var v = obj.Value.ToString();
                        if (v.Equals("yes") || v.Equals("no"))
                        {
                            ps[nm] = new Constant(Typs.Bool, v);
                        }
                        else
                        {
                            ps[nm] = new Constant(v);
                        }

                        break;
                    case JsonValueKind.Number:
                        ps[nm] = new Constant(obj.Value.GetDouble());
                        break;
                    case JsonValueKind.True:
                        ps[nm] = new Constant(Typs.Bool, "yes");
                        break;
                    case JsonValueKind.False:
                        ps[nm] = new Constant(Typs.Bool, "yes");
                        break;
                    case JsonValueKind.Array:
                        var list = obj.Value.EnumerateArray().Select(x => x.ToString()).ToList();
                        ps[nm] = new Constant(new ListType(Typs.String), list);
                        break;
                    default:
                        throw new ArgumentException(
                            "Parameters are not allowed to be complex expressions, they should be simple values. " +
                            "Simplify the value for parameter " + obj.Name);
                }
            }

            return ps;
        }


        private static string Get(this JsonElement json, string key)
        {
            if (json.TryGetProperty(key, out var p))
            {
                return p.GetString();
            }

            throw new ArgumentException($"The obligated property {key} is missing");
        }

        private static string TryGet(this JsonElement json, string key)
        {
            if (json.TryGetProperty(key, out var p))
            {
                return p.GetString();
            }

            return null;
        }
    }
}