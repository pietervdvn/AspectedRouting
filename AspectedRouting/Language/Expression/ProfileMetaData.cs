using System;
using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Language.Functions;
using AspectedRouting.Language.Typ;
using AspectedRouting.Tests;

namespace AspectedRouting.Language.Expression
{
    public class ProfileMetaData
    {
        public string Name { get; }
        public string Description { get; }
        public string Author { get; }
        public string Filename { get; }
        public List<string> VehicleTyps { get; }

        /*
         * Which tags are included in the routerdb but are _not_ used for routeplanning?
         * Typically these are tags that are useful for navigation (name of the road, is this a tunnel, ...)
         * but not relevant for determining the road
         */
        public List<string> Metadata { get; }

        public Dictionary<string, IExpression> DefaultParameters { get; }
        public Dictionary<string, Dictionary<string, IExpression>> Behaviours { get; }

        public IExpression Access { get; }
        public IExpression Oneway { get; }
        public IExpression Speed { get; }

        public IExpression ObstacleAccess { get; }
        public IExpression ObstacleCost { get; }

        public Dictionary<string, IExpression> Priority { get; }


        /**
         * Moment of last change of any upstream file
         */
        public DateTime LastChange { get; }

        public ProfileMetaData(string name, string description, string author, string filename,
            List<string> vehicleTyps, Dictionary<string, IExpression> defaultParameters,
            Dictionary<string, Dictionary<string, IExpression>> behaviours,
            IExpression access, IExpression oneway, IExpression speed,
            IExpression obstacleAccess, IExpression obstacleCost,
            Dictionary<string, IExpression> priority, List<string> metadata, DateTime lastChange)
        {
            Name = name;
            Description = description;
            Author = author;
            Filename = filename;
            VehicleTyps = vehicleTyps;
            Access = access.Optimize(out _);
            Oneway = oneway.Optimize(out _);
            Speed = speed.Optimize(out _);
            ObstacleAccess = obstacleAccess.Optimize(out _);
            ObstacleCost = obstacleCost.Optimize(out _);
            Priority = priority;
            Metadata = metadata;
            LastChange = lastChange;
            DefaultParameters = defaultParameters;
            Behaviours = behaviours;

            CheckTypes(Access, "access");
            CheckTypes(Oneway, "oneway");
            CheckTypes(Speed, "speed");
            CheckTypes(ObstacleAccess, "obstacleaccess");
            CheckTypes(ObstacleCost, "obstaclecost");
        }

        private static void CheckTypes(IExpression e, string name)
        {
            if (e == null)
            {
                throw new Exception("No expression given for " +name);
            }
            if (e.Types.Count() == 1)
            {
                return;
            }

            throw new Exception("Insufficient specialization: " + name + " has multiple types left, namely " +
                                e.Types.Pretty());
        }

        public List<IExpression> AllExpressions(Context ctx)
        {
            var l = new List<IExpression> { Access, Oneway, Speed, ObstacleAccess, ObstacleCost };
            l.AddRange(DefaultParameters.Values);
            l.AddRange(Behaviours.Values.SelectMany(b => b.Values));
            l.AddRange(Priority.Values);
            

            var allExpr = new List<IExpression>();
            allExpr.AddRange(l);
            foreach (var e in l)
            {
                e.Visit(expression =>
                {
                    if (expression is FunctionCall fc)
                    {
                        var called = ctx.GetFunction(fc.CalledFunctionName);
                        allExpr.Add(called);
                    }

                    return true;
                });
            }

            return allExpr;
        }

        public List<IExpression> AllExpressionsFor(string behaviourName, Context context)
        {
            var allExpressions = new List<IExpression>
            {
                Access,
                Oneway,
                Speed,
                ObstacleAccess,
                ObstacleCost
            };

            var behaviourContext = new Context(context);
            var behaviourParameters = ParametersFor(behaviourName);


            foreach (var (paramName, valueexpression) in Priority)
            {
                var weightingFactor = behaviourParameters[paramName].Evaluate(behaviourContext);
                if (weightingFactor is double d)
                {
                    if (d == 0.0)
                    {
                        continue;
                    }
                }

                if (weightingFactor is int i)
                {
                    if (i == 0)
                    {
                        continue;
                    }
                }

                allExpressions.Add(valueexpression);
            }

            return allExpressions;
        }

        public Dictionary<string, IExpression> ParametersFor(string behaviour)
        {
            var parameters = new Dictionary<string, IExpression>();

            foreach (var (k, v) in DefaultParameters)
            {
                parameters[k.TrimStart('#')] = v;
            }

            foreach (var (k, v) in Behaviours[behaviour])
            {
                parameters[k.TrimStart('#')] = v;
            }

            return parameters;
        }

        public ProfileResult Run(Context c, string behaviour, Dictionary<string, string> tags)
        {
            if (!Behaviours.ContainsKey(behaviour))
            {
                throw new ArgumentException(
                    $"Profile {Name} does not contain the behaviour {behaviour}\nTry one of {string.Join(",", Behaviours.Keys)}");
            }

            c = c.WithParameters(ParametersFor(behaviour))
                .WithAspectName(Name);
            tags = new Dictionary<string, string>(tags);
            var canAccess = Access.Run(c, tags);
            tags["access"] = "" + canAccess;
            var speed = (double)Speed.Run(c, tags);
            tags["speed"] = "" + speed;
            var oneway = Oneway.Run(c, tags);
            tags["oneway"] = "" + oneway;

            c.AddFunction("speed", new AspectMetadata(new Constant(Typs.Double, speed),
                "speed", "Actual speed of this function", "NA", "NA", "NA", true));
            c.AddFunction("oneway", new AspectMetadata(new Constant(Typs.String, oneway),
                "oneway", "Actual direction of this function", "NA", "NA", "NA", true));
            c.AddFunction("access", new AspectMetadata(new Constant(Typs.String, canAccess),
                "access", "Actual access of this function", "NA", "NA", "NA", true));


            var priority = 0.0;
            var weightExplanation = new List<string>();
            foreach (var (paramName, expression) in Priority)
            {
                var aspectInfluence = (double)c.Parameters[paramName].Evaluate(c);
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (aspectInfluence == 0)
                {
                    continue;
                }


                var aspectWeightObj = new Apply(
                    Funcs.EitherFunc.Apply(Funcs.Id, Funcs.Const, expression)
                    , new Constant(tags)).Evaluate(c);

                double aspectWeight;
                switch (aspectWeightObj)
                {
                    case bool b:
                        aspectWeight = b ? 1.0 : 0.0;
                        break;
                    case double d:
                        aspectWeight = d;
                        break;
                    case int j:
                        aspectWeight = j;
                        break;
                    case string s:
                        if (s.Equals("yes"))
                        {
                            aspectWeight = 1.0;
                            break;
                        }
                        else if (s.Equals("no"))
                        {
                            aspectWeight = 0.0;
                            break;
                        }

                        throw new Exception($"Invalid value as result for {paramName}: got string {s}");
                    default:
                        throw new Exception($"Invalid value as result for {paramName}: got object {aspectWeightObj}");
                }

                weightExplanation.Add($"({paramName} = {aspectInfluence}) * {aspectWeight}");
                priority += aspectInfluence * aspectWeight;
            }

            if (priority <= 0)
            {
                canAccess = "no";
            }

            if (canAccess is string canAccessString && oneway is string onewayString)
            {
                return new ProfileResult(canAccessString, onewayString, speed, priority,
                    string.Join("\n  ", weightExplanation));
            }
            else
            {
                throw new Exception("CanAccess or oneway are not strings but " + canAccess.GetType().ToString() +
                                    " and " + (oneway?.GetType()?.ToString() ?? "<null>"));
            }
        }


        public override string ToString()
        {
            return $"Profile: {Name} {Filename}\naccess={Access}\noneway={Oneway}\nspeed={Speed}\n" +
                   $"priorities = {string.Join(" + ", Priority.Select(kv => "#" + kv.Key + " * " + kv.Value))} ";
        }
    }
}