using System;
using System.Collections.Generic;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Typ;
using Type = AspectedRouting.Language.Typ.Type;

namespace AspectedRouting.Language.Functions
{
    public class IfDotted : Function
    {
        private static Var a = new Var("a");
        private static Var b = new Var("b");

        public override string Description { get; } = "An if_then_else, but one which takes an extra argument and applies it on the condition, then and else.\n" +
                                                      "Consider `fc`, `fthen` and `felse` are all functions taking an `a`, then:\n" +
                                                      "`(ifDotted fc fthen felse) a` === `(if (fc a) (fthen a) (felse a)`" +
                                                      "Selects either one of the branches, depending on the condition." +
                                                      " The 'then' branch is returned if the condition returns the string `yes` or `true` or the boolean `true`" +
                                                      "If the `else` branch is not set, `null` is returned in the condition is false." +
                                                      "In case the condition returns 'null', then the 'else'-branch is taken.";
        public override List<string> ArgNames { get; } = new List<string> { "condition", "then", "else" };

        public IfDotted() : base("if_then_else_dotted", true,
            new[]
            {

                Curry.ConstructFrom(a,
                    new Curry(b, Typs.Bool),
                    new Curry(b, a),
                    b),
                Curry.ConstructFrom(a,
                    new Curry(b, Typs.String),
                    new Curry(b, a),
                    b),
                Curry.ConstructFrom(a,
                    new Curry(b, Typs.Bool),
                    new Curry(b, a),
                    new Curry(b, a),
                    b),
                Curry.ConstructFrom(a,
                    new Curry(b, Typs.String),
                    new Curry(b, a),
                    new Curry(b, a),
                    b)
            }
        )
        {
            Funcs.AddBuiltin(this, "ifdotted");
            Funcs.AddBuiltin(this, "ifDotted");
        }

        private IfDotted(IEnumerable<Type> types) : base("if_then_else_dotted", types)
        {
        }

        public override object Evaluate(Context c, params IExpression[] arguments)
        {
            var conditionfunc = arguments[0];
            var thenfunc = arguments[1];
            IExpression elsefunc = null;
            IExpression argument = arguments[2];

            if (arguments.Length == 4)
            {
                elsefunc = arguments[2];
                argument = arguments[3];
            }

            var condition = ((IExpression)conditionfunc).Apply(argument).Evaluate(c);

            if (condition != null && (condition.Equals("yes") || condition.Equals("true") || condition.Equals(true)))
            {
                return thenfunc.Apply(argument).Evaluate(c);
            }
            else
            {
                return elsefunc?.Apply(argument)?.Evaluate(c);
            }
        }

        public override IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            var unified = Types.SpecializeTo(allowedTypes);
            if (unified == null)
            {
                return null;
            }

            return new IfDotted(unified);
        }
    }
}