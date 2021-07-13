using BenchmarkDotNet.Attributes;
using System;
using Oldsu.Bancho;
using BenchmarkDotNet.Running;
using Version = Oldsu.Enums.Version;

namespace Oldsu.Bancho.Benchmarks
{
    [RPlotExporter]
    public class BanchoSerializerBenchmark
    {
        [Benchmark]
        public void SerializeLogin()
        {
            BanchoSerializer.Serialize(new Packet.Out.Generic.Login { LoginStatus = 2 });
        }
    }

    public class Program
    {
        public static void Main() => BenchmarkRunner.Run<Oldsu.Bancho.Benchmarks.BanchoSerializerBenchmark>();
    }
}

