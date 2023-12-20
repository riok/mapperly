using Microsoft.CodeAnalysis;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Output;

public readonly record struct MapperAndDiagnostics(MapperNode Mapper, ImmutableEquatableArray<Diagnostic> Diagnostics);
