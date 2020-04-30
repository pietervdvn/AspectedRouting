using System;
using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Typ;
using static AspectedRouting.Deconstruct;
using Type = AspectedRouting.Typ.Type;

namespace AspectedRouting.Functions
{
    public class Apply : IExpression
    {
        /// <summary>
        /// Maps the expected return type onto the argument needed for that.
        /// The argument is specialized for this return type
        /// </summary>
        public readonly Dictionary<Type, (IExpression f, IExpression a)> FunctionApplications;

        // Only used for when there is no typechecking possible
        private readonly string _debugInfo;

        public IExpression F => FunctionApplications.Values.First().f;
        public IExpression A => FunctionApplications.Values.First().a;

        public IEnumerable<Type> Types => FunctionApplications.Keys;

        private Apply(string debugInfo, Dictionary<Type, (IExpression f, IExpression a)> argument)
        {
            _debugInfo = debugInfo;
            FunctionApplications = argument;
        }

        public Apply(IExpression f, IExpression argument)
        {
            if (f == null || argument == null)
            {
                throw new NullReferenceException();
            }

            FunctionApplications = new Dictionary<Type, (IExpression f, IExpression a)>();

            var argTypesCleaned = argument.Types.RenameVars(f.Types);
            var typesCleaned = argTypesCleaned.ToList();
            foreach (var funcType in f.Types)
            {
                if (!(funcType is Curry c))
                {
                    continue;
                }

                var expectedArgType = c.ArgType;
                var expectedResultType = c.ResultType;


                foreach (var argType in typesCleaned)
                {
                    // we try to unify the argType with the expected type
                    var substitutions = expectedArgType.UnificationTable(argType);
                    if (substitutions == null)
                    {
                        continue;
                    }

                    var actualArgType = expectedArgType.Substitute(substitutions);
                    var actualResultType = expectedResultType.Substitute(substitutions);

                    var actualFunction = f.Specialize(new Curry(actualArgType, actualResultType));
                    var actualArgument = argument.Specialize(actualArgType);

                    if (actualFunction == null || actualArgument == null)
                    {
                        continue;
                    }

                    if (FunctionApplications.ContainsKey(actualResultType))
                    {
                        continue;
                    }

                    FunctionApplications.Add(actualResultType, (actualFunction, actualArgument));
                }
            }

            if (!FunctionApplications.Any())
            {
                try
                {
                    _debugInfo = $"\n{f.Optimize().TypeBreakdown()}\n" +
                                 $"is applied on an argument with types:" +
                                 $"{string.Join(", ", argument.Optimize().Types)}";
                }
                catch (Exception e)
                {
                    _debugInfo = $"\n{f.TypeBreakdown()}\n" +
                                 $"{argument.TypeBreakdown()}";
                }
            }
        }


        public object Evaluate(Context c, params IExpression[] arguments)
        {
            if (Types.Count() > 1)
            {
                // We try to select the smallest type
            }

            var type = Types.First();
            var (fExpr, argExpr) = FunctionApplications[type];


            var arg = argExpr;
            var allArgs = new IExpression[arguments.Length + 1];
            allArgs[0] = arg;
            for (var i = 0; i < arguments.Length; i++)
            {
                allArgs[i + 1] = arguments[i];
            }

            return fExpr.Evaluate(c, allArgs);
        }


        IExpression IExpression.Specialize(IEnumerable<Type> allowedTypes)
        {
            return Specialize(allowedTypes);
        }

        private Apply Specialize(IEnumerable<Type> allowedTypes)
        {
            var newArgs = new Dictionary<Type, (IExpression f, IExpression a)>();

            foreach (var allowedType in allowedTypes)
            {
                foreach (var (resultType, (funExpr, argExpr)) in FunctionApplications)
                {
                    var substitutions = resultType.UnificationTable(allowedType);
                    if (substitutions == null)
                    {
                        continue;
                    }

                    var actualResultType = resultType.Substitute(substitutions);
                    var actualFunction = funExpr.Specialize(substitutions);
                    var actualArgument = argExpr.Specialize(substitutions);

                    if (actualFunction == null || actualArgument == null)
                    {
                        continue;
                    }

                    newArgs[actualResultType] = (actualFunction, actualArgument);
                }
            }

            if (!newArgs.Any())
            {
                return null;
            }

            return new Apply(_debugInfo, newArgs);
        }

        public IExpression Optimize()
        {
            // (eitherfunc dot id) id
            // => (const dot _) id => dot id => id
            // or => (constRight _ id) id => id id => id 
            if (
                UnApplyAny(
                    UnApplyAny(
                        UnApplyAny(
                            IsFunc(Funcs.Const),
                            IsFunc(Funcs.Dot)),
                        Any()),
                    IsFunc(Funcs.Id)
                ).Invoke(this)
                && UnApplyAny(UnApplyAny(
                        UnApplyAny(
                            IsFunc(Funcs.ConstRight),
                            Any()),
                        IsFunc(Funcs.Id)),
                    IsFunc(Funcs.Id)
                ).Invoke(this))
            {
                return Funcs.Id;
            }


            if (Types.Count() > 1)
            {
                var optimized = new Dictionary<Type, (IExpression f, IExpression a)>();
                foreach (var (resultType, (f, a)) in FunctionApplications)
                {
                    var fOpt = f.Optimize();
                    var aOpt = a.Optimize();
                    optimized.Add(resultType, (fOpt, aOpt));
                }

                return new Apply(_debugInfo, optimized);
            }

            {
                var (f, a) = FunctionApplications.Values.First();

                var (newFa, expr) = OptimizeApplicationPair(f, a);
                if (expr != null)
                {
                    return expr;
                }

                (f, a) = newFa.Value;
                return new Apply(f, a);
            }
        }

        private ((IExpression fOpt, IExpression fArg)?, IExpression result) OptimizeApplicationPair(IExpression f,
            IExpression a)
        {
            f = f.Optimize();
            a = a.Optimize();


            switch (f)
            {
                case Id _:
                    return (null, a);

                case Apply apply:

                    // const x y -> y
                    var (subF, subArg) = apply.FunctionApplications.Values.First();


                    if (subF is Const _)
                    {
                        return (null, subArg);
                    }

                    if (subF is ConstRight _)
                    {
                        return (null, a);
                    }

                    var f0 = new List<IExpression>();
                    var f1 = new List<IExpression>();


                    // ((dot f0) f1)
                    // ((dot f0) f1) arg is the actual expression, but arg is already split of
                    if (UnApply(
                            UnApply(
                                IsFunc(Funcs.Dot),
                                Assign(f0)
                            ),
                            Assign(f1)).Invoke(f)
                    )
                    {
                        // f0 (f1 arg)
                        // which used to be (f0 . f1) arg
                        return ((f0.First(), new Apply(f1.First(), subArg)), null);
                    }


                    break;
            }

            return ((f, a), null);
        }

        public void Visit(Func<IExpression, bool> visitor)
        {
            var continueVisit = visitor(this);
            if (!continueVisit)
            {
                return;
            }
            foreach (var (_, (f, a)) in FunctionApplications)
            {
                f.Visit(visitor);
                a.Visit(visitor);
            }
        }

        public override string ToString()
        {
            if (!FunctionApplications.Any())
            {
                return "NOT-TYPECHECKABLE APPLICATION: " + _debugInfo;
            }

            var (f, arg) = FunctionApplications.Values.First();
            if (f is Id _)
            {
                return arg.ToString();
            }

            return $"({f} {arg.ToString().Indent()})";
        }
    }
}