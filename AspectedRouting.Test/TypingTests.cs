using System.Linq;
using AspectedRouting.Language;
using AspectedRouting.Language.Typ;
using Xunit;

namespace AspectedRouting.Test
{
    public class TypingTests
    {
        
        [Fact]
        public void JoinApply_Id()
        {
            var tp =
                new Curry(
                    new Curry(
                        new Var("x"), Typs.String
                    ), new Curry(
                        new Var("x"), Typs.String
                    )
                );
            Assert.Equal("($x -> string) -> $x -> string", tp.ToString());

            /*
             *  ($x -> string) -> $x -> string
             *  ($a -> $a    )
             * should give the unification table
             * ($x --> string)
             */

            var unificationTable = tp.ArgType.UnificationTable(Funcs.Id.Types.First());
            Assert.Equal("string", unificationTable["$a"].ToString());
            Assert.Equal("string", unificationTable["$x"].ToString());
            Assert.Equal(2, unificationTable.Count);
        }
    }
}