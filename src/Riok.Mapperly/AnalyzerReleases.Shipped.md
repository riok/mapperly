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
RMG034  | Mapper   | Warning  | Not all enum values in the target Enum are mapped
