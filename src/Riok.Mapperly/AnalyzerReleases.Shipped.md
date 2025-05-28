## Release 1.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
RMG001  | Mapper   | Error    | A mapping method has an unsupported signature.
RMG002  | Mapper   | Error    | No accessible parameterless constructor found.
RMG003  | Mapper   | Warning  | No overlapping enum members found.
RMG004  | Mapper   | Warning  | Ignored target member not found.
RMG005  | Mapper   | Error    | Mapping target member not found.
RMG006  | Mapper   | Error    | Mapping source member not found.
RMG007  | Mapper   | Error    | Could not map member.
RMG008  | Mapper   | Error    | Could not create mapping.
RMG009  | Mapper   | Info     | Cannot map to read only member.
RMG010  | Mapper   | Info     | Cannot map from write only member.
RMG011  | Mapper   | Info     | Cannot map to write only member path.
RMG012  | Mapper   | Info     | Source member was not found for target member
RMG013  | Mapper   | Error    | No accessible constructor with mappable arguments found
RMG014  | Mapper   | Warning  | Cannot map to the configured constructor to be used by Mapperly
RMG015  | Mapper   | Info     | Cannot map to init only member path
RMG016  | Mapper   | Error    | Init only member cannot handle target paths
RMG017  | Mapper   | Warning  | An init only member can have one configuration at max

## Release 2.3

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
RMG018  | Mapper   | Error    | Partial static mapping method in an instance mapper
RMG019  | Mapper   | Error    | Partial instance mapping method in a static mapper
RMG020  | Mapper   | Info     | Source member is not mapped to any target member

## Release 2.4

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
RMG021  | Mapper   | Warning  | Ignored source member not found

## Release 2.5

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
RMG022  | Mapper   | Error    | Invalid object factory signature

## Release 2.6

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
RMG023  | Mapper   | Error    | Mapping source member for a required target member not found
RMG024  | Mapper   | Error    | The reference handler parameter is not of the correct type
RMG025  | Mapper   | Error    | To use reference handling it needs to be enabled on the mapper attribute

## Release 2.7

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
RMG026  | Mapper   | Info     | Cannot map from indexed member
RMG027  | Mapper   | Warning  | A constructor parameter can have one configuration at max
RMG028  | Mapper   | Error    | Constructor parameter cannot handle target paths

## Release 2.8

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
RMG029  | Mapper   | Error    | Queryable projection mappings do not support reference handling
RMG030  | Mapper   | Error    | Reference loop detected while mapping to an init only member
RMG031  | Mapper   | Warning  | Reference loop detected while mapping to a constructor member
RMG032  | Mapper   | Warning  | The enum mapping strategy ByName cannot be used in projection mappings
RMG033  | Mapper   | Info     | Object mapped to another object without deep clone

### Removed Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
RMG019  | Mapper   | Disabled | Partial instance mapping method in a static mapper

## Release 2.9

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
RMG034  | Mapper   | Error    | Derived source type is specified multiple times, a source type may only be specified once
RMG035  | Mapper   | Error    | Derived source type is not assignable to parameter type
RMG036  | Mapper   | Error    | Derived target type is not assignable to return type
RMG037  | Mapper   | Info     | An enum member could not be found on the source enum
RMG038  | Mapper   | Info     | An enum member could not be found on the target enum
RMG039  | Mapper   | Error    | Enum source value is specified multiple times, a source enum value may only be specified once
RMG040  | Mapper   | Error    | A target enum member value does not match the target enum type
RMG041  | Mapper   | Error    | A source enum member value does not match the source enum type
RMG042  | Mapper   | Error    | The type of the enum fallback value does not match the target enum type
RMG043  | Mapper   | Warning  | Enum fallback values are only supported for the ByName and ByValueCheckDefined strategies, but not for the ByValue strategy
RMG044  | Mapper   | Warning  | An ignored enum member can not be found on the source enum
RMG045  | Mapper   | Warning  | An ignored enum member can not be found on the target enum
RMG046  | Mapper   | Error    | The used C# language version is not supported by Mapperly, Mapperly requires at least C# 9.0
RMG047  | Mapper   | Error    | Cannot map to member path due to modifying a temporary value, see CS1612

## Release 3.2

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
RMG048  | Mapper   | Error    | Used mapper members cannot be nullable
RMG049  | Mapper   | Warning  | Source member is ignored and also explicitly mapped
RMG050  | Mapper   | Warning  | Target member is ignored and also explicitly mapped

## Release 3.3

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
RMG051  | Mapper   | Warning  | Invalid ignore source member found, nested ignores are not supported
RMG052  | Mapper   | Warning  | Invalid ignore target member found, nested ignores are not supported
RMG053  | Mapper   | Error    | The flag MemberVisibility.Accessible cannot be disabled, this feature requires .NET 8.0 or greater
RMG054  | Mapper   | Error    | Mapper class containing 'static partial' method must not have any instance methods 
RMG055  | Mapper   | Error    | The source type does not implement ToString with the provided formatting parameters, string format and format provider cannot be applied
RMG056  | Mapper   | Error    | Invalid format provider signature
RMG057  | Mapper   | Error    | Format provider not found
RMG058  | Mapper   | Error    | Multiple default format providers found, only one is allowed

### Removed Rules
Rule ID | Category | Severity | Notes
--------|----------|----------|-------
RMG018  | Mapper   | Disabled | Partial static mapping method in an instance mapper

## Release 3.5

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
RMG059  | Mapper   | Error    | Multiple default user mappings found, only one is allowed
RMG060  | Mapper   | Warning  | Multiple user mappings discovered without specifying an explicit default
RMG061  | Mapper   | Error    | The referenced mapping was not found
RMG062  | Mapper   | Error    | The referenced mapping name is ambiguous
RMG063  | Mapper   | Error    | Cannot configure an enum mapping on a non-enum mapping
RMG064  | Mapper   | Error    | Cannot configure an object mapping on a non-object mapping
RMG065  | Mapper   | Warning  | Cannot configure an object mapping on a queryable projection mapping, apply the configurations to an object mapping method instead
RMG066  | Mapper   | Warning  | No members are mapped in an object mapping
RMG067  | Mapper   | Error    | Invalid usage of the MapPropertyAttribute
RMG068  | Mapper   | Info     | Cannot inline user implemented queryable expression mapping
RMG069  | Mapper   | Warning  | Runtime target type or generic type mapping does not match any mappings
RMG070  | Mapper   | Error    | Mapping nested member not found

## Release 3.6

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
RMG071  | Mapper   | Warning  | Nested properties mapping is not used
RMG072  | Mapper   | Warning  | The source type of the referenced mapping does not match
RMG073  | Mapper   | Warning  | The target type of the referenced mapping does not match

## Release 4.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
RMG074  | Mapper   | Error    | Multiple mappings are configured for the same target member
RMG075  | Mapper   | Error    | Invalid usage of the MapValueAttribute
RMG076  | Mapper   | Warning  | Cannot assign null to non-nullable member
RMG077  | Mapper   | Error    | Cannot assign constant value because the type of the value does not match the type of the target
RMG078  | Mapper   | Error    | Cannot assign method return type because the type of the value does not match the type of the target
RMG079  | Mapper   | Error    | The referenced method could not be found or has an unsupported signature
RMG080  | Mapper   | Error    | The MapValueAttribute does not support types and arrays
RMG012  | Mapper   | Warning  | Source member was not found for target member
RMG020  | Mapper   | Warning  | Source member is not mapped to any target member
RMG037  | Mapper   | Warning  | An enum member could not be found on the source enum
RMG038  | Mapper   | Warning  | An enum member could not be found on the target enum
RMG081  | Mapper   | Error    | A mapping method with additional parameters cannot be a default mapping
RMG082  | Mapper   | Warning  | An additional mapping method parameter is not mapped
RMG083  | Mapper   | Info     | Cannot map to read only type
RMG084  | Mapper   | Error    | Multiple mappings are configured for the same source string
RMG085  | Mapper   | Error    | Invalid usage of fallback value
RMG086  | Mapper   | Error    | The source of the explicit mapping from a string to an enum is not of type string
RMG087  | Mapper   | Error    | The target of the explicit mapping from an enum to a string is not of type string
RMG088  | Mapper   | Info     | The attribute to build the name of the enum member is missing
RMG089  | Mapper   | Info     | Mapping nullable source to non-nullable target member
RMG090  | Mapper   | Info     | Mapping nullable source type to non-nullable target type

### Removed Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
RMG017  | Mapper   | Warning  | An init only member can have one configuration at max
RMG026  | Mapper   | Info     | Cannot map from indexed member
RMG027  | Mapper   | Warning  | A constructor parameter can have one configuration at max
RMG028  | Mapper   | Warning  | Constructor parameter cannot handle target paths

### Release 4.1

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
RMG091 | Mapper | Error | Circular referenced mapping detected
RMG092 | Mapper | Error | Source type is not assignable to the included source type
RMG093 | Mapper | Error | Target type is not assignable to the included target type
