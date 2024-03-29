using System;
using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Typ;
using Type = AspectedRouting.Language.Typ.Type;

namespace AspectedRouting.Language.Functions
{
    /// <summary>
    /// The constructor takes a dictionary "key --> expression".
    /// If a string is given as argument, the respective argument is returned. If the key is not found, 'null' is returned
    /// 
    /// If a table/dictionary/collection of tags is given, the key of the arguments is used and the expression is used as function on every respective value of the argument.
    /// If the key is not found, null is returned for this expression instead.
    /// </summary>
    public class Mapping : Function
    {
        public readonly Dictionary<string, IExpression> StringToResultFunctions;

        public Mapping(IReadOnlyList<string> keys, IEnumerable<IExpression> expressions) :
            base(
                $"mapping ({MappingToString(keys, expressions.SpecializeToCommonTypes(out var specializedTypes, out var specializedExpressions))})",
                false,
                specializedTypes
                    .Select(returns => new Curry(Typs.String, returns)))

        {
            StringToResultFunctions = new Dictionary<string, IExpression>();
            var ls = specializedExpressions.ToList();
            for (var i = 0; i < keys.Count; i++)
            {
                StringToResultFunctions[keys[i]] = ls[i];
            }
        }

        private Mapping(Dictionary<string, IExpression> newFunctions) :
            base($"mapping {MappingToString(newFunctions)}", false,
                newFunctions.SelectMany(nf => nf.Value.Types).Select(tp => new Curry(Typs.String, tp)).ToHashSet()
            )
        {
            StringToResultFunctions = newFunctions;
        }


        public override IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            var allowedTypesList = allowedTypes.ToList();
            var unified = Types.SpecializeTo(allowedTypesList);
            if (unified == null)
            {
                return null;
            }


            var newFunctions = new Dictionary<string, IExpression>();
            var functionType = unified.Select(c => ((Curry)c).ResultType);
            var enumerable = functionType.ToList();

            foreach (var (k, expr) in StringToResultFunctions)
            {
                var exprSpecialized = expr.Specialize(enumerable);
                if (exprSpecialized == null)
                {
                    /*throw new Exception($"Could not specialize a mapping of type {string.Join(",", Types)}\n" +
                                        $"to types {string.Join(", ", allowedTypesList)};\n" +
                                        $"Expression {expr}:{string.Join(" ; ", expr.Types)} could not be specialized to {string.Join("; ",functionType)}");
               
               
*/
                    return null;
                }

                newFunctions[k] = exprSpecialized;
            }

            return new Mapping(newFunctions);
        }

        public override object Evaluate(Context c, params IExpression[] arguments)
        {

            var s = arguments[0].Evaluate(c);
            while (s is Constant constant)
            {
                s = constant.Evaluate(c);
            }


            var key = (string)s;
            var otherARgs = arguments.ToList().GetRange(1, arguments.Length - 1);
            if (!StringToResultFunctions.ContainsKey(key))
            { // This is really roundabout, but it has to be
                return null;
            }

            var resultFunction = StringToResultFunctions[key];
            if (resultFunction == null)
            {
                return null;
            }

            return resultFunction.Evaluate(c, otherARgs.ToArray());
        }

        public override IExpression Optimize(out bool somethingChanged)
        {
            var optimizedFunctions = new Dictionary<string, IExpression>();
            somethingChanged = false;
            foreach (var (k, e) in StringToResultFunctions)
            {
                var opt = e.Optimize(out var sc);
                somethingChanged |= sc;
                if (!opt.Types.Any())
                {
                    var typeOptStr = string.Join(";", opt.Types);
                    var typeEStr = string.Join("; ", e.Types);
                    throw new NullReferenceException($"Optimized version is null, has different or empty types: " +
                                                     $"\n{typeEStr}" +
                                                     $"\n{typeOptStr}");
                }

                optimizedFunctions[k] = opt;
            }

            if (!somethingChanged)
            {
                return this;
            }

            return new Mapping(optimizedFunctions);

        }

        public static Mapping Construct(params (string key, IExpression e)[] exprs)
        {
            return new Mapping(exprs.Select(e => e.key).ToList(),
                exprs.Select(e => e.e).ToList());
        }

        private static string MappingToString(Dictionary<string, IExpression> dict)
        {
            var txt = "";
            foreach (var (k, v) in dict)
            {
                txt += "\n  " + k + ": " + v?.ToString()?.Indent() + ",";
            }

            return "{" + txt + "}";
        }

        private static string MappingToString(IReadOnlyList<string> keys, IEnumerable<IExpression> expressions)
        {
            var txt = "";
            var exprs = expressions.ToList();
            for (int i = 0; i < keys.Count; i++)
            {
                txt += "\n" + keys[i] + ": " + exprs[i].ToString().Indent() + ",";
            }

            return "{" + txt + "}";
        }

        public override void Visit(Func<IExpression, bool> f)
        {
            var continueVisit = f(this);
            if (!continueVisit)
            {
                return;
            }

            foreach (var (_, e) in StringToResultFunctions)
            {
                e.Visit(f);
            }
        }
    }
}