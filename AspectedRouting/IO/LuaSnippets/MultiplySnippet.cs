using AspectedRouting.Language;

namespace AspectedRouting.IO.LuaSnippets
{
    public class MultiplySnippet : ListFoldingSnippet
    {
        public MultiplySnippet() : base(Funcs.Multiply, "1") { }

        public override string Combine(string assignTo, string value)
        {
            return assignTo + " = " + assignTo + " * " + value;
        }
    }
}