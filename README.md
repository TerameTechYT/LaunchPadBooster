# LaunchPadBooster

A utility library for easier development of Stationeers mods.

This library is automatically included in StationeersLaunchPad starting in v0.2.0.

## Usage

Most features are accessed through the `Mod` class. Your mod should create one static instance of this and use it for registering things to the game.

```cs
using LaunchPadBooster;
using System.Collections.Generic;
using UnityEngine;

public class MyMod : MonoBehaviour
{
  public static readonly Mod MOD = new("MyMod", "1.0.0");

  public void OnLoaded(List<GameObject> prefabs)
  {
    // setup your mod
  }
}
```

### Prefabs

```cs
public void OnLoaded(List<GameObject> prefabs)
{
  MOD.AddPrefabs(prefabs); // this will register your prefabs so they show up in the game
  // Add setup handlers to run when the full game data is available
  MOD.SetupPrefabs() // run on all prefabs
    .SetBlueprintMaterials(); // fill in blueprint materials to match builtin blueprints
  MOD.SetupPrefabs("MyPrefabName") // filter to a specific prefab name
    .SetPaintableColor(ColorType.White); // replace the paintable material and all uses of it with a game color
  MOD.SetupPrefabs<MyPrefabType>() // filter to a prefab type
    .SetExitTool(PrefabNames.Drill) // set the deconstruct tool
    .SetEntry2Tool(PrefabNames.IronSheets, index: 1) // chain multiple handlers for this setup filter
    .RunFunc(myPrefab => { // run a custom setup function during prefab registration
      // do something
    })
    .IgnoreEmpty(); // by default a warning message will print if this setup filter didn't match any prefabs
}
```

### SaveData

If your prefabs have their own SaveData type (instead of using an existing SaveData type from the game), it needs to be added to the list of types.

```cs
MOD.AddSaveDataType<MyCustomSaveData>();
```

### Network Messages

To add custom network messages, extend `ModNetworkMessage`.
```cs
public class MyCustomMessage : ModNetworkMessage<MyCustomMessage>
{
  // must have a default constructor
  public MyCustomMessage() { }

  public override void Serialize(RocketBinaryWriter writer)
  {
    // write data
  }

  public override void Deserialize(RocketBinaryReader reader)
  {
    // read data
  }

  public override void Process(long hostId)
  {
    // handle message
  }
}
```

These message types must be registered with your mod.
```cs
MOD.RegisterNetworkMessage<MyCustomMessage>();
```

### Multiplayer

If your mod adds prefabs or network messages, it will automatically be marked as required for multiplayer. If you don't have either of these, but still make changes that will require both server and client to have the mod installed, you can manually mark it as required.
```cs
MOD.SetMultiplayerRequired();
```

On connection, the client and server will check that the required mods are present and compatible. The default check requires the version strings to match exactly. If you want to allow a looser version requirement, you can set the version check function.
```cs
MOD.SetVersionCheck(version => version.StartsWith("0.2."));
```

### Utilities

`PrefabNames` has constants for the names of common tools and construction materials for easy use in prefab setup.

`PrefabUtils` has a set of methods for finding and setting up prefabs
```cs
// get a builtin color material
var material = PrefabUtils.GetColorMaterial(ColorType.Orange);

// Extension helper methods
// Replace all instances of the given material in this prefab with a builtin color material
myThing.ReplaceMaterialWithColor(myPrefab.MyMainMaterial, ColorType.White);
// Set deconstruct tool
myStructure.SetExitTool(PrefabNames.Drill); // final deconstruct tool
myStructure.SetExitTool(PrefabNames.Grinder, 1); // deconstruct from buildstate 1 to 0
// Set build tools for second build state (index 1)
myStructure.SetEntryTool(PrefabNames.Screwdriver, 1);
myStructure.SetEntry2Tool(PrefabNames.IronSheets, 1);

// Lookup existing prefabs to copy some parts of
// These should only be used during setup. While the game is running use Prefab.Find instead
var kit = PrefabUtils.Find<MultiConstructor>("ItemKitLogicSwitch");
kit.Constructables.Add(myPrefab);
```

`ReflectionUtils` has a set of methods for easily getting reflection objects using lambdas
```cs
// MethodInfo helpers
var method = ReflectionUtils.Method(() => default(MyType).MyMethod()); // instance method
var staticMethod = ReflectionUtils.Method(() => MyType.MyStaticMethod()); // static method
var getter = ReflectionUtils.PropertyGetter(() => default(MyType).Property); // property get function
var setter = ReflectionUtils.PropertySetter(() => default(MyType).Property); // property set function
var addOperator = ReflectionUtils.Operator(() => default(MyType) + default(MyType)); // operator function
var castOperator = ReflectionUtils.Operator(() => (MyType2)default(MyType)); // cast function

// FieldInfo helpers
var field = ReflectionUtils.Field(() => default(MyType).Field); // instance field
var staticField = ReflectionUtils.Field(() => MyType.StaticField); // static field

// The MethodInfo for an async method will just set up an object to hold the state machine state
// AsyncMethod returns the MoveNext function of the state machine that contains the actual async method body
var asyncMethod = ReflectionUtils.AsyncMethod(() => default(MyType).MyAsyncMethod());
```