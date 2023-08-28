---
sidebar_position: 2
description: How Mapperly is tested and linted.
---

# Tests and linting

Mapperly is continuously tested by GitHub Actions.
Tests are separated into integration and unit tests located in the `test` directory
and use [xUnit](https://xunit.net/) and [VerifyTests](https://github.com/VerifyTests/Verify).
You can run the tests by running

```bash
dotnet test
```

or by running the discovered tests in your IDE.

## Unit tests

The unit tests are located in the `tests/Riok.Mapperly.*.Tests` projects.
These unit tests usually test exactly one unit of code or one type of mapping by isolating its code
and verifying the reported diagnostics and the emitted code.
Unit tests are easy to debug (you can debug them like any other code),
but be reminded that these kind of tests only run on the latest supported target framework.

The `TestSourceBuilder` class can be used to generate the source code of a Mapper class.
The `TestHelper` class can be used to run the source generator and assert or snapshot the result.

## Integration tests

The integration tests are located in `tests/Riok.Mapperly.IntegrationTests`.
Integration tests are implementations of "a bit of everything" mappers.
The generated code as well as the mapped objects are verified.
These tests run locally by referencing the source generator as an analyzer.
In the CI pipeline, the integration tests reference the built NuGet package and
are run on several supported target frameworks (including .NET 7.0 but also .NET Framework).

If the content of the snapshot is different depending on the target framework used,
`[VersionedSnapshot(...)]` can be applied on a test class or method.
The version of each version,
which produces a changed snapshot content should be passed to the `VersionedSnapshotAttribute`.
Eg.
if a snapshots content is different for .NET 6.0, .NET 7.0 and .NET Framework 4.8 but .NET 8.0 is the same as .NET 7.0,
`[VersionedSnapshot(Versions.NET6_0 | Versions.NET7_0)]` can be applied.
The resulting snapshot is then stored three times:
in `default` for .NET Framework 4.8, in `NET6_0` for .NET 6.0 and in `NET7_0` for .NET 7.0 and later.
You may need to manually update older versions.
The received snapshots of the tests are saved in the GitHub Actions as artifacts
and can be downloaded.

Debugging integration tests is a lot harder than debugging unit tests.
Therefore if an integration test needs to be debugged,
it is often easier to implement an unit test for the to be tested behaviour
and debug the unit test instead of the integration test.
See also the [debugging documentation](./debugging.md) topic on how to debug Mapperly.

## VerifyTests

Several tests use [VerifyTests/Verify](https://github.com/VerifyTests/Verify)
and [VerifyTests/Verify.SourceGenerators](https://github.com/VerifyTests/Verify.SourceGenerators)
to verify reported diagnostics and snapshot emitted code.
To work with the tests of Mapperly you may find it helpful to read the documentation of it.

## Linting

The source of Mapperly is linted with multiple dotnet analyzers.
To fix issues locally run

```bash
dotnet csharpier .
dotnet format style
dotnet format analyzers
```

and to verify there are no issues run

```bash
dotnet csharpier --check .
dotnet format style --verify-no-changes
dotnet format analyzers --verify-no-changes
```

CSharpier should be run automatically via a git hook on commit.
