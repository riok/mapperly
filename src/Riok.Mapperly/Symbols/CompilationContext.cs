using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Symbols;

public sealed record CompilationContext(
    Compilation Compilation,
    WellKnownTypes Types,
    ImmutableArray<Compilation> NestedCompilations,
    FileNameBuilder FileNameBuilder
);
