using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Typ;

namespace AspectedRouting.Language.Functions
{
    public class All : Function
    {
        public All() : base("all", true,
            new[]
            {
                new Curry(new ListType(Typs.Bool), Typs.Bool)
            }
        )
        {
        }

        public All(IEnumerable<Type> types) : base("all", types)
        {
        }

        public override object Evaluate(Context c, params IExpression[] arguments)
        {
            var arg = ((IEnumerable<object>)arguments[0].Evaluate(c)).Select(o => (string)o);


            if (arg.Any(str => str == null || str.Equals("no") || str.Equals("false")))
            {
                return "no";
            }


            return "yes";
        }

        public override IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            var unified = Types.SpecializeTo(allowedTypes);
            if (unified == null)
            {
                return null;
            }

            return new All(unified);
        }
    }
}