using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Typ;
using Type = AspectedRouting.Language.Typ.Type;

namespace AspectedRouting.Language.Functions
{
    public class Parse : Function
    {
        public override string Description { get; } = "Parses a string into a numerical value. Returns 'null' if parsing fails or no input is given. If a duration is given (e.g. `01:15`), then the number of minutes (75) is returned";
        public override List<string> ArgNames { get; } = new List<string> { "s" };

        public Parse() : base("parse", true,
            new[]
            {
                new Curry(Typs.String, Typs.Double),
                new Curry(Typs.String, Typs.PDouble),
            })
        {
        }

        private Parse(IEnumerable<Type> specializedTypes) : base("parse", specializedTypes)
        {
        }

        public override IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            var unified = Types.SpecializeTo(allowedTypes);
            if (unified == null)
            {
                return null;
            }

            return new Parse(unified);
        }

        public override object Evaluate(Context c, params IExpression[] arguments)
        {
            var arg = (string)arguments[0].Evaluate(c);
            var expectedType = ((Curry)Types.First()).ResultType;

            var duration = Regex.Match(arg, @"^(\d+):(\d+)$");
            if (duration.Success)
            {
                // This is a duration of the form 'hh:mm' -> we return the total minute count
                var hours = int.Parse(duration.Groups[1].Value);
                var minutes = int.Parse(duration.Groups[2].Value);
                arg = (hours * 60 + minutes).ToString();
            }

            try
            {

                switch (expectedType)
                {
                    case PDoubleType _:
                    case DoubleType _:
                        return double.Parse(arg);
                    default: return int.Parse(arg);
                }
            }
            catch (Exception)
            {
                Console.Error.WriteLine("Could not parse " + arg + " as " + expectedType);
                return null;
            }

        }
    }
}