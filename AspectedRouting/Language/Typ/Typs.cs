using System.Collections.Generic;
using System.Linq;

namespace AspectedRouting.Language.Typ
{
    public static class Typs
    {
        public static readonly List<Type> BuiltinTypes = new List<Type>();


        public static readonly Type Double = new DoubleType();
        public static readonly Type PDouble = new PDoubleType();
        public static readonly Type Nat = new NatType();
        public static readonly Type Int = new IntType();
        public static readonly Type String = new StringType();
        public static readonly Type Tags = new TagsType();
        public static readonly Type Bool = new BoolType();


        public static void AddBuiltin(Type t)
        {
            BuiltinTypes.Add(t);
        }


        public static HashSet<Type> SpecializeTo(this IEnumerable<Type> types0, IEnumerable<Type> types1)
        {
            var results = new HashSet<Type>();

            var enumerable = types1.ToList();
            foreach (var t0 in types0)
            {
                foreach (var t1 in enumerable)
                {
                    var unification = t0.Unify(t1);
                    if (unification != null)
                    {
                        results.Add(unification);
                    }
                }
            }

            if (results.Any())
            {
                return results;
            }

            return null;
        }


        public static Type Unify(this Type t0, Type t1)
        {
            var table = t0.UnificationTable(t1);
            if (table == null)
            {
                return null;
            }

            return t0.Substitute(table);
        }

        /// <summary>
        /// Tries to unify t0 with all t1's.
        /// Every match is returned
        /// </summary>
        /// <param name="t0"></param>
        /// <param name="t1"></param>
        /// <returns></returns>
        public static IEnumerable<Type> UnifyAny(this Type t0, IEnumerable<Type> t1)
        {
            var result = t1.Select(t0.Unify).Where(unification => unification != null).ToHashSet();
            if (!result.Any())
            {
                return null;
            }

            return result;
        }

        /// <summary>
        /// Tries to unify t0 with all t1's.
        /// Every match is returned, but only if every unification could be performed
        /// </summary>
        /// <param name="t0"></param>
        /// <param name="t1"></param>
        /// <returns></returns>
        public static IEnumerable<Type> UnifyAll(this Type t0, IEnumerable<Type> t1)
        {
            var result = t1.Select(t0.Unify).ToHashSet();
            if (result.Any(x => x == null))
            {
                return null;
            }

            return result;
        }

        public static Type Substitute(this Type t0, Dictionary<string, Type> substitutions)
        {
            switch (t0)
            {
                case Var a when substitutions.TryGetValue(a.Name, out var t):
                    return t;
                case ListType l:
                    return new ListType(l.InnerType.Substitute(substitutions));
                case Curry c:
                    return new Curry(c.ArgType.Substitute(substitutions), c.ResultType.Substitute(substitutions));
                default:
                    return t0;
            }
        }

        public static Dictionary<string, Type> UnificationTable(this Type t0, Type t1)
        {
            var substitutionsOn0 = new Dictionary<string, Type>();


            bool AddSubs(string key, Type valueToAdd)
            {
                if (substitutionsOn0.TryGetValue(key, out var oldSubs))
                {
                    return oldSubs.Equals(valueToAdd);
                }

                substitutionsOn0[key] = valueToAdd;
                return true;
            }

            bool AddAllSubs(Dictionary<string, Type> table)
            {
                if (table == null)
                {
                    return false;
                }

                foreach (var (key, tp) in table)
                {
                    if (!AddSubs(key, tp))
                    {
                        return false;
                    }
                }

                return true;
            }

            switch (t0)
            {
                case Var a:
                    if (!AddSubs(a.Name, t1))
                    {
                        return null;
                    }

                    break;
                case ListType l0 when t1 is ListType l1:
                {
                    var table = l0.InnerType.UnificationTable(l1.InnerType);
                    if (!AddAllSubs(table))
                    {
                        return null;
                    }

                    break;
                }

                case Curry curry0 when t1 is Curry curry1:
                {
                    var tableA = curry0.ArgType.UnificationTable(curry1.ArgType);
                    var tableB = curry0.ResultType.UnificationTable(curry1.ResultType);
                    if (!(AddAllSubs(tableA) && AddAllSubs(tableB)))
                    {
                        return null;
                    }

                    break;
                }

                default:

                    if (t1 is Var v)
                    {
                        AddSubs(v.Name, t0);
                    }
                    else if (!t0.Equals(t1))
                    {
                        return null;
                    }

                    break;
            }


            return substitutionsOn0;
        }

        public static HashSet<string> UsedVariables(this Type t0, HashSet<string> addTo = null)
        {
            addTo ??=new HashSet<string>();
            switch (t0)
            {
                case Var a:
                    addTo.Add(a.Name);
                    break;
                case ListType l0:
                {
                    l0.InnerType.UsedVariables(addTo);
                    break;
                }

                case Curry curry0:
                {
                    curry0.ArgType.UsedVariables(addTo);
                    curry0.ResultType.UsedVariables(addTo);
                    break;
                }
            }


            return addTo;
        }

        public static bool IsSuperSet(this Type t0, Type t1)
        {
            switch (t0)
            {
                case StringType _ when t1 is BoolType _:
                    return true;
                case DoubleType _ when t1 is PDoubleType _:
                    return true;
                case DoubleType _ when t1 is IntType _:
                    return true;
                case DoubleType _ when t1 is NatType _:
                    return true;

                case PDoubleType _ when t1 is NatType _:
                    return true;

                case IntType _ when t1 is NatType _:
                    return true;

                case ListType l0 when t1 is ListType l1:
                    return l0.InnerType.IsSuperSet(l1.InnerType);

                case Curry c0 when t1 is Curry c1:
                {
                    return c0.ResultType.IsSuperSet(c1.ResultType) &&
                           c1.ArgType.IsSuperSet(c0.ArgType); // contravariance for arguments: reversed order!
                }
            }


            return t0.Equals(t1);
        }

        public static IEnumerable<Type> RenameVars(this IEnumerable<Type> toRename, IEnumerable<Type> noUseVar)
        {
            var usedToRename = toRename.SelectMany(t => UsedVariables(t));
            var blacklist = noUseVar.SelectMany(t => UsedVariables(t)).ToHashSet();
            var alreadyUsed = blacklist.Concat(usedToRename).ToHashSet();
            var variablesToRename = usedToRename.Intersect(blacklist).ToHashSet();
            var subsTable = new Dictionary<string, Type>();
            foreach (var v in variablesToRename)
            {
                var newValue = Var.Fresh(alreadyUsed);
                subsTable.Add(v, newValue);
                blacklist.Add(newValue.Name);
            }

            return toRename.Select(t => t.Substitute(subsTable));
        }


        public static List<Type> Uncurry(this Type t)
        {
            var args = new List<Type>();
            while (t is Curry c)
            {
                args.Add(c.ArgType);
                t = c.ResultType;
            }

            args.Add(t);
            return args;
        }
    }
}