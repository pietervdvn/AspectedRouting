using System;
using System.Collections.Generic;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Functions;

namespace AspectedRouting.Language
{
    public class Context
    {
        public readonly Dictionary<string, IExpression> Parameters = new Dictionary<string, IExpression>();

        public readonly Dictionary<string, AspectMetadata> DefinedFunctions = new Dictionary<string, AspectMetadata>();

        public readonly string AspectName;

        public Context()
        {
        }

        protected Context(string aspectName, Dictionary<string, IExpression> parameters,
            Dictionary<string, AspectMetadata> definedFunctions)
        {
            AspectName = aspectName;
            Parameters = parameters;
            DefinedFunctions = definedFunctions;
        }

        public Context(Context c) : this(c.AspectName, c.Parameters, c.DefinedFunctions)
        {
        }

        public void AddParameter(string name, string value)
        {
            Parameters.Add(name, new Constant(value));
        }

        public void AddParameter(string name, IExpression value)
        {
            Parameters.Add(name, value);
        }

        public void AddFunction(string name, AspectMetadata function)
        {
            if (Funcs.Builtins.ContainsKey(name))
            {
                throw new ArgumentException("Function " + name + " already exists, it is a builtin function");
            }

            if (DefinedFunctions.ContainsKey(name) && !function.ProfileInternal)
            {
                throw new ArgumentException("Function " + name + " already exists");
            }

            DefinedFunctions[name] = function;
        }

        public AspectMetadata GetAspect(string name)
        {
            if (name.StartsWith("$"))
            {
                name = name.Substring(1);
            }

            if (DefinedFunctions.ContainsKey(name))
            {
                return DefinedFunctions[name];
            }

            throw new ArgumentException(
                $"The aspect {name} is not a defined function. Known functions are " +
                string.Join(", ", DefinedFunctions.Keys));
        }

        public IExpression GetFunction(string name)
        {
            if (name.StartsWith("$"))
            {
                name = name.Substring(1);
            }

            if (Funcs.Builtins.ContainsKey(name))
            {
                return Funcs.Builtins[name];
            }

            if (DefinedFunctions.ContainsKey(name))
            {
                return DefinedFunctions[name];
            }

            throw new ArgumentException(
                $"The function {name} is not a defined nor builtin function. Known functions are " +
                string.Join(", ", DefinedFunctions.Keys));
        }

        public Context WithParameters(Dictionary<string, IExpression> parameters)
        {
            return new Context(AspectName, parameters, DefinedFunctions);
        }

        public Context WithAspectName(string name)
        {
            return new Context(name, Parameters, DefinedFunctions);
        }
    }
}