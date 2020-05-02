namespace AspectedRouting.Language.Typ
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