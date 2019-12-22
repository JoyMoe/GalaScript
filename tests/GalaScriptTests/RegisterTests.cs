using System;
using System.Linq;
using GalaScript;
using GalaScript.Interfaces;
using NUnit.Framework;

namespace GalaScriptTests
{
    
    public class RegisterTests
    {
        private readonly IScriptEngine _engine;

        public RegisterTests()
        {
            _engine = new ScriptEngine();
        }

        [Test]
        public void BaseRegisterTest()
        {
            Func<int, int, int> addFunc = (a, b) => a + b;
            Action throwAction = () => throw new Exception();

            _engine.Register("add", addFunc);
            _engine.Register("throw", throwAction);

            Assert.AreEqual(3, _engine.Run("[add 1 2]"));
            Assert.Throws<Exception>(() => _engine.Run("[throw]"));
        }

        [Test]
        public void ArrayParameterTest()
        {
            Func<int[], int> sumFunc = nums => nums.Sum();

            _engine.Register("sum", sumFunc);

            Assert.AreEqual(10, _engine.Run("[sum 1 2 3 4]"));
        }

        // for DefaultParameterTest (inner-method's parameters DON'T have DefaultValue)
        string DoubleString(string str = null)
            => str == null ? string.Empty : (str + str);

        [Test]
        public void DefaultParameterTest()
        {
            Func<string, string> doubleStringFunc = DoubleString;
            
            var ps = doubleStringFunc.Method.GetParameters();

            _engine.Register("doubleString", doubleStringFunc);

            Assert.AreEqual(string.Empty, _engine.Run("[doubleString]"));
            Assert.AreEqual("abcabc", _engine.Run("[doubleString abc]"));
            Assert.AreEqual(string.Empty, _engine.Run("[doubleString key=abc]"));
            Assert.AreEqual("abcabc", _engine.Run("[doubleString str=abc]"));
        }

        // for DefaultParameterTest2
        string BuildMessage(string msg, string appname="System", int channel=0)
            => $"{appname}/{channel}:{msg}";

        [Test]
        public void DefaultParameterTest2()
        {
            Func<string, string, int, string> msgFunc = BuildMessage;

            _engine.Register("msg", msgFunc);

            Assert.AreEqual("System/0:message", _engine.Run("[msg message]"));
            Assert.AreEqual("System/1:message", _engine.Run("[msg message channel=1]"));
            Assert.AreEqual("Default/0:message", _engine.Run("[msg message appname=Default]"));
            Assert.AreEqual("Default/2:mymsg", _engine.Run("[msg appname=Default channel=2 msg=mymsg]"));
        }
    }
}
