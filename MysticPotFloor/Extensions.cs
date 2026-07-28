using Mirror;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MysticPotFloor
{
    public static class Extensions
    {
        public static void SetAssetId(this NetworkIdentity instance, uint assetId)
        {
            var prop = instance.GetType().GetProperty(nameof(NetworkIdentity.assetId));
            prop.SetValue(instance, assetId);
        }
        public static GameObject SetAssetId(this GameObject gameObject, uint assetId)
        {
            if (gameObject == null)
            {
                Core.LoggerError($"GameObject is null!");
                return gameObject;
            }
            //Core.Logger(gameObject.name + ": " + assetId);
            if (gameObject.TryGetComponent<NetworkIdentity>(out var identity))
            {
                //UnityEngine.Object.Destroy(identity);
                identity.SetAssetId(assetId);
            }
            else
            {
                Core.LoggerError($"{gameObject.name} has no identity");
            }
            if (gameObject.TryGetComponent<ModAssetId>(out var mod))
            {
                Core.LoggerError($"GameObject {gameObject} has already ModAssetId");
                mod.AssetId = assetId;
                return gameObject;
            }
            gameObject.AddComponent<ModAssetId>().AssetId = assetId;
            return gameObject;
        }
        public static uint GetAssetId(this GameObject gameObject)
        {
            if (gameObject == null)
            {
                Core.LoggerError($"GameObject is null!");
                return 0;
            }
            if (gameObject.TryGetComponent<NetworkIdentity>(out var identity))
            {
                return identity.assetId;
            }
            if (gameObject.TryGetComponent<ModAssetId>(out var mod))
            {
                return mod.AssetId;
            }
            return 0;
        }
        public static uint GetAssetId(this Component gameObject)
        {
            if (gameObject == null)
            {
                Core.LoggerError($"GameObject is null!");
                return 0;
            }
            if (gameObject.TryGetComponent<NetworkIdentity>(out var identity))
            {
                return identity.assetId;
            }
            if (gameObject.TryGetComponent<ModAssetId>(out var mod))
            {
                return mod.AssetId;
            }
            return 0;
        }
    }
}
