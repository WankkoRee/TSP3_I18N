using System;

using HarmonyLib;

using UnityEngine;

namespace TSP3_I18N.Patch
{
    class Util
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(AssetBundle))]
        [HarmonyPatch("LoadAsset_Internal")]
        [HarmonyPatch(new Type[] { typeof(string), typeof(Type) })]
        static void AssetBundle_LoadAsset_Internal_Patch(string name, Type type, AssetBundle __instance)
        {
            Plugin.Log.LogDebug($"AssetBundle.LoadAsset_Internal( {name}, {type} )");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AssetBundle))]
        [HarmonyPatch("LoadAssetAsync_Internal")]
        [HarmonyPatch(new Type[] { typeof(string), typeof(Type) })]
        static void AssetBundle_LoadAssetAsync_Internal_Patch(string name, Type type, AssetBundle __instance)
        {
            Plugin.Log.LogDebug($"AssetBundle.LoadAssetAsync_Internal( {name}, {type} )");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AssetBundle))]
        [HarmonyPatch("LoadAssetWithSubAssets_Internal")]
        [HarmonyPatch(new Type[] { typeof(string), typeof(Type) })]
        static void AssetBundle_LoadAssetWithSubAssets_Internal_Patch(string name, Type type, AssetBundle __instance)
        {
            Plugin.Log.LogDebug($"AssetBundle.LoadAssetWithSubAssets_Internal( {name}, {type} )");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AssetBundle))]
        [HarmonyPatch("LoadAssetWithSubAssetsAsync_Internal")]
        [HarmonyPatch(new Type[] { typeof(string), typeof(Type) })]
        static void AssetBundle_LoadAssetWithSubAssetsAsync_Internal_Patch(string name, Type type, AssetBundle __instance)
        {
            Plugin.Log.LogDebug($"AssetBundle.LoadAssetWithSubAssetsAsync_Internal( {name}, {type} )");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AssetBundle))]
        [HarmonyPatch("LoadFromFile_Internal")]
        [HarmonyPatch(new Type[] { typeof(string), typeof(uint), typeof(ulong) })]
        static void AssetBundle_LoadFromFile_Internal_Patch(string path, uint crc, ulong offset, AssetBundle __instance)
        {
            Plugin.Log.LogDebug($"AssetBundle.LoadFromFile_Internal( {path}, {crc}, {offset} )");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AssetBundle))]
        [HarmonyPatch("LoadFromFileAsync_Internal")]
        [HarmonyPatch(new Type[] { typeof(string), typeof(uint), typeof(ulong) })]
        static void AssetBundle_LoadFromFileAsync_Internal_Patch(string path, uint crc, ulong offset, AssetBundle __instance)
        {
            Plugin.Log.LogDebug($"AssetBundle.LoadFromFileAsync_Internal( {path}, {crc}, {offset} )");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AssetBundle))]
        [HarmonyPatch("LoadFromMemory_Internal")]
        [HarmonyPatch(new Type[] { typeof(byte[]), typeof(uint) })]
        static void AssetBundle_LoadFromMemory_Internal_Patch(byte[] binary, uint crc, AssetBundle __instance)
        {
            Plugin.Log.LogDebug($"AssetBundle.LoadFromMemory_Internal( {binary}.Length={binary.Length}, {crc} )");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AssetBundle))]
        [HarmonyPatch("LoadFromMemoryAsync_Internal")]
        [HarmonyPatch(new Type[] { typeof(byte[]), typeof(uint) })]
        static void AssetBundle_LoadFromMemoryAsync_Internal_Patch(byte[] binary, uint crc, AssetBundle __instance)
        {
            Plugin.Log.LogDebug($"AssetBundle.LoadFromMemoryAsync_Internal( {binary}.Length={binary.Length}, {crc} )");
        }
    }
}
