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
    public class TestInterpreter
    {
        [Fact]
        public void MaxSpeedAspect_Evaluate_CorrectMaxSpeed()
        {
            var json =
                "{\"name\": \"legal_maxspeed_be\",\"description\": \"Gives, for each type of highway, which the default legal maxspeed is in Belgium. This file is intended to be reused for in all vehicles, from pedestrian to car. In some cases, a legal maxspeed is not really defined (e.g. on footways). In that case, a socially acceptable speed should be taken (e.g.: a bicycle on a pedestrian path will go say around 12km/h)\",\"unit\": \"km/h\",\"$max\": {\"maxspeed\": \"$parse\",\"highway\": {\"residential\": 30},\"ferry\":5}}";

            var aspect = JsonParser.AspectFromJson(null, json, null);
            var tags = new Dictionary<string, string>
            {
                {"maxspeed", "42"},
                {"highway", "residential"},
                {"ferry", "yes"}
            };

            Assert.Equal("tags -> pdouble", string.Join(", ", aspect.Types));
            Assert.Equal(42d, new Apply(aspect, new Constant(tags)).Evaluate(null));
        }

        [Fact]
        public void MaxSpeed_AnalyzeTags_AllTagsReturned()
        {
            var json =
                "{\"name\": \"legal_maxspeed_be\",\"description\": \"Gives, for each type of highway, which the default legal maxspeed is in Belgium. This file is intended to be reused for in all vehicles, from pedestrian to car. In some cases, a legal maxspeed is not really defined (e.g. on footways). In that case, a socially acceptable speed should be taken (e.g.: a bicycle on a pedestrian path will go say around 12km/h)\"," +
                "\"unit\": \"km/h\"," +
                "\"$max\": {\"maxspeed\": \"$parse\",\"highway\": {\"residential\": 30},\"ferry\":5}}";

            var aspect = JsonParser.AspectFromJson(null, json, null);

            Assert.Equal(
                new Dictionary<string, HashSet<string>>
                {
                    {"maxspeed", new HashSet<string>()},
                    {"highway", new HashSet<string> {"residential"}},
                    {"ferry", new HashSet<string>()}
                },
                aspect.PossibleTags());
        }

        [Fact]
        public void EitherFunc_Const_Id_Specialized_Has_Correct_Type()

        {
            var b = new Var("b");
            var c = new Var("c");

            var eitherFunc = new Apply(Funcs.Const, Funcs.Id);
            Assert.Single(eitherFunc.Types);
            Assert.Equal(new Curry(b,
                new Curry(c, c)), eitherFunc.Types.First());

            var (_, (func, arg)) = eitherFunc.FunctionApplications.First();


            // func  == const : (c -> c) -> b -> (c -> c)
            var funcType = new Curry(new Curry(c, c),
                new Curry(b, new Curry(c, c)));
            Assert.Equal(funcType, func.Types.First());

            // arg == id (but specialized): (c -> c)
            var argType = new Curry(c, c);
            Assert.Equal(argType, arg.Types.First());
        }


        [Fact]
        public void EitherFunc_SpecializeToString_Const()
        {
            var a = new Constant("a");
            
            var mconst = new Apply(new Apply(Funcs.EitherFunc, Funcs.Id), Funcs.Const);
            var specialized = new Apply(mconst, a).Specialize(Typs.String);

            Assert.Equal("((($firstArg $id) $firstArg) \"a\")", specialized.ToString());
            Assert.Equal("string; $b -> string", string.Join("; ", new Apply(mconst, a).Types));
            Assert.Equal("\"a\"", specialized.Evaluate(null).Pretty());

            Assert.Equal("\"a\"", new Apply(new Apply(mconst, a), new Constant("42")).Specialize(Typs.String)
                .Evaluate(null)
                .Pretty());
        }

        [Fact]
        public void Parse_Five_5()
        {
            var str = new Constant("5");
            var parsed = new Apply(Funcs.Parse, str).Specialize(Typs.Double);
            var o = parsed.Evaluate(null);
            Assert.Equal(5d, o);
        }

        [Fact]
        public void Concat_TwoString_AB()
        {
            var a = new Constant("a");
            var b = new Constant("b");
            Assert.Equal("string -> string -> string", Funcs.Concat.Types.First().ToString());
            var ab = Funcs.Concat.Apply(a, b);
            Assert.Equal("(($concat \"a\") \"b\")", ab.ToString());
            Assert.Equal("ab", ab.Evaluate(null));
        }

        [Fact]
        public void Id_Evaluate_ReturnsInput()
        {
            var a = new Constant("a");
            var aId = Funcs.Id.Apply(a);
            Assert.Equal("\"a\" : string", aId + " : " + string.Join(", ", aId.Types));
            Assert.Equal("a", aId.Evaluate(null));
        }

        [Fact]
        public void MaxTest()
        {
            var ls = new Constant(new ListType(Typs.Double),
                new[] {1.1, 2.0, 3.0}.Select(d => (object) d));
            Assert.Equal("[1.1, 2, 3] : list (double)",
                ls.Evaluate(null).Pretty() + " : " + string.Join(", ", ls.Types));
            var mx = Funcs.Max.Apply(ls);


            Assert.Equal("($max [1.1, 2, 3]) : double", mx + " : " + string.Join(", ", mx.Types));
            Assert.Equal(3d, mx.Evaluate(null));
            var mxId = Funcs.Id.Apply(mx);
            // identity function is not printed
            Assert.Equal("($max [1.1, 2, 3]) : double", mxId + " : " + string.Join(", ", mxId.Types));
            Assert.Equal(3d, mxId.Evaluate(null));
        }

        [Fact]
        public void Fresh_SomeType_NewVarName()
        {
            Assert.Equal("$d", Var.Fresh(
                Curry.ConstructFrom(new Var("a"),
                    new Var("b"),
                    new Var("b"),
                    new Var("c"),
                    new Var("aa"))).Name);
        }

        [Fact]
        public void Mapping_Evaluate_CorrectMapping()
        {
            var mapping = Mapping.Construct(
                ("a", new Constant(5.0)),
                ("b", new Constant(-3))
            );
            Assert.Equal("5", mapping.Evaluate(null, new Constant("a")).Pretty());
            Assert.Equal("-3", mapping.Evaluate(null, new Constant("b")).Pretty());
        }

        [Fact]
        public void TestStringGeneration()
        {
            var v = Var.Fresh(new HashSet<string> {"$a", "$b"});
            Assert.Equal("$c", v.Name);
        }

        [Fact]
        public void TestDeconstruct()
        {
            var a = new Constant("a");

            var app = new Apply(
                new Apply(
                    new Apply(Funcs.Id, Funcs.Id), Funcs.Id), a);
            var (f, args ) = app.DeconstructApply().Value;
            Assert.Equal(Funcs.Id.Name, ((Function) f).Name);
            Assert.Equal(new List<IExpression> {Funcs.Id, Funcs.Id, a}.Select(e => e.ToString()),
                args.Select(e => e.ToString()));
        }

        [Fact]
        public void SpecializeToSmallest_IsSmallestType()
        {
            var app = new Apply(Funcs.Id, Funcs.Parse);
            Assert.Equal(2, app.Types.Count());
            var smaller = app.SpecializeToSmallestType();
            Assert.Single(smaller.Types);
        }
    }
}