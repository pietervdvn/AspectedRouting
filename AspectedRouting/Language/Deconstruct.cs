using System;
using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Language.Expression;

namespace AspectedRouting.Language
{
    public static class Deconstruct
    {
        /// <summary>
        /// Fully deconstruct nested applies, used to convert from ((f a0) a1) ... an) to f(a0, a1, a2, a3
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static (IExpression f, List<IExpression> args)? DeconstructApply(this IExpression e)
        {
            if (!(e is Apply _))
            {
                return null;
            }

            var argss = new List<IExpression>();

            var fs = new List<IExpression>();

            while (UnApply(Assign(fs), Assign(argss)).Invoke(e))
            {
                e = fs.First();
                fs.Clear();
            }

            argss.Reverse();

            return (e, argss);
        }


        public static Func<IExpression, bool> Assign(List<IExpression> collect)
        {
            return e =>
            {
                collect.Add(e);
                return true;
            };
        }

        public static Func<IExpression, bool> IsFunc(Function f)
        {
            return e =>
            {
                if (!(e is Function fe))
                {
                    return false;
                }

                return f.Name.Equals(fe.Name);
            };
        }

        public static Func<IExpression, bool> UnApplyAny(
            Func<IExpression, bool> matchFunc,
            Func<IExpression, bool> matchArg)
        {
            return UnApply(matchFunc, matchArg, true);
        }
        public static Func<IExpression, bool> UnApply(
            Func<IExpression, bool> matchFunc,
            Func<IExpression, bool> matchArg,
            bool matchAny = false)
        {
            return e =>
            {
                if (!(e is Apply apply))
                {
                    return false;
                }

                foreach (var (_, (f, a)) in apply.FunctionApplications)
                {
                    var doesMatch = matchFunc.Invoke(f) && matchArg.Invoke(a);
                    if (matchAny)
                    {
                        if (doesMatch)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (!doesMatch)
                        {
                            // All must match - otherwise we might have registered a wrong collectiin
                            return false;
                        }
                    }
                }

                return !matchAny;
            };
        }

        public static readonly Func<IExpression, bool> Any = e => true;
        
    }
}