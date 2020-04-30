using System;
using System.Collections.Generic;
using Type = AspectedRouting.Typ.Type;

namespace AspectedRouting.Functions
{
    public class AspectMetadata : IExpression
    {
        public string Name { get; }
        public string Description { get; }
        public string Author { get; }
        public string Unit { get; }
        public string Filepath { get; }
        public readonly IExpression ExpressionImplementation;

        public readonly bool ProfileInternal;

        public AspectMetadata(IExpression expressionImplementation,
            string name, string description, string author, string unit, string filepath, bool profileInternal = false)
        {
            Name = name;
            Description = description;
            Author = author;
            Unit = unit;
            Filepath = filepath;
            ExpressionImplementation = expressionImplementation;
            ProfileInternal = profileInternal;
        }


        public IEnumerable<Type> Types => ExpressionImplementation.Types;

        public object Evaluate(Context c, params IExpression[] arguments)
        {
            return ExpressionImplementation.Evaluate(c, arguments);
        }

        public IExpression Specialize(IEnumerable<Type> allowedTypes)
        {
            return new AspectMetadata(
                ExpressionImplementation.Specialize(allowedTypes),
                Name, Description, Author, Unit, Filepath);
        }

        public IExpression Optimize()
        {
            return new AspectMetadata(ExpressionImplementation.Optimize(),
                Name, Description, Author, Unit, Filepath);
        }

        public void Visit(Func<IExpression, bool> f)
        {
            var continueVisit = f(this);
            if (!continueVisit)
            {
                return;
            }

            ExpressionImplementation.Visit(f);
        }

        public override string ToString()
        {
            return $"# {Name}; {Unit} by {Author}\n\n#   by {Author}\n#   {Description}\n{ExpressionImplementation}";
        }
    }
}