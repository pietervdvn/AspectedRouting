using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Typ;

namespace AspectedRouting.Language.Functions
{
    public class Max : Function
    {
        public override string Description { get; } =
            "Returns the biggest value in the list. For a list of booleans, this acts as 'or'";

        public override List<string> ArgNames { get; } = new List<string> { "list" };

        public Max() : base("max", true,
            new[]
            {
                new Curry(new ListType(Typs.Nat), Typs.Nat),
                new Curry(new ListType(Typs.Int), Typs.Int),
                new Curry(new ListType(Typs.PDouble), Typs.PDouble),
                new Curry(new ListType(Typs.Double), Typs.Double),
                new Curry(new ListType(Typs.Bool), Typs.Bool),
            })

        {
            Funcs.AddBuiltin(this, "or");
        }

        private Max(IEnumerable<Type> specializedTypes) : base("max", specializedTypes)
        {
        }

        public override IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            var unified = Types.SpecializeTo(allowedTypes);
            if (unified == null)
            {
                return null;
            }

            return new Max(unified);
        }


        public override object Evaluate(Context c, params IExpression[] arguments)
        {
            var ls = ((IEnumerable<object>)arguments[0].Evaluate(c)).Where(o => o != null);

            var expectedType = (Types.First() as Curry).ResultType;
            switch (expectedType)
            {
                case BoolType _:
                    if (ls.Select(o => o.Equals("yes") || o.Equals("true")).Any(b => b))
                    {
                        return "yes";
                    }

                    return "no";
                default:
                    return ls.Select(o =>
                    {
                        switch (o)
                        {
                            case double d:
                                return d;
                            case int i:
                                return i;
                            default:
                                throw new SwitchExpressionException("Unknwon type: " + o);
                        }
                    }).Max();
            }
        }
    }
}