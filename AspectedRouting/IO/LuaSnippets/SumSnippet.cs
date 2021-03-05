using AspectedRouting.Language;

namespace AspectedRouting.IO.LuaSnippets
{
    public class SumSnippet : ListFoldingSnippet
    {
        public SumSnippet() : base(Funcs.Sum, "0") { }
        public override string Combine(string assignTo, string value)
        {
            return assignTo + " = " + assignTo + " + " + value;
        }
    }
}