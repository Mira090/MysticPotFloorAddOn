using HarmonyLib;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static StageEntity_Choice;
using Random = System.Random;

namespace MysticPotFloor
{
    public class Core : HorayModBase
    {
        #region Version
        public static Core Instance { get; private set; }


        [HarmonyPatch(typeof(Application), nameof(Application.version), MethodType.Getter)]
        public static class GameVersionPatch
        {
            static void Postfix(ref string __result)
            {
                __result += ".: " + Core.Instance.metadata.modName + " v" + Core.Instance.metadata.modVersion;
            }
        }
        #endregion


        public static void Logger(string message)
        {
            Debug.Log("[MysticPotFloor] " + message);
        }
        public static void LoggerWarning(string message)
        {
            Debug.LogWarning("[MysticPotFloor] " + message);
        }
        public static void LoggerWarning(System.Exception message)
        {
            Debug.LogWarning("[MysticPotFloor] " + message);
        }
        public static void LoggerError(string message)
        {
            Debug.LogError("[MysticPotFloor] " + message);
        }
        public static void LoggerError(System.Exception message)
        {
            Debug.LogError("[MysticPotFloor] " + message);
        }
        public static uint AssetId { get; private set; } = 4;
        public static GameObject PotObject { get; private set; }

        public static Harmony ModPatches { get; private set; }
        public static bool IsInitialized { get; private set; } = false;
        protected override void OnModLoaded()
        {
            base.OnModLoaded();
            if (!IsInitialized)
            {
                IsInitialized = true;
                Instance = this;

                ModPatches = new Harmony("com.Mira.MysticPotFloor");
                ModPatches.PatchAll();

                HorayModAPI.OnAllDatabasesReady += OnAllDatabasesReady;
            }
        }

        private void OnAllDatabasesReady()
        {
            /*
            Modify(21, race =>//チャプター4
            {
                foreach(var stage in race.stages)
                {
                    if(stage is StageEntity_Choice choice)
                    {
                        if (stage.id == "Stage_Desert_Chapter2")
                            ModifyStageDesert(choice);
                    }
                }
            });*/
            Modify(27, race =>//チャプター5
            {
                foreach (var stage in race.stages)
                {
                    if(stage is StageEntity_Choice choice)
                    {
                        //Core.Logger("Stage: " + stage.id);
                        if (stage.id == "Stage_Desert_Chapter2")
                            ModifyStageDesert(choice);
                        if (stage.id == "Stage_DeepCave")
                            ModifyStageDeepCave(choice);
                    }
                }
            });
        }
        private void ModifyStageDesert(StageEntity_Choice choice)
        {
            Core.Logger("ModifyStageEntity: " + choice.id);
            //Core.Logger("1");
            FloorGenerator inventory = null;
            foreach (var item in choice.uniqueFloorPrefabs)
            {
                if (item.floorMainEventType == EFloorMainEventType.InventoryStorage)
                {
                    inventory = item;
                    break;
                }
            }
            if (inventory == null)
            {
                Core.LoggerError("InventoryStorage FloorGenerator not found!");
                return;
            }
            choice.unknownUniqueEvents = new[] { EFloorMainEventType.Anvil, EFloorMainEventType.MaxHP, EFloorMainEventType.InventoryStorage, (EFloorMainEventType)ECustomFloorMainEventType.MysticPot };

            if (Core.PotObject != null)
            {
                UnityEngine.Object.Destroy(Core.PotObject);
            }
            var pot = UnityEngine.Object.Instantiate(inventory);
            pot.gameObject.SetAssetId(Core.AssetId);

            pot.floorMainEventType = (EFloorMainEventType)ECustomFloorMainEventType.MysticPot;
            pot.gameObject.name = "Desert_Pot";
            Core.PotObject = pot.gameObject;
            Core.PotObject.hideFlags = HideFlags.HideAndDontSave;
            FloorGenerator.FloorGenerators.Remove(pot);

            choice.uniqueFloorPrefabs = choice.uniqueFloorPrefabs.Concat(new[] { pot }).ToArray();
            RaceDatabase.floorGeneratorDictionary[Core.AssetId] = Core.PotObject;
            Core.Logger("ModifyStageEntity: Completed");
        }
        private void ModifyStageDeepCave(StageEntity_Choice choice)
        {
            for (int q = 0; q < choice.unknownNormalEvents.Length; q++)
            {
                if (choice.unknownNormalEvents[q] == EFloorMainEventType.Dice)
                {
                    choice.unknownNormalEvents[q] = (EFloorMainEventType)ECustomFloorMainEventType.MysticPot;
                }
            }
            foreach (var step in choice.steps)
            {
                for (int q = 0; q < step.possibleMainEvents.Length; q++)
                {
                    if (step.possibleMainEvents[q] == EFloorMainEventType.Dice)
                    {
                        step.possibleMainEvents[q] = (EFloorMainEventType)ECustomFloorMainEventType.MysticPot;
                    }
                }
            }
        }

        public static void Modify(int id, Action<RaceEntity> modifier)
        {
            var value = RaceDatabase.FindById(id);
            if (value != null)
            {
                try
                {
                    modifier(value);
                    return;
                }
                catch (Exception arg)
                {
                    Debug.LogError(string.Format("[{0}] Race Modify {1} failed: {2}", "MysticPotFloor", id, arg));
                    return;
                }
            }

            Debug.LogWarning(string.Format("[{0}] Race Modify: id {1} not found", "MysticPotFloor", id));
        }

        protected override void OnModUnloaded()
        {
            IsInitialized = false;

            HorayModAPI.OnAllDatabasesReady -= OnAllDatabasesReady;

            if (ModPatches != null)
            {
                ModPatches.UnpatchSelf();
            }
            base.OnModUnloaded();
        }
        [HarmonyPatch(typeof(LibraryFloorMetadataBaker), "Awake")]
        public static class LibraryFloorMetadataBakerPatch
        {

            static void Postfix()
            {
                var list = LibraryFloorMetadataBaker.Instance.GetPreset("Desert_Common/Event");
                var metadata = new LibraryFloorMetadataBaker.RoomMetadata();
                metadata.name = "16 Desert";
                metadata.monsterDensity = 1;
                metadata.mainLayer = list[5].mainLayer;
                metadata.size = list[5].size;
                metadata.teleportPoint = list[5].teleportPoint;
                metadata.passagePoints = list[5].passagePoints;
                metadata.mainProps = list[5].mainProps.ToArray();
                metadata.mainProps[^1] = new LibraryFloorMetadataBaker.PropMetadata()
                {
                    id = "MysticPot",
                    x = list[5].mainProps[^1].x,
                    y = 7.8f,
                    localScale = list[5].mainProps[^1].localScale,
                    options = list[5].mainProps[^1].options
                };
                list.Add(metadata);
            }
        }
        [HarmonyPatch(typeof(ProceduralFloorMetadataBaker), "Awake")]
        public static class ProceduralFloorMetadataBakerPatch
        {
            static void Postfix()
            {
                var list = ProceduralFloorMetadataBaker.Instance.GetPreset("DeepCaveChapter4_Common/Event");
                var metadata = new ProceduralFloorMetadataBaker.TestRoomMetadata();
                metadata.name = "16 _DeepCave";
                metadata.monsterDensity = 1;
                metadata.mainLayer = list[3].mainLayer;
                metadata.teleportPoint = list[3].teleportPoint;
                metadata.hasPassage = list[3].hasPassage;
                metadata.passageArea = list[3].passageArea;
                metadata.passageLayer = list[3].passageLayer;
                metadata.passageProps = list[3].passageProps;
                metadata.mainProps = list[3].mainProps.ToArray();
                metadata.mainProps[2] = new ProceduralFloorMetadataBaker.TestPropMetadata()
                {
                    id = "MysticPot",
                    x = list[3].mainProps[2].x,
                    y = list[3].mainProps[2].y,
                    localScale = list[3].mainProps[2].localScale,
                    options = list[3].mainProps[2].options
                };
                list.Add(metadata);
            }
        }



        [HarmonyPatch(typeof(Resources), nameof(Resources.LoadAll), new Type[] { typeof(string), typeof(Type) })]
        public static class ResourcesLoadAllPatch
        {
            static void Postfix(string path, Type systemTypeInstance, ref UnityEngine.Object[] __result)
            {
                //Core.Logger("Postfix: (" + systemTypeInstance.ToString() + ") " + path);
                if (systemTypeInstance == typeof(KeywordEntity) && path == "Keyword")
                {
                    var list = __result.ToList();


                    var mysticPot = ScriptableObject.CreateInstance<KeywordEntity>();
                    mysticPot.name = "Z_Node_Event_MysticPot";
                    mysticPot.keyword = "Node_Event_16";
                    mysticPot.visualText = new LocalizedString("TutorialMessageBoxTitle_MysticPot");
                    mysticPot.description = new LocalizedString("UI_WorldMap_Event_StoneTablet_Description");
                    mysticPot.detailedValue = new LocalizedString();
                    mysticPot.displayDetails = false;
                    foreach (var item in list)
                        if (item is KeywordEntity entity)
                        {
                            if (entity.keyword == "Node_Event_Enchant")
                            {
                                mysticPot.resourcePrefab = UnityEngine.Object.Instantiate(entity.resourcePrefab);
                                if (mysticPot.resourcePrefab.TryGetComponent<Image>(out var image))
                                {
                                    image.sprite = AssetLoader.LoadSprite("UI\\MysticPotIcon") ?? entity.keywordImage;
                                    mysticPot.keywordImage = image.sprite;
                                }
                            }
                        }

                    Core.Logger("New Keyword: " + mysticPot.visualText.ToString());
                    list.Add(mysticPot);


                    __result = list.ToArray();
                }
            }
        }
    }
}
