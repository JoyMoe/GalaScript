using System.Linq;
using GalaScript.Evaluators;
using GalaScript.Interfaces;
using Moq;
using NUnit.Framework;

namespace GalaScriptTests
{
    public class EvaluatorTests
    {
        [SetUp]
        public void Setup()
        {
            //
        }

        [Test]
        public void TestConstantEvaluator()
        {
            var e = new ConstantEvaluator("foo");
            Assert.AreEqual("foo", e.Evaluate());
        }

        [Test]
        public void TestIntegerConstantEvaluator()
        {
            var e = new IntegerConstantEvaluator(100);
            Assert.AreEqual(100, e.Evaluate());
        }

        [Test]
        public void TestDecimalConstantEvaluator()
        {
            var e = new DecimalConstantEvaluator(2.0m);
            Assert.AreEqual(2.0m, e.Evaluate());
        }

        [Test]
        public void TestStringConstantEvaluator()
        {
            var e = new StringConstantEvaluator("bar");
            Assert.AreEqual("bar", e.Evaluate());
        }

        [Test]
        public void TestLabelEvaluator()
        {
            var e = new LabelEvaluator("bar");
            Assert.AreEqual("bar", e.Evaluate());
        }

        [Test]
        public void TestFunctionEvaluator()
        {
            var mock = new Mock<IScriptEngine>(MockBehavior.Strict);
            mock.Setup(d => d.Call(
                    It.IsAny<IScriptEvaluator>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()
                ))
                .Returns((IScriptEvaluator caller, string name, object[] parameters) => name);

            var e = new FunctionEvaluator(mock.Object, "foo", new IEvaluator[0]);
            Assert.AreEqual("foo", e.Evaluate());
        }

        [Test]
        public void TestTextEvaluator()
        {
            var mock = new Mock<IScriptEngine>(MockBehavior.Strict);
            mock.Setup(d => d.Call(
                    It.IsAny<IScriptEvaluator>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()
                ))
                .Callback((IScriptEvaluator caller, string name, object[] parameters) =>
                {
                    Assert.AreEqual("print", name);
                })
                .Returns((IScriptEvaluator caller, string name, object[] parameters) => parameters);

            var e = new TextEvaluator(mock.Object, "bar", '-');
            var result = (object[])e.Evaluate();
            Assert.AreEqual('-', result.First());
            Assert.AreEqual("bar", result.Last());
        }

        [Test]
        public void TestAliasEvaluator()
        {
            var mock = new Mock<IScriptEngine>(MockBehavior.Strict);

            mock.Setup(d => d.GetAlias(
                    It.IsAny<IScriptEvaluator>(),
                    It.IsAny<string>()
                ))
                .Returns((IScriptEvaluator caller, string name) => name);

            mock.Setup(d => d.SetAlias(
                    It.IsAny<IScriptEvaluator>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ))
                .Callback((IScriptEvaluator caller, string name, object value) =>
                {
                    Assert.AreEqual("foo", name);
                    Assert.AreEqual("bar", value);
                });

            var bar = new StringConstantEvaluator("bar");

            var get = new AliasEvaluator(mock.Object, "foo");
            Assert.AreEqual("foo", get.Evaluate());

            var set = new AliasEvaluator(mock.Object, "foo", bar);
            set.Evaluate();
        }
    }
}
