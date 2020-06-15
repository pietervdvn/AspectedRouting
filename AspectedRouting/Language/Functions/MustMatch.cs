using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Typ;
using Type = AspectedRouting.Language.Typ.Type;

namespace AspectedRouting.Language.Functions
{
    public class MustMatch : Function
    {
        public override string Description { get; } =
            "Checks that every specified key is present and gives a non-false value\n." +
            "" +
            "\n" +
            "If, on top, a value is present with a mapping, every key/value will be executed and must return a value that is not 'no' or 'false'\n" +
            "Note that this is a privileged builtin function, as the parser will automatically inject the keys used in the called function.";

        public override List<string> ArgNames { get; } = new List<string>
        {
            "neededKeys (filled in by parser)",
            "f"
        };

        public MustMatch() : base("mustMatch", true,
            new[]
            {
                // [String] -> (Tags -> [string]) -> Tags -> bool
                Curry.ConstructFrom(Typs.Bool, // Result type on top!
                    new ListType(Typs.String), // List of keys to check for
                    new Curry(Typs.Tags, new ListType(Typs.String)), // The function to execute on every key
                    Typs.Tags // The tags to apply this on
                )
            })
        {
        }

        private MustMatch(IEnumerable<Type> types) : base("mustMatch", types)
        {
        }

        public override IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            var unified = Types.SpecializeTo(allowedTypes);
            if (unified == null)
            {
                return null;
            }

            return new MustMatch(unified);
        }

        public override object Evaluate(Context c, params IExpression[] arguments)
        {
            var neededKeys = (IEnumerable<object>) arguments[0].Evaluate(c); 
            var function = arguments[1];

            var tags = (Dictionary<string, string>) arguments[2].Evaluate(c);

            foreach (var oo in neededKeys)
            {
                var o = oo;
                while (o is IExpression e)
                {
                    o = e.Evaluate(c);
                }
                if (!(o is string tagKey))
                {
                    continue;
                }

                if (!tags.ContainsKey(tagKey)) return "no";
            }

            var result = (IEnumerable<object>) function.Evaluate(c, new Constant(tags));

            if (!result.Any(o =>
                o == null ||
                (o is string s && (s.Equals("no") || s.Equals("false")))))
            {
                return "yes";
            }

            return "no";
        }
    }
}