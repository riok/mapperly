using BenchmarkDotNet.Running;

// using Riok.Mapperly.Benchmarks;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
// var source =new SourceGeneratorBenchmarks();
// source.SetupLargeCompile();
// source.LargeCompile();
//
// Span<int> s = stackalloc int[4];
// var r = Do(s);
// static Span<int> Do(Span<int> src)
// {
//     return src.Slice(2);
// }
