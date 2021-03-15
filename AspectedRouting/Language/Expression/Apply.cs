using System;
using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Language.Functions;
using AspectedRouting.Language.Typ;
using static AspectedRouting.Language.Deconstruct;
using Type = AspectedRouting.Language.Typ.Type;

namespace AspectedRouting.Language.Expression
{
    public class Apply : IExpression
    {
        // Only used for when there is no typechecking possible
        private readonly string _debugInfo;

        /// <summary>
        ///     Maps the expected return type onto the argument needed for that.
        ///     The argument is specialized for this return type
        /// </summary>
        public readonly Dictionary<Type, (IExpression f, IExpression a)> FunctionApplications;

        private Apply(string debugInfo, Dictionary<Type, (IExpression f, IExpression a)> argument)
        {
            _debugInfo = debugInfo;
            FunctionApplications = argument;
        }

        public Apply(IExpression f, IExpression argument)
        {
            if (f == null || argument == null) {
                throw new NullReferenceException();
            }

            FunctionApplications = new Dictionary<Type, (IExpression f, IExpression a)>();

            var typesCleaned = argument.Types.RenameVars(f.Types).ToList();
            foreach (var funcType in f.Types) {
                if (!(funcType is Curry c)) {
                    continue;
                }

                var expectedArgType = c.ArgType;
                var expectedResultType = c.ResultType;


                foreach (var argType in typesCleaned) {
                    // we try to unify the argType with the expected type
                    var substitutions = expectedArgType.UnificationTable(argType);
                    if (substitutions == null) {
                        continue;
                    }

                    var actualArgType = expectedArgType.Substitute(substitutions);
                    var actualResultType = expectedResultType.Substitute(substitutions);

                    var actualFunction = f.Specialize(new Curry(actualArgType, actualResultType));
                    var actualArgument = argument.Specialize(actualArgType);

                    if (actualFunction == null || actualArgument == null) {
                        continue;
                    }

                    if (FunctionApplications.ContainsKey(actualResultType)) {
                        continue;
                    }

                    FunctionApplications.Add(actualResultType, (actualFunction, actualArgument));
                }
            }

            if (!FunctionApplications.Any()) {
                try {
                    _debugInfo = $"\n{f.Optimize().TypeBreakdown().Indent()}\n" +
                                 "is given the argument: " +
                                 "(" + argument.Optimize().TypeBreakdown() + ")";
                }
                catch (Exception) {
                    _debugInfo = $"\n (NO OPT) {f.TypeBreakdown().Indent()}\n" +
                                 "is given the argument: " +
                                 "(" + argument.TypeBreakdown() + ")";
                }
            }
        }

        public IExpression F => FunctionApplications.Values.First().f;
        public IExpression A => FunctionApplications.Values.First().a;

        public IEnumerable<Type> Types => FunctionApplications.Keys;


        public object Evaluate(Context c, params IExpression[] arguments)
        {
            if (!Types.Any()) {
                throw new ArgumentException("Trying to invoke an invalid expression: " + this);
            }

            var type = Types.First();
            var (fExpr, argExpr) = FunctionApplications[type];


            var arg = argExpr;
            var allArgs = new IExpression[arguments.Length + 1];
            allArgs[0] = arg;
            for (var i = 0; i < arguments.Length; i++) {
                allArgs[i + 1] = arguments[i];
            }

            return fExpr.Evaluate(c, allArgs);
        }


        IExpression IExpression.Specialize(IEnumerable<Type> allowedTypes)
        {
            return Specialize(allowedTypes);
        }

        public IExpression PruneTypes(Func<Type, bool> allowedTypes)
        {
            var passed = this.FunctionApplications.Where(kv => allowedTypes.Invoke(kv.Key));
            if (!passed.Any()) {
                return null;
            }
            return new Apply("pruned", new Dictionary<Type, (IExpression A, IExpression F)>(passed));
        }

        public IExpression Optimize()
        {
            if (Types.Count() == 0) {
                throw new ArgumentException("This application contain no valid types, so cannot be optimized" + this);
            }

            // (eitherfunc dot id) id
            // => (const dot _) id => dot id => id
            // or => (constRight _ id) id => id id => id 
            if (
                UnApplyAny(
                    UnApplyAny(
                        UnApplyAny(
                            IsFunc(Funcs.Const),
                            IsFunc(Funcs.Dot)),
                        Any),
                    IsFunc(Funcs.Id)
                ).Invoke(this)
                && UnApplyAny(UnApplyAny(
                        UnApplyAny(
                            IsFunc(Funcs.ConstRight),
                            Any),
                        IsFunc(Funcs.Id)),
                    IsFunc(Funcs.Id)
                ).Invoke(this)) {
                return Funcs.Id;
            }


            if (Types.Count() > 1) {
                // Too much types to optimize
                var optimized = new Dictionary<Type, (IExpression f, IExpression a)>();
                foreach (var (resultType, (f, a)) in FunctionApplications) {
                    var fOpt = f.Optimize();
                    var aOpt = a.Optimize();
                    optimized.Add(resultType, (fOpt, aOpt));
                }

                return new Apply(_debugInfo, optimized);
            }

            {
                // id a => a
                var arg = new List<IExpression>();
                if (
                    UnApplyAny(
                        IsFunc(Funcs.Id),
                        Assign(arg)
                    ).Invoke(this)) {
                    return arg.First();
                }
            }


            {
                // ifdotted fcondition fthen felse arg => if (fcondition arg) (fthen arg) (felse arg)
                var fcondition = new List<IExpression>();
                var fthen = new List<IExpression>();
                var felse = new List<IExpression>();
                var arg = new List<IExpression>();

                if (
                    UnApplyAny(
                        UnApply(
                            UnApply(
                                UnApply(
                                    IsFunc(Funcs.IfDotted),
                                    Assign(fcondition)),
                                Assign(fthen)),
                            Assign(felse)),
                        Assign(arg)
                    ).Invoke(this)) {
                    var a = arg.First();
                    return
                        Funcs.If.Apply(
                            fcondition.First().Apply(a),
                            fthen.First().Apply(a),
                            felse.First().Apply(a)
                        );
                }
            }

            {
                var (f, a) = FunctionApplications.Values.First();

                var (newFa, expr) = OptimizeApplicationPair(f, a);
                if (expr != null) {
                    return expr;
                }

                (f, a) = newFa.Value;
                return new Apply(f, a);
            }
        }

        public void Visit(Func<IExpression, bool> visitor)
        {
            var continueVisit = visitor(this);
            if (!continueVisit) {
                return;
            }

            foreach (var (_, (f, a)) in FunctionApplications) {
                f.Visit(visitor);
                a.Visit(visitor);
            }
        }

        private Apply Specialize(IEnumerable<Type> allowedTypes)
        {
            var newArgs = new Dictionary<Type, (IExpression f, IExpression a)>();

            foreach (var allowedType in allowedTypes) {
                foreach (var (resultType, (funExpr, argExpr)) in FunctionApplications) {
                    var substitutions = resultType.UnificationTable(allowedType, true);
                    if (substitutions == null) {
                        continue;
                    }

                    var actualResultType = allowedType.Substitute(substitutions);

                    // f : a -> b
                    // actualResultType = b (b which was retrieved after a reverse substitution)

                    var actualFunction = funExpr.Specialize(substitutions);
                    var actualArgument = argExpr.Specialize(substitutions);

                    if (actualFunction == null || actualArgument == null) {
                        // One of the subexpressions can't be optimized
                        return null;
                    }

                    newArgs[actualResultType] = (actualFunction, actualArgument);
                }
            }

            if (!newArgs.Any()) {
                return null;
            }

            return new Apply(_debugInfo, newArgs);
        }

        private ((IExpression fOpt, IExpression fArg)?, IExpression result) OptimizeApplicationPair(IExpression f,
            IExpression a)
        {
            f = f.Optimize();

            a = a.Optimize();

            switch (f) {
                case Id _:
                    return (null, a);

                case Apply apply:

                    if (apply.F is Const _) {
                        // (const x) y -> y
                        // apply == (const x) thus we return 'x' and ignore 'a'

                        return (null, apply.A);
                    }

                    if (apply.F is ConstRight _) {
                        // constRight x y -> y
                        // apply == (constRight x) so we return a
                        return (null, a);
                    }

                    var f0 = new List<IExpression>();
                    var f1 = new List<IExpression>();
                    if (UnApply(
                            UnApply(
                                IsFunc(Funcs.Dot),
                                Assign(f0)
                            ),
                            Assign(f1)).Invoke(apply)
                    ) {
                        // apply == ((dot f0) f1)
                        // ((dot f0) f1) a is the actual expression, but arg is already split of

                        // f0 (f1 arg)
                        // which used to be (f0 . f1) arg
                        return ((f0.First(), new Apply(f1.First(), a)), null);
                    }


                    break;
            }

            return ((f, a), null);
        }

        public override string ToString()
        {
            if (!FunctionApplications.Any()) {
                return "NOT-TYPECHECKABLE APPLICATION: " + _debugInfo;
            }

            var (f, arg) = FunctionApplications.Values.First();
            if (f is Id _) {
                return arg.ToString();
            }

            var extra = "";
            if (FunctionApplications.Count() > 1) {
                extra = " [" + FunctionApplications.Count + " IMPLEMENTATIONS]";
            }

            return $"({f} {arg.ToString().Indent()})" + extra;
        }
    }
}