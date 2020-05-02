using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Language;
using AspectedRouting.Language.Functions;
using Xunit;

namespace AspectedRouting.Test
{
    public class TestAnalysis
    {
        [Fact]
        public void OnAll_SmallTagSet_AllCombinations()
        {
            var possibleTags = new Dictionary<string, List<string>>
            {
                {"a", new List<string> {"x", "y"}}
            };

            var all = possibleTags.OnAllCombinations(dict => ObjectExtensions.Pretty(dict), new List<string>()).ToList();
            Assert.Equal(3, all.Count);
            Assert.Contains("{}", all);
            Assert.Contains("{a=x;}", all);
            Assert.Contains("{a=y;}", all);
        }

        [Fact]
        public void OnAll_TwoTagSet_AllCombinations()
        {
            var possibleTags = new Dictionary<string, List<string>>
            {
                {"a", new List<string> {"x", "y"}},
                {"b", new List<string> {"u", "v"}}
            };

            var all = possibleTags.OnAllCombinations(dict => dict.Pretty(), new List<string>()).ToList();
            Assert.Equal(9, all.Count);
            Assert.Contains("{}", all);
            Assert.Contains("{a=x;}", all);
            Assert.Contains("{a=y;}", all);

            Assert.Contains("{b=u;}", all);
            Assert.Contains("{b=v;}", all);

            Assert.Contains("{a=x;b=u;}", all);
            Assert.Contains("{a=x;b=v;}", all);
            Assert.Contains("{a=y;b=u;}", all);
            Assert.Contains("{a=y;b=v;}", all);
        }
    }
}