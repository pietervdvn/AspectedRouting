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
        public Dictionary<string, IExpression> Priority { get; }
        /**
         * Moment of last change of any upstream file
         */
        public DateTime LastChange { get; }

        public ProfileMetaData(string name, string description, string author, string filename,
            List<string> vehicleTyps, Dictionary<string, IExpression> defaultParameters,
            Dictionary<string, Dictionary<string, IExpression>> behaviours,
            IExpression access, IExpression oneway, IExpression speed,
            Dictionary<string, IExpression> priority, List<string> metadata, DateTime lastChange)
        {
            Name = name;
            Description = description;
            Author = author;
            Filename = filename;
            VehicleTyps = vehicleTyps;
            Access = access.Optimize();
            Oneway = oneway.Optimize();
            Speed = speed.Optimize();
            Priority = priority;
            Metadata = metadata;
            LastChange = lastChange;
            DefaultParameters = defaultParameters;
            Behaviours = behaviours;

            CheckTypes(Access, "access");
            CheckTypes(Oneway, "oneway");
            CheckTypes(Speed, "speed");
            
        }

        private static void CheckTypes(IExpression e, string name)
        {
            if (e.Types.Count() == 1) {
                return;
            }

            throw new Exception("Insufficient specialization: " + name + " has multiple types left, namely " + e.Types.Pretty());
        }

        public List<IExpression> AllExpressions(Context ctx)
        {
            var l = new List<IExpression> {Access, Oneway, Speed};
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


        public ProfileResult Run(Context c, string behaviour, Dictionary<string, string> tags)
        {
            if (!Behaviours.ContainsKey(behaviour))
            {
                throw new ArgumentException(
                    $"Profile {Name} does not contain the behaviour {behaviour}\nTry one of {string.Join(",", Behaviours.Keys)}");
            }

            var parameters = new Dictionary<string, IExpression>();

            foreach (var (k, v) in DefaultParameters)
            {
                parameters[k.TrimStart('#')] = v;
            }

            foreach (var (k, v) in Behaviours[behaviour])
            {
                parameters[k.TrimStart('#')] = v;
            }

            c = c.WithParameters(parameters)
                .WithAspectName(this.Name);
            tags = new Dictionary<string, string>(tags);
            var canAccess = Access.Run(c, tags);
            tags["access"] = "" + canAccess;
            var speed = (double) Speed.Run(c, tags);
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
                var aspectInfluence = (double) c.Parameters[paramName].Evaluate(c);
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

            return new ProfileResult((string) canAccess, (string) oneway, speed, priority,
                string.Join("\n  ", weightExplanation));
        }


        public override string ToString()
        {
            return $"Profile: {Name} {Filename}\naccess={Access}\noneway={Oneway}\nspeed={Speed}\n" +
                   $"priorities = {string.Join(" + ", Priority.Select(kv => "#" + kv.Key + " * " + kv.Value))} ";
        }
    }
}