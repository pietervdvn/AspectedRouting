using System;
using System.Collections.Generic;
using AspectedRouting.Language.Typ;
using Type = AspectedRouting.Language.Typ.Type;

namespace AspectedRouting.Language.Functions
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
            var paramName = ParamName.TrimStart('#'); // context saves paramnames without '#'
            return c?.Parameters?.GetValueOrDefault(paramName, null);
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