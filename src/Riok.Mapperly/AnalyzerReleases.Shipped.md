## Release 1.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
RMG001  | Mapper   | Error    | A mapping method has an unsupported signature.
RMG002  | Mapper   | Error    | No accessible parameterless constructor found.
RMG003  | Mapper   | Warning  | No overlapping enum members found.
RMG004  | Mapper   | Warning  | Ignored target property not found.
RMG005  | Mapper   | Error    | Mapping target property not found.
RMG006  | Mapper   | Error    | Mapping source property not found.
RMG007  | Mapper   | Error    | Could not map property.
RMG008  | Mapper   | Error    | Could not create mapping.
RMG009  | Mapper   | Info     | Cannot map to read only property.
RMG010  | Mapper   | Info     | Cannot map from write only property.
RMG011  | Mapper   | Info     | Cannot map to write only property path.
RMG012  | Mapper   | Info     | Mapping source property not found.
RMG013  | Mapper   | Error    | No accessible constructor with mappable arguments found
RMG014  | Mapper   | Warning  | Cannot map to the configured constructor to be used by Mapperly
RMG015  | Mapper   | Info     | Cannot map to init only property path
RMG016  | Mapper   | Error    | Init only property cannot handle target paths
RMG017  | Mapper   | Warning  | An init only property can have one configuration at max

## Release 2.3

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
RMG018  | Mapper   | Error    | Partial static mapping method in an instance mapper
RMG019  | Mapper   | Error    | Partial instance mapping method in a static mapper
RMG020  | Mapper   | Info     | Source property is not mapped to any target property

## Release 2.4

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
RMG021  | Mapper   | Warning  | Ignored source property not found

## Release 2.5

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
RMG022  | Mapper   | Error    | Invalid object factory signature

## Release 2.6

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
RMG023  | Mapper   | Error    | Mapping source property for a required target property not found
RMG024  | Mapper   | Error    | The reference handler parameter is not of the correct type
RMG025  | Mapper   | Error    | To use reference handling it needs to be enabled on the mapper attribute

## Release 2.7

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
RMG026  | Mapper   | Info     | Cannot map from indexed property
