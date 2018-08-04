# Display Channels

Display channels are a very usefull technique. Normaly you can edit the ui tree only once - in the loading phase. 
After that many errors will ocur caused on the implementation of the rendering and animation system.

A single display channel contains all the rendering and animation data and these can easily be swapped. 
So its possible to manipulate unloaded display channels and active it when needed.
With these technique its possible to create a custom loading screen and split the ui in parts to optimize rendering.

A single channel contains:
- ui object tree
- all animation groups
- running animation groups
- states

An unloaded channel wouldn't animated and therefore it cannot react on events.

## Create new Channel

```csharp
var channel = new Graphix.Channel();
channel.Name = "MyChannel";
```

## Load ui in the channel

```csharp
channel.Import(prototypeLoader);
```

## Set channel as active one

```csharp
renderer.Animation.Channel = channel;
```

## Register a channel in the core

With the registration it is possible for the ui to change to this channel.

To set a channel as an active one a registration is not required.

```csharp
renderer.Animation.Channels.Add(channel);
```

## More

For more functions with the channel just have a look at [Graphix.DisplayChannel](../Graphix/DisplayChannel.cs).