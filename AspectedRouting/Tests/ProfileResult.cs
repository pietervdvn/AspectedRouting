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

        public override string ToString()
        {

            return string.Join("\n  ",
                "access "+Access,
                "oneway "+Oneway,
                "speed "+Speed,
                "priority "+Priority,
                "because "+PriorityExplanation
                );
        }
    }
}