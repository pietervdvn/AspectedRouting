using System;
using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Language;
using AspectedRouting.Language.Typ;
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
            if (passed.Any())
            {
                return new LuaLiteral(passed, Lua);
            }

            return null;
        }

        public IExpression Optimize(out bool somethingChanged)
        {
            somethingChanged = false;
            return this;
        }

        public void Visit(Func<IExpression, bool> f)
        {
            f(this);
        }

        public bool Equals(IExpression other)
        {
            if (other is LuaLiteral ll)
            {
                return ll.Lua.Equals(this.Lua);
            }

            return false;
        }

        public string Repr()
        {
            if (this.Types.Count() == 1 && this.Types.First() == Typs.Tags)
            {
                return $"new LuaLiteral(Typs.Tags, \"{this.Lua}\")";
            }

            return $"new LuaLiteral(\"{this.Lua}\")";
        }
    }
}