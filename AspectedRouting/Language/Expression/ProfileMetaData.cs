using System.Collections.Generic;
using System.Linq;

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

        public override string ToString()
        {
            return $"Profile: {Name} {Filename}\naccess={Access}\noneway={Oneway}\nspeed={Speed}\n" +
                   $"priorities = {string.Join(" + ", Priority.Select(kv => "#" + kv.Key + " * " + kv.Value))} ";
        }
    }
}