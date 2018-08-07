# Scripts

With the optional extension `Graphix.Script` it is possible to bind JavaScript files to the ui.
Normaly you can control the core only with your ui or with native .NET code. With this extension you get an
extra option.

The script is running in a special container that has only access to the core and some additional functions.
It can modify or control the whole view, load or change [channels](../channel.md), edit animations and so on.

To start your script you only have to add this to the native code:

```csharp
//add this code before you load any prototypes
Graphix.Script.ScriptAccess.Register();

//create the script core to run scripts
var js = Graphix.Script.ScriptCore(renderer.Animation);

//load single file and execute
js.LoadFile(@"Path\to\file.js");
```

With the scripts you can use two sets of funtions to do what you want to do:

- [Basic utils](basic.md)
- [Core functions](core.md)