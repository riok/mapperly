using BenchmarkDotNet.Running;
using Riok.Mapperly.Benchmarks;

// BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

var source = new SourceGeneratorBenchmarks();
source.SetupLargeCompile();
source.LargeCompile();
