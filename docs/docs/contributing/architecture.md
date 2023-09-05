---
sidebar_position: 1
description: The architecture of Mapperly.
---

# Architecture

Mapperly is an incremental .NET source generator implementation.
Source generators are explained [here](https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md)
and [here](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md).

## Solution

- `benchmarks` Benchmarks to analyze the performance of the generated code and the source generator itself
- `build` Build scripts
- `docs` Documentation of Mapperly
- `samples` Sample implementations of Mappers using Mapperly
- `src` Source code of Mapperly
  - `Riok.Mapperly` The source generator implementation
  - `Riok.Mapperly.Abstractions` Abstractions and attributes to be used by the application code to configure Mapperly.
    This is referenced by the source generator but is not needed at runtime.
  - `Riok.Mapperly.Templates` Templates of code files which are embedded as resources into `Riok.Mapperly` and may be emitted during source generation depending on enabled features.
- `test` Unit- and integration tests of Mapperly

## Flow

This describes the process implemented by Mapperly on a higher level.
For each discovered `MapperAttribute` a new `DescriptorBuilder` is created.
The `DescriptorBuilder` is responsible to build a `MapperDescriptor` which holds all the mappings.
The `DescriptorBuilder` does this by following this process:

1. Extracting the configuration from the attributes
2. Extracting user implemented object factories
3. Extracting user implemented and user defined mapping methods.
   It instantiates a `User*Mapping` (eg. `UserDefinedNewInstanceMethodMapping`) for each discovered mapping method and adds it to the queue of mappings to work on.
4. Extracting external mappings
5. For each mapping in the queue the `DescriptorBuilder` tries to build its implementation bodies.
   This is done by a so called `*MappingBodyBuilder`.
   A mapping body builder tries to map each property from the source to the target.
   To do this, it asks the `DescriptorBuilder` to create mappings for the according types.
   To create a mapping from one type to another, the `DescriptorBuilder` loops through a set of `*MappingBuilder`s.
   Each of the mapping builders try to create a mapping (an `ITypeMapping` implementation) for the asked type mapping by using
   one approach on how to map types (eg. an explicit cast is implemented by the `ExplicitCastMappingBuilder`).
   These mappings are queued in the queue of mappings which need the body to be built (currently body builders are only used for object to object (property-based) mappings).
6. The `SourceEmitter` emits the code described by the `MapperDescriptor` and all its mappings.
   The syntax objects are created by using `SyntaxFactory` and `SyntaxFactoryHelper`.
   The `SyntaxFactoryHelper` tries to simplify creating formatted syntax trees.
   If indentation is needed,
   the `SyntaxFactoryHelper` instance of the `SourceEmitterContext`/`TypeMappingBuildContext` can be used.

## Roslyn multi targeting

Mapperly targets multiple Roslyn versions by building multiple NuGet packages
and merging them together into a single one.
Multi-targeting is needed to support new language features,
such as required members introduced in C# 11,
while still supporting older compiler versions.

See `build/package.sh` for details.

To introduce support for a new roslyn version see [common tasks](./common-tasks.md#add-support-for-a-new-roslyn-version).
