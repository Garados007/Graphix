# Script - Core utilitys

To access many stuff from the core very easy there is a global variable `$` defined that defines many
usefull methods.

## _AnimationRuntime_ $.Runtime

Returns the current runtime. With this you can access everything from this.

```js
$.Runtime.DoClose(); //close the whole application
```

## _PrototypeBase_ $.LoadPrototype(_string_ name)

Load a registered .NET prototype and create an instance.

```js
var pb = $.LoadPrototype("Image");
```

## _PrototypeBase_ $.LoadPrototype(_PrototypeLoader_ prototypeLoader, _string_ name)

Load a specific prototype from a prototype loader and returns its direct reference.

```js
var pb = $.LoadPrototype(pl, "Button");
```

## _IValueWrapper_ $.CreateValue(_PrototypeLoader_ prototypeLoader, _PrototypeBase_ prototype, _string_ name, _string_ value)

This creates a new value wrapper from the specific type and put a value on it. This value is previosly converted.
The values `prototypeLoader` and `prototype` is only used for conversion. Most value types (currently all)
doesn't need this values so you can set it with `null`.

```js
var value = $.CreateValue(null, null, "ScreenPos", "42%");
```

> **Hint:** The values `prototypeLoader` and `prototype` would maybe removed in a future release.

## _any_ $.TransformValue(_PrototypeLoader_ prototypeLoader, _PrototypeBase_ prototype, _string_ name, _string_ value)

Transform the given value using the registred converter for this type. The values `prototypeLoader` and `prototype` is only used for conversion. Most value types (currently all)
doesn't need this values so you can set it with `null`.

```js
var screenPos = $.TransformValue(null, null, "ScreenPos", "42%");
```

> **Hint:** The values `prototypeLoader` and `prototype` would maybe removed in a future release.

## _AnimationActivation_ $.CreateActivation(_string_ name)

Create a registered activator object.

```js
var statusChange = $.CreateActivation("StatusChange");
```

## _AnimationEffect_ $.CreateEffect(_string_ name)

Create a registered animation effect object,

```js
var aScreenPos = $.CreateEffect("AScreenPos");
```

## _FlatPrototype_ $.GetObject(_string_ name)

Search for an object in the current channel.

```js
var menu = $.GetObject("Menu");
```

## _FlatPrototype_ $.GetObject(_DisplayChannel_ channel, _string_ name)

Search for an object in the given channel.

```js
var menu = $.GetObject(channel, "Menu");
```

## _PrototypeBase_ $.GetObject(_PrototypeLoader_ prototypeLoader, _string_ name)

Search for an object in a prototype loader.

```js
var menu = $.GetObject(pl, "Menu");
```

## _void_ $.RunAnimation(_string_ key)

Execute all animations that contains the script activator with the given the given key.

```js
$.RunAnimation("openMenu");
```

## _any_ $.LoadData(_string_ fileName)

Load the stored JSON data from a local file. If this file does not exists this method returns `null`.

```js
var data = $.LoadData("data/savegame.json");
```

## _void_ $.SaveData(_string_ fileName, _any_ data)

Save the formated JSON object as data to the local file. If this file exists it would be overriden.

```js
$.SaveData("data/savegame.json", {
    score: 100,
    lives: 3
});
```

> **Warning:** for `data` are only plain JavaScript types allowed. This Object would be later serialized with the
native JSON converter. Graphix types and methods cannot be converted!

> **Hint:** This data is stored as plain text. There is not security for user manipulation.

## _void_ $.AddAnimationAction(_string_ name, _method_ effect)

Add this method to the global [AnimAction](../../Graphix/Physic/AnimationEffect.cs) Effect Routine.
This Method will be called each time an effect call any actions with the given name.

```js
$.AddAnimationAction("open-menu", function () {
    //do some stuff
});
```


