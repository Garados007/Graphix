# UI Language - Animations

Each object and prototype can be animated. Every animation can change values or trigger events. 
With the animation engine you can create complex animations without a single line of code.

The easiest way to add animations is to add it in the prototype or object definition directly.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Objects>
  <Prototype object-name="MyOwnPrototype">
    <Animation>

    </Animation>
  </Prototype>
</Objects>
```

The animation block consists of many animation groups like that:

```xml
<Animation>
  <Group name="animGroup">
    <Activation>

    </Activation>
    <Effects timing="1">

    </Effects>
  </Group>
</Animation>
```

Each group get a optional name where this animation can accessed later. The group consists of two parts:

1. The `Activation` context. Here are some Activators, that could start the animation if any condition has happened.
2. The `Effects` context. Here is the real magic. Here are all effects listet that this animation would do.

Effects has a optional timing multiplier that decides how fast this animation should run. Numbers lower then 1 means that this
complete group should run slower than normal and numbers higher then 1 faster. 1 is normal speed.

If the `Activation` block or the `Effects` block is empty it can be omited.

## Animation activators

Activators can start this animation automaticly if the specified condition happened.

### StatusChange

```xml
<StatusChange old="OldStatus" new="NewStatus" enable="true" />
```

Activates if a status has been changed.

The optional attribute `old` says it can only be activated if the old status has this given root.
The optional attribute `new` says it can only be activated if the new status has this given root.
The optional attribute `enable` can turn this activator on or off.

The values can be reference values (more [here](variables.md)).

More about status see [here](status.md).

### AfterAnimation

```xml
<AfterAnimation target="objectName" effect="effectGroup" enable="true" />
```

Activates after another animation has finished completly.

The required attribute `target` specifies the name of the object (or prototype) which contains the animation.
The required attribute `effect` specifies the name of the animation group.
The optional attribute `enable` can turn this activator on or off.

The values can be reference values (more [here](variables.md)).

### Click

```xml
<Click enable="true" button="Left" />
```

This animation whould start if the user click on this object.

The optional attribute `enable` can turn this activator on or off.
The optional attribute `button` define which button has to pressed to activate. If the attribute `button` is not set any button will work.

The values can be reference values (more [here](variables.md)).

### KeyDown

```xml
<KeyDown enable="true" key="Enter" />
```

This animation whould start if the user pressed a key on the keyboard.

The optional attribute `enable` can turn this activator on or off.
The optional attribute `key` define which key has to pressed to activate. If the attribute `key` is not set any key will work.

The values can be reference values (more [here](variables.md)).

### KeyUp

```xml
<KeyUp enable="true" key="Enter" />
```

This animation whould start if the user released a key on the keyboard.

The optional attribute `enable` can turn this activator on or off.
The optional attribute `key` define which key has to released to activate. If the attribute `key` is not set any key will work.

The values can be reference values (more [here](variables.md)).

### KeyPress

```xml
<KeyPress enable="true" char="f" />
```

This animation whould start if the user enter a single char on the keyboard.

The optional attribute `enable` can turn this activator on or off.
The optional attribute `key` define which character has to insert. If the attribute `key` is not set any character will work.

The values can be reference values (more [here](variables.md)).

## Animation effects

The animation effect to the real magic here. Each of them can manipulate variables or trigger some events.

Each effect can run in async mode or sequential mode (default). If the effects are in sequential mode then they
would executed one by one. Every effect in async mode run seperate from the others.

Each effect has the following optional attributes (they are not used from every effect):

| attribute | value type | Description |
|-|-|-|
| `repeat` | `Repeat` | Describe how often this effect whoul executed |
| `reverse` | `Bool` | Run Animation backwards or forward |
| `time-start` | `Int` | Start time since the start of the execution of this group |
| `time-offset` | `Int` | Start time since the last effect finishes |
| `time-duration` | `Int` | Duration time of this effect |
| `time-finish` | `Int` | Time when this effect has to be finished. Measured in seconds since the start of execution in this group |
| `mode` | `AnimMode` | Timing function that manipulate the flow of execution |
| `enable` | `Bool` | Enable this effect. If disabled this effect whould be skipped |
| `async` | `Bool` | Runs this effect in async mode |

The time values are measured in seconds.

Hint 1: Some of the effects change a variable. If the variable should referenced trough an id then the attribute `target-id`
should be used instead of `param`. The `param` attribute gets only the variable name in the group without the prefix `@`.
Modifing core values and real values are not enabled.

Hint 2: In the preview code of the effects the above defined attributes are omited for better readability.

Hint 3: Each attribute in the effects could be a reference to a variable (except direct linking to animations or prototypes).

### AInt

```xml
<AInt param="variable" value-change="2" />
```

Modify a Int variable.

**Attributes:**

| attribute | required | value type | Description |
|-|-|-|-|
| `value-start`  | false | `Int` | The value at the start of animation |
| `value-change` | false | `Int` | The changement to this value during this animation |
| `value-finish` | false | `Int` | The value at the end of animation |
| `param` | true | `String` | The variable name that should be changed |

### AInt

```xml
<AInt param="variable" value-change="2" />
```

Modify a Int variable.

**Attributes:**

| attribute | required | value type | Description |
|-|-|-|-|
| `value-start`  | false | `Int` | The value at the start of animation |
| `value-change` | false | `Int` | The changement to this value during this animation |
| `value-finish` | false | `Int` | The value at the end of animation |
| `param` | true | `String` | The variable name that should be changed |

### AColor

```xml
<AColor param="variable" value-change="2" />
```

Modify a double variable.

**Attributes:**

| attribute | required | value type | Description |
|-|-|-|-|
| `value-start`  | false | `Color` | The value at the start of animation |
| `value-change` | false | `Color` | The changement to this value during this animation |
| `value-finish` | false | `Color` | The value at the end of animation |
| `param` | true | `String` | The variable name that should be changed |

### AScreenPos

```xml
<AScreenPos param="variable" value-change="2" />
```

Modify a double variable.

**Attributes:**

| attribute | required | value type | Description |
|-|-|-|-|
| `value-start`  | false | `ScreenPos` | The value at the start of animation |
| `value-change` | false | `ScreenPos` | The changement to this value during this animation |
| `value-finish` | false | `ScreenPos` | The value at the end of animation |
| `param` | true | `String` | The variable name that should be changed |

### ABool

```xml
<ABool param="variable" value-change="2" />
```

Modify a double variable.

**Attributes:**

| attribute | required | value type | Description |
|-|-|-|-|
| `value-start`  | false | `Bool` | The value at the start of animation |
| `value-change` | false | `Bool` | The changement to this value during this animation |
| `value-finish` | false | `Bool` | The value at the end of animation |
| `param` | true | `String` | The variable name that should be changed |

### AString

```xml
<AString param="variable" value-change="2" />
```

Modify a double variable.

**Attributes:**

| attribute | required | value type | Description |
|-|-|-|-|
| `value-start`  | false | `String` | The value at the start of animation |
| `value-change` | false | `String` | The changement to this value during this animation |
| `value-finish` | false | `String` | The value at the end of animation |
| `param` | true | `String` | The variable name that should be changed |

### Call

```xml
<Call effect="animEffect" timing="1" />
```

Calls another effect to run (this effect will not wait it's finished). The effect has to be in the same prototype level.

The `effect` attribute follows the same rule with `target-id` as above with `param`.

**Attributes:**

| attribute | required | value type | Description |
|-|-|-|-|
| `timing`  | false | `Double` | Timing multipler for the called animation (default 1) |
| `effect` | true | `String` | The effect name that should be called |

### Action

```xml
<Action name="doAction" />
```

Start an action that was registred in the core.

**Attributes:**

| attribute | required | value type | Description |
|-|-|-|-|
| `name`  | true | `String` | The name of the action that should be executed |

### Sound

```xml
<Sound file="audio/crash.wav" />
```

Start a sound file (this effect will not wait until its finished). Its recommendet to have the sound files as WAVE file.

**Attributes:**

| attribute | required | value type | Description |
|-|-|-|-|
| `file`  | true | `String` | The path to the sound file |
| `volume` | false | `Double` | The volume modifier for the sound file |

### Close

```xml
<Close />
```

Close the whole application.

## Extend Animation Groups

If you extend a prototype (see [here](prototype.md)) with an animation a new animation group would be added.
To extend or modify an animation group direct their is another method.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Objects>
  <Animation target="object" targetname="menu|dialog">
    <Group extendtype="append" extend="openAnimation">
      <Activation />
      <Effects />
    </Group>
  </Animation>
</Objects>
```

The required property `target` say if an object (`object`) or a prototype (`prototype`) should be extended.

The required property `targetname` explains which object you want to modify. If you want to edit an object inside 
an object or prototype, you have to bind them together like `<prototype/object name>|<child name>` (`|` is
seperator).

Inside the animation block you can define many animation groups. `extendtype` has the following options:

| extendtype | Description |
|-|-|
| `append` | Append the specified rules to an existing one |
| `new` | Add this animation group to the animation list (do not extend) |

If you append rules, than the required attribute `extend` defines the animation group that has to extend.
If you create a new group, than the required attribute `name` defines the name of the group.

## Specify the variable source for effects

Its possible to specify the target for effects. Normaly every effect affects the current prototype that 
this animation take place. Sometimes its better, that the effect focus one children object and search 
every variable from there.

```xml
<Group name="animGroup">
  <Effects>
    <EffectName target="childrenName" />
  </Effects>
</Group>
```

To do this its only required to add the attribute `target` to the effect definition.

## Identify groups with Ids

To access any animation group in the effects or activator its only required to add an unique id to
the animation block itself. Its only possible to do this at the fresh definition (later extension is not
supported).

```xml
<Group id="2000">

</Group>
```

Its not recommendet to use ids with animation groups. Use names instead.

If you want to use ids then check that this id is unique in the whole project.
