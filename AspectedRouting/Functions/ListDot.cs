using System.Collections.Generic;
using AspectedRouting.Typ;

namespace AspectedRouting.Functions
{
    public class ListDot : Function
    {
        public override string Description { get; } =
            "Listdot takes a list of functions `[f, g, h]` and and an argument `a`. It applies the argument on every single function." +
            "It conveniently lifts the argument out of the list.";

        public override List<string> ArgNames { get; } = new List<string>{"list","a"};

        public ListDot() : base("listDot", true,
            new[]
            {
                // [a -> b] -> a -> [b]
                new Curry(
                    new ListType(new Curry(new Var("a"), new Var("b"))),
                    new Curry(new Var("a"),
                        new ListType(new Var("b")))
                )
            }
        )
        {
        }

        private ListDot(IEnumerable<Type> types) : base("listDot", types)
        {
        }

        public override object Evaluate(Context c, params IExpression[] arguments)
        {
            var listOfFuncs = (IEnumerable<IExpression>) arguments[0].Evaluate(c);
            var arg = arguments[1];

            var result = new List<object>();
            foreach (var f in listOfFuncs)
            {
                result.Add(f.Evaluate(c, arg));
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

            return new ListDot(unified);
        }
    }
}