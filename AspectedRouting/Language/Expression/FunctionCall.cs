using System;
using System.Collections.Generic;
using System.Linq;
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

        public FunctionCall(string name, Type type): this(name, new []{type}){
            
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

        public IExpression PruneTypes(Func<Type, bool> allowedTypes)
        {
            var passedTypes = this.Types.Where(allowedTypes);
            if (!passedTypes.Any()) {
                return null;
            }

            return new FunctionCall(this._name, passedTypes);
        }

        public IExpression Optimize(out bool somethingChanged)
        {
            somethingChanged = false;
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

        public bool Equals(IExpression other)
        {
            if (other is FunctionCall fc)
            {
                return fc._name.Equals(this._name);
            }

            return false;
        }

        public string Repr()
        {
            return "new FunctionCall(\"" + this._name + "\")";
        }

        public override string ToString()
        {
            return $"${_name}";
        }
    }
}