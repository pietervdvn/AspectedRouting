using AspectedRouting.Language;

namespace AspectedRouting.IO.LuaSnippets
{
    public class MinSnippet : ListFoldingSnippet
    {
        public MinSnippet() : base(Funcs.Min, "nil") { }
        public override string Combine(string assignTo, string value)
        {
            return Utils.Lines("if ( " + assignTo + " == nil or " + assignTo + " > " + value + " ) then",
                "    " + assignTo + " = " + value,
                "end");
        }
    }
}