using System;
using System.Collections.Generic;
using System.Linq;
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
            Types = new[] { new Var("parameter") };
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
            var value = c?.Parameters?.GetValueOrDefault(paramName, null);
            if (value is Constant constant)
            {
                return constant.Evaluate(c);
            }
            return value;
        }

        public IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            /*   var filtered = allowedTypes.Where(at => !(at is Curry));
               if (filtered.Count() == 0)
               {
                   return null;
               }*/
            var unified = Types.SpecializeTo(allowedTypes);
            if (unified == null)
            {
                return null;
            }

            return new Parameter(unified, ParamName);
        }

        public IExpression PruneTypes(System.Func<Type, bool> allowedTypes)
        {
            var passedTypes = this.Types.Where(allowedTypes);
            if (!passedTypes.Any())
            {
                return null;
            }

            return new Parameter(passedTypes, this.ParamName);
        }

        public IExpression Optimize(out bool somethingChanged)
        {
            somethingChanged = false;
            return this;
        }

        public IExpression OptimizeWithArgument(IExpression arg)
        {
            throw new NotSupportedException("Trying to invoke a parameter");
        }

        public void Visit(Func<IExpression, bool> f)
        {
            f(this);
        }

        public bool Equals(IExpression other)
        {
            if (other is Parameter p)
            {
                return p.ParamName.Equals(this.ParamName);
            }

            return false;
        }

        public string Repr()
        {
            return "new Parameter(\"" + this.ParamName + "\")";
        }

        public override string ToString()
        {
            return ParamName;
        }


    }
}