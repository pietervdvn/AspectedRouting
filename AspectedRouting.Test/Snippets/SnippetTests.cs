using System.Collections.Generic;
using AspectedRouting.IO.LuaSkeleton;
using AspectedRouting.IO.LuaSnippets;
using AspectedRouting.Language;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Functions;
using AspectedRouting.Language.Typ;
using Xunit;

namespace AspectedRouting.Test.Snippets;

public class SnippetTests
{
    [Fact]
    public void DefaultSnippet_SimpleDefault_GetsLua()
    {
        var gen = new DefaultSnippet();
        var lua = new LuaSkeleton(new Context(), true);
        var code = gen.Convert(lua, "result", new List<IExpression>
        {
            new Constant("the_default_value"),
            Funcs.Id,
            new Constant("value")
        });
        Assert.Contains("if (result == nil) then\n    result = \"the_default_value\"", code);
    }


    [Fact]
    public void FirstOfSnippet_SimpleFirstOf_GetLua()
    {
        var gen = new FirstMatchOfSnippet();
        var lua = new LuaSkeleton(new Context(), true);

        // FirstMatchOf: [a] -> (Tags -> [a]) -> Tags -> a

        // Order: [string]
        var order = new Constant(new List<IExpression>
        {
            new Constant("bicycle"),
            new Constant("access")
        });

        // Func: (Tags -> [a])
        var func = new Apply(
            Funcs.StringStringToTags,
            new Mapping(
                new[] { "bicycle", "access" },
                new IExpression[]
                {
                    Funcs.Id,
                    Funcs.Id
                }
            )
        );

        var tags = new LuaLiteral(new[] { Typs.Tags }, "tags");

        var code = gen.Convert(lua, "result",
            new List<IExpression>
            {
                order,
                func,
                tags
            }
        );
        // First the more general ones!
        Assert.Equal(
            "if (tags[\"access\"] ~= nil) then\n    result = tags[\"access\"]\n    \nend\nif (tags[\"bicycle\"] ~= nil) then\n    result = tags[\"bicycle\"]\n    \nend\n",
            code);
    }


    [Fact]
    public void SimpleMappingSnippet_SimpleMapping_GeneratesLua()
    {
        var mapping = new Mapping(
            new[] { "1", "-1" },
            new IExpression[]
            {
                new Constant("with"),
                new Constant("against")
            }
        );
        var gen = new SimpleMappingSnippet(mapping);
        var code = gen.Convert(new LuaSkeleton(new Context(), true), "result", new List<IExpression>
        {
            new LuaLiteral(Typs.String, "tags.oneway")
        });

        var expected =
            "local v\nv = tags.oneway\n\nif (v == \"1\") then\n    result = \"with\"\nelseif (v == \"-1\") then\n    result = \"against\"\nend";
        Assert.Equal(expected, code);
    }


}