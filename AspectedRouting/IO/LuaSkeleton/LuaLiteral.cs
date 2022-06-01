using System;
using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Language;
using Type = AspectedRouting.Language.Typ.Type;

namespace AspectedRouting.IO.LuaSkeleton
{
    public class LuaLiteral : IExpression
    {
        public readonly string Lua;

        public LuaLiteral(Type type, string lua) : this(new[] { type }, lua) { }

        public LuaLiteral(IEnumerable<Type> types, string lua)
        {
            Lua = lua;
            Types = types;
        }

        public IEnumerable<Type> Types { get; }

        public object Evaluate(Context c, params IExpression[] arguments)
        {
            throw new NotImplementedException();
        }

        public IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            return this;
        }

        public IExpression PruneTypes(System.Func<Type, bool> allowedTypes)
        {
            var passed = Types.Where(allowedTypes);
            if (passed.Any()) {
                return new LuaLiteral(passed, Lua);
            }

            return null;
        }

        public IExpression Optimize()
        {
            return this;
        }

        public void Visit(Func<IExpression, bool> f)
        {
            throw new NotImplementedException();
        }
    }
}