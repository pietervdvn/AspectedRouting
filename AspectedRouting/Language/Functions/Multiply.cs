using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Typ;

namespace AspectedRouting.Language.Functions
{
    public class Multiply : Function
    {
        public override string Description { get; } =
            "Multiplies all the values in a given list. On a list of booleans, this acts as 'and' or 'all', as `false` and `no` are interpreted as zero. Null values are ignored and thus considered to be `one`";

        public override List<string> ArgNames { get; } = new List<string> { "list" };


        public Multiply() : base("multiply", true,
            new[]
            {
                new Curry(new ListType(Typs.Nat), Typs.Nat),
                new Curry(new ListType(Typs.Int), Typs.Int),
                new Curry(new ListType(Typs.PDouble), Typs.PDouble),
                new Curry(new ListType(Typs.Double), Typs.Double),
                new Curry(new ListType(Typs.Bool), Typs.Bool)
            })
        {
        }

        private Multiply(IEnumerable<Type> specializedTypes) : base("multiply", specializedTypes)
        {
        }

        public override IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            var unified = Types.SpecializeTo(allowedTypes);
            if (unified == null)
            {
                return null;
            }

            return new Multiply(unified);
        }


        public override object Evaluate(Context c, params IExpression[] arguments)
        {
            var ls = ((IEnumerable<object>)arguments[0].Evaluate(c)).Where(o => o != null);
            var expectedType = ((Curry)Types.First()).ResultType;


            switch (expectedType)
            {
                case BoolType _:
                    foreach (var oo in ls)
                    {
                        var o = oo;
                        while (o is IExpression e)
                        {
                            o = e.Evaluate(c);
                        }

                        if (!(o is string s))
                        {
                            return "no";
                        }

                        if (!(s.Equals("yes") || s.Equals("true")))
                        {
                            return "no";
                        }
                    }

                    return "yes";
                case DoubleType _:
                case PDoubleType _:
                    var mult = 1.0;
                    foreach (var oo in ls)
                    {
                        var o = oo;
                        while (o is IExpression e)
                        {
                            o = e.Evaluate(c);
                        }

                        mult *= (double)o;
                    }

                    return mult;
                default:
                    var multI = 1;
                    foreach (var oo in ls)
                    {
                        var o = oo;
                        while (o is IExpression e)
                        {
                            o = e.Evaluate(c);
                        }

                        multI *= (int)o;
                    }

                    return multI;
            }
        }
    }
}