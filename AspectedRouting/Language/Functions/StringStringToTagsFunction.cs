using System.Collections.Generic;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Typ;

namespace AspectedRouting.Language.Functions
{
    public class StringStringToTagsFunction : Function
    {
        public override string Description { get; } =
            "*stringToTags* converts a function `string -> string -> a` into a function `tags -> [a]`. " +
            "It is used internally to convert a hash of functions. `stringToTags` shouldn't be needed when implementing profiles.";

        public override List<string> ArgNames { get; } = new List<string> {"f", "tags"};

        private static readonly Type _baseFunction =
            Curry.ConstructFrom(new Var("a"), Typs.String, Typs.String);


        public StringStringToTagsFunction() : base("stringToTags", true,
            new[]
            {
                new Curry(_baseFunction,
                    new Curry(Typs.Tags, new ListType(new Var("a"))))
            }
        )
        {
        }

        private StringStringToTagsFunction(IEnumerable<Type> unified) : base("stringToTags", unified)
        {
        }

        public override object Evaluate(Context c, params IExpression[] arguments)
        {
            var f = arguments[0];
            var tags = (Dictionary<string, string>) arguments[1].Evaluate(c);
            var result = new List<object>();
            foreach (var (k, v) in tags)
            {
                var r = f.Evaluate(c, new Constant(k), new Constant(v));
                if (r == null)
                {
                    continue;
                }

                result.Add(r);
            }

            return result;
        }

        public override IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            var unified = Types.SpecializeTo(allowedTypes);
            if (unified == null)
            {
                return null;
            }

            return new StringStringToTagsFunction(unified);
        }
    }
}