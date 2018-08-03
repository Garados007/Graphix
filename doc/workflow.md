# How does it work?

```text
+---------+-------------------------------------+
| DirectX |                                     |
+---------+--------+        .NET Framework      |
|     SharpDX      |                            |
+------------------+------------------+         |
|            Graphix Core             |         |
+------------------+------------------+---------+
|      XML UI      |         Game Logic         |
+------------------+------------------+---------+
```

The Graphix Core use the DirectX 11 Engine for Windows to render its graphics. To make DX compatible with the .NET
Framework SharpDX is used. The Graphix Core take all the complexity of rendering, file management, ui and animation and create
a simpel api to access it.

The [XML UI](ui/index.md) is a specific and strong language that is responsible for the user interface.
The Graphix Core loads the data and prepair the interface. If some ressources are needed then these are loaded, cached
and managed automaticly without any performance loss. With this language it is very easy to create very complex
user interfaces.

With the animation system its possible do animate everything and everytime on the ui.

# Whats are States?

```text
+---------+        +------+    +----------+    +--------+
| Loading |------->| Main |--->|   Game   |--->|  Game  |--+
| Screen  |   +--->| Menu |    | Selector |    | Screen |  |
+---------+   |    +------+    +----------+    +--------+  |
              +--------------------------------------------+
```

Many Games have different states like in the grafic above. Often these states can split in many sub states.
In Graphix all these states can [represented](ui/status.md) and interacted with animations.