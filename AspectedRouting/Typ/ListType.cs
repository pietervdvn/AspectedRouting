namespace AspectedRouting.Typ
{
    public class ListType : Type
    {
        public Type InnerType { get; }

        public ListType(Type of) : base($"list ({of})", false)
        {
            InnerType = of;
        }
    }
}