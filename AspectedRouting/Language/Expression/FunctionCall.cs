using System;
using System.Collections.Generic;
using AspectedRouting.Language.Typ;
using Type = AspectedRouting.Language.Typ.Type;

namespace AspectedRouting.Language.Expression
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
            
            var func = c.GetFunction(_name);
            c = c.WithAspectName(_name);
            return func.Evaluate(c, arguments);
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

        public IExpression OptimizeWithArgument(IExpression argument)
        {

            if (_name.Equals(Funcs.Id.Name))
            {
                return argument;
            }

            return this.Apply(argument);




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