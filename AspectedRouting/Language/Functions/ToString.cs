using System.Collections.Generic;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Typ;

namespace AspectedRouting.Language.Functions
{
    public class ToString : Function
    {
        public override string Description { get; } = "Converts a value into a human readable string";
        public override List<string> ArgNames { get; } = new List<string> { "obj" };

        public ToString() : base("to_string", true,
            new[] { new Curry(new Var("a"), Typs.String) })
        {
        }

        public ToString(IEnumerable<Type> types) : base("to_string", types)
        {
        }

        public override object Evaluate(Context c, params IExpression[] arguments)
        {
            var a = arguments[0];
            return a.ToString();
        }

        public override IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            var unified = Types.SpecializeTo(allowedTypes);
            if (unified == null)
            {
                return null;
            }

            return new ToString(unified);
        }
    }
}