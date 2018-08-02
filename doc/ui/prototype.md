# UI Language - Prototypes

The most essential part of the ui are the prototypes. The prototypes define how something looks and behave.
It is very easy to define a prototype:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Objects>
  <Prototype object-name="MyOwnPrototype">

  </Prototype>
</Objects>
```

Here is an empty prototype created with the name `MyOwnPrototype`. The name can only have letters in upper or 
lower case or numbers. The names doesn't match a keyword in the `Objects` root. The name is required.

## Inherit existing prototypes

To extend an existing prototype is very easy:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Objects>
  <Prototype object-name="A">

  </Prototype>
  <Prototype object-name="B" base="A">

  </Prototype>
  <Prototype object-name="C" base="A">

  </Prototype>
</Objects>
```

Prototype A is the base prototype and define some stuff. The prototypes B and C inherits everything from A and add
own stuff. Stuff that is defined in B is not accessible in C.

If you add later something to A directly (e.g. animations) then B and C contains it automaticly too.

## Create instances of prototypes

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Objects>
  <Prototype object-name="A" />
  <A name="objectA">

  </A>
</Objects>
```

To create an instance of a prototype you only need to define the object name as the xml node name. The required
attribute `name` defines the accessible name for this object. After this creation this object is visible in
the global scope and displayed each frame.

Inside the xml node the behavior is exact the behavior of a prototype. You can add children instances, bind
variables or add animations.

This definition follow the same rules as the inheritance of prototypes but you create an object instead of a prototype.

## Add child instances to a prototype

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Objects>
  <Prototype object-name="A" />
  <Prototype object-name="B">
    <Container>
      <A name="a1"/>
      <A name="a2"/>
    </Container>
  </Prototype>
</Objects>
```

Its possible to bind instances of other prototypes as a child to another prototype. The prototype B defines the outer
bounds of both instances of A and group them. Each creation of B creates two new instances of A.

The Names of the child instances are only accessible through their parent.

## Add parameter and variables

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Objects>
  <Prototype object-name="A">
    <Parameter>
      <String name="stringParam" />
    </Parameter>
  </Prototype>
</Objects>
```

Here you can define new parameters and attach them to the prototype. Inherited prototypes has access 
to their parent parameters.

Parameters define in most prototypes their view and behavior. And Parameter can be linked together. 
For more information about this topic see [here](variables.md).

## Set value of inherited parameters

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Objects>
  <Prototype object-name="A">
    <Parameter>
      <String name="Url" />
    </Parameter>
  </Prototype>

  <Prototype object-name="B" base="A">
    <ParameterSet>
      <Url value="data/image.png" />
    </ParameterSet>
  </Prototype>
</Objects>
```

The value of the inherited parameter can easy setted in its children. Therefor is the section `ParameterSet`.
In most situation its more convenient to set the value direct:

```xml
<Prototype object-name="B" base="A">
  <Url value="data/image.png" />
</Prototype>
```

This is only acceptable if the parameter name is not a defined keyword.

If in the same prototype is another parameter with the same name defined, so the set of this parameter referes always the
current instance.

For more information about variable setting and binding see [here](variables.md).

## Attach animations

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Objects>
  <Prototype object-name="A">
    <Animation>

    </Animation>
  </Prototype>
</Objects>
```

Each prototype can modified and animated with several animations. Therefor is the section `Animation`.
These Animation can manipulate variables of the prototypes or trigger some events.

More more information about animations see [here](animation.md).

## Extend an existing prototype

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Objects>
  <Prototype object-name="A">
    <Parameter>
      <String name="varA" />
    </Parameter>
  </Prototype>

  <Prototype extends="A" extendtype="append">
    <Parameter>
      <String name="varB" />
    </Parameter>
  </Prototype>
</Objects>
```

Its possible to extend a prototype directly. All definitions that is defined in the extensions are directly added to
main prototype A. No new prototype would be created. Its required that the prototype was previosly defined.

The only possible extension type is `append` therefore the attribute `extendtype` is optional.

This method is useful if you want do split a prototype among multiple files.

## Import core prototypes

There is only one way to import prototypes that are defined in the core.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Objects>
  <Prototype object-name="PrototypeBase" 
      dotnet="Graphix.Prototypes.PrototypeBase" />
</Objects>
```

After that a new Prototype is created that equals the given core prototype. The complete class name must be put
in the `dotnet` attribute.

The real inheritance in the core implementation is here ignored. Each import looks like it was the first and no 
inheritance has happen.

All Prototypes inherits implicit from `Graphix.Prototypes.PrototypeBase` even its not imported.
