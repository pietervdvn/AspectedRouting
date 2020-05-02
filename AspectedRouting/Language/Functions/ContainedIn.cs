using System.Collections.Generic;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Typ;

namespace AspectedRouting.Language.Functions
{
    public class ContainedIn : Function
    {
        public override string Description { get; } =
            "Given a list of values, checks if the argument is contained in the list.";

        public override List<string> ArgNames { get; } = new List<string>{"list","a"};

        public ContainedIn() : base("containedIn", true,
            new[]
            {
                // [a] -> a -> bool
                new Curry(
                    new ListType(new Var("a")),
                    new Curry(new Var("a"),
                        Typs.Bool))
            }
        )
        {
        }

        private ContainedIn(IEnumerable<Type> types) : base("containedIn", types)
        {
        }

        public override object Evaluate(Context c, params IExpression[] arguments)
        {
            var list = (IEnumerable<IExpression>) arguments[0].Evaluate(c);
            var arg = arguments[1];

            var result = new List<object>();
            foreach (var f in list)
            {
                var o = f.Evaluate(c);
                while (o is IExpression e)
                {
                    o = e.Evaluate(c);
                }
                if (f.Equals(arg))
                {
                    return true;
                }
            }

            return result;
        }

        public override IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            var unified = Types.SpecializeTo(allowedTypes);
            if (unified == null)
            {
                return null;
            }

            return new ContainedIn(unified);
        }
    }
}