namespace AspectedRouting.Tests
{
    public struct ProfileResult
    {
        public readonly string Access;
        public readonly string Oneway;
        public readonly double Speed;
        public readonly double Priority;

        public readonly string PriorityExplanation;

        public ProfileResult(string access, string oneway, double speed, double weight, string priorityExplanation = "")
        {
            Access = access;
            Oneway = oneway;
            Speed = speed;
            Priority = weight;
            PriorityExplanation = priorityExplanation;
        }

        private static string str(string s)
        {
            if (s == null) {
                return "<null>";
            }

            if (s == "") {
                return "<empty string>";
            }

            return s;
        } 
        
        public override string ToString()
        {

            return string.Join("\n  ",
                "  access "+str(Access),
                "oneway "+str(Oneway),
                "speed "+Speed,
                "priority "+Priority,
                "because \n  "+str(PriorityExplanation)
                );
        }

        public override bool Equals(object? obj)
        {
            if (!(obj is ProfileResult other)) {
                return false;
            }
            return other.Access == this.Access && other.Oneway == this.Oneway && other.Priority == this.Priority && other.Speed == this.Speed;
        }
    }
}