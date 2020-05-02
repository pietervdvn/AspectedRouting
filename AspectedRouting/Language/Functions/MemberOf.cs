using System.Collections.Generic;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Typ;
using Type = AspectedRouting.Language.Typ.Type;

namespace AspectedRouting.Language.Functions
{
    public class MemberOf : Function
    {
        public override string Description { get; } =
            "This function returns true, if the way is member of a relation matching the specified function.\n" +
            "\n" +
            "In order to use this for itinero 1.0, the membership _must_ be the top level expression.\n" +
            "\n" +
            "Conceptually, when the aspect is executed for a way, every relation will be used as argument in the subfunction `f`\n" +
            "If this subfunction returns 'true', the entire aspect will return true.\n\n" +
            "In the lua implementation for itinero 1.0, this is implemented slightly different:" +
            " a flag `_relation:<aspect_name>=\"yes\"` will be set if the aspect matches on every way for where this aspect matches.\n" +
            "However, this plays poorly with parameters (e.g.: what if we want to cycle over a highway which is part of a certain cycling network with a certain `#network_name`?) " +
            "Luckily, parameters can only be simple values. To work around this problem, an extra tag is introduced for _every single profile_:" +
            "`_relation:<profile_name>:<aspect_name>=yes'. The subfunction is thus executed `countOr(relations) * countOf(profiles)` time, yielding `countOf(profiles)` tags." +
            " The profile function then picks the tags for himself and strips the `<profile_name>:` away from the key.\n\n" +
            "\n\n" +
            "In the test.csv, one can simply use `_relation:<aspect_name>=yes` to mimic relations in your tests";

        public override List<string> ArgNames { get; } = new List<string>
        {
            "f","tags"
        };

        public MemberOf() : base(
            "memberOf", true,
            new[]
            {
                new Curry(
                    new Curry(Typs.Tags, Typs.Bool),
                    new Curry(Typs.Tags, Typs.Bool))
            }
        )
        {
        }

        public MemberOf(IEnumerable<Type> types) : base("memberOf", types)
        {
        }

        public override object Evaluate(Context c, params IExpression[] arguments)
        {
          //  var subFunction = arguments[0];
            
            var tags =(Dictionary<string, string>) arguments[1].Evaluate(c);
            var name = c.AspectName.TrimStart('$');
            
            if(tags.TryGetValue("_relation:"+name, out var v))
            {
                return v;
            }
            return "no";
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