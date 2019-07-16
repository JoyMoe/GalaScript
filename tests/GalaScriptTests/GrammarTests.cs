using System.IO;
using System.Linq;
using GalaScript;
using GalaScript.Evaluators;
using GalaScript.Interfaces;
using NUnit.Framework;
using Sprache;

namespace GalaScriptTests
{
    public class GrammarTests
    {
        private readonly IParser _parser;

        public GrammarTests()
        {
            var engine = new ScriptEngine();
            _parser = engine.Parser;

            var js =
                from op in Parse.String("@js").Token()
                from str in Parse.AnyChar.Except(Parse.String("@js$")).AtLeastOnce().Text()
                from ending in Parse.String("@js$")
                select new JurassicEvaluator(str);

            _parser.RegisterEvaluator(js);

            var ts =
                from op in Parse.String("@ts").Token()
                from str in Parse.AnyChar.Except(Parse.String("@ts$")).AtLeastOnce().Text()
                from ending in Parse.String("@ts$")
                select new JurassicEvaluator(str);

            _parser.RegisterEvaluator(ts);
        }

        [Test]
        public void TestLabel()
        {
            var foo = _parser.Prepare("* Label").FirstOrDefault();
            Assert.IsInstanceOf<LabelEvaluator>(foo);
            Assert.AreEqual("Label", foo.Evaluate());

            var bar = _parser.Prepare("* l_1").FirstOrDefault();
            Assert.IsInstanceOf<LabelEvaluator>(bar);
            Assert.AreEqual("l_1", bar.Evaluate());

            Assert.Catch<ParseException>(() => _parser.Prepare("*"));
            Assert.Catch<ParseException>(() => _parser.Prepare("* "));
            Assert.Catch<ParseException>(() => _parser.Prepare("* 1"));
            Assert.Catch<ParseException>(() => _parser.Prepare("* _1"));
            Assert.Catch<ParseException>(() => _parser.Prepare("* -1"));
            Assert.Catch<ParseException>(() => _parser.Prepare("* l*1"));
            Assert.Catch<ParseException>(() => _parser.Prepare("* l-1"));
        }

        [Test]
        public void TestText()
        {
            var foo = _parser.Prepare("- Hello").FirstOrDefault();
            Assert.IsInstanceOf<TextEvaluator>(foo);
            Assert.AreEqual("Hello", foo.Evaluate());

            var bar = _parser.Prepare("+ World").FirstOrDefault();
            Assert.IsInstanceOf<TextEvaluator>(bar);
            Assert.AreEqual("World", bar.Evaluate());

            var foobar = _parser.Prepare("- Hello World \\ \" + - * , . ; # [ ] /").FirstOrDefault();
            Assert.IsInstanceOf<TextEvaluator>(foobar);
            Assert.AreEqual("Hello World \\ \" + - * , . ; # [ ] /", foobar.Evaluate());

            var empty = _parser.Prepare("-").FirstOrDefault();
            Assert.IsInstanceOf<TextEvaluator>(empty);
            Assert.AreEqual(string.Empty, empty.Evaluate());
        }

        [Test]
        public void TestFunction()
        {
            var foo = _parser.Prepare("[add 2 0]").FirstOrDefault();
            Assert.IsInstanceOf<FunctionEvaluator>(foo);

            var bar = _parser.Prepare("[split \"hello world\"]").FirstOrDefault();
            Assert.IsInstanceOf<FunctionEvaluator>(bar);

            Assert.IsInstanceOf<FunctionEvaluator>(_parser.Prepare("[ add 2 0 ]").FirstOrDefault());
            Assert.IsInstanceOf<FunctionEvaluator>(_parser.Prepare(" [ add 2 0 ] ").FirstOrDefault());

            Assert.Catch<ParseException>(() => _parser.Prepare("[add 2 0"));
            Assert.Catch<ParseException>(() => _parser.Prepare("[add"));
            Assert.Catch<ParseException>(() => _parser.Prepare("["));
        }

        [Test]
        public void TestAlias()
        {
            var exp = _parser.Prepare("[add 2 0] : $foo").FirstOrDefault();
            Assert.IsInstanceOf<AliasEvaluator>(exp);

            Assert.IsInstanceOf<AliasEvaluator>(_parser.Prepare("[add 2 0]:$foo").FirstOrDefault());
            Assert.IsInstanceOf<AliasEvaluator>(_parser.Prepare("[ add 2 0 ]:$foo").FirstOrDefault());
            Assert.IsInstanceOf<AliasEvaluator>(_parser.Prepare(" [ add 2 0 ] : $foo ").FirstOrDefault());

            Assert.Catch<ParseException>(() => _parser.Prepare("[add 2 0 : $foo"));
            Assert.Catch<ParseException>(() => _parser.Prepare("[add : $foo"));
            Assert.Catch<ParseException>(() => _parser.Prepare("[ : $foo"));
            Assert.Catch<ParseException>(() => _parser.Prepare("[add 2 0] : "));
        }

        [Test]
        public void TestComment()
        {
            var foo = _parser.Prepare("# This is a comment").FirstOrDefault();
            Assert.IsNull(foo);

            var bar = _parser.Prepare("[add 2 0] # This is an in-line comment").FirstOrDefault();
            Assert.IsInstanceOf<FunctionEvaluator>(bar);

            var foobar = _parser.Prepare("[add 2 0] : $foo # This is an in-line comment").FirstOrDefault();
            Assert.IsInstanceOf<AliasEvaluator>(foobar);
        }

        [Test]
        public void TestMacro()
        {
            var foo = _parser.Prepare(@"!foo [$a $b $c]
    [add $a $b $c]
!").FirstOrDefault();

            Assert.IsInstanceOf<MacroEvaluator>(foo);

            var bar = _parser.Prepare(@"!bar
    [add 2 2 2]
!").FirstOrDefault();

            Assert.IsInstanceOf<MacroEvaluator>(bar);
        }

        [Test]
        public void TestCustomJsParser()
        {
            var foo = _parser.Prepare(@"
@js
result = ""hello""
@js$
").FirstOrDefault();

            Assert.IsInstanceOf<JurassicEvaluator>(foo);
            Assert.AreEqual("hello", foo.Evaluate());

            var bar = _parser.Prepare(@"
@ts
result = ""hello""
@ts$
").FirstOrDefault();

            Assert.IsInstanceOf<JurassicEvaluator>(bar);
            Assert.AreEqual("hello", bar.Evaluate());
        }

        [Test]
        public void TestScript()
        {
            var exps = _parser.Prepare(File.ReadAllText("misc/test.gs"));

            Assert.AreEqual(24, exps.Count());
        }
    }
}
