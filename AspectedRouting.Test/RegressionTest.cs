using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using AspectedRouting.IO.jsonParser;
using AspectedRouting.Language;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Functions;
using AspectedRouting.Language.Typ;
using Xunit;

namespace AspectedRouting.Test;

public class RegressionTest
{
    [Fact]
    public void IfDotted_ShouldBeParsed()
    {
        var carOneway = Funcs.Const.Apply(new Constant("result of car.oneway")).Specialize(new Curry(
            Typs.Tags, Typs.String));

        var doc = JsonDocument.Parse("{\"oneway\":{\"$ifdotted\":{\"$const\": \"#follow_restrictions\"},\"then\": \"$car.oneway\",\"else\": {\"$const\": \"both-ignored-restrictions\"}}}");

        var parsingContext = new Context();
        parsingContext .AddFunction("car.oneway", new AspectMetadata(
            carOneway, "car.oneway","oneway function", "test", "with|against|both",
            "N/A", false
        ));
        parsingContext.AddParameter("follow_restrictions","no");
        var aspect = JsonParser.ParseProfileProperty(doc.RootElement,parsingContext, "oneway");
        var oneway = new Dictionary<string, string>();
        
        var c = new Context();
        c .AddFunction("car.oneway", new AspectMetadata(
            carOneway, "car.oneway","oneway function", "test", "with|against|both",
            "N/A", false
        ));

        c.AddParameter("follow_restrictions","yes");
        var result = aspect.Run(c, oneway);
        Assert.Equal("result of car.oneway", result);

        var c0 = new Context();
        c0.AddFunction("car.oneway", new AspectMetadata(
            carOneway, "car.oneway","oneway function", "test", "with|against|both",
            "N/A", false
        ));
        c0.AddParameter("follow_restrictions","no");
        var result0 = aspect.Run(c0, oneway);
        Assert.Equal("both-ignored-restrictions", result0);

    }
}