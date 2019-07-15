using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using GalaScript;
using GalaScript.Interfaces;

namespace GalaScriptBenchmarks
{
    [ClrJob(true), CoreJob, MonoJob, CoreRtJob]
    [RPlotExporter, RankColumn, MemoryDiagnoser]
    public class SingleLine
    {
        private readonly IEngine _engine;

        private static decimal Add(params decimal[] arguments) =>
            arguments.Aggregate(0.0m, (acc, argument) => acc + argument);

        private const string Line = "[add 2 0]";

        private IEvaluator _exp;

        public SingleLine()
        {
            _engine = new ScriptEngine();

            _engine.Register("add", (Func<decimal[], decimal>)Add);

            _engine.Prepare(Line);

            _exp = _engine.GetParser().Prepare(Line).FirstOrDefault();
        }

        [Benchmark]
        public object RunSingleLineNative()
        {
            var result = Add(2.0m, 0.0m);

            if (result is decimal ret && ret != 2.0m)
            {
                throw new Exception();
            }

            return result;
        }

        [Benchmark]
        public object RunSingleLineDirectly()
        {
            var result = _exp.Evaluate();

            if (result is decimal ret && ret != 2.0m)
            {
                throw new Exception();
            }

            return result;
        }

        [Benchmark]
        public object RunSingleLineByScriptEvaluator()
        {
            var result = _engine.Evaluate();

            if (result is decimal ret && ret != 2.0m)
            {
                throw new Exception();
            }

            return result;
        }
    }

    [ClrJob(true), CoreJob, MonoJob, CoreRtJob]
    [RPlotExporter, RankColumn, MemoryDiagnoser]
    public class Block
    {
        private readonly IEngine _engine;

        private static decimal Add(params decimal[] arguments) =>
            arguments.Aggregate(0.0m, (acc, argument) => acc + argument);

        private const string Script = @"
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
";

        public Block()
        {
            _engine = new ScriptEngine();

            _engine.Register("add", (Func<decimal[], decimal>) Add);

            _engine.Prepare(Script);
        }

        [Benchmark]
        public object RunBlockByScriptEvaluator()
        {
            return _engine.Evaluate();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<SingleLine>();
            BenchmarkRunner.Run<Block>();
        }
    }
}
