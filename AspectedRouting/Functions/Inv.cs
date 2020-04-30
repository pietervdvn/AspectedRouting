using System.Collections.Generic;
using AspectedRouting.Typ;

namespace AspectedRouting.Functions
{
    public class Inv : Function
    {
        public override string Description { get; } = "Calculates `1/d`";
        public override List<string> ArgNames { get; } = new List<string> {"d"};

        public Inv() : base("inv", true, new[]
        {
            new Curry(Typs.PDouble, Typs.PDouble),
            new Curry(Typs.Double, Typs.Double),
        })
        {
        }

        public Inv(IEnumerable<Type> types) : base("inv", types)
        {
        }

        public override object Evaluate(Context c, params IExpression[] arguments)
        {
            var arg = (double) arguments[0].Evaluate(c);
            return 1 / arg;
        }

        public override IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            var unified = Types.SpecializeTo(allowedTypes);
            if (unified == null)
            {
                return null;
            }

            return new Inv(unified);
        }
    }
}