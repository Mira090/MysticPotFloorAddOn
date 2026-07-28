using HarmonyLib;
using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MysticPotFloor
{
    public class ModAssetId : MonoBehaviour
    {
        public static readonly Dictionary<uint, GameObject> CustomNetworkPrefabs = new Dictionary<uint, GameObject>();
        public uint AssetId
        {
            get => _assetId;
            internal set
            {
                if (CustomNetworkPrefabs.ContainsKey(value))
                    CustomNetworkPrefabs.Remove(value);
                _assetId = value;
                CustomNetworkPrefabs[_assetId] = gameObject;
            }
        }
        [SerializeField]
        private uint _assetId;
        public void ToIdentity()
        {
            var identity = gameObject.AddComponent<NetworkIdentity>();
            identity.SetAssetId(AssetId);

            if (gameObject.TryGetComponent<Charm_Basic>(out var charm))
            {
                charm.enabled = true;
            }
            if (gameObject.TryGetComponent<StoneTablet>(out var tablet))
            {
                tablet.enabled = true;
            }
            if (gameObject.TryGetComponent<ComboEffectBase>(out var combo))
            {
                combo.enabled = true;
            }
            if (gameObject.TryGetComponent<Miracle>(out var miracle))
            {
                miracle.enabled = true;
            }
            if (gameObject.TryGetComponent<PassiveObject>(out var perk))
            {
                perk.enabled = true;
            }
            if (gameObject.TryGetComponent<Sephirite>(out var sephirite))
            {
                sephirite.enabled = true;
            }

            UnityEngine.Object.Destroy(this);
        }


        [HarmonyPatch(typeof(UnityEngine.Object))]
        public static class ObjectInstantiatePatch
        {
            [HarmonyPatch("Internal_CloneSingle")]
            [HarmonyPrefix]
            static void Prefix0(UnityEngine.Object data)
            {
                PrePatch(data);
            }
            [HarmonyPatch(nameof(UnityEngine.Object.Instantiate), new Type[] { typeof(UnityEngine.Object), typeof(Vector3), typeof(Quaternion) })]
            [HarmonyPrefix]
            static void Prefix1(UnityEngine.Object original)
            {
                //Core.Logger(original.name + ": Patch");
                PrePatch(original);
            }
            [HarmonyPatch(nameof(UnityEngine.Object.Instantiate), new Type[] { typeof(UnityEngine.Object), typeof(Vector3), typeof(Quaternion), typeof(Transform) })]
            [HarmonyPrefix]
            static void Prefix2(UnityEngine.Object original)
            {
                PrePatch(original);
            }
            [HarmonyPatch(nameof(UnityEngine.Object.Instantiate), new Type[] { typeof(UnityEngine.Object) })]
            [HarmonyPrefix]
            static void Prefix3(UnityEngine.Object original)
            {
                PrePatch(original);
            }
            [HarmonyPatch(nameof(UnityEngine.Object.Instantiate), new Type[] { typeof(UnityEngine.Object), typeof(Transform), typeof(bool) })]
            [HarmonyPrefix]
            static void Prefix4(UnityEngine.Object original)
            {
                PrePatch(original);
            }
            [HarmonyPatch(nameof(UnityEngine.Object.Instantiate), new Type[] { typeof(UnityEngine.Object), typeof(Scene) })]
            [HarmonyPrefix]
            static void Prefix5(UnityEngine.Object original)
            {
                PrePatch(original);
            }

            [HarmonyPatch("Internal_CloneSingle")]
            [HarmonyPostfix]
            static void Postfix0(ref UnityEngine.Object __result, UnityEngine.Object data)
            {
                PostPatch(ref __result, data);
            }
            [HarmonyPatch(nameof(UnityEngine.Object.Instantiate), new Type[] { typeof(UnityEngine.Object), typeof(Vector3), typeof(Quaternion) })]
            [HarmonyPostfix]
            static void Postfix1(ref UnityEngine.Object __result, UnityEngine.Object original)
            {
                //Core.Logger(original.name + ": Patch");
                PostPatch(ref __result, original);
            }
            [HarmonyPatch(nameof(UnityEngine.Object.Instantiate), new Type[] { typeof(UnityEngine.Object), typeof(Vector3), typeof(Quaternion), typeof(Transform) })]
            [HarmonyPostfix]
            static void Postfix2(ref UnityEngine.Object __result, UnityEngine.Object original)
            {
                PostPatch(ref __result, original);
            }
            [HarmonyPatch(nameof(UnityEngine.Object.Instantiate), new Type[] { typeof(UnityEngine.Object) })]
            [HarmonyPostfix]
            static void Postfix3(ref UnityEngine.Object __result, UnityEngine.Object original)
            {
                PostPatch(ref __result, original);
            }
            [HarmonyPatch(nameof(UnityEngine.Object.Instantiate), new Type[] { typeof(UnityEngine.Object), typeof(Transform), typeof(bool) })]
            [HarmonyPostfix]
            static void Postfix4(ref UnityEngine.Object __result, UnityEngine.Object original)
            {
                PostPatch(ref __result, original);
            }
            [HarmonyPatch(nameof(UnityEngine.Object.Instantiate), new Type[] { typeof(UnityEngine.Object), typeof(Scene) })]
            [HarmonyPostfix]
            static void Postfix5(ref UnityEngine.Object __result, UnityEngine.Object original)
            {
                PostPatch(ref __result, original);
            }
            static void PrePatch(UnityEngine.Object original)
            {
                if (original == null)
                    return;
                if (original is GameObject gameObject && gameObject.TryGetComponent<ModAssetId>(out var _) && gameObject.TryGetComponent<NetworkIdentity>(out var identity))
                {
                    Core.Logger("PrePatching...");
                    UnityEngine.Object.DestroyImmediate(identity);
                    //Core.Logger("result1: " + gameObject.TryGetComponent<NetworkIdentity>(out var _));
                }
                else if (original is MonoBehaviour behaviour && behaviour.gameObject.TryGetComponent<ModAssetId>(out var _) && behaviour.gameObject.TryGetComponent<NetworkIdentity>(out var identity2))
                {
                    Core.Logger("PrePatching...");
                    UnityEngine.Object.DestroyImmediate(identity2);
                    //Core.Logger("result1: " + behaviour.gameObject.TryGetComponent<NetworkIdentity>(out var _));
                }
            }
            static void PostPatch(ref UnityEngine.Object __result, UnityEngine.Object original)
            {
                if (original == null || __result == null)
                    return;
                if (__result is GameObject gameObject && gameObject.TryGetComponent<ModAssetId>(out var mod))
                {
                    Core.Logger("PostPatching... " + (gameObject.TryGetComponent<NetworkIdentity>(out var _) ? "?" : "OK"));
                    mod.ToIdentity();
                }
                else if (__result is MonoBehaviour behaviour && behaviour.gameObject.TryGetComponent<ModAssetId>(out var mod2))
                {
                    Core.Logger("PostPatching... " + (behaviour.gameObject.TryGetComponent<NetworkIdentity>(out var _) ? "?" : "OK"));
                    mod2.ToIdentity();
                }

                if (original is GameObject gameObject2 && gameObject2.TryGetComponent<ModAssetId>(out var mod3) && !gameObject2.TryGetComponent<NetworkIdentity>(out var _))
                {
                    gameObject2.AddComponent<NetworkIdentity>().SetAssetId(mod3.AssetId);
                    Core.Logger("Patched");
                }
                else if (original is MonoBehaviour behaviour2 && behaviour2.gameObject.TryGetComponent<ModAssetId>(out var mod4) && !behaviour2.gameObject.TryGetComponent<NetworkIdentity>(out var _))
                {
                    behaviour2.gameObject.AddComponent<NetworkIdentity>().SetAssetId(mod4.AssetId);
                    Core.Logger("Patched");
                }
            }
        }
        [HarmonyPatch(typeof(NetworkClient), nameof(NetworkClient.GetPrefab))]
        public static class NetworkClientGetPrefabPatch
        {
            static void Postfix(uint assetId, ref GameObject prefab, ref bool __result)
            {
                if (!ModAssetId.CustomNetworkPrefabs.ContainsKey(assetId))
                    return;
                prefab = ModAssetId.CustomNetworkPrefabs[assetId];
                __result = prefab != null;
            }
        }
    }
}
