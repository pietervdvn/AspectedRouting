using System.Collections.Generic;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Typ;

namespace AspectedRouting.Language.Functions
{
    public class HeadFunction : Function
    {
        public HeadFunction() : base("head", true,
            new[] {
                new Curry(new ListType(new Var("a")),
                    new Var("a")),

                new Curry(
                    new Curry(new Var("b"), new ListType(new Var("a"))),
                    new Curry(new Var("b"), new Var("a")))
            }
        ) { }

        private HeadFunction(IEnumerable<Type> unified) : base("head", unified) { }

        public override string Description { get; } =
            "Select the first non-null value of a list; returns 'null' on empty list or on null";

        public override List<string> ArgNames { get; } = new List<string> {"ls"};

        public override object Evaluate(Context c, params IExpression[] arguments)
        {
            object o = arguments[0];
            if (o is Apply app) {
                o = app.Evaluate(c, arguments.SubArray(1));
            }

            while (o is IExpression e) {
                o = e.Evaluate(c);
            }

            if (!(o is IEnumerable<object> ls)) {
                return null;
            }

            foreach (var v in ls) {
                if (v != null) {
                    return v;
                }
            }

            return null;
        }

        public override IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            var unified = Types.SpecializeTo(allowedTypes);
            if (unified == null) {
                return null;
            }

            return new HeadFunction(unified);
        }
    }
}