using System;
using System.Collections.Generic;
using AspectedRouting.Language;
using AspectedRouting.Language.Expression;

namespace AspectedRouting.IO.LuaSnippets
{
    /// <summary>
    /// A lua snippet is a piece of code which converts a functional expression into an imperative piece of lua code.
    /// While the output is less readable then the functional approach, it is more performant
    /// 
    /// </summary>
    public abstract class LuaSnippet
    {
        /// <summary>
        /// Indicates which function is implemented
        /// </summary>
        public readonly Function ImplementsFunction;

        protected LuaSnippet(Function implements)
        {
            ImplementsFunction = implements;
        }

        public abstract string Convert(LuaSkeleton.LuaSkeleton lua, string assignTo, List<IExpression> args);
    }
}