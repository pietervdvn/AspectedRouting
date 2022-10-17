using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectedRouting.Language.Typ
{
    public class Curry : Type
    {
        public readonly Type ArgType;
        public readonly Type ResultType;

        public Curry(Type argType, Type resultType) : base(ToString(argType, resultType), false)
        {
            ArgType = argType;
            ResultType = resultType;
        }


        private static string ToString(Type argType, Type resultType)
        {
            var arg = argType.ToString();
            if (argType is Curry)
            {
                arg = $"({arg})";
            }

            return arg + " -> " + resultType;
        }

        public static Curry ConstructFrom(Type resultType, params Type[] types)
        {
            return ConstructFrom(resultType, types.ToList());
        }

        public static Curry ConstructFrom(Type resultType, List<Type> types)
        {
            if (types.Count == 0)
            {
                throw new Exception("No argument types given");
            }

            if (types.Count == 1)
            {
                return new Curry(types[0], resultType);
            }

            var arg = types[0];
            var rest = types.GetRange(1, types.Count - 1);
            return new Curry(arg, ConstructFrom(resultType, rest));
        }
    }
}