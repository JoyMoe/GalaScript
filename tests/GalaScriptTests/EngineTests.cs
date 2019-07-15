using System;
using System.Linq;
using GalaScript;
using GalaScript.Interfaces;
using NUnit.Framework;

namespace GalaScriptTests
{
    public class EngineTests
    {
        private readonly IEngine _engine;

        private static decimal Add(params decimal[] arguments) =>
            arguments.Aggregate(0.0m, (acc, argument) => acc + argument);

        private static string Echo(params string[] arguments) =>
            arguments.Aggregate("", (str, argument) => str + argument);

        public EngineTests()
        {
            _engine = new ScriptEngine();
            _engine.Register("add", (Func<decimal[], decimal>)Add);
            _engine.Register("echo", (Func<string[], string>)Echo);
        }

        [Test, Order(1)]
        public void TestFunction()
        {
            Assert.AreEqual("hello", _engine.Evaluate("[echo \"hello\"]"));
            Assert.AreEqual("world", _engine.Evaluate("[echo \"world\"]"));
            Assert.AreEqual("hello world", _engine.Evaluate("[echo \"hello\" \" \" \"world\"]"));
            Assert.AreEqual("hello world", _engine.Evaluate("[echo \"hello world\"]"));

            Assert.AreEqual(2.0, _engine.Evaluate("[add 2 0]"));
            Assert.AreEqual(4.0, _engine.Evaluate("[add 2 2]"));
            Assert.AreEqual(1.0, _engine.Evaluate("[add 2 -1]"));
            Assert.AreEqual(6.0, _engine.Evaluate("[add 2 2 2]"));
        }

        [Test, Order(2)]
        public void TestReturn()
        {
            Assert.AreEqual(8.0, _engine.Evaluate("[add ret 2]"));
        }

        [Test, Order(3)]
        public void TestPushPeekPopEax()
        {
            Assert.AreEqual(8.0, _engine.Evaluate("[peek eax]"));
            Assert.AreEqual(8.0, _engine.Evaluate("[pop eax]"));
            Assert.AreEqual(8.0, _engine.Evaluate("[push eax]"));
            Assert.AreEqual(8.0, _engine.Evaluate("[pop eax]"));

            Assert.AreEqual(6.0, _engine.Evaluate("[pop eax]"));
            Assert.AreEqual(1.0, _engine.Evaluate("[pop eax]"));
            Assert.AreEqual(4.0, _engine.Evaluate("[pop eax]"));
            Assert.AreEqual(2.0, _engine.Evaluate("[pop eax]"));

            for (var i = 0; i < 4; i++)
            {
                _engine.Evaluate("[pop eax]");
            }

            Assert.Catch<InvalidOperationException>(() => _engine.Evaluate("[peek eax]"));
            Assert.Catch<InvalidOperationException>(() => _engine.Evaluate("[pop eax]"));
        }

        [Test, Order(4)]
        public void TestPushPeekPopEbx()
        {
            Assert.AreEqual(2.0, _engine.Evaluate("[add 2 0]"));

            for (var i = 0; i < 11; i++)
            {
                Assert.AreEqual(2.0, _engine.Evaluate("[push ebx]"));
            }

            Assert.AreEqual(2.0, _engine.Evaluate("[peek ebx]"));

            for (var i = 0; i < 10; i++)
            {
                Assert.AreEqual(2.0, _engine.Evaluate("[pop ebx]"));
            }

            Assert.Catch<InvalidOperationException>(() => _engine.Evaluate("[peek ebx]"));
            Assert.Catch<InvalidOperationException>(() => _engine.Evaluate("[pop ebx]"));

            Assert.Catch<ArgumentException>(() => _engine.Evaluate("[push 1]"));
            Assert.Catch<ArgumentException>(() => _engine.Evaluate("[peek 1]"));
            Assert.Catch<ArgumentException>(() => _engine.Evaluate("[pop 1]"));
        }

        [Test, Order(5)]
        public void TestAlias()
        {
            Assert.AreEqual(2.0, _engine.Evaluate("[add 2 0] : $foo"));
            Assert.AreEqual(4.0, _engine.Evaluate("[add 2 2]:$bar"));
            Assert.AreEqual(6.0, _engine.Evaluate("[add $foo $bar]"));
        }

        [Test, Order(6)]
        public void TestPrepare()
        {
            _engine.Prepare("[add 2 2]");

            Assert.AreEqual(4.0, _engine.Evaluate());
        }

        [Test, Order(7)]
        public void TestMacro()
        {
            _engine.Prepare(@"
!foo [$a $b $c]
    [add $a $b]
    [add ret $c]
!

[foo 2 2 2]
");

            Assert.AreEqual(6.0, _engine.Evaluate());
        }

        [Test, Order(7)]
        public void TestGotoGoif()
        {
            _engine.Prepare(@"
*label1
[add 1 1]:$va
[pop ebx]
[goto *label3]

* label2
[add ret 2]
[goto *end]

*start # start is a special label
# start here
- Hello World
- GalaScript
+ Test
[add 2 2]
[push ebx]
[goto *label1]

* label3
[add $va 2]
[goif *label2]

*end
");

            Assert.AreEqual(6.0, _engine.Evaluate());

            Assert.Catch<ArgumentException>(() => _engine.Evaluate("[goto *none]"));
        }
    }
}
