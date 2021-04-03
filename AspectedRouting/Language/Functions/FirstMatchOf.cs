using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Typ;
using Type = AspectedRouting.Language.Typ.Type;

namespace AspectedRouting.Language.Functions
{
    public class FirstMatchOf : Function
    {
        public override string Description { get; } = "This higherorder function takes a list of keys, a mapping (function over tags) and a collection of tags. It will try the function for the first key (and it's respective value). If the function fails (it gives null), it'll try the next key.\n\n" +
                                                      "E.g. `$firstMatchOf ['maxspeed','highway'] {'maxspeed' --> $parse, 'highway' --> {residential --> 30, tertiary --> 50}}` applied on `{maxspeed=70, highway=tertiary}` will yield `70` as that is the first key in the list; `{highway=residential}` will yield `30`.";
        public override List<string> ArgNames { get; } = new List<string> {"s"};

        public FirstMatchOf() : base("first_match_of", true,
            new[]
            {
                // [String] -> (Tags -> [a]) -> Tags -> a
                Curry.ConstructFrom(new Var("a"), // Result type on top!
                    new ListType(Typs.String),
                    new Curry(Typs.Tags, new ListType(new Var("a"))),
                    Typs.Tags
                )
            })
        {
            Funcs.AddBuiltin( this,"firstMatchOf");
        }

        private FirstMatchOf(IEnumerable<Type> types) : base("first_match_of", types)
        {
        }

        public override IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            var unified = Types.SpecializeTo(allowedTypes);
            if (unified == null)
            {
                return null;
            }

            return new FirstMatchOf(unified);
        }

        public override object Evaluate(Context c, params IExpression[] arguments)
        {
            var order = ((IEnumerable<object>) arguments[0].Evaluate(c))
                .Select(o =>
                {
                    if (o is string s)
                    {
                        return s;
                    }
                    else
                    {
                        return (string) ((IExpression) o).Evaluate(c);
                    }
                });
            var function = arguments[1];
            var tags = (Dictionary<string, string>) arguments[2].Evaluate(c);

            var singletonDict = new Dictionary<string, string>();

            foreach (var tagKey in order)
            {
                if (!tags.TryGetValue(tagKey, out var tagValue)) continue;
                singletonDict.Clear();
                singletonDict[tagKey] = tagValue;
                var result = function.Evaluate(c, new Constant(singletonDict));

                if (result == null || (result is List<object> ls && !ls.Any()))
                {
                    continue;
                }

                return ((List<object>) result).First();
            }

            return null;
        }
    }
}