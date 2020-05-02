using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AspectedRouting.Language.Functions;
using AspectedRouting.Language.Typ;
using Type = AspectedRouting.Language.Typ.Type;

namespace AspectedRouting.Language
{
    public interface IExpression
    {
        IEnumerable<Type> Types { get; }

        /// <summary>
        /// Evaluates the expression.
        /// Gives null if the expression should not give a value
        /// </summary>
        /// <returns></returns>
        object Evaluate(Context c, params IExpression[] arguments);

        /// <summary>
        /// Creates a copy of this expression, but only execution paths of the given types are kept
        /// Return null if no execution paths are found
        /// </summary>
        IExpression Specialize(IEnumerable<Type> allowedTypes);

        IExpression Optimize();

        void Visit(Func<IExpression, bool> f);
    }

    public static class ExpressionExtensions
    {
        public static object Run(this IExpression e, Context c, Dictionary<string, string> tags)
        {
            return e.Apply(new []{new Constant(tags)}).Evaluate(c);
        }

        public static IExpression Specialize(this IExpression e, Type t)
        {
            if (t == null)
            {
                throw new NullReferenceException("Cannot specialize to null");
            }

            return e.Specialize(new[] {t});
        }

        public static IExpression Specialize(this IExpression e, Dictionary<string, Type> substitutions)
        {
            var newTypes = new HashSet<Type>();

            foreach (var oldType in e.Types)
            {
                var newType = oldType.Substitute(substitutions);
                if (newType == null)
                {
                    continue;
                }

                newTypes.Add(newType);
            }

            if (!newTypes.Any())
            {
                return null;
            }

            return e.Specialize(newTypes);
        }

        public static IEnumerable<Type> SpecializeToCommonTypes(this IEnumerable<IExpression> exprs,
            out IEnumerable<IExpression> specializedExpressions)
        {
            exprs.SpecializeToCommonTypes(out var types, out specializedExpressions);
            return types;
        }


        /// <summary>
        /// Runs over all expresions, determines a common ground by unifications
        /// THen specializes every expression onto this common ground
        /// </summary>
        /// <returns>The common ground of types</returns>
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public static IEnumerable<IExpression> SpecializeToCommonTypes(this IEnumerable<IExpression> exprs,
            out IEnumerable<Type> specializedTypes, out IEnumerable<IExpression> specializedExpressions)
        {
            specializedTypes = null;
            var expressions = exprs.ToList();
            foreach (var f in expressions)
            {
                if (specializedTypes == null)
                {
                    specializedTypes = f.Types.ToHashSet();
                    continue;
                }

                var specialized = specializedTypes.SpecializeTo(f.Types.RenameVars(specializedTypes));
                // ReSharper disable once JoinNullCheckWithUsage
                if (specialized == null)
                {
                    throw new ArgumentException("Could not unify\n   "
                                                + "<previous items>: " + string.Join(", ", specializedTypes) +
                                                "\nwith\n   "
                                                + f.Optimize() + ": " + string.Join(", ", f.Types));
                }

                specializedTypes = specialized;
            }

            var tps = specializedTypes;
            return specializedExpressions = expressions.Select(expr => expr.Specialize(tps));
        }
    }
}