# Constructor mappings

Mapperly supports using parameterized constructors of mapping target types.
Mapperly resolves the constructor to be used by the following priorities:
* accessible constructors annotated with `MapperConstructor`
* accessible parameterless constructors
* accessible constructors ordered in descending by their parameter count
* constructors with a `System.ObsoleteAttribute` attribute, unless they have a `MapperConstructor` attribute

The first constructor which allows the mapping of all parameters is used.
Constructor parameters are mapped in a case insensitive matter.
