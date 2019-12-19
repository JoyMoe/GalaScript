using GalaScript.Evaluators;
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
        public void TestAliasEvaluator()
        {
            // var e = new AliasEvaluator();
            // Assert.AreEqual("foo", e.Evaluate());
        }

        [Test]
        public void TestConstantEvaluator()
        {
            var e = new ConstantEvaluator("foo");
            Assert.AreEqual("foo", e.Evaluate());
        }

        [Test]
        public void TestDecimalConstantEvaluator()
        {
            var e = new DecimalConstantEvaluator(2.0m);
            Assert.AreEqual(2.0m, e.Evaluate());
        }

        [Test]
        public void TestFunctionEvaluator()
        {
            // var e = new FunctionEvaluator();
            // Assert.AreEqual("foo", e.Evaluate());
        }

        [Test]
        public void TestLabelEvaluator()
        {
            var e = new LabelEvaluator("bar");
            Assert.AreEqual("bar", e.Evaluate());
        }

        [Test]
        public void TestStringConstantEvaluator()
        {
            var e = new StringConstantEvaluator("bar");
            Assert.AreEqual("bar", e.Evaluate());
        }

        [Test]
        public void TestTextEvaluator()
        {
            // var e = new TextEvaluator("bar");
            // Assert.AreEqual("bar", e.Evaluate());
        }
    }
}
