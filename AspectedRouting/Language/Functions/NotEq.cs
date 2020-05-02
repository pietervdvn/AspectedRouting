using System.Collections.Generic;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Typ;

namespace AspectedRouting.Language.Functions
{
    public class NotEq : Function
    {
        public override string Description { get; } = "OVerloaded function, either boolean not or returns 'yes' if the two passed in values are _not_ the same;";
        public override List<string> ArgNames { get; } = new List<string> {"a", "b"};

        public NotEq() : base("notEq", true,
            new[]
            {
                Curry.ConstructFrom(Typs.Bool, new Var("a"), new Var("a")),
                Curry.ConstructFrom(Typs.String, new Var("a"), new Var("a")),
                new Curry(Typs.Bool, Typs.Bool), 
            })
        {
            Funcs.AddBuiltin(this, "not");
        }

        private NotEq(IEnumerable<Type> types) : base("notEq", types)
        {
        }

        public override IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            var unified = Types.SpecializeTo(allowedTypes);
            if (unified == null)
            {
                return null;
            }

            return new NotEq(unified);
        }

        public override object Evaluate(Context c, params IExpression[] arguments)
        {

            if (arguments.Length == 1)
            {
                var booleanArg = arguments[0].Evaluate(c);
                return booleanArg.Equals("no");
            }
            
            var arg0 = arguments[0].Evaluate(c);
            var arg1 = arguments[1].Evaluate(c);
            if ((!(arg0?.Equals(arg1) ?? false)))
            {
                return "yes";
            }
            else
            {
                return "no";
            }
        }
    }
}