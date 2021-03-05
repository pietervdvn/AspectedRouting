using System.Collections.Generic;
using System.Linq;
using AspectedRouting.IO.jsonParser;
using AspectedRouting.Language;
using AspectedRouting.Language.Expression;
using AspectedRouting.Language.Functions;
using AspectedRouting.Language.Typ;
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

        private IExpression MustMatchJsonWithOr()
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

        private string IfDottedConditionJson
            = "{" +
              "\"$ifdotted\": {\"$eq\": \"yes\"}," +
              "\"then\":{\"$const\": \"a\"}," +
              "\"else\": {\"$const\": \"b\"}" +
              "}";

        private string IfSimpleConditionJson
            = "{" +
              "\"$if\": true," +
              "\"then\":\"thenResult\"," +
              "\"else\": \"elseResult\"}";

        [Fact]
        public void TestParsing_SimpleIf_CorrectExpression()
        {
            var c = new Context();
            var ifExpr = JsonParser.ParseExpression(c, IfSimpleConditionJson);

            Assert.Single(ifExpr.Types);
            Assert.Equal(ifExpr.Types.First(), Typs.String);

            var resultT = ifExpr.Evaluate(c);
            Assert.Equal("thenResult", resultT);
            resultT = ifExpr.Optimize().Evaluate(c);
            Assert.Equal("thenResult", resultT);
        }

        [Fact]
        public void TestEvaluate_DottedIf_CorrectExpression()
        {
            var ifExpr = Funcs.IfDotted.Apply(
                Funcs.Eq.Apply(new Constant("abc")),
                Funcs.Const.Apply(new Constant("a")),
                Funcs.Const.Apply(new Constant("b"))
            );

            var c = new Context();
            var ifResultMatch = ifExpr.Evaluate(c, new Constant("abc"));
            Assert.Equal("a", ifResultMatch);

            var ifResultNoMatch = ifExpr.Evaluate(c, new Constant("def"));
            Assert.Equal("b", ifResultNoMatch);
        }

        [Fact]
        public void TestParsing_DottedIf_CorrectExpression()
        {
            var c = new Context();
            var ifExpr = JsonParser.ParseExpression(c, IfDottedConditionJson);
            ifExpr = ifExpr.Optimize();
            var resultT = ifExpr.Evaluate(c,
                new Constant(Typs.String, "yes"));
            var resultF = ifExpr.Evaluate(c,
                new Constant(Typs.String, "no"));
            Assert.Equal("a", resultT);
            Assert.Equal("b", resultF);
        }


        private string constString = "{\"$const\": \"a\"}";

        [Fact]
        public void Parse_ConstString_TypeIsFree()
        {
            var e = JsonParser.ParseExpression(new Context(), constString);
            Assert.Single(e.Types);
            Assert.Equal(new Curry(new Var("d"), Typs.String), e.Types.First());
        }


        [Fact]
        public void TypeInference_EitherIdConstConst_CorrectType()
        {
            /*
             * id : a -> a
             * dot: (b -> c) -> (a -> b) -> a -> c
             * const - throw away b: a -> b -> a
             * eitherFunc: (a -> b) -> (c -> d) -> (a -> b)
            * eitherFunc: (a -> b) -> (c -> d) -> (c -> d)
   
             *
             * All with free vars:
             * id: a -> a
             * dot: (b -> c) -> (x -> b) -> x -> c
             * const: y -> z -> y
             * eitherfunc: (d -> e) -> (f -> g) -> (d -> e)
             *             (d -> e) -> (f -> g) -> (f -> g)
             */

            /*
             * (((eitherfunc id) dot) const)
             *
             *  (eitherfunc id)
             *  [(d -> e) -> (f -> g) -> (d -> e)] (a -> a)
             *  [(d -> e) -> (f -> g) -> (f -> g)] (a -> a)
             *
             * Gives:
             *   d ~ a
             *   e ~ a
             * thus:
             * (f -> g) -> (a -> a)
             * (f -> g) -> (f -> g)
             *
             * ((eitherfunc id) dot)
             * [(f -> g) -> (a -> a)] ((b -> c) -> (x -> b) -> x -> c)
             * [(f -> g) -> (f -> g)] (b -> c) -> (x -> b) -> (x -> c)
             *
             * Thus: (f -> g) ~ (b -> c) -> ((x -> b) -> x -> c)
             * thus: f ~ (b -> c)
             *       g ~ ((x -> b) -> (x -> c))
             * thus:
             * (a -> a)
             * (b -> c) -> ((x -> b) -> (x -> c))
             *
             *
             * 
             * (((eitherfunc id) dot) const):
             * [(a -> a)] (y -> (z -> y))
             * [(b -> c) -> ((x -> b) -> (x -> c))] (y -> (z -> y))
             *
             * Thus: case 1:
             *     a ~ (y -> (z -> y)
             *     Type is: (y -> z -> y) === typeof(const)
             * case2:
             *     (b -> c) ~  (y -> (z -> y))
             *     thus: b ~ y
             *           c ~ (z -> y)
             *     ((x -> y) -> (x -> (z -> y))))
             *     = ((x -> y) -> x -> z -> y === mix of dot and const
             * 
             */

            var a = new Var("a");
            var c = new Var("c");
            var d = new Var("d");


            var e = Funcs.Either(Funcs.Id, Funcs.Dot, Funcs.Const);
            var types = e.Types.ToList();
            Assert.Equal(Curry.ConstructFrom(c, c, d), types[0]);
            Assert.Equal(Curry.ConstructFrom(
                c, // RESULT TYPE
                new Curry(a, c),
                a, d
            ), types[1]);
        }


        [Fact]
        public void RenameVars_Constant_ConstantType()
        {
            // Funcs.Const.RenameVars(noUse: ["a","b","d","e","f"]  should give something like 'c -> g -> c'
            var a = new Var("a");
            var b = new Var("b");

            var c = new Var("c");
            var d = new Var("d");

            var e = new Var("e");
            var f = new Var("f");
            var newTypes = Funcs.Const.Types.RenameVars(new[]
            {
                new Curry(e, e),
                new Curry(new Curry(b, f), new Curry(new Curry(a, b), new Curry(a, f)))
            }).ToList();
            Assert.Single(newTypes);
            Assert.Equal(new Curry(c, new Curry(d, c)),
                newTypes[0]);
        }

        [Fact]
        public void BuildSubstitution_TagsToStringTagsToBool_ShouldUnify()
        {
            var biggerType = new Curry(Typs.Tags, Typs.String);
            var smallerType = new Curry(Typs.Tags, Typs.Bool);
            // The expected type (biggerType) on the left, the argument type on the right (as it should be)
            var unificationTable = biggerType.UnificationTable(smallerType);
            Assert.NotNull(unificationTable);
            unificationTable = smallerType.UnificationTable(biggerType);
            Assert.Null(unificationTable);
        }

        [Fact]
        public void BuildSubstitution_TagsToDoubleTagsToPDouble_ShouldUnify()
        {
            var biggerType = new Curry(Typs.Tags, Typs.Double);
            var smallerType = new Curry(Typs.Tags, Typs.PDouble);
            var unificationTable = biggerType.UnificationTable(smallerType);
            Assert.NotNull(unificationTable);
            unificationTable = smallerType.UnificationTable(biggerType);
            Assert.Null(unificationTable);
        }

        [Fact]
        public void BuildSubstitution_DoubleToStringPDoubleToString_ShouldUnify()
        {
            var biggerType = new Curry(Typs.PDouble, Typs.Bool);
            var smallerType = new Curry(Typs.Double, Typs.Bool);
            // We expect something that is able to handle PDoubles, but it is able to handle the wider doubles - should be fine
            var unificationTable = biggerType.UnificationTable(smallerType);
            Assert.NotNull(unificationTable);
            unificationTable = smallerType.UnificationTable(biggerType);
            Assert.Null(unificationTable);
        }

        [Fact]
        public void Typechecker_EitherFunc_CorrectType()
        {
            var id = new Apply(Funcs.EitherFunc, Funcs.Id);
            Assert.Equal(2, id.Types.Count());

            var idconst = new Apply(id, Funcs.Const);
            Assert.Equal(2, idconst.Types.Count());

            var e =
                new Apply(idconst, new Constant("a"));
            Assert.Equal(2, e.Types.Count());
        }

        [Fact]
        public void SpecializeToSmallest_Parse_SmallestType()
        {
            var smallest = Funcs.Parse.SpecializeToSmallestType();
            Assert.Single(smallest.Types);
            Assert.Equal(new Curry(Typs.String, Typs.PDouble), smallest.Types.First());
        }

        [Fact]
        public void Unify_TwoSubtypes_DoesNotUnify()
        {
            var tags2double = new Curry(Typs.Tags, Typs.Double);
            var tags2pdouble = new Curry(Typs.Tags, Typs.PDouble);
            var unifA = tags2double.Unify(tags2pdouble, true);
            Assert.Null(unifA);
            var unifB = tags2pdouble.Unify(tags2double, true);
            Assert.NotNull(unifB);

            var unifC = tags2double.Unify(tags2pdouble, false);
            Assert.NotNull(unifC);
            var unifD = tags2pdouble.Unify(tags2double, false);
            Assert.Null(unifD);
        }


        [Fact]
        public void Specialize_WiderType_StillSmallerType()
        {
            var f = Funcs.Eq;
            var strstrb = new Curry(
                Typs.String,
                new Curry(Typs.String, Typs.Bool));
            var f0 = f.Specialize(strstrb);
            Assert.Equal(new[] {strstrb}, f0.Types);

            var strstrstr = new Curry(
                Typs.String,
                new Curry(Typs.String, Typs.String));

            var f1 = f.Specialize(strstrstr);

            Assert.Equal(new[] {strstrb, strstrstr}, f1.Types);
        }

        [Fact]
        public void SpecializeToCommonType()
        {
            var p0 = Funcs.Parse.Specialize(new Curry(Typs.String, Typs.PDouble));
            var p1 = Funcs.Const.Apply(new Constant(1.0)).Specialize(
                new Curry(new Var("a"), Typs.Double));

            var exprs = new[] {p0, p1};
            var newTypes = exprs.SpecializeToCommonTypes(out var _);
            Assert.Single(newTypes);

            exprs = new[] {p1, p0};
            newTypes = exprs.SpecializeToCommonTypes(out var _);
            Assert.Single(newTypes);
        }


        [Fact]
        public void ParseFunction_InvalidInput_NullOutput()
        {
            var f = Funcs.Parse;
            var c = new Context();
            var result = f.Evaluate(c, new Constant("abc"));

            Assert.Null(result);
        }
        
        [Fact]
        public void ParseFunction_Duration_TotalMinutes()
        {
            var f = Funcs.Parse;
            var c = new Context();
            var result = f.Evaluate(c, new Constant("01:15"));

            Assert.Equal(75.0, result);
        }

        [Fact]
        public void ApplyDefaultFunctionWithId_ApplicationIsSuccessfull()
        {
            var e = new Apply(new Apply(Funcs.Default, new Constant("a")), Funcs.Id);
            Assert.Single(e.Types);
            
            Assert.Equal("string -> string", e.Types.First().ToString());
            
        }
        
    }
}