using AspectedRouting.IO.LuaSkeleton;
using AspectedRouting.Language;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Functions;
using AspectedRouting.Language.Typ;
using Xunit;

namespace AspectedRouting.Test;

public class OptimizationsTests
{
    [Fact]
    public void AppliedListDot_Optimize_ListOfApplications()
    {
        var lit = new LuaLiteral(Typs.Nat, "tags");
        var e0 = Funcs.ListDot.Apply(
            new Constant(new[]
            {
                Funcs.Eq.Apply(new Constant(5)),
                Funcs.Eq.Apply(new Constant(42))
            }));

        var e = e0.Apply(lit).SpecializeToSmallestType();
        var x = e.Optimize(out var sc);
        Assert.True(sc);
        Assert.Equal(
            new Constant(new[]
            {
                Funcs.Eq.Apply(new Constant(5)).Apply(lit),
                Funcs.Eq.Apply(new Constant(42)).Apply(lit)
            }).SpecializeToSmallestType().ToString(), x.ToString());
    }

    [Fact]
    public void AdvancedApplied_Optimized_ListOfAppliedValues()
    {
        var legal_access_be = new FunctionCall("$legal_access_be", new Curry(Typs.Tags, Typs.String));
        var legal_access_pedestrian = new FunctionCall("$pedestrian.legal_access", new Curry(Typs.Tags, Typs.String));
        var tags = new LuaLiteral(Typs.Tags, "tags");
        var e = new Apply( // string
            Funcs.Head,
            new Apply( // list (string)
                new Apply( // tags -> list (string)
                    Funcs.ListDot,
                    new Constant(new[]
                    {
                        legal_access_be,
                        legal_access_pedestrian
                    })),
                tags));
        var eOpt = e.Optimize(out var sc);
        Assert.True(sc);
        Assert.Equal(
            Funcs.Head.Apply(new Constant(
                new[]
                {
                    legal_access_be.Apply(tags),
                    legal_access_pedestrian.Apply(tags)
                }
            )).ToString(),
            eOpt.ToString()
        );
    }


    [Fact]
    public void advancedExpr_Optimize_Works()
    {
        var e = new Apply( // double
            new Apply( // tags -> double
                new Apply( // (tags -> double) -> tags -> doubleTag
                    Funcs.Default,
                    new Constant(0)),
                new Apply( // tags -> double
                    new Apply( // (tags -> list (double)) -> tags -> double
                        Funcs.Dot,
                        Funcs.Head),
                    new Apply( // tags -> list (double)
                        Funcs.StringStringToTags,
                        new Mapping(
                            new[]
                            {
                                "access"
                            },
                            new[]
                            {
                                new Mapping(
                                    new[]
                                    {
                                        "private",
                                        "destination",
                                        "permissive"
                                    },
                                    new[]
                                    {
                                        new Constant(-500), new Constant(-3), new Constant(-1)
                                    }
                                )
                            }
                        )))),
            new LuaLiteral(Typs.Tags, "tags"));
        var eOpt = e.Optimize(out var sc);
        Assert.True(sc);
        Assert.NotEmpty(eOpt.Types);
    }

    [Fact]
    public void optimizeListdotAway()
    {
        var tagsToStr = new Curry(Typs.Tags, Typs.PDouble);
        var e = new Apply( // pdouble
            new Apply( // tags -> pdouble
                Funcs.Id,
                new Apply( // tags -> pdouble
                    new Apply( // (tags -> list (pdouble)) -> tags -> pdouble
                        Funcs.Dot,
                        Funcs.Min),
                    new Apply( // tags -> list (pdouble)
                        Funcs.ListDot,
                        new Constant(new IExpression[]
                        {
                            new FunctionCall("$legal_maxspeed_be", tagsToStr),
                            new FunctionCall("$car.practical_max_speed", tagsToStr),
                            new Apply( // tags -> pdouble
                                Funcs.Const,
                                new Parameter("#maxspeed"))
                        })))),
            new LuaLiteral(Typs.Tags, "tags"));
        var opt = e.SpecializeToSmallestType().Optimize(out var sc);
        Assert.True(sc);


    }

    [Fact]
    public void Regression_ShouldOptimize()
    {
        var e = new Apply( // nat
            new Apply( // tags -> nat
                new Apply( // nat -> tags -> nat
                    new Apply( // (nat -> tags -> nat) -> nat -> tags -> nat
                        Funcs.ConstRight,
                        Funcs.Id),
                    Funcs.Const),
                new LuaLiteral(Typs.PDouble, "distance")),
            new LuaLiteral(Typs.Tags, "tags")).SpecializeToSmallestType();
        var opt = e.Optimize(out var sc);
        Assert.True(sc);

    }
}