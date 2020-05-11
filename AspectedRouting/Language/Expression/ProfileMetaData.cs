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
        public List<string> Metadata { get; }

        public Dictionary<string, IExpression> DefaultParameters { get; }
        public Dictionary<string, Dictionary<string, IExpression>> Behaviours { get; }

        public IExpression Access { get; }
        public IExpression Oneway { get; }
        public IExpression Speed { get; }
        public Dictionary<string, IExpression> Priority { get; }

        public ProfileMetaData(string name, string description, string author, string filename,
            List<string> vehicleTyps, Dictionary<string, IExpression> defaultParameters,
            Dictionary<string, Dictionary<string, IExpression>> behaviours,
            IExpression access, IExpression oneway, IExpression speed,
            Dictionary<string, IExpression> priority, List<string> metadata)
        {
            Name = name;
            Description = description;
            Author = author;
            Filename = filename;
            VehicleTyps = vehicleTyps;
            Access = access;
            Oneway = oneway;
            Speed = speed;
            Priority = priority;
            Metadata = metadata;
            DefaultParameters = defaultParameters;
            Behaviours = behaviours;
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

            c = c.WithParameters(parameters);
            
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