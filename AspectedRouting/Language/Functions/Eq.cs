using System.Collections.Generic;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Typ;

namespace AspectedRouting.Language.Functions
{
    public class Eq : Function
    {  public override string Description { get; } = "Returns 'yes' if both values _are_ the same";
        public override List<string> ArgNames { get; } = new List<string>{"a","b"};
        public Eq() : base("eq", true,
            new[]
            {
                Curry.ConstructFrom(Typs.Bool, new Var("a"), new Var("a")),
                Curry.ConstructFrom(Typs.String, new Var("a"), new Var("a"))
            })
        {
        }

        private Eq(IEnumerable<Type> types) : base("eq", types)
        {
        }

        public override IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            var unified = Types.SpecializeTo(allowedTypes);
            if (unified == null)
            {
                return null;
            }

            return new Eq(unified);
        }

        public override object Evaluate(Context c, params IExpression[] arguments)
        {
            var arg0 = arguments[0].Evaluate(c);
            var arg1 = arguments[1].Evaluate(c);
            if (arg0 == null || arg1 == null)
            {
                return null;
            }

            if (arg0.Equals(arg1))
            {
                return "yes";
            }

            return "no";
        }
    }
}