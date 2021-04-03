using System.Collections.Generic;
using AspectedRouting.Language;
using AspectedRouting.Language.Functions;
using Xunit;

namespace AspectedRouting.Test
{
    public class MustMatchTest
    {
        [Fact]
        public void MustMatch_SimpleInput()
        {
            var mapValue = new Mapping(new[] {"residential", "living_street"},
                new[] {
                    new Constant("yes"),
                    new Constant("no")
                });
            var mapTag = new Mapping(new[] {"highway"}, new[] {mapValue});
            var mm = Funcs.MustMatch
                    .Apply(
                        new Constant(new[] {new Constant("highway")}),
                        Funcs.StringStringToTags.Apply(mapTag)
                    )
                ;


            var residential = mm.Apply(new Constant(new Dictionary<string, string> {
                {"highway", "residential"}
            })).Evaluate(new Context());
            Assert.Equal("yes", residential);

            var living = mm.Apply(new Constant(new Dictionary<string, string> {
                {"highway", "living_street"}
            })).Evaluate(new Context());
            Assert.Equal("no", living);

            var unknown = mm.Apply(new Constant(new Dictionary<string, string> {
                {"highway", "unknown_type"}
            })).Evaluate(new Context());
            Assert.Equal("yes", unknown);

            var missing = mm.Apply(new Constant(new Dictionary<string, string> {
                {"proposed:highway", "unknown_type"}
            })).Evaluate(new Context());
            Assert.Equal("no", missing);
        }
    }
}