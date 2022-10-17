using System.Collections.Generic;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Typ;

namespace AspectedRouting.Language.Functions
{
    public class Concat : Function
    {
        public override string Description { get; } = "Concatenates two strings";
        public override List<string> ArgNames { get; } = new List<string> { "a", "b" };
        public Concat() : base("concat", true,
            new[]
            {
                Curry.ConstructFrom(Typs.String, Typs.String, Typs.String)
            })
        {
        }

        private Concat(IEnumerable<Type> types) : base("concat", types)
        {
        }

        public override IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            var unified = Types.SpecializeTo(allowedTypes);
            if (unified == null)
            {
                return null;
            }

            return new Concat(unified);
        }

        public override object Evaluate(Context c, params IExpression[] arguments)
        {
            var arg0 = (string)arguments[0].Evaluate(c);
            var arg1 = (string)arguments[1].Evaluate(c);
            return arg0 + arg1;
        }
    }
}