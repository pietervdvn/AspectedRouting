using System.Collections.Generic;
using System.Linq;

namespace AspectedRouting.Functions
{
    public class ProfileMetaData
    {
        public string Name { get; }
        public string Description { get; }
        public string Author { get; }
        public string Filename { get; }
        public List<string> VehicleTyps { get; }
        public List<string> Metadata { get; }

        public Dictionary<string, object> DefaultParameters { get; }
        public Dictionary<string, Dictionary<string, object>> Profiles { get; }

        public IExpression Access { get; }
        public IExpression Oneway { get; }
        public IExpression Speed { get; }
        public Dictionary<string, IExpression> Weights { get; }

        public ProfileMetaData(string name, string description, string author, string filename,
            List<string> vehicleTyps, Dictionary<string, object> defaultParameters,
            Dictionary<string, Dictionary<string, object>> profiles,
            IExpression access, IExpression oneway, IExpression speed,
            Dictionary<string, IExpression> weights, List<string> metadata)
        {
            Name = name;
            Description = description;
            Author = author;
            Filename = filename;
            VehicleTyps = vehicleTyps;
            Access = access;
            Oneway = oneway;
            Speed = speed;
            Weights = weights;
            Metadata = metadata;
            DefaultParameters = defaultParameters;
            Profiles = profiles;
        }

        public override string ToString()
        {
            return $"Profile: {Name} {Filename}\naccess={Access}\noneway={Oneway}\nspeed={Speed}\n" +
                   $"weights ={string.Join(" + ", Weights.Select(kv => "#" + kv.Key + " * " + kv.Value))} ";
        }
    }
}