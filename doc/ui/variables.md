# UI Language - Variables and Parameter

Variables and parameter are two terms with the same meaning - these are information bind to prototypes or
objects and deliver the core with information how to render these or the animation system to animate these.

All Parameter need to be defined in an prototype or object. For more information how to do this see
[here](prototype.md).

## Variable types

Each variable type is defined in the core and can simply used. If the value input is invalid an execution would be thrown.

### String

```xml
<String name="string" value="abc" />
```

This variable type can store a single sequence of chars. To enter specific chars use the xml escaping.

### Double

```xml
<Double name="double" value="3.141" />
```

This variable can store a single number with its decimals.

You can use `.` and `,` as decimal separator.

### Int

```xml
<Int name="integer" value="42" />
```

This variable can store a single number without its fraction.

### Bool

```xml
<Bool name="boolean" value="true" />
```

This variable can store only the two values `true` and `false`.

### Repeat

```xml
<Repeat name="repeat" value="100" />
```

This variable stores how often something should be repeated. The only valid inputs are 
positive integers, `none` and `infinite`.

### Status

```xml
<Status name="status" value="Main|Sub|SubSub" />
```

Stores a single status indicator. For more information about this topic see [here](status.md).

### ScreenPos

```xml
<ScreenPos name="screen-pos" value="15%" />
```

Define a length for positioning on the screen. Each value consits of two parts: a double value and a dimension.

Valid dimensions are:

| Dimension | Example | Description |
|-|-|-|
| _blank_ | `15` | Absolute position in pixel |
| `%` | `15%` | Relative position in the same axis to its parent element. Lets say the parent has a width of 200px so 15% means 30px from the left of the parent if the scope is in the x direction. |
| `%w` | `15%w` | Relative position to the width axis to its parent element. Lets say the parent has a width of 200px so 15% means 30px even the scope is in the y direction. |
| `%h` | `15%h` | Relative position to the height axis to its parent element. Lets say the parent has a height of 200px so 15% means 30px event the scope is in the x direction. |
| `v` | `15v` | Relative position in the same axis to the screen. Lets say the screen has a width of 2000px so 15% means 300px from the left on the screen if the scope is in the x direction. |
| `vw` | `15vw` | Relative position to the width of the screen. Lets say the screen has a width of 2000px so 15% means 300px event the scope is in the y direction. |
| `vh` | `15vh` | Relative position to the height of the screen. Lets say the screen has a height of 2000px so 15% means 300px event the scope is in the x direction. |

### Align

```xml
<Align name="align" value="Center" />
```

The horizontal alignment of an element. Valid values are:

| Value | Description |
|-|-|
| `Left` | Align this element on the left side |
| `Center` | Align this element in the middle |
| `Right` | Align this element on the right side |

### Valign

```xml
<Valign name="valign" value="Center" />
```

The vertical alignment of an element. Valid values are:

| Value | Description |
|-|-|
| `Top` | Align this element on the top side |
| `Center` | Align this element in the middle |
| `Bottom` | Align this element on the bottom side |

### AnimMode

```xml
<AnimMode name="anim-mode" value="Linear" />
```

Describe an animation modificator that modifies the animation function.

| Value | Description | Function |
|-|-|-|
| `Linear` | No changement, the animation is called normal | `f(t) = t` |
| `SwingIn` | The animation is slower at the beginning and very fast at the end | `f(t) = t²` |
| `SwingOut` | The animation is fast at the beginning and slow at the end | `f(t) = - t² + 2 t` |
| `Swing` | The animation is slow at the beginning and at the end. In the middle its fast. | if `t<0.5` then `f(t) = 2 t²` else `f(t) = -2 t² + 4 t - 1` |
| `Focus` | The animation is fast at the beginning and at the end. In the middle its slow. | `f(t) = 0.5 ( 2t - 1 )³ + 0.5` |
| `Jump` | The animation jumps from 0 to 1 in the middle of the time. | if `t<0.5` then `f(t) = 0` else `f(t) = 1` |

### Color

```xml
<Color name="color" value="green" />
```

This variable can store a specific color. They are 5 ways to define a color:

#### 1. Color name

Simply use the name of the color and use it. If you want a list of possible names take a look at
[MSDN](https://docs.microsoft.com/de-de/dotnet/api/system.drawing.knowncolor?view=netframework-4.5.2). The names are case insensitive.

#### 2. Gray level value

An integer value between 0 and 255 to define the gray level.

- `0` - Black
- `128` - Medium Gray
- `255` - White

#### 3. Gray level and alpha value

Two integer values between 0 and 255 seperated with a comma. First one defines the gray level, second one the alpha value (opacity).

- `0,255` - Black, no opacity
- `128,128` - Medium Gray with half opacity
- `0,0` - Black, full opacity. Full transparent

#### 4. Red, Green, Blue value

Three integer values between 0 and 255 seperated with a comma. First one is for red, second for green, third for blue.

- `0,0,0` - Black
- `255,0,0` - Red
- `128,0,0` - Dark red

#### 5. Red, Green, Blue, Alpha value

Four integer values between 0 and 255 seperated with a comma. First one is for red, second for green, third for blue, forth one the alpha value (opacity).

- `0,255,0,255` - Green, no opacity
- `0,0,255,128` - Blue, half opacity
- `0,0,0,0` - Black, full opacity. Full transparent

## Set Value

They are many options to set a value of a variable.

### 1. Set it direct in the definition

```xml
<Parameter>
  <String name="Url" value="data/image.png" />
</Parameter>
```

The value gets its own default value. Its recommendet to always set a default value for each definition.

### 2. Set it direct in an inheritance

```xml
<ParameterSet>
  <Url value="data/image.png" />
</ParameterSet>
```

The value is setted after child prototype is produced.

### 3. Put it as content

```xml
<ParameterSet>
  <Url>
    data/image.png 
  </Url>
</ParameterSet>
```

Long value definitions are better to define in this way. Trailing spaces are eliminated.

### 4. Link it

```xml
<ParameterSet>
  <Url ref="@ImageUrl" />
</ParameterSet>
<Parameter>
  <String name="ImageUrl" value="data/image.png" />
</Parameter>
```

The linked variable get its own value from an different variable. In this case `Url` is defined in the parent prototype. 
`Url` gets now its value always from `ImageUrl` that is newly defined here. Every changement to `ImageUrl` would change 
`Url` in the same way and vice versa.

This method is very usefull in combination of math expressions (see below) or prototype grouping (see
[here](prototype.md)).

For more information about link references see below.

## Linking types

Every variable of the same type can linked together. After linking they behave like they are only one variable.
Any changements affects every variable that are linked together. To reference a value you need to put its reference
in the `ref` attribute:

```xml
<ParameterSet>
  <Url ref="@ImageUrl" />
</ParameterSet>
```

The `@` defines where the core has to look for the name. They are different kinds of lookups:

| Prefix | Key Type | Description |
|-|-|-|
| `$` | String | System values. These values are delivered from the core. |
| `@` | String | Prototype references. this values are defined in the current scope or in parent prototypes |
| `#` | Integer | Strong id reference. Id is bound to its source. This id has to be unique in the whole ui! |

The reference of Prototype keys are a little bit difficult. Lets have a look at following example:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Objects>
  <Prototype object-name="A">
    <Parameter>
      <String name="Url" />
    </Parameter>
  </Prototype>

  <Prototype object-name="B">
    <Container>
      <A name="a1">
        <Url ref="@ChildUrl" />
      </A>
    </Container>
    <Parameter>
      <String name="ChildUrl" />
    </Parameter>
  </Prototype>
</Objects>
```

In this example we can reference a value from the parent prototype. But how does it work? To understand this we have to look 
where and in which order the core search for the reference key:

1. Search in the current prototype for the key
2. Search in the inherited prototype for the key
   - Repeat 2 until the root is reached
3. Search in the parent object where this prototype is in the container for the key
   1. Search in the inherited prototype for the key
      - Repeat 3.1 until the root is reached
   2. Repeat 3 until all parent are searched
4. Key not found

This search process is only done once at compile time.
Because only objects can have a parent the backtracing is very limited and later changements at the prototype doesn't affect
anything at the linking.

Because the `ref` attribute is not allowed at variable declaration you can either define the value in the `value` attribute
or the reference.

```xml
<Parameter>
  <String name="Url" value="data/image.png" />
  <String name="PlayerName" value="$player-name" />
</Parameter>
```

## Define unique id to variable

```xml
<Parameter>
  <String name="Url" value="data/image.png" id="1000" />
</Parameter>
```

The only place to put the id on is on declaration. The id needs to be unique in the whole project. Therefore it's
not recommendet to use it.

If you put an id to an variable it is accessible from anywhere.

It is recommendet to only put ids on values in global object definition.

## Mathematical expressions

Every mathematical expression is a own kind of variable declaration. It can be referenced like a normal variable and 
its value is the result of a computation. 

```xml
<Math type="Int" name="mathValue">
  <Int value="100" />
</Math>
```
In the inside of the `Math` body you can define or reference any variable
or use special calculation therms. 
The value in the `type` attribute can be any type that is defined above. The result value would be that type and the 
`Math` value expects that its body returns a value of that type.

There is it not neccessary to define variable names because they are not accessible from 
outside.

### Calc statement

Calculate the result of an expression

```xml
<Math type="Int" name="mathValue">
  <Calc type="Int" method="add" precompile="false">
    <Int value="17" />
    <Int value="25" />
  </Calc>
</Math>
```

The required attribute `type` can be one of following values:

| type | Description |
|-|-|
| `Double` | Returns a double value |
| `Int` | Returns an int value |
| `Bool` | Returns a boolean value |
| `String` | Returns a string value |

The required attribute `method` can be one of following values:

| method | Description | Number of values | Types |
|-|-|-|-|
| `add` | Add (+) | _any_ | int, double |
| `sub` | Subtract (-) | _any_ | int, double |
| `mult` | Multiply (*) | _any_ | int, double |
| `div` | Divide (/) | 2 | int, double |
| `neg` | Negate (-) | 1 | int, double |
| `pow` | Power of numbers | _any_ | int, double |
| `and` | Boolean and | _any_ | bool |
| `or` | Boolean or | _any_ | bool |
| `xor` | Boolean xor | _min_ 1 | bool |
| `nor` | Boolean nor | _min_ 1 | bool |
| `xnor` | Boolean xnor | _min_ 1 | bool |
| `not` | Boolean not | 1 | bool |
| `nand` | Boolean nand | _min_ 1 | bool |
| `concat` | Concat strings together | _any_ | string |
| `fromDouble` | Convert value from double to target | 1 | _all_ |
| `fromInt` | Convert value from int to target | 1 | _all_ |
| `fromBool` | Convert value from bool to target | 1 | _all_ |
| `fromString` | Convert value from string to target | 1 | _all_ |

The optional attribute `precompile` defines how the value should compiles:

| precompile | Description |
|-|-|
| `true` | The value is only calculated at the first time. The value is buffered after that. |
| `false` (default) | The value whould be calculated each time the value was requested. |

### If statement

Decide between two values and return only one of them.

```xml
<Math type="Int" name="mathValue">
  <If precompile="false">
    <Condition>
      <Calc type="Bool" method="not">
        <Bool value="false" />
      </Calc>
    </Condition>
    <True>
      <Int value="42" />
    </True>
    <False>
      <Int value="100" />
    </False>
  </If>
</Math>
```

In the `Condition` section is a boolean value required. If the condition is true then the value from the `True`
section would returned otherwise from the `False` section.

The optional attribute `precompile` defines how the value should compiles:

| precompile | Description |
|-|-|
| `true` | The value is only calculated at the first time. The value is buffered after that. |
| `false` (default) | The value whould be calculated each time the value was requested. |

### Check statement

Compares two values and returns a boolean value with the result.

```xml
<Math type="Bool" name="mathValue">
  <Check method="lt" precompile="false">
    <Int value="100" />
    <Int value="10" />
  </Check>
</Math>
```

The required attribute `method` can be on of the following values:

| method | Description |
|-|-|
| `eq` | Both are equal (==) |
| `neq` | Both are not equal (!=) |
| `lt` | Left is lower than right (<) |
| `lteq` | Left is lower or equal right (<=) |
| `gt` | Left is greater than right (>) |
| `gteq` | Left is greater or equal right (>=) |

The optional attribute `precompile` defines how the value should compiles:

| precompile | Description |
|-|-|
| `true` | The value is only calculated at the first time. The value is buffered after that. |
| `false` (default) | The value whould be calculated each time the value was requested. |

