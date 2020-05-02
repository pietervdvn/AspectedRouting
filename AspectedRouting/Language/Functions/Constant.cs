using System;
using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Language.Typ;
using Type = AspectedRouting.Language.Typ.Type;

namespace AspectedRouting.Language.Functions
{
    public class Constant : IExpression
    {
        public IEnumerable<Type> Types { get; }
        private readonly object _o;


        public Constant(IEnumerable<Type> types, object o)
        {
            Types = types;
            _o = o;
        }

        public Constant(Type t, object o)
        {
            Types = new[] {t};
            _o = o;
        }

        public Constant(IExpression[] exprs)
        {
            Types = exprs.SpecializeToCommonTypes(out var specializedVersions).Select(t => new ListType(t));
            _o = specializedVersions;
        }

        public Constant(IEnumerable<IExpression> xprs)
        {
            var exprs = xprs.ToList();
            try
            {
                Types = exprs.SpecializeToCommonTypes(out var specializedVersions).Select(t => new ListType(t));
                _o = specializedVersions.ToList();
            }
            catch (Exception e)
            {
                throw new Exception($"While creating a list with members {string.Join(", ", exprs.Select(e => e.Optimize()))} {e.Message}", e);
            }
        }

        public Constant(string s)
        {
            Types = new[] {Typs.String};
            _o = s;
        }

        public Constant(double d)
        {
            if (d >= 0)
            {
                Types = new[] {Typs.Double, Typs.PDouble};
            }
            else
            {
                Types = new[] {Typs.Double};
            }

            _o = d;
        }

        public Constant(int i)
        {
            if (i >= 0)
            {
                Types = new[] {Typs.Double, Typs.Nat, Typs.Nat, Typs.PDouble};
            }
            else
            {
                Types = new[] {Typs.Double, Typs.Int};
            }

            _o = i;
        }

        public Constant(Dictionary<string, string> tags)
        {
            Types = new[] {Typs.Tags};
            _o = tags;
        }

        public object Evaluate(Context c, params IExpression[] args)
        {
            if (_o is IExpression e)
            {
                return e.Evaluate(c).Pretty();
            }


            if (Types.Count() > 1) return _o;

            var t = Types.First();
            switch (t)
            {
                case DoubleType _:
                case PDoubleType _:
                    if (_o is int i)
                    {
                        return
                            (double) i; // I know, it seems absurd having to write this as it is nearly the same as the return beneath, but it _is_ needed
                    }

                    return (double) _o;
                case IntType _:
                case NatType _:
                    return (int) _o;
            }

            return _o;
        }

        public void EvaluateAll(Context c, HashSet<IExpression> addTo)
        {
            addTo.Add(this);
        }

        public IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            var enumerable = allowedTypes.ToList();
            var unified = Types.SpecializeTo(enumerable);
            if (unified == null)
            {
                return null;
            }

            var newO = _o;
            if (_o is IExpression e)
            {
                newO = e.Specialize(enumerable);
            }

            if (_o is IEnumerable<IExpression> es)
            {
                var innerTypes = enumerable
                    .Where(t => t is ListType)
                    .Select(t => ((ListType) t).InnerType);
                newO = es.Select(x => x.Specialize(innerTypes)).Where(x => x != null);
            }

            return new Constant(unified, newO);
        }

        public IExpression Optimize()
        {
            if (_o is IEnumerable<IExpression> exprs)
            {
                return new Constant(exprs.Select(x => x.Optimize()));
            }

            if (_o is IExpression expr)
            {
                return new Constant(expr.Types, expr.Optimize());
            }

            return this;
        }

        public void Visit(Func<IExpression, bool> f)
        {
            if (_o is IExpression e)
            {
                e.Visit(f);
            }

            if (_o is IEnumerable<IExpression> es)
            {
                foreach (var x in es)
                {
                    x.Visit(f);
                }
            }

            f(this);
        }

        public override string ToString()
        {
            return _o.Pretty();
        }
    }

    public static class ObjectExtensions
    {
        public static string Pretty(this object o, Context context = null)
        {
            switch (o)
            {
                case Dictionary<string, string> d:
                    var txt = "";
                    foreach (var (k, v) in d)
                    {
                        txt += $"{k}={v};";
                    }

                    return $"{{{txt}}}";
                case Dictionary<string, List<string>> d:
                    var t = "";
                    foreach (var (k, v) in d)
                    {
                        var values = v.Pretty();
                        if (!v.Any())
                        {
                            values = "*";
                        }

                        t += k + "=" + values + "\n";
                    }

                    return t;

                case Constant c:
                    return "<" + c.Evaluate(context).Pretty() + ">";
                case List<object> ls:
                    return "[" + string.Join(", ", ls.Select(obj => obj.Pretty(context))) + "]";
                case object[] arr:
                    return arr.ToList().Pretty();
                case double[] arr:
                    return arr.Select(d => (object) d).ToList().Pretty();
                case string s:
                    return "\"" + s.Replace("\"", "\\\"") + "\"";
                case IEnumerable<object> ls:
                    return "[" + string.Join(", ", ls.Select(obj => obj.Pretty(context))) + "]";
            }

            return o.ToString();
        }
    }
}