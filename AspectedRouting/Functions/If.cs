using System.Collections.Generic;
using AspectedRouting.Typ;

namespace AspectedRouting.Functions
{
    public class If : Function
    {
        private static Var a = new Var("a");
        
        public override string Description { get; } = "Selects either one of the branches, depending on the condition." +
                                                      "If the `else` branch is not set, `null` is returned in the condition is false.";
        public override List<string> ArgNames { get; } = new List<string> {"condition", "then", "else"};

        public If() : base("if_then_else", true,
            new[]
            {
                Curry.ConstructFrom(a, Typs.Bool, a, a),
                Curry.ConstructFrom(a, Typs.Bool, a)
            }
        )
        {
            Funcs.AddBuiltin(this, "if");
        }

        private If(IEnumerable<Type> types) : base("if_then_else", types)
        {
        }

        public override object Evaluate(Context c, params IExpression[] arguments)
        {
            var condition =  arguments[0].Evaluate(c);
            var then = arguments[1];
            IExpression _else = null;
            if (arguments.Length > 2)
            {
                _else = arguments[2];
            }

            if (condition != null && (condition.Equals("yes") || condition.Equals("true")))
            {
                return then.Evaluate(c);
            }
            else
            {
                return _else?.Evaluate(c);
            }
        }

        public override IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            var unified = Types.SpecializeTo(allowedTypes);
            if (unified == null)
            {
                return null;
            }

            return new If(unified);
        }
    }
}