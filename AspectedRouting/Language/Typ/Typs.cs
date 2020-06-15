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


        public static HashSet<Type> SpecializeTo(this IEnumerable<Type> types0, IEnumerable<Type> allowedTypes)
        {
            var results = new HashSet<Type>();

            allowedTypes = allowedTypes.ToList();
            foreach (var t0 in types0)
            {
                foreach (var allowed in allowedTypes)
                {
                    var unified = t0.Unify(allowed, true);
                    if (unified != null)
                    {
                        results.Add(unified);
                    }
                }
            }

            if (results.Any())
            {
                return results;
            }

            return null;
        }


        /// <summary>
        /// Unifies the two types, where t0 is the allowed, wider type and t1 is the actual, smaller type (unless reverseSuperset is true).
        /// Unification will attempt to keep the type as small as possible
        /// </summary>
        /// <param name="t0">The expected, wider type</param>
        /// <param name="t1">The actual type</param>
        /// <param name="reverseSuperset">True if the supertyping relationship should be reversed</param>
        /// <returns></returns>
        public static Type Unify(this Type t0, Type t1, bool reverseSuperset = false)
        {
            var table = t0.UnificationTable(t1, reverseSuperset);
            if (table == null)
            {
                return null;
            }

            var subbed = t0.Substitute(table);
            if (reverseSuperset)
            {
                return SelectSmallestUnion(t1, subbed);
            }
            return SelectSmallestUnion(subbed, t1);
        }
        
        private static Type SelectSmallestUnion(this Type wider, Type smaller, bool reverse = false)
        {
            switch (wider)
            {
                case Var a:
                    return a;
                case ListType l when smaller is ListType lsmaller:
                    return new ListType(
                        l.InnerType.SelectSmallestUnion(
                            l.InnerType.SelectSmallestUnion(lsmaller.InnerType, reverse)));
                case Curry cWider when smaller is Curry cSmaller:
                    var arg =
                        cWider.ArgType.SelectSmallestUnion(cSmaller.ArgType, !reverse);
                    var result =
                        cWider.ResultType.SelectSmallestUnion(cSmaller.ResultType, reverse);
                    return new Curry(arg, result);
                default:
                    if (wider.IsSuperSet(smaller) && !(smaller.IsSuperSet(wider)))
                    {
                        return smaller;
                    }

                    return wider;
            }
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
            var result = t1.Select(t => t0.Unify(t)).Where(unification => unification != null).ToHashSet();
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
            var result = t1.Select(t => t0.Unify(t)).ToHashSet();
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

        /// <summary>
        /// The unification table is built when the type of an argument is introspected to see if it fits in the excpect type
        /// t0 here is the **expected** (wider) type, whereas t1 is the **actual** argument type.
        /// In other words, if we expect a `double`, a `pdouble` fits in there too.
        /// If we expect a function capable of handling pdoubles and giving strings, a function capable of handling doubles and giving bools will work just as well
        /// </summary>
        /// <param name="t0"></param>
        /// <param name="t1"></param>
        /// <returns></returns>
        public static Dictionary<string, Type> UnificationTable(this Type t0, Type t1, bool reverseSupersetRelation = false)
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
                    var table = l0.InnerType.UnificationTable(l1.InnerType, reverseSupersetRelation);
                    if (!AddAllSubs(table))
                    {
                        return null;
                    }

                    break;
                }

                case Curry curry0 when t1 is Curry curry1:
                {
                    // contravariance for arguments: reversed 
                    var tableA = curry0.ArgType.UnificationTable(curry1.ArgType, !reverseSupersetRelation);
                    var tableB = curry0.ResultType.UnificationTable(curry1.ResultType, reverseSupersetRelation);
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
                        break;
                    }
                    
                    if (!reverseSupersetRelation && !t0.IsSuperSet(t1))
                    {
                        return null;
                    }

                    if (reverseSupersetRelation && !t1.IsSuperSet(t0))
                    {
                        return null;
                    }

                    break;
            }


            return substitutionsOn0;
        }

        public static HashSet<string> UsedVariables(this Type t0, HashSet<string> addTo = null)
        {
            addTo ??= new HashSet<string>();
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
            // Variables used in 'toRename'
            var usedToRename = toRename.SelectMany(t => UsedVariables(t)).ToHashSet();
            // Variables that should not be used
            var blacklist = noUseVar.SelectMany(t => UsedVariables(t)).ToHashSet();
            // variables that should be renamed
            var variablesToRename = usedToRename.Intersect(blacklist).ToHashSet();

            // All variables that are used and thus not free anymore, sum of 'usedToRename' and the blacklist
            var alreadyUsed = blacklist.Concat(usedToRename).ToHashSet();

            // The substitution table
            var subsTable = new Dictionary<string, Type>();
            foreach (var v in variablesToRename)
            {
                var newValue = Var.Fresh(alreadyUsed);
                subsTable.Add(v, newValue);
                alreadyUsed.Add(newValue.Name);
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