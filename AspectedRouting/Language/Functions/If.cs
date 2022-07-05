using System.Collections.Generic;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Typ;

namespace AspectedRouting.Language.Functions
{
    public class If : Function
    {
        private static Var a = new Var("a");
        private static Var b = new Var("b");

        public override string Description { get; } = "Selects either one of the branches, depending on the condition." +
                                                      " The 'then' branch is returned if the condition returns the string `yes` or `true`." +
                                                      " Otherwise, the `else` branch is taken (including if the condition returns `null`)" +
                                                      "If the `else` branch is not set, `null` is returned if the condition evaluates to false.";
        public override List<string> ArgNames { get; } = new List<string> {"condition", "then", "else"};

        public If() : base("if_then_else", true,
            new[]
            {
                Curry.ConstructFrom(a, Typs.Bool, a, a),
                Curry.ConstructFrom(a, Typs.Bool, a),
                Curry.ConstructFrom(a, Typs.String, a, a),
                Curry.ConstructFrom(a, Typs.String, a)
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
            IExpression @else = null;
            if (arguments.Length > 2)
            {
                @else = arguments[2];
            }

            if (condition != null && (condition.Equals("yes") || condition.Equals("true") || condition.Equals(true)))
            {
                return then.Evaluate(c);
            }
            else
            {
                return @else?.Evaluate(c);
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