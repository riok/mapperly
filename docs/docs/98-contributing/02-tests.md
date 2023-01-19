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

Debugging integration tests is a lot harder than debugging unit tests.
Therefore if an integration test needs to be debugged,
it is often easier to implement an unit test for the to be tested behaviour
and debug the unit test instead of the integration test.
See also the [debugging documentation](./debugging) topic on how to debug Mapperly.

## VerifyTests

Several tests use [VerifyTests/Verify](https://github.com/VerifyTests/Verify)
and [VerifyTests/Verify.SourceGenerators](https://github.com/VerifyTests/Verify.SourceGenerators)
to verify reported diagnostics and snapshot emitted code.
To work with the tests of Mapperly you may find it helpful to read the documentation of it.

## Linting

The source of Mapperly is linted with multiple dotnet analyzers.
The format is checked with `dotnet format`.
To fix issues locally run
```bash
dotnet format
```

and to verify there are no issues run
```bash
dotnet format --verify-no-changes
```
