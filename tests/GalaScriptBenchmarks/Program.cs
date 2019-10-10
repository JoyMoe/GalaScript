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
        private readonly IScriptEngine _engine;

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
            _engine.Reset();
        }
    }

    [ClrJob(true), CoreJob, MonoJob, CoreRtJob]
    [RPlotExporter, RankColumn, MemoryDiagnoser]
    public class Block
    {
        private readonly IScriptEngine _engine;

        private static decimal Add(params decimal[] arguments) =>
            arguments.Aggregate(0.0m, (acc, argument) => acc + argument);

        public Block()
        {
            _engine = new ScriptEngine();

            _engine.Register("add", (Func<decimal[], decimal>) Add);

            _engine.Prepare("test.gs");
        }

        [Benchmark]
        public void RunBlockByScriptEvaluator()
        {
            _engine.Run();
            _engine.Reset();
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
