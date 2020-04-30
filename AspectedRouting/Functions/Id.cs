using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Typ;

namespace AspectedRouting.Functions
{
    public class Id : Function
    {  public override string Description { get; } = "Returns the argument unchanged - the identity function. Seems useless at first sight, but useful in parsing";
        public override List<string> ArgNames { get; } = new List<string>{"a"};
        public Id() : base("id", true,
            new[]
            {
                new Curry(new Var("a"), new Var("a")),
            })
        {
        }

        private Id(IEnumerable<Type> types) : base("id", types)
        {
        }

        public override IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            var unified = Types.SpecializeTo(allowedTypes);
            if (unified == null)
            {
                return null;
            }

            return new Id(unified);
        }

        public override object Evaluate(Context c,params IExpression[] arguments)
        {
            return arguments[0].Evaluate(c,arguments.ToList().GetRange(1, arguments.Length-1).ToArray());
        }
    }
}