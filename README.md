# TiledUtilities

A set of utilities for working with [Tiled](http://mapeditor.org) maps in Unity.

*Note: This is very much a WIP. Lots of things are yet to be implemented.*

The goal of this is to be an editor tool (no runtime logic) that can import a Tiled TMX map and produce a usable game scene. Since we don't want to ship TMX files, everything is built out into standard Unity objects and the tools attempt to incorporate the best of Tiled with the best of Unity, such as leveraging Unity prefabs while allowing Tiled objects to override properties in the prefabs.

The tool converts maps from Tiled:

![Tiled](https://github.com/UnityCommunity/TiledUtilities/raw/master/Readme_Tiled.png)

Into objects in Unity:

![Tiled](https://github.com/UnityCommunity/TiledUtilities/raw/master/Readme_Unity.png)

## Tileset Tool

![Tileset Tool Window](https://github.com/UnityCommunity/TiledUtilities/raw/master/Readme_TilesetTool.png)

Open with `Window->Tiled->Tileset Tool`.

This tool automatically sets a texture to use multiple sprites and provide a faster way to cut up the texture into the necessary tiles in a way that is compatible with the map converter, mainly by keeping empty sprites so that the GID indexing can be done more easily. This tool is also much faster than the built in Sprite Editor for chopping up large tilesets, possibly because it doesn't check if the sprites are empty.

## Tiled Map

This is a simple script placed on an object in your scene to drive importing. The script itself just holds a string so it's not adding any Tiled logic into your runtime. You can create one easily via the menus:

![Create Tiled Map](https://github.com/UnityCommunity/TiledUtilities/raw/master/Readme_CreateTiledMap.png)

Once created the map component will show an error if there isn't a valid path to a TMX/XML file:

![Inspector Error](https://github.com/UnityCommunity/TiledUtilities/raw/master/Readme_TiledMapError.png)

You can type in a path or use the helpful locate button. Once you have it configured, a button will appear to allow importing:

![Inspector Valid](https://github.com/UnityCommunity/TiledUtilities/raw/master/Readme_TiledMapValid.png)

Here's how the import process works:

1. Iterate all layers in the map. Create a new GameObject for the layer using the Z axis to control layering. Then do type specific logic:
  - For tile layers, generate 1 sprite per tile underneath the layer objects. Blank spaces simply don't generate sprites, thus you can have a large sparse map and it doesn't create tons of unnecessary empty sprites in your scene.
  - For object groups:
    1. Instantiate prefabs for each object by looking for a prefab asset that has a file name equivalent to the `Type` property of the object. In the example above, the door objects have a `Type` of `Door` and so the converter finds the `Door` prefab and instantiates it and places it.
      - Objects that have no type are ignored.
      - Objects with a type that doesn't have a matching prefab generates an error.
    2. Scan all `MonoBehaviour` components on the object. On each of those scripts, it attempts to match each of the properties for the TMX object up to the serialized properties of the script object. If found, it will update the script to the value found in TMX. This allows your TMX files to drive setting values on the prefabs instantiated, such that you can setup properties in Tiled that carry over to Unity, allowing you to centralize level editing in Tiled.
2. Iterate all `MonoBehaviour` components on all created objects and invoke the `OnTmxMapImported` method giving them a chance to finish any initialization, such as establishing references to other created objects, changing their default sprite, or anything else.

## Credits

Example artwork by [Kenney](http://kenney.nl).
