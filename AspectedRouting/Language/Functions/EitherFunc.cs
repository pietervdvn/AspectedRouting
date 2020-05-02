using System;
using System.Collections.Generic;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Typ;
using Type = AspectedRouting.Language.Typ.Type;

namespace AspectedRouting.Language.Functions
{
    public class EitherFunc : Function
    {
        public override string Description { get; } =
            "EitherFunc is a small utility function, mostly used in the parser. It allows the compiler to choose a function, based on the types.\n\n" +
            "" +
            "Consider the mapping `{'someKey':'someValue'}`. Under normal circumstances, this acts as a pointwise-function, converting the string `someKey` into `someValue`, just like an ordinary dictionary would do. " +
            "However, in the context of `mustMatch`, we would prefer this to act as a _check_, that the highway _has_ a key `someKey` which is `someValue`, thus acting as " +
            "`{'someKey': {'$eq':'someValue'}}. " +
            "Both behaviours are automatically supported in parsing, by parsing the string as `(eitherFunc id eq) 'someValue'`. " +
            "The type system is then able to figure out which implementation is needed.\n\n" +
            "Disclaimer: _you should never ever need this in your profiles_";

        public override List<string> ArgNames { get; } = new List<string>{"f","g","a"};
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