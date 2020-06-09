using System.Collections.Generic;
using AspectedRouting.IO.itinero1;
using AspectedRouting.IO.LuaSkeleton;
using AspectedRouting.Language;
using AspectedRouting.Language.Functions;
using Xunit;

namespace AspectedRouting.Test
{
    public class LuaPrinterTest
    {
        [Fact]
        public void ToLua_SimpleMapping_Table()
        {
            var mapping = new Mapping(
                new[] {"a", "b", "c"},
                new[]
                {
                    new Constant(5),
                    new Constant(6),
                    new Constant(7),
                }
            );

            var luaPrinter = new LuaSkeleton(new Context());
            var result = luaPrinter.MappingToLua(mapping);

            Assert.Equal(
                "{\n    a = 5,\n    b = 6,\n    c = 7\n}"
                , result);
        }

        [Fact]
        public void ToLua_NestedMapping_Table()
        {
            var mapping = new Mapping(
                new[] {"a"},
                new[]
                {
                    new Mapping(new[] {"b"},
                        new[]
                        {
                            new Constant(42),
                        }
                    )
                }
            );
            var luaPrinter = new LuaSkeleton(new Context());
            var result = luaPrinter.MappingToLua(mapping);
            Assert.Equal("{\n    a = {\n        b = 42\n    }\n}", result);
        }

        [Fact]
        public void Sanity_EveryBasicFunction_HasDescription()
        {
            var missing = new List<string>();
            foreach (var (_, f) in Funcs.Builtins)
            {
                if (string.IsNullOrEmpty(f.Description))
                {
                    missing.Add(f.Name);
                }
            }

            Assert.True(0 == missing.Count,
                "These functions do not have a description: " + string.Join(", ", missing));
        }
        
        [Fact]
        public void Sanity_EveryBasicFunction_HasArgNames()
        {
            var missing = new List<string>();
            foreach (var (_, f) in Funcs.Builtins)
            {
                if (f.ArgNames == null)
                {
                    missing.Add(f.Name);
                }
            }

            Assert.True(0 == missing.Count,
                "These functions do not have a description: " + string.Join(", ", missing));
        }
    }
}