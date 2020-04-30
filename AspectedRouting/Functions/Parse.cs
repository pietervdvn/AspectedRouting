using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Typ;

namespace AspectedRouting.Functions
{
    public class Parse : Function
    {
        public override string Description { get; } = "Parses a string into a numerical value";
        public override List<string> ArgNames { get; } = new List<string>{"s"};

        public Parse() : base("parse", true,
            new[]
            {
                new Curry(Typs.String, Typs.Double),
            })
        {
        }

        private Parse(IEnumerable<Type> specializedTypes) : base("parse", specializedTypes)
        {
        }

        public override IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            var unified = Types.SpecializeTo(allowedTypes);
            if (unified == null)
            {
                return null;
            }

            return new Parse(unified);
        }

        public override object Evaluate(Context c, params IExpression[] arguments)
        {
            var arg = (string) arguments[0].Evaluate(c);
            var expectedType = ((Curry) Types.First()).ResultType;
            switch (expectedType)
            {
                case PDoubleType _:
                case DoubleType _:
                    return double.Parse(arg);
                default: return int.Parse(arg);
            }
        }
    }
}