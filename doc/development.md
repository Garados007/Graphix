# Backend Development

## Create basic Application

_(to view a full basic project have a look at [Graphix.DemoApp](../Graphix.DemoApp/Program.cs))_

To create basic app you need to create an empty console project and change it in project settings to Windows Forms.
Alternativly you can create a Windows Forms project and delete Form1.

Make all References:
```csharp
using Graphix;
using Graphix.Rendering;
```

After that you need to create a `Renderer` in the main loop. In this case we choose the normal Graphix rendering engine.

```csharp
Renderer renderer = new Renderer();
```

This renderer requires to be intialized first.

```csharp
renderer.SetupForm("Graphix Demo", 0);
renderer.SetupBaseRenderer();
```
Now we need to define the Loading function. This Loading function is called when the screen is shown and after
the loading is finished the game starts. This method is perfect to view very fast a screen and the user can see
something is working insteed of long time waiting.

```csharp
renderer.LoadAssets += () =>
{

};
```

After that the very basic configuration is done. Now its time to view the window.
```csharp
renderer.Run();
```

## Load XML UI Scripts

Every UI Script is loaded with the `PrototypeLoader`. After its loaded the data is pushed to the renderer.
```csharp
PrototypeLoader pl = new PrototypeLoader();
pl.Load(@"ui\demo.xml");
renderer.Import(pl);
```

## Configure the renderer

The following can done everytime. For more details for the renderer lets have a look in the documentation (added to the code).


### Show FPS and Status

If this flag is on then the FPS and Status would be shown in the top left corner.

```csharp
renderer.ShowFps = true;
```

### Change VSync

Normaly VSync is active (mostly 60 FPS) to save energy and the cpu, gpu. If you want to turn it off you can do this in this way:

```csharp
renderer.VSync = false;
```

### Create Snapshots of the ui

If you turn this flag on and press the key F7 on your keybord (when running) then a file `snapshot.xml` would be created.
This contains the current ui tree with its current values (if some has changed).

```csharp
renderer.EnableSnapShot = true;
```

### Preload Image file

You can preload a single image file so its ready when the ui need it (preloading is not required. The ui would load
it automaticly if needed).

```csharp
renderer.PreloadImage(@"Path\to\file");
```

### Preload Animation files

To smooth your animations you can preload its files. If the file is later needed the core doesn't need to load it.

```csharp
renderer.PreloadAnimImageDir(@"Path\to\dir");
```

### Animation clockspeed

You can change the animation clockspeed. The value is stores as an intervall in milliseconds.

```csharp
renderer.Animation.AnimationTimer = 40;
```

### Close the application

You can call this method to close the application. After this a save shutdown would be initiated.

```csharp
renderer.Animation.DoClose();
```

## More Interaction to the UI

Most time you want better interaction between the logic and the ui.

### Add System Values

If you want to deliver some values direct to the ui this would be nice for you:

```csharp
ValueWrapper<string> playerName1 = new ValueWrapper<T>();
PrototypeLoader.SystemValues.Add("playerName1", playerName1);
playerName1.Value = "Gandalf"
```
```xml
<PlayerName ref="@playerName1" />
```

Its important that all system values are registred before the ui is loaded that reference to it.

### Change Status

It is very easy to set the current status for the ui:

```csharp
renderer.CurrentStatus = renderer.GetStatus("Main|SubStatus");
```

This works only if the ui has the status previosly defined.

### Hook status changes

To be always informed if the status changes

```csharp
renderer.Animation.StatusChanged += (oldStatus, newStatus) =>
{
};
```

### Push Status with delay

This status would be active after some time

```csharp
renderer.Animation.PushStatus(renderer.GetStatus("Main|SubStatus"), 1000);
```
The status change would be active after 1 second.

_More status Queue manipulation is documented in the `AnimationRuntime` class._

### Start animation

The animation would start immeadetly.

```csharp
renderer.Animation.ExecuteAnimation(animation);
```

### Play sound file

The sound would loaded and played asynchron.

```csharp
renderer.Animation.SoundPlayer.PlaySound(@"path\to\file", 1);
```

### Hook Mouse Down

Get informed when the user click on the screen.

```csharp
renderer.MouseDown += (position, screenSize, button) =>
    {
        //your handler
    };
```

### Hook Keyboard Key Down

Get informed when the user click on the keyboard.

```csharp
renderer.KeyDown += (key) =>
    {
        //your handler
    };
```

### Hook Keyboard Key Up

Get informed when the user release a button on the keyboard.

```csharp
renderer.KeyUp += (key) =>
    {
        //your handler
    };
```

### Hook Keyboard Key Press

Get informed when the user enter a single character

```csharp
renderer.KeyPress += (character) =>
    {
        //your handler
    };
```

### Hook Application close

Get informed when someone (ui or direct call) want to close the application. Use this method to release you
handles and save your data. Keep in mind that your call should be done after 2 seconds. Nobody like apps that run in background forever.

```csharp
renderer.Animation.OnClose += () =>
    {
        //your handler
    };
```

## Utilities

### Write to log file

There is a global log file for this application. In this file are all information and exceptions stored.

Log single text:
```csharp
Logger.Log("debug message");
```

Log an exception:
```csharp
Logger.Error(e);
```

The output is automaticly formatted.

### Channels

A channel is a very usefull technique for splitting the ui in different parts. For more information have a look at
[Display Channel](channel.md).

## Extend core

Its very easy to extend the core with own prototypes and animations.

### Add own Prototype

First you need to create your prototype class that inherits at least `Graphix.Prototypes.PrototypeBase`.

```csharp
public class OwnPrototype : Graphix.Prototypes.PrototypeBase
{

}
```

After that you need to register this prototype in the core before any ui script would use it.

```csharp
Graphix.PrototypeLoader.AddDotNetPrototype<OwnPrototype>();
```

For some examples look at the other definitions in `Graphix.Prototypes` (e.g. [Line](../Graphix/Prototypes/Line.cs)).

### Add own Renderer

First you need to create your renderer class that inherits from `Graphix.Rendering.SpecialRenderer`

```csharp
public class OwnRenderer : Graphix.Rendering.SpecialRenderer
{
    public override void Render(Graphix.Rendering.RenderArgs args)
    {

    }
}
```

After that you need to add it to the rendering form.

```csharp
renderer.AddSpecialRenderer<OwnPrototype, OwnRenderer>();
```

### Add own Parameter Type


You can access your own variable types in the xml ui.

```csharp
Graphix.PrototypeLoader.AddParameterType<OwnType>("OwnType",
    (prototypeLoader, prototypeBase, textValue) =>
    {
        //do something with the textValue and create your OwnType
        return new OwnType();
    });
```

After that you can use this type in the ui.

### Add activator

Add your own animation activators for your own conditions. First you need to inherit the class `Graphix.Physic.AnimationActivation`.

```csharp
public class OwnActivation : Graphix.Physic.AnimationActivation
{
    public override XmlNode ToXml(XmlDocument xml, Graphix.PrototypeExporter.Dict dict)
    {
        return xml.createElement("OwnActivation");
    }

    public override IValueWrapper[] GetValueWrapper()
    {
        return new IValueWrapper[0];
    }

    public override void MoveTargets(PrototypeFlattenerHelper helper)
    {
        
    }

    public override AnimationActivation Clone()
    {
        return new OwnActivation();
    }
}
```

After you create your activator definition you can add it to the core so you can use it in the xml ui.

```csharp
Graphix.PrototypeLoader.AddActivator<OwnActivation>("OwnActivation",
    (prototypeLoader, prototypeBase, activator, xmlNode) =>
    {
        //fill your activator with the data from the xmlNode
    });
```

### Add Effect

You can add your own effect that can do something. First you need to inherit `Graphix.Physic.AnimationEffect`.

```csharp
public class OwnEffect : Graphix.Physic.AnimationEffect
{
    public override XmlNode ToXml(XmlDocument xml, Graphix.PrototypeExporter.Dict dict)
    {
        var node = xml.CreateElement("OwnEffect");
        AddParamsToXml(xml, node, dict);
        return node;
    }

    public override IValueWrapper[] GetValueWrapper()
    {
        return new IValueWrapper[]
        {
            TimeStart, TimeOffset, TimeFinish, TimeDuration, Reverse, Repeat, Async, Enable,
            //add here your own values
        };
    }

    public override void StartAnimate(Graphix.Physic.AnimationRuntime runtime)
    {
        //prepair your magic
    }

    public override void Animate(double time)
    {
        //real animation magic
    }

    public override AnimationEffect ProtClone()
    {
        return new OwnEffect();
    }
}
```

When you have finished this you can add your class to the core.

```csharp
Graphix.PrototypeLoader.AddEffect<OwnEffect>("OwnEffect",
    (prototypeLoader, prototypeBase, effect, xmlNode) =>
    {
        //load the data from the xmlNode and put it to the effect
    });
```
