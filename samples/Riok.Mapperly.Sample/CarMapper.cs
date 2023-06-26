using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Sample;

// Tasks

// Existing Mapper
// Link all try find
// Pass context
// Update TryFind to handle parameters
// Not a fan of placeholder parameters

// Idea
// Context should pass down all available params
// FindOrBuildMapping should update current context with used params
// When creating Builder ctx.CurrentParams as arg

// Scope custom methods current UserMethodScope

// Problem: with recursion and resolving used parameters
// Normally when referencing itself or a method builder Mapperly will use a place holder
// method invocation and continue building
// The initialization of the body is left until later, where self referencing can be handled by
// calling itself via Find as it has been registered in the MappingCollection

// Extra params require that all children be calculated and initialized
// this is so that we can calculate what parameters are available, recursively passing
// the params up to the creator
// This is a problem: we haven't added the method mapper to mappingCollection
// so when a self referential mapping occurs it tries to construct the method all over.
// FindOrBuild will cause it to be built as it hasn't been registered
// Leading to recursion
// We can't add the mapping to MappingCollection as it hasn't been fully initialized,
// we don't know what parameters are in use.
// Use some kind of promise/callback system

// Solutions
// Check how mapperly detects self referencing, - probably relies upon the Find/Queue trick
// Register a temporary/real mapping of source, target and parameters
// Rework new instance/member mapping to pre calculate the used parameters
// Perhaps some kind of scoped Context where Find will return the mapping itself????
// - Probably infinitely loop, same as placeholder trick
// Whenever a self referencing loop is created pass all parameters in


// Parameter Resolving:
// Additional parameters can be complex type with members
// Those members can match/resolve to existing methods parameters

// Shared: use (source/target) TypeKey maps to list of mappings
// Incomplete: use TypeKey to find in dict
// Scoped: use TypeKey reset on exit scope

// Need to define a way of sorting mappings


// Method Resolving:
// Can't have more than one of a Source/Target in a scope
// Problem: How to resolve user methods, go for simple or longer?

// Always: always use UserMethod where possible
// Fallback: Try to generate mapping, using UserMethod if parameters match

// Method resolving poses a challenge due to the issue of matching convertable types, names and order.
// Order is challenging because a MethodMapping generated with certain parameters in scope could would
// create an entirely different method for a similar Parameter scope. Even if order of parameters in maintained

// Scoped: methods only match when they are user methods
// Mapperly can generate mapping methods but the method may only be used by the parent MethodMapping
// This solves the issue of determining if a function can be reused
// Pros: Faster to generate, easier to follow control flow
// Cons: code bloat, repeated functions
//      - Always match user methods as long as names and types match
//      - Same as above but ignore types??

// Smart resolved: methods are matched by name and sequential order
// Pros: Code reuse
// Cons: Slower to resolve, bug prone

// Combo: allow limited function reuse when Source, Target and Parameters match
// Extra parameters will likely be the same between methods, ie int id will be common

// Problem: how to support UserMethodMapping????
// How to handle covariance? IE Dog -> IAnimal
//      - Maybe let user explicitly map
// Should use matching name.
// How to handle order
//      - Could scan through context params looking for a matching value

// Enums of source and target have different numeric values -> use ByName strategy to map them
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName)]
public static partial class CarMapper
{
    [MapProperty(nameof(Car.Manufacturer), nameof(CarDto.Producer))] // Map property with a different name in the target type
    public static partial CarDto MapCarToDto(Car car);

    // public static partial C Map(A src, int value, int v1);
    //
    // public static partial DogDto Map(Dog src, int value, int v1);
    //
    // public static partial DataADto Map(DataA src);
    //
    // public static partial DataBDto Map(DataB src);

    public static partial B Map(A src, int value);

    public static C MapToC(string str, int value) => new C() { StringValue = str, Value = value.ToString() };
}

public class A
{
    public string Nested { get; set; }
}

public class B
{
    public C Nested { get; init; }
}

public class C
{
    public string StringValue { get; init; }
    public string Value { get; init; }
}

// public class A
// {
//     public string StringValue { get; set; }
// }
//
// public class B
// {
//     public string StringValue { get; set; }
//     public string Value { get; init; }
// }
//
// public record C
// {
//     public int Value { get; init; }
//     public int V1 { get; init; }
// }
//
// public class Dog
// {
//     public DogOwner Owner { get; set; }
//     public int Value { get; set; }
// }
//
// public class DogOwner
// {
//     public string Name { get; set; }
// }
//
// public class DogOwnerDto
// {
//     public string Name { get; set; }
// }
//
// public class DogDto
// {
//     public DogOwnerDto Owner { get; set; }
//     public int Value { get; set; }
// }
//
// public record DataA(DataC Nest);
//
// public record DataADto(DataD Nest);
//
// public record DataB(DataC Nest);
//
// public record DataBDto(DataD Nest);
//
// public record DataC(int Value)
// {
//     public int Value { get; init; } = Value;
// }
//
// public record DataD
// {
//     public int Value { get; init; }
// }
