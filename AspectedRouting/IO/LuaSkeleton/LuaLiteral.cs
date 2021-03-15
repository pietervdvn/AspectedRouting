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
        public IEnumerable<Type> Types { get; }

        public LuaLiteral(Type type, string lua):this(new [] {type}, lua)
        {
            
        }

        public LuaLiteral(IEnumerable<Type> types, string lua)
        {
            Lua = lua;
            Types = types;
        }
        
        public object Evaluate(Context c, params IExpression[] arguments)
        {
            throw new NotImplementedException();
        }

        public IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            return this;
        }

        public IExpression PruneTypes(Func<Type, bool> allowedTypes)
        {
            var passed = this.Types.Where(allowedTypes);
            if (passed.Any()) {
                return new LuaLiteral(passed, this.Lua);
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