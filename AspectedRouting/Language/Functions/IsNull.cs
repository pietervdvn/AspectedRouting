using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Typ;
using Type = AspectedRouting.Language.Typ.Type;

namespace AspectedRouting.Language.Functions
{
    public class IsNull : Function
    {
        public override string Description { get; } = "Returns true if the given argument is null";
        public override List<string> ArgNames { get; } = new List<string>{"a"};

        public IsNull() : base("is_null", true,
            new[]
            {
                new Curry(new Var("a"), Typs.Bool),
            })
        {
        }

        private IsNull(IEnumerable<Type> specializedTypes) : base("is_null", specializedTypes)
        {
        }

        public override IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            var unified = Types.SpecializeTo(allowedTypes);
            if (unified == null)
            {
                return null;
            }

            return new IsNull(unified);
        }

        public override object Evaluate(Context c, params IExpression[] arguments)
        {
            var arg = (string) arguments[0].Evaluate(c);
            if (arg == null)
            {
                return "yes";
            }

            return "no";
        }
    }
}