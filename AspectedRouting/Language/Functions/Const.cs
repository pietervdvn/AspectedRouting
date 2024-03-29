using System.Collections.Generic;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Typ;
using Type = AspectedRouting.Language.Typ.Type;

namespace AspectedRouting.Language.Functions
{
    public class Const : Function
    {
        public override string Description { get; } =
            "Small utility function, which takes two arguments `a` and `b` and returns `a`. Used extensively to insert freedom";

        public override List<string> ArgNames { get; } = new List<string> { "a", "b" };

        public Const() : base("firstArg", true,
            new[]
            {
                Curry.ConstructFrom(new Var("a"), new Var("a"), new Var("b"))
            }
        )
        {
            Funcs.AddBuiltin(this, "const");
        }

        private Const(IEnumerable<Type> types) : base("firstArg", types
        )
        {
        }

        public override object Evaluate(Context c, params IExpression[] arguments)
        {
            if (arguments.Length == 1)
            {
                return arguments[0].Evaluate(c);
            }

            var f = arguments[0];
            var args = new IExpression[arguments.Length - 2];
            for (var i = 2; i < arguments.Length; i++)
            {
                args[i - 2] = arguments[i];
            }

            return f.Evaluate(c, args);
        }

        public override IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            var unified = Types.SpecializeTo(allowedTypes);
            if (unified == null)
            {
                return null;
            }

            return new Const(unified);
        }
    }
}