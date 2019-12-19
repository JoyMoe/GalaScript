using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using GalaScript;
using GalaScript.Interfaces;

namespace GalaScriptBenchmarks
{
    [SimpleJob(RuntimeMoniker.Net47)]
    [SimpleJob(RuntimeMoniker.Mono)]
    [SimpleJob(RuntimeMoniker.NetCoreApp30)]
    [SimpleJob(RuntimeMoniker.CoreRt30)]
    [RPlotExporter]
    [RankColumn]
    [MemoryDiagnoser]
    public class SingleLine
    {
        private readonly IScriptEngine _engine;

        private const string Line = "[add 2 0]";

        private IEvaluator _exp;

        public SingleLine()
        {
            _engine = new ScriptEngine();

            _engine.Prepare(Line);

            _exp = _engine.Parser.Prepare(Line).FirstOrDefault();
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

    [SimpleJob(RuntimeMoniker.Net47)]
    [SimpleJob(RuntimeMoniker.Mono)]
    [SimpleJob(RuntimeMoniker.NetCoreApp30)]
    [SimpleJob(RuntimeMoniker.CoreRt30)]
    [RPlotExporter]
    [RankColumn]
    [MemoryDiagnoser]
    public class Block
    {
        private readonly IScriptEngine _engine;

        public Block()
        {
            _engine = new ScriptEngine();

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
