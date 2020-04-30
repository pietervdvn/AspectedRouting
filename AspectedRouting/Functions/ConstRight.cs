using System.Collections.Generic;
using AspectedRouting.Typ;
using Type = AspectedRouting.Typ.Type;

namespace AspectedRouting.Functions
{
    public class ConstRight : Function
    {        public override string Description { get; } =
            "Small utility function, which takes two arguments `a` and `b` and returns `b`. Used extensively to insert freedom";

        public override List<string> ArgNames { get; } = new List<string>{"a","b"};

        public ConstRight() : base("constRight", true,
            new[]
            {
                Curry.ConstructFrom(new Var("b"), new Var("a"), new Var("b"))
            }
        )
        {
        }

        private ConstRight(IEnumerable<Type> types) : base("constRight", types
        )
        {
        }

        public override object Evaluate(Context c,params IExpression[] arguments)
        {
            var argsFor1 = new IExpression[arguments.Length - 2];
            for (var i = 2; i < arguments.Length; i++)
            {
                argsFor1[i - 2] = arguments[i];
            }

            return arguments[1].Evaluate(c, argsFor1);
        }

        public override IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            var unified = Types.SpecializeTo(allowedTypes);
            if (unified == null)
            {
                return null;
            }

            return new ConstRight(unified);
        }
    }
}