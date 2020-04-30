using System;
using System.Collections.Generic;
using AspectedRouting.Typ;
using Type = AspectedRouting.Typ.Type;

namespace AspectedRouting.Functions
{
    public class FunctionCall : IExpression
    {
        private readonly string _name;

        public string CalledFunctionName
        {
            get
            {
                if (_name.StartsWith("$"))
                {
                    return _name.Substring(1);
                }
                else
                {
                    return _name;
                }
            }
        }

        public IEnumerable<Type> Types { get; }

        public FunctionCall(string name, IEnumerable<Type> types)
        {
            _name = name;
            Types = types;
        }

        public object Evaluate(Context c, params IExpression[] arguments)
        {
            return c.GetFunction(_name).Evaluate(c, arguments);
        }

        public IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            var unified = Types.SpecializeTo(allowedTypes);
            if (unified == null)
            {
                return null;
            }

            return new FunctionCall(_name, unified);
        }

        public IExpression Optimize()
        {
            return this;
        }

        public void Visit(Func<IExpression, bool> f)
        {
            f(this);
        }

        public override string ToString()
        {
            return $"${_name}";
        }
    }
}