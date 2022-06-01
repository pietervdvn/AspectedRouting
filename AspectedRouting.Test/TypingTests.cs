using System;
using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Language;
using AspectedRouting.Language.Functions;
using AspectedRouting.Language.Typ;
using Xunit;
using Type = AspectedRouting.Language.Typ.Type;

namespace AspectedRouting.Test
{
    public class TypingTests
    {
        [Fact]
        public void SpecializeToCommonTypes_X2PDouble_Y2Double_Gives_X2Double()
        {
            // Regression test:
            // [ x : (x -> pdouble), y : (y -> double)] is wrongly types as (x -> pdouble), hence killing y in the subsequent typing
            var exprs = new List<IExpression> {
                new Constant(new List<Type> {
                    new Curry(Typs.Tags, Typs.Double),
                    new Curry(new Var("b"), new Curry(Typs.Tags, Typs.Double))
                }, "x"),

                new Constant(
                    new List<Type> {
                        Typs.PDouble,
                        new Curry(new Var("b"), Typs.PDouble)
                    }
                    , "y")
            };


            exprs.SpecializeToCommonTypes(out var specializedTypes, out var specializedExpressions);
            Assert.All(specializedTypes, Assert.NotNull);
            Assert.All(specializedExpressions, Assert.NotNull);
            Assert.Single(specializedTypes);
            Assert.Equal(new Curry(Typs.Tags, Typs.Double), specializedTypes.First());
        }


        [Fact]
        public void WidestCommonGround_A2PdoubleAndT2Double_T2Double()
        {
            var v = Typs.WidestCommonType(new Curry(new Var("a"), Typs.PDouble),
                new Curry(Typs.Tags, Typs.Double)
            );
            Assert.NotNull(v);
            var (x, subsTable) = v.Value;
            Assert.NotNull(x);
            Assert.Equal(
                new Curry(Typs.Tags, Typs.Double), x
            );
        }


        [Fact]
        public void WidestCommonGround_StringAndString_String()
        {
            var v = Typs.WidestCommonType(Typs.String, Typs.String);
            Assert.NotNull(v);
            var (x, subsTable) = v.Value;
            Assert.NotNull(x);
            Assert.Equal(
                Typs.String, x
            );
        }

        [Fact]
        public void SpecializeToCommonTypes_ValueAndFuncType_ShouldFail()
        {
            // Regression test:
            // [ x : (x -> pdouble), y : (y -> double)] is wrongly types as (x -> pdouble), hence killing y in the subsequent typing
            var exprs = new List<IExpression> {
                new Constant(
                    new Curry(Typs.Tags, Typs.Double),
                    "x"),
                new Constant(
                    Typs.PDouble, "y")
            };

            Assert.Throws(new ArgumentException().GetType(),
                () => exprs.SpecializeToCommonTypes(out _, out _));
        }


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