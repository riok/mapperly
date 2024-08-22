using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Riok.Mapperly.Benchmarks.AgressiveInliningBenchmarks;

[ArtifactsPath("artifacts")]
[MemoryDiagnoser]
[InProcess]
[CategoriesColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
public class AgressiveInliningBenchmarks
{
    private static readonly MyComplexStruct myComplexStruct = MyComplexStruct.Instance;

    private static readonly MyComplexClass myComplexClass = MyComplexClass.Instance;

    [Benchmark(Description = "WithoutAggressiveInlining"), BenchmarkCategory("Class")]
    public MyComplexClassDto MapClassWithoutAggressiveInlining() => StaticMapperWithoutAggressiveInlining.Map(myComplexClass);

    [Benchmark(Description = "WithoutAggressiveInlining"), BenchmarkCategory("Struct")]
    public MyComplexStructDto MapStructWithoutAggressiveInlining() => StaticMapperWithoutAggressiveInlining.Map(myComplexStruct);

    [Benchmark(Description = "WithAggressiveInlining"), BenchmarkCategory("Class")]
    public MyComplexClassDto MapClassWithAggressiveInlining() => StaticMapperWithAggressiveInlining.Map(myComplexClass);

    [Benchmark(Description = "WithAggressiveInlining"), BenchmarkCategory("Struct")]
    public MyComplexStructDto MapStructWithAggressiveInlining() => StaticMapperWithAggressiveInlining.Map(myComplexStruct);
}
