# Common tasks

This page provides to-do lists for some common tasks one may encounter while contributing to Mapperly.

## New diagnostic

To introduce a new diagnostic follow these steps:

1. Ensure no existing diagnostic in `Riok.Mapperly.Diagnostics.DiagnosticDescriptors.cs` matches the new use case.
2. Create a new public static readonly entry following the existing naming and formatting style,
   as ID use `RMG<NUMBER>` with `<NUMBER>` being the highest already used number plus one.
   The highest used number can be found in `AnalyzerReleases.Shipped.md` as there may be removed diagnostics which are not present in `DiagnosticDescriptors` anymore
   but which should still not be used for new diagnostics.
3. Add the new diagnostic to `AnalyzerReleases.Shipped.md` (Mapperly does not use the `Unshipped` file).
4. Add a unit test generating and asserting the added diagnostic (use `TestHelperOptions.AllowDiagnostics` and `Should().HaveDiagnostic(...)`.

It is not necessary to update the `Analyzer diagnostics` documentation page manually,
as it is generated automatically on the basis of the `AnalyzerReleases.Shipped.md` file.

## New public API

If new public API surface is introduced in `Riok.Mapperly.Abstractions`,
add the new API to the `PublicAPI.Shipped.txt` file directly.
Mapperly does not use the `PublicAPI.Unshipped.txt` file.

## Add support for a new roslyn version

To support a new roslyn version via multi targeting follow these steps (see also [architecture/roslyn multi targeting](01-architecture.md#roslyn-multi-targeting)):

1. Include the new version in `roslyn_versions` in `build/package.sh`.
2. Create a new file `Riok.Mapperly.Roslyn$(Version).props` in `src/Riok.Mapperly` similar to the existing ones
   and define constants and include dependencies as needed.
3. Update the default `ROSLYN_VERSION` in `src/Riok.Mapperly/Riok.Mapperly.csproj`.
4. Update the `Microsoft.CodeAnalysis.CSharp` dependency version in `test/Riok.Mapperly.Tests/Riok.Mapperly.Tests.csproj`.
5. Adjust the .NET version matrix of the `integration-test` GitHub Actions job (defined in `.github/workflows/test.yml`)
   to include a dotnet version which is based on the added Roslyn version.
6. Adjust the .NET version in the `global.json` file as needed.
7. If generated code changes based on the new Roslyn version,
   introduce a new `roslynVersionName` in `Riok.Mapperly.IntegrationTests.BaseMapperTest.GetRoslynVersion()` and generate the new snapshots.

## Mapping syntax

Mapperly Mappings use Roslyn syntax trees.
[RoslynQuoter](https://roslynquoter.azurewebsites.net/) and [SharpLab](https://sharplab.io/)
are fantastic tools to understand and work with Roslyn syntax trees.
