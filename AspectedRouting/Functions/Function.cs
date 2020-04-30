using System;
using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Typ;
using Type = AspectedRouting.Typ.Type;

namespace AspectedRouting.Functions
{
    public abstract class Function : IExpression
    {
        public string Name { get; }
        public virtual List<string> ArgNames { get; } = null;

        public virtual string Description { get; } = "";

        protected Function(string name, bool isBuiltin, IEnumerable<Curry> types)
        {
            Name = name;
            if (isBuiltin)
            {
                Funcs.AddBuiltin(this);
            }

            Types = types;
        }

        protected Function(string name, IEnumerable<Type> types)
        {
            Name = name;
            Types = types;
        }

        public IEnumerable<Type> Types { get; }

        public abstract object Evaluate(Context c, params IExpression[] arguments);
        public abstract IExpression Specialize(IEnumerable<Type> allowedTypes);

        public virtual IExpression Optimize()
        {
            return this;
        }

        public virtual void Visit(Func<IExpression, bool> f)
        {
            f(this);
        }

        public override string ToString()
        {
            return $"${Name}";
        }

        public IExpression Apply(params IExpression[] args)
        {
            return this.Apply(args.ToList());
        }

        /// <summary>
        /// Gives an overview per argument what the possible types are.
        /// The return type has an empty string as key
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, List<Type>> ArgBreakdown()
        {
            var dict = new Dictionary<string, List<Type>>();

            if (ArgNames == null)
            {
                throw new Exception("ArgNames not set for "+Name);
            }
            
            foreach (var n in ArgNames)
            {
                dict[n] = new List<Type>();
            }

            dict[""] = new List<Type>();
            foreach (var type in Types)
            {
                var restType = type;
                foreach (var n in ArgNames)
                {
                    if (!(restType is Curry c))
                    {
                        dict[n].Add(null);
                        continue;
                    }

                    dict[n].Add(c.ArgType);
                    restType = c.ResultType;
                }

                dict[""].Add(restType);
            }

            return dict;
        }
    }
}