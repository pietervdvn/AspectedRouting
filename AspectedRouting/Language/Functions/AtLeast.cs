using System;
using System.Collections.Generic;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Typ;
using Type = AspectedRouting.Language.Typ.Type;

namespace AspectedRouting.Language.Functions
{
    public class AtLeast : Function
    {
        public override string Description { get; } =
            "Returns 'yes' if the second argument is bigger then the first argument. (Works great in combination with $dot)";

        public override List<string> ArgNames { get; } = new List<string> {"minimum", "f", "then","else"};

        private static Type a = new Var("a");
        private static Type b = new Var("b");

        public AtLeast() : base("atleast", true,
            new[]
            {
                Curry.ConstructFrom(a,
                    Typs.Double, 
                    new Curry(b,Typs.Double),
                    a, a, b),
            })
        {
        }

        private AtLeast(IEnumerable<Type> types) : base("atleast", types)
        {
        }

        public override IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            var unified = Types.SpecializeTo(allowedTypes);
            if (unified == null)
            {
                return null;
            }

            return new AtLeast(unified);
        }

        public override object Evaluate(Context c, params IExpression[] arguments)
        {
            var minimum = arguments[0].Evaluate(c);
            var then = arguments[2].Evaluate(c);
            var @else = arguments[3].Evaluate(c);
            var x = arguments[4];

            var arg1 = arguments[1].Evaluate(c, x);
            
            if (minimum == null || arg1 == null)
            {
                return null;
            }

            if (minimum is IExpression e)
            {
                minimum = e.Evaluate(c);
            }
            
            if (arg1 is IExpression e1)
            {
                arg1 = e1.Evaluate(c);
            }

            if (minimum is int i0)
            {
                minimum = (double) i0;
            }
            
            if (arg1 is int i1)
            {
                arg1 = (double) i1;
            }

            if (minimum is double d0 && arg1 is double d1)
            {
                if (d0 <= d1)
                {
                    return then;
                }

                return @else;
            }

            throw new InvalidCastException("One of the arguments is not a number: "+minimum+", "+arg1);

        }
    }
}