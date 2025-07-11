using Assets.Scripts;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Motherboards;
using UnityEngine;

namespace LaunchPadBooster.Utils
{
  public static class PrefabUtils
  {
    public static Material GetColorMaterial(ColorType color, bool emissive = false)
    {
      var swatch = GameManager.GetColorSwatch((int)color);
      return emissive ? swatch.Emissive : swatch.Normal;
    }

    // Replaces the given material with the builtin color swatch material on all renderers
    public static void ReplaceMaterialWithColor(this Thing thing, Material material, ColorType color, bool emissive = false)
    {
      var replaceMat = GetColorMaterial(color, emissive);

      if (thing.PaintableMaterial == material)
        thing.PaintableMaterial = replaceMat;

      foreach (var renderer in thing.GetComponentsInChildren<MeshRenderer>(true))
      {
        var mats = renderer.sharedMaterials;
        for (var i = 0; i < mats.Length; i++)
        {
          if (mats[i] == material)
            mats[i] = replaceMat;
        }
        renderer.sharedMaterials = mats;
      }
    }

    public static bool CanSetBuildTool(Thing thing, string toolName, int index, out Structure structure, out Item tool)
    {
      structure = thing as Structure;
      if (structure == null)
      {
        tool = null;
        Debug.LogWarning($"Cannot set build tool on non-structure prefab {thing.PrefabName}");
        return false;
      }
      if (index < 0 || index >= structure.BuildStates.Count)
      {
        Debug.LogWarning($"Invalid build state index {index} for prefab {structure.PrefabName}");
        tool = null;
        return false;
      }
      tool = Prefab.Find<Item>(toolName);
      if (tool == null)
      {
        Debug.LogWarning($"Could not find build tool {toolName} for prefab {structure.PrefabName}");
        return false;
      }
      return true;
    }

    public static void SetEntryTool(this Structure structure, string toolName, int index = 0)
    {
      if (!CanSetBuildTool(structure, toolName, index, out _, out var tool))
        return;
      structure.BuildStates[index].Tool.ToolEntry = tool;
    }

    public static void SetEntry2Tool(this Structure structure, string toolName, int index = 0)
    {
      if (!CanSetBuildTool(structure, toolName, index, out _, out var tool))
        return;
      structure.BuildStates[index].Tool.ToolEntry = tool;
    }

    public static void SetExitTool(this Structure structure, string toolName, int index = 0)
    {
      if (!CanSetBuildTool(structure, toolName, index, out _, out var tool))
        return;
      structure.BuildStates[index].Tool.ToolEntry = tool;
    }

    // This should only be used prior to prefab load in order to set references
    // Use Prefab.Find<T> after load
    public static T FindPrefab<T>(string prefabName) where T : Thing => FindPrefab<T>(Animator.StringToHash(prefabName));
    public static Thing FindPrefab(string prefabName) => FindPrefab<Thing>(prefabName);

    public static T FindPrefab<T>(int prefabHash) where T : Thing
    {
      var prefab = WorldManager.Instance.SourcePrefabs.Find(prefab => prefab.PrefabHash == prefabHash);
      return prefab as T;
    }
    public static Thing FindPrefab(int prefabHash) => FindPrefab<Thing>(prefabHash);

    private static Material _blueprintMaterial;
    public static Material GetBlueprintMaterial()
    {
      if (_blueprintMaterial == null)
      {
        foreach (var prefab in WorldManager.Instance.SourcePrefabs)
        {
          if (prefab.Blueprint == null)
            continue;
          if (!prefab.Blueprint.TryGetComponent<MeshRenderer>(out var renderer))
            continue;
          _blueprintMaterial = renderer.sharedMaterials[0];
          break;
        }
      }
      return _blueprintMaterial;
    }
  }
}