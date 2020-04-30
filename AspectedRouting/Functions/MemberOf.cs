using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Typ;

namespace AspectedRouting.Functions
{
    public class MemberOf : Function
    {
        public override string Description { get; } =
            "This function uses memberships of relations to calculate values.\n" +
            "\n" +
            "Consider all the relations the scrutinized way is part of." +
            "The enclosed function is executed for every single relation which is part of it, generating a list of results." +
            "This list of results is in turn returned by 'memberOf'" +
            "\n" +
            "In itinero 1/lua, this is implemented by converting the matching relations and by adding the tags of the relations to the dictionary (or table) with the highway tags." +
            "The prefix is '_relation:n:key=value', where 'n' is a value between 0 and the number of matching relations (implying that all of these numbers are scanned)." +
            "The matching relations can be extracted by the compiler for the preprocessing.\n\n" +
            "For testing, the relation can be emulated by using e.g. '_relation:0:key=value'";

        public override List<string> ArgNames { get; } = new List<string>
        {
            "f","tags"
        };

        public MemberOf() : base(
            "memberOf", true,
            new[]
            {
                new Curry(
                    new Curry(Typs.Tags, new Var("a")),
                    new Curry(Typs.Tags, new ListType(new Var("a"))))
            }
        )
        {
        }

        public MemberOf(IEnumerable<Type> types) : base("memberOf", types)
        {
        }

        public override object Evaluate(Context c, params IExpression[] arguments)
        {
            var f = arguments[0];
            var tags = (Dictionary<string, string>) arguments[1].Evaluate(c);

            
            var prefixes = new Dictionary<int, Dictionary<string, string>>();
            foreach (var (k, v) in tags)
            {
                if (!k.StartsWith("_relation:")) continue;
                var s = k.Split(":")[1];
                if (int.TryParse(s, out var i))
                {
                    var key = k.Substring(("_relation:" + i + ":").Length);
                    if (prefixes.TryGetValue(i, out var relationTags))
                    {
                        relationTags[key] = v;
                    }
                    else
                    {
                        prefixes[i] = new Dictionary<string, string>
                        {
                            {key, v}
                        };
                    }
                }
            }

            
            // At this point, we have all the tags of all the relations
            // Time to run the function on all of them

            var result = new List<object>();
            foreach (var relationTags in prefixes.Values)
            {
                var o = f.Evaluate(c, new Constant(relationTags));
                if (o != null)
                {
                    result.Add(o);
                }
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

            return new MemberOf(unified);
        }
    }
}