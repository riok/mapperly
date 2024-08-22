using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Benchmarks.AgressiveInliningBenchmarks;

[Mapper(EnableAggressiveInlining = true)]
public static partial class StaticMapperWithAggressiveInlining
{
    public static partial MyComplexClassDto Map(MyComplexClass source);

    public static partial MyComplexStructDto Map(MyComplexStruct source);
}

[Mapper(EnableAggressiveInlining = false)]
public static partial class StaticMapperWithoutAggressiveInlining
{
    public static partial MyComplexClassDto Map(MyComplexClass source);

    public static partial MyComplexStructDto Map(MyComplexStruct source);
}
