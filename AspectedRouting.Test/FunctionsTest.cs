using System.Collections.Generic;
using AspectedRouting.IO.jsonParser;
using AspectedRouting.Language;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Functions;
using Xunit;

namespace AspectedRouting.Test
{
    public class FunctionsTest
    {
        private IExpression MustMatchJson()
        {
            var json = "{" +
                       "\"name\":\"test\"," +
                       "\"description\":\"test\"," +
                       "\"$mustMatch\":{\"a\":\"b\",\"x\":\"y\"}}";
            return JsonParser.AspectFromJson(new Context(), json, "test.json");
        }


        [Fact]
        public void TestAll_AllTags_Yes()
        {
            var tagsAx = new Dictionary<string, string>
            {
                {"a", "b"},
                {"x", "y"}
            };

            var expr = new Apply(MustMatchJson(), new Constant(tagsAx)).Optimize();
            var result = expr.Evaluate(new Context());
            Assert.Equal("yes", result);
        }

        [Fact]
        public void TestAll_NoMatch_No()
        {
            var tagsAx = new Dictionary<string, string>
            {
                {"a", "b"},
            };

            var expr = new Apply(MustMatchJson(), new Constant(tagsAx)).Optimize();
            var result = expr.Evaluate(new Context());
            Assert.Equal("no", result);
        }

        [Fact]
        public void TestAll_NoMatchDifferent_No()
        {
            var tagsAx = new Dictionary<string, string>
            {
                {"a", "b"},
                {"x", "someRandomValue"}
            };

            var expr = new Apply(MustMatchJson(), new Constant(tagsAx)).Optimize();
            var result = expr.Evaluate(new Context());
            Assert.Equal("no", result);
        }
    }
}