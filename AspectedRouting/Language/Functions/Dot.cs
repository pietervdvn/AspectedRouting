using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Typ;
using Type = AspectedRouting.Language.Typ.Type;

namespace AspectedRouting.Language.Functions
{
    public class Dot : Function
    {
        public override string Description { get; } =
            "Higher order function: converts `f (g a)` into `(dot f g) a`. In other words, this fuses `f` and `g` in a new function, which allows the argument to be lifted out of the expression ";

        public override List<string> ArgNames { get; } = new List<string> {"f", "g", "a"};
        public static readonly Var A = new Var("a");
        public static readonly Var B = new Var("b");
        public static readonly Var C = new Var("c");

        public Dot() : base("dot", true, new[]
        {
            // (.) : (b -> c) -> (a -> b) -> a -> c
            Curry.ConstructFrom(C,
                new Curry(B, C),
                new Curry(A, B),
                A
            )
        })
        {
        }

        public Dot(IEnumerable<Type> types) : base("dot", types)
        {
        }

        public override object Evaluate(Context c, params IExpression[] arguments)
        {
            if (arguments.Count() <= 2)
            {
                
            }

            var f0 = arguments[0];
            var f1 = arguments[1];
            var resultType = ((Curry) f1.Types.First()).ResultType;
            var a = arguments[2];
            return f0.Evaluate(c, new Constant(resultType, f1.Evaluate(c, a)));
        }

        public override IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            var unified = Types.SpecializeTo(allowedTypes);
            if (unified == null)
            {
                return null;
            }

            return new Dot(unified);
        }
    }
}