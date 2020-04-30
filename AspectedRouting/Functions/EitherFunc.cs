using System;
using System.Collections.Generic;
using AspectedRouting.Typ;
using Type = AspectedRouting.Typ.Type;

namespace AspectedRouting.Functions
{
    public class EitherFunc : Function
    {
        private static Var a = new Var("a");
        private static Var b = new Var("b");
        private static Var c = new Var("c");
        private static Var d = new Var("d");


        // (a -> b) -> (c -> d) -> (a -> b)
        private static Curry formAB = new Curry(
            new Curry(a, b),
            new Curry(
                new Curry(c, d),
                new Curry(a, b))
        );
        // (a -> b) -> (c -> d) -> (c -> d)

        private static Curry formCD = new Curry(
            new Curry(a, b),
            new Curry(
                new Curry(c, d),
                new Curry(c, d))
        );

        public EitherFunc() : base("eitherFunc", true,
            new[]
            {
                formAB, formCD
            })
        {
        }

        private EitherFunc(IEnumerable<Type> unified) : base("eitherFunc", unified)
        {
        }

        public override object Evaluate(Context _,params IExpression[] arguments)
        {
            throw new ArgumentException("EitherFunc not sufficiently specialized");
        }

        public override IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            var unified = Types.SpecializeTo(allowedTypes);
            if (unified == null)
            {
                return null;
            }

            var isFormAb = formAB.UnifyAll(unified) != null;
            var isFormCd = formCD.UnifyAll(unified) != null;

            if (isFormAb && isFormCd)
            {
                return new EitherFunc(unified); // Can't make a decision yet
            }

            if (isFormAb)
            {
                return Funcs.Const.Specialize(unified);
            }

            if (isFormCd)
            {
                return Funcs.ConstRight.Specialize(unified);
            }

            return null;
        }
    }
}