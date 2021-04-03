using AspectedRouting.Language;
using AspectedRouting.Language.Functions;
using Xunit;

namespace AspectedRouting.Test
{
    public class MappingTest
    {
        [Fact]
        public static void SimpleMapping_SimpleHighway_GivesResult()
        {
            var maxspeed = new Mapping(new[] {"residential", "living_street"},
                new[] {
                    new Constant(30),
                    new Constant(20)
                }
            );
           var resMaxspeed= maxspeed.Evaluate(new Context(), new Constant("residential"));
           Assert.Equal(30, resMaxspeed);
           var livingStreetMaxspeed= maxspeed.Evaluate(new Context(), new Constant("living_street"));
           Assert.Equal(20, livingStreetMaxspeed);
           var undefinedSpeed = maxspeed.Evaluate(new Context(), new Constant("some_unknown_highway_type"));
           Assert.Null(undefinedSpeed);
        }
    }
}