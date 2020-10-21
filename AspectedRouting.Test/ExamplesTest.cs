using Xunit;

namespace AspectedRouting.Test
{
    public class ExamplesTest
    {
        [Fact]
        public void Integration_TestExamples()
        {
            var input = "./Examples/";
            var output = "./output/";
            var err = Program.MainWithError(new[] {input, output, "--no-repl"});
            Assert.Null(err);
        }
    }
}