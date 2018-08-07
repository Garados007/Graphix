# Script - basic functions

This functions and types are direct accessible from the global scope and can used anywhere. They provide 
basic features that are not direct dependent to the Graphix core.

## Functions

### _void_ load(_string_ filePath)

Load and execute the given script in the global context.

```js
load("js/utils.js");
```

### _void_ log(_string_ msg)

Write a message direct to the local log file.

### _void_ log(_Exception_ msg)

Write an error direct to the local log file.

### _int_ setTimeout(_method_ callMethod, _int_ timeout)

Calls the given method after a specific amount of milliseconds. The return value is an id
that can be used to cancel this timeout. The function callMethod cannot take any arguments.

```js
//write hello after 1 second
setTimeout(function () {
    log("hello");
}, 1000);
```

### _int_ setInterval(_method_ callMethod, _int_ interval)

Call the given method after some amount of time repeadly again and again. The return value is an id
that can be used to cancel this interval. The function callMethod cannot take any arguments.

```js
//write current time in log every 1 second
var time = 0;
setInterval(function() {
    log("" + (++time));
}, 1000);
```

### _void_ clearTimeout(_int_ id)

Cancel the given timeout. This timeout will never be executed. Finished timeouts does not require any 
canceling. If you shutdown application the canceling is not required.

If the given id is invalid or no more used nothing happens.

```js
var id = setTimeout(function() {
    log("hello");
}, 1000);
clearTimeout(id);
```

### _void_ clearInterval(_int_ id)

Cancel the given interval. If you shutdown application the canceling is not required.

If the given id is invalid or no more used nothing happens.

```js
var id = setInterval(function() {
    log("hello");
}, 1000);
clearTimeout(id); //stop spamming
```

### _void_ run(_method_ callMethod)

Call the given method directly in an asynchronous thread. The `callMethod` cannot take any arguments.
This function is simular to `setTimeout(callMethod, 0)`.

```js
run(function() {
    log("I was called in an extra Thread");
});
```

### _any_ cast(_Type_ type, _any_ arg)

Try to force cast of core types. `type` takes the core type param and `arg` the argument.

If the cast is not successful an error whould be called.

```js
var doubleVal = cast(double, 1);
```

> **Hint:** Use casting only if the normal automatic conversion wouldn't work.

## Types

They are some types from the .NET Framework and Graphix Core accessible. These are:

- `int`
- `double`
- `string`
- `bool`
- `PrototypeLoader` - Loads and manage prototypes
- `DisplayChannel` - Manage [channels](../channel.md)
- `AnimationGroup` - Usefull for creating animations

You can use this types for casting or creating new objects like

```js
var pl = new PrototypeLoader();
pl.Load("demo.xml");
```