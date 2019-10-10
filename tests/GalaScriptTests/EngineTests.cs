using System;
using System.Linq;
using GalaScript;
using GalaScript.Interfaces;
using NUnit.Framework;

namespace GalaScriptTests
{
    public class EngineTests
    {
        private readonly IScriptEngine _engine;

        private static decimal Add(params decimal[] arguments) =>
            arguments.Aggregate(0.0m, (acc, argument) => acc + argument);

        private static string Echo(params string[] arguments) =>
            arguments.Aggregate("", (str, argument) => str + argument);

        public EngineTests()
        {
            _engine = new ScriptEngine(true);
            _engine.Register("add", (Func<decimal[], decimal>)Add);
            _engine.Register("echo", (Func<string[], string>)Echo);
        }

        [Test, Order(1)]
        public void TestFunction()
        {
            Assert.AreEqual("hello", _engine.Run("[echo \"hello\"]"));
            Assert.AreEqual("world", _engine.Run("[echo \"world\"]"));
            Assert.AreEqual("hello world", _engine.Run("[echo \"hello\" \" \" \"world\"]"));
            Assert.AreEqual("hello world", _engine.Run("[echo \"hello world\"]"));

            Assert.AreEqual(2.0, _engine.Run("[add 2 0]"));
            Assert.AreEqual(4.0, _engine.Run("[add 2 2]"));
            Assert.AreEqual(1.0, _engine.Run("[add 2 -1]"));
            Assert.AreEqual(6.0, _engine.Run("[add 2 2 2]"));
        }

        [Test, Order(2)]
        public void TestReturn()
        {
            Assert.AreEqual(8.0, _engine.Run("[add ret 2]"));
        }

        [Test, Order(3)]
        public void TestPushPeekPop()
        {
            Assert.AreEqual(8.0, _engine.Run("[push]"));

            Assert.AreEqual(8.0, _engine.Run("[peek]"));
            Assert.AreEqual(8.0, _engine.Run("[pop]"));

            Assert.Catch<InvalidOperationException>(() => _engine.Run("[peek]"));
            Assert.Catch<InvalidOperationException>(() => _engine.Run("[pop]"));
        }

        [Test, Order(5)]
        public void TestAlias()
        {
            Assert.AreEqual(2.0, _engine.Run("[add 2 0] : $foo"));
            Assert.AreEqual(4.0, _engine.Run("[add 2 2]:$bar"));
            Assert.AreEqual(6.0, _engine.Run("[add $foo $bar]"));
        }

        [Test, Order(6)]
        public void TestPrepare()
        {
            _engine.Prepare("[add 2 2]");

            Assert.AreEqual(4.0, _engine.Run());
        }

        [Test, Order(7)]
        public void TestMacro()
        {
            _engine.Prepare(@"
!foo [$a $b $c]
    [add $a $b]
    [add ret $c]
    [add ret $bar]
!

[foo 2 2 2]
");

            Assert.AreEqual(10.0, _engine.Run());

            _engine.Prepare(@"
!bar
    [add $bar 2]
    [add ret 2]
!

[bar]
");

            Assert.AreEqual(8.0, _engine.Run());
        }

        [Test, Order(7)]
        public void TestImport()
        {
            _engine.Prepare("misc/foo.gs");

            Assert.AreEqual(6.0, _engine.Run());
        }

        [Test, Order(8)]
        public void TestGotoGoif()
        {
            _engine.Prepare("misc/test.gs");

            Assert.AreEqual(6.0, _engine.Run());

            Assert.AreEqual(25, _engine.Current.CurrentLineNumber);

            Assert.Catch<ArgumentException>(() => _engine.Run("[goto *none]"));
        }
    }
}
