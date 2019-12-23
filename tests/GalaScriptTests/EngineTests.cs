using System;
using GalaScript;
using GalaScript.Exceptions;
using GalaScript.Interfaces;
using NUnit.Framework;

namespace GalaScriptTests
{
    public class EngineTests
    {
        private readonly IScriptEngine _engine;

        public EngineTests()
        {
            _engine = new ScriptEngine(true);
        }

        [Test]
        public void TestFunction()
        {
            Assert.AreEqual(2.0, _engine.Run("[add 2 0]"));
            Assert.AreEqual(1.0, _engine.Run("[add 2 -1]"));
            Assert.AreEqual(4.0, _engine.Run("[mul 2 2]"));
            Assert.AreEqual(1.0, _engine.Run("[div 2 2]"));

            Assert.AreEqual(false, _engine.Run("[gt 2 2]"));
            Assert.AreEqual(true, _engine.Run("[ge 2 2]"));
            Assert.AreEqual(false, _engine.Run("[lt 2 2]"));
            Assert.AreEqual(true, _engine.Run("[le 2 2]"));
            Assert.AreEqual(true, _engine.Run("[eq 2 2]"));
            Assert.AreEqual(false, _engine.Run("[ne 2 2]"));

            Assert.AreEqual(-1, _engine.Run("[cmp 1 2]"));
            Assert.AreEqual(0, _engine.Run("[cmp 2 2]"));
            Assert.AreEqual(1, _engine.Run("[cmp 2 1]"));

            Assert.Throws<MethodNotFoundException>(() => _engine.Run("[foo bar]"), "Method \"foo\" Not Found");
        }

        [Test]
        public void TestReturn()
        {
            Assert.AreEqual(1.0, _engine.Run("[add 1 0]"));

            Assert.AreEqual(3.0, _engine.Run("[add ret 2]"));
        }

        [Test]
        public void TestPushPeekPop()
        {
            Assert.AreEqual(1.0, _engine.Run("[add 1 0]"));

            Assert.AreEqual(1.0, _engine.Run("[push]"));

            Assert.AreEqual(1.0, _engine.Run("[peek]"));
            Assert.AreEqual(1.0, _engine.Run("[pop]"));

            Assert.Throws<InvalidOperationException>(() => _engine.Run("[peek]"));
            Assert.Throws<InvalidOperationException>(() => _engine.Run("[pop]"));
        }

        [Test]
        public void TestAlias()
        {
            Assert.AreEqual(2.0, _engine.Run("[add 2 0] : $foo"));
            Assert.AreEqual(4.0, _engine.Run("[add 2 2]:$bar"));
            Assert.AreEqual(6.0, _engine.Run("[add $foo $bar]"));
        }

        [Test]
        public void TestPrepare()
        {
            _engine.Prepare("[add 2 2]");

            Assert.AreEqual(4.0, _engine.Run());
        }

        [Test]
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

        [Test]
        public void TestImport()
        {
            _engine.Prepare("misc/foo.gs");

            Assert.AreEqual(6.0, _engine.Run());
        }

        [Test]
        public void TestGotoGoif()
        {
            _engine.Prepare("misc/test.gs");

            Assert.AreEqual(6.0, _engine.Run());

            Assert.AreEqual(25, _engine.Current.CurrentLineNumber);

            Assert.Throws<ArgumentException>(() => _engine.Run("[goto *none]"));
        }
    }
}
