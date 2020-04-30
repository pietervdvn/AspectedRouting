using System;
using System.Collections.Generic;
using AspectedRouting.Typ;
using Type = AspectedRouting.Typ.Type;

namespace AspectedRouting.Functions
{
    public class Parameter : IExpression
    {
        public IEnumerable<Type> Types { get; }
        public readonly string ParamName;


        public Parameter(string s)
        {
            Types = new[] {new Var("parameter") };
            ParamName = s;
        }

        private Parameter(IEnumerable<Type> unified, string paramName)
        {
            Types = unified;
            ParamName = paramName;
        }

        public object Evaluate(Context c, params IExpression[] args)
        {
            return c?.Parameters?.GetValueOrDefault(ParamName, null);
        }

        public void EvaluateAll(Context c, HashSet<IExpression> addTo)
        {
            var v = Evaluate(c);
            if (v != null)
            {
                addTo.Add(this);
            }
        }

        public IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            var unified = Types.SpecializeTo(allowedTypes);
            if (unified == null)
            {
                return null;
            }

            return new Parameter(unified, ParamName);
        }

        public IExpression Optimize()
        {
            return this;
        }

        public void Visit(Func<IExpression, bool> f)
        {
            f(this);
        }

        public override string ToString()
        {
            return ParamName;
        }
    }
}