using System.Collections.Generic;
using AspectedRouting.Typ;

namespace AspectedRouting.Functions
{
    /// <summary>
    /// Converts a function 'string -> string -> a' onto a function 'tags -> [a]'
    /// </summary>
    public class StringStringToTagsFunction : Function
    {
        private static Type baseFunction =
            Curry.ConstructFrom(new Var("a"), Typs.String, Typs.String);


        public StringStringToTagsFunction() : base("stringToTags", true,
            new[]
            {
                new Curry(baseFunction, new Curry(Typs.Tags, new ListType(new Var("a"))))
            }
        )
        {
        }

        private StringStringToTagsFunction(IEnumerable<Type> unified) : base("stringToTags",  unified)
        {
        }

        public override object Evaluate(Context c, params IExpression[] arguments)
        {
            var f = arguments[0];
            var tags = (Dictionary<string, string>) arguments[1].Evaluate(c);
            var result = new List<object>();
            foreach (var (k, v) in tags)
            {
                result.Add(f.Evaluate(c, new Constant(k), new Constant(v)));
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