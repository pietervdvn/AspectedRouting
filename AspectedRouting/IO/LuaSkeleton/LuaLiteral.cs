using System;
using System.Collections.Generic;
using AspectedRouting.Language;
using Type = AspectedRouting.Language.Typ.Type;

namespace AspectedRouting.IO.LuaSkeleton
{
    public class LuaLiteral : IExpression
    {
        public readonly string Lua;
        public IEnumerable<Type> Types { get; }

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

        public IExpression Optimize()
        {
           return this;
        }

        public IExpression OptimizeWithArgument(IExpression argument)
        {
            return this.Apply(argument);
        }

        public void Visit(Func<IExpression, bool> f)
        {
            throw new NotImplementedException();
        }
    }
}