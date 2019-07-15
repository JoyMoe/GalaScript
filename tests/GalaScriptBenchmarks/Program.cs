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

            _exp = _engine.Parser.Prepare(Line).FirstOrDefault();
        }

        [Benchmark]
        public void RunSingleLineNative()
        {
            Add(2.0m, 0.0m);
        }

        [Benchmark]
        public void RunSingleLineDirectly()
        {
            _exp.Evaluate();
        }

        [Benchmark]
        public void RunSingleLineByScriptEvaluator()
        {
            _engine.Run();
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
        public void RunBlockByScriptEvaluator()
        {
            _engine.Run();
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
