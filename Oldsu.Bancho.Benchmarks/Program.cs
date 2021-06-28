﻿using BenchmarkDotNet.Attributes;
using System;
using Oldsu.Bancho;
using BenchmarkDotNet.Running;

namespace Oldsu.Bancho.Benchmarks
{
    [RPlotExporter]
    public class BanchoSerializerBenchmark
    {
        [Benchmark]
        public void Serialize() => BanchoSerializer.Serialize(new Packet.Out.B394a.Login { LoginStatus = 2, Privilege = 1 });
    }

    public class Program
    {
        public static void Main() => BenchmarkRunner.Run<Oldsu.Bancho.Benchmarks.BanchoSerializerBenchmark>();
    }
}
