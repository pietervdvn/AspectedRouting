using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Typ;

namespace AspectedRouting.Language.Functions
{
    public class Sum : Function
    {
        public override string Description { get; } = "Sums all the numbers in the given list. If the list is a list of booleans, `yes` or `true` will be considered to equal `1`. Null values are ignored (and thus handled as being `0`)";
        public override List<string> ArgNames { get; } = new List<string> { "list" };

        public Sum() : base("sum", true,
            new[]
            {
                new Curry(new ListType(Typs.Nat), Typs.Nat),
                new Curry(new ListType(Typs.Int), Typs.Int),
                new Curry(new ListType(Typs.PDouble), Typs.PDouble),
                new Curry(new ListType(Typs.Double), Typs.Double),
                new Curry(new ListType(Typs.Bool), Typs.Int),
            })
        {
            Funcs.AddBuiltin(this, "plus");
            Funcs.AddBuiltin(this, "add");

        }


        private Sum(IEnumerable<Type> specializedTypes) : base("sum", specializedTypes)
        {
        }

        public override IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            var unified = Types.SpecializeTo(allowedTypes);
            if (unified == null)
            {
                return null;
            }

            return new Sum(unified);
        }


        public override object Evaluate(Context c, params IExpression[] arguments)
        {
            var ls = ((IEnumerable<object>)arguments[0]
                .Evaluate(c))
                .Where(o => o != null);
            var expectedType = (Types.First() as Curry).ResultType;

            switch (expectedType)
            {
                case BoolType _:
                    var sumB = 0;
                    foreach (var o in ls)
                    {
                        if (o.Equals("yes") || o.Equals("true"))
                        {
                            sumB++;
                        }
                    }

                    return sumB;
                case DoubleType _:
                case PDoubleType _:
                    var sum = 0.0;
                    foreach (var o in ls)
                    {
                        sum += (double)o;
                    }

                    return sum;
                default:
                    var sumI = 1;
                    foreach (var o in ls)
                    {
                        sumI += (int)o;
                    }

                    return sumI;
            }
        }
    }
}