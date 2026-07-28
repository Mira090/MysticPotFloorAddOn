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
        [Obsolete]
        public static GameObject OriginalObject { get; private set; }
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

                HorayModAPI.OnFloorAllocatedServerside += OnFloorAllocated;
                HorayModAPI.OnFloorAllocatedClientside += OnFloorAllocated;
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

            //Core.Logger("2");
            //Core.OriginalObject = inventory.gameObject;
            //Core.Logger("3");
            if (Core.PotObject != null)
            {
                UnityEngine.Object.Destroy(Core.PotObject);
            }
            var pot = UnityEngine.Object.Instantiate(inventory);
            //Core.Logger("4");
            pot.gameObject.SetAssetId(Core.AssetId);
            //Core.Logger("5");

            //Core.Logger("8");
            pot.floorMainEventType = (EFloorMainEventType)ECustomFloorMainEventType.MysticPot;
            pot.gameObject.name = "Desert_Pot";
            //Core.Logger("9");
            Core.PotObject = pot.gameObject;
            //Core.PotObject.AddComponent<LogComponent>();
            Core.PotObject.hideFlags = HideFlags.HideAndDontSave;
            FloorGenerator.FloorGenerators.Remove(pot);

            //Core.Logger("10");
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

        private void OnFloorAllocated(string guid, string floorName, FloorGenerator generator)
        {
            //特になし
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

            HorayModAPI.OnFloorAllocatedServerside -= OnFloorAllocated;
            HorayModAPI.OnFloorAllocatedClientside -= OnFloorAllocated;
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


        //[HarmonyPatch(typeof(DungeonManager), nameof(DungeonManager.FloorAlloc))]
        [Obsolete]
        public static class FloorAllocPatch
        {
            static bool Prefix(DungeonManager __instance, string guid)
            {
                FloorData floor;
                if (!__instance.generatedFloors.TryGetValue(guid, out floor))
                {
                    Debug.LogError(" floor alloc 실패. (guid : " + guid + ")");
                    return true;
                }
                if (floor.prefabAssetId != Core.AssetId)
                    return true;
                using (List<FloorGenerator>.Enumerator enumerator = FloorGenerator.FloorGenerators.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current.guid == guid)
                        {
                            return true;
                        }
                    }
                }
                int pdType = 0;
                if (SaveManager.Current.GetInt("DeathCount", 0) > 5 && !SaveManager.CurrentRun.GetBool("PocketDimensionMet", false))
                {
                    double num = new Random(floor.seed + 1).NextDouble() * 100.0;
                    int valueOrDefault = __instance.dungeonEnvironment.GetValueOrDefault("PocketDimensionPercent", 1);
                    if (num < (double)valueOrDefault)
                    {
                        pdType = 1;
                    }
                }
                int luckyType = 0;
                int num2 = 0;
                foreach (PlayerSpawner playerSpawner in PlayerSpawner.MultiplayerList)
                {
                    num2 = Mathf.Max(num2, playerSpawner.PlayerAvatar.GetCustomStat(ECustomStat.Luck));
                }
                num2 = Mathf.Min(num2, 20);
                double num3 = 4.0 + (double)num2 * 0.05;
                if (new Random(floor.seed).NextDouble() * 100.0 < num3)
                {
                    luckyType = 1;
                }
                GameObject gameObject = UnityEngine.Object.Instantiate(Core.OriginalObject, new Vector3((float)(floor.globalX * 500), (float)(floor.globalY * 500)), Quaternion.identity);
                FloorGenerator component = gameObject.GetComponent<FloorGenerator>();
                component.floorMainEventType = (EFloorMainEventType)ECustomFloorMainEventType.MysticPot;
                gameObject.name = Core.PotObject.name + "(Clone)";
                component.Connect(floor, floor.seed, luckyType, pdType);
                NetworkServer.Spawn(gameObject);
                component.NetworkrequestGeneration = true;
                return false;
            }
        }
        //[HarmonyPatch(typeof(UI_NewWorldMapPanel), "CreateFloorElements")]
        [Obsolete]
        public static class CreateFloorElementsPatch
        {
            static bool Prefix(UI_NewWorldMapPanel __instance, ref UI_WorldMapStageElement __result, FloorData floorNode, RectTransform parent, float x, float y)
            {
                if (floorNode.prefabAssetId != Core.AssetId)
                    return true;
                //Melon<Core>.Logger.Msg("Panel: 1");
                FloorGenerator potGenerator = Core.PotObject.GetComponent<FloorGenerator>();
                //Melon<Core>.Logger.Msg("Panel: 2");
                UI_WorldMapStageElement element = UnityEngine.Object.Instantiate<GameObject>(potGenerator.GetFloorNodePrefab(floorNode.seed), parent).GetComponent<UI_WorldMapStageElement>();
                //Melon<Core>.Logger.Msg("Panel: 3");
                element.SetFloor(floorNode, potGenerator);
                //Melon<Core>.Logger.Msg("Panel: 4");
                element.rectTransform.anchoredPosition = new Vector2(x, y);
                //Melon<Core>.Logger.Msg("Panel: 5");
                element.OnClick += (Action<UI_WorldMapStageElement>)(typeof(UI_NewWorldMapPanel)).GetMethod("HandleStageElementClicked", BindingFlags.Instance | BindingFlags.NonPublic).CreateDelegate(typeof(Action<UI_WorldMapStageElement>), __instance);
                __result = element;
                //Melon<Core>.Logger.Msg("CreateFloorElements");
                return false;
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
        [HarmonyPatch(typeof(StageEntity_Choice), nameof(StageEntity_Choice.GenerateStage))]
        public static class StageEntityChoicePatch
        {
            private static bool Prefix(StageEntity_Choice __instance, FloorData[] __result, DungeonManager dungeon, int seed, int stageOrder, bool createMiracle, bool createAnvil, bool enhancedDisturbance, string bossGenerateParameter, int hardModeAllBoss)
            {
                Core.Logger("GenerateStage: " + __instance.name + ", seed: " + seed + ", stageOrder: " + stageOrder);

                return true;
                //if(__instance.firstFloor.TryGetComponent<NetworkIdentity>(out var _))
                    //return true;
                Core.Logger("GenerateStage: " + __instance.name + ", seed: " + seed + ", stageOrder: " + stageOrder);

                List<FloorData> list = new List<FloorData>();
                List<FloorData> list2 = new List<FloorData>();
                Dictionary<EFloorThreatType, List<FloorGenerator>> dictionary = new Dictionary<EFloorThreatType, List<FloorGenerator>>();
                Dictionary<EFloorThreatType, Dictionary<EFloorMainEventType, List<FloorGenerator>>> dictionary2 = new Dictionary<EFloorThreatType, Dictionary<EFloorMainEventType, List<FloorGenerator>>>();
                foreach (EFloorThreatType value2 in Enum.GetValues(typeof(EFloorThreatType)))
                {
                    dictionary[value2] = new List<FloorGenerator>();
                    dictionary2[value2] = new Dictionary<EFloorMainEventType, List<FloorGenerator>>();
                    foreach (EFloorMainEventType value3 in Enum.GetValues(typeof(EFloorMainEventType)))
                    {
                        dictionary2[value2][value3] = new List<FloorGenerator>();
                    }
                }

                List<FloorGenerator> list3 = new List<FloorGenerator>();
                GameObject[] array = __instance.randomEventFloorPrefabs;
                for (int i = 0; i < array.Length; i++)
                {
                    FloorGenerator component = array[i].GetComponent<FloorGenerator>();
                    dictionary[component.floorThreatType].Add(component);
                }

                array = __instance.fixedEventFloorPrefabs;
                for (int i = 0; i < array.Length; i++)
                {
                    FloorGenerator component2 = array[i].GetComponent<FloorGenerator>();
                    dictionary2[component2.floorThreatType][component2.floorMainEventType].Add(component2);
                    if (component2.floorThreatType == EFloorThreatType.Boss)
                    {
                        list3.Add(component2);
                    }
                }

                int num = 0;
                System.Random random = new System.Random(seed);
                System.Random sysRand = new System.Random(seed);
                System.Random random2 = new System.Random(seed);
                System.Random random3 = new System.Random(seed);
                System.Random sysRand2 = new System.Random(seed);
                System.Random random4 = new System.Random(seed + 10);
                System.Random random5 = new System.Random(seed + 20);
                FloorData item = new FloorData
                {
                    guid = Guid.NewGuid().ToString(),
                    name = __instance.firstFloor.name,
                    stageName = __instance.name,
                    seed = random2.Next(),
                    globalY = stageOrder,
                    globalX = num++,
                    nodeProgress = 0,
                    prefabAssetId = __instance.firstFloor.GetAssetId(),
                    hasTraveler = false,
                    difficulty = __instance.firstDifficulty,
                    mainEventType = __instance.firstFloor.floorMainEventType,
                    threatType = __instance.firstFloor.floorThreatType,
                    disturbance = ""
                };
                List<FloorData> list4 = new List<FloorData> { item };
                bool flag = false;
                bool flag2 = false;
                foreach (PlayerSpawner multiplayer in PlayerSpawner.MultiplayerList)
                {
                    flag = flag || multiplayer.PlayerAvatar.GetCustomStat(ECustomStat.TABLET) > 0;
                    if (multiplayer.PlayerAvatar.maxRerollDice > 0)
                    {
                        flag2 = true;
                    }
                }

                List<EFloorMainEventType> list5 = new List<EFloorMainEventType>();
                EFloorMainEventType[] array2 = __instance.unknownNormalEvents;
                foreach (EFloorMainEventType eFloorMainEventType in array2)
                {
                    switch (eFloorMainEventType)
                    {
                        case EFloorMainEventType.StoneTablet:
                        case EFloorMainEventType.Enchant:
                            if (!flag)
                            {
                                Debug.Log("표준 이벤트 조정 : 석판&인챈트 노드는 첫번째 죽음 컷씬 이후 나와야 하므로 넘어감");
                                continue;
                            }

                            break;
                        case EFloorMainEventType.Dice:
                            if (!flag2)
                            {
                                Debug.Log("표준 이벤트 조정 : 주사위 노드는 리롤 운명각인 이후 나와야 하므로 넘어감");
                                continue;
                            }

                            break;
                    }

                    list5.Add(eFloorMainEventType);
                }

                Dictionary<EFloorMainEventType, List<FloorGenerator>> dictionary3 = new Dictionary<EFloorMainEventType, List<FloorGenerator>>();
                FloorGenerator[] array3 = __instance.uniqueFloorPrefabs;
                foreach (FloorGenerator floorGenerator in array3)
                {
                    if (!dictionary3.ContainsKey(floorGenerator.floorMainEventType))
                    {
                        dictionary3[floorGenerator.floorMainEventType] = new List<FloorGenerator>();
                    }

                    dictionary3[floorGenerator.floorMainEventType].Add(floorGenerator);
                }

                List<FloorData> list6 = new List<FloorData>();
                List<FloorData> list7 = new List<FloorData>();
                List<FloorData> list8 = new List<FloorData>();
                List<FloorData>[] array4 = new List<FloorData>[__instance.steps.Length + 1];
                for (int j = 0; j < array4.Length; j++)
                {
                    array4[j] = new List<FloorData>();
                }

                array4[0].Add(item);
                for (int k = 0; k < __instance.steps.Length; k++)
                {
                    Debug.Log("Create Step : " + k);
                    bool flag3 = false;
                    for (int l = 0; l < __instance.steps[k].matchEvents.Length; l++)
                    {
                        if (__instance.steps[k].matchEvents[l].threat == EFloorThreatType.Boss)
                        {
                            flag3 = true;
                            break;
                        }
                    }

                    int num2 = __instance.steps.Length - 1;
                    if (num2 <= 0)
                    {
                        num2 = 1;
                    }

                    float num3 = (float)k / (float)num2;
                    int difficulty = Mathf.CeilToInt((float)(__instance.lastDifficulty - 1 - __instance.firstDifficulty) * num3 + (float)__instance.firstDifficulty) + 1;
                    int num4 = random.Next(__instance.steps[k].choiceMin, __instance.steps[k].choiceMax + 1);
                    int num5 = 1;
                    bool flag4 = false;
                    if (flag3)
                    {
                        string[] array5 = bossGenerateParameter.Split(',');
                        for (int i = 0; i < array5.Length; i++)
                        {
                            if (array5[i] == "ALL")
                            {
                                flag4 = true;
                            }
                        }

                        if (hardModeAllBoss > 0)
                        {
                            num5 += hardModeAllBoss;
                            if (num5 > list3.Count)
                            {
                                num5 = list3.Count;
                            }
                        }

                        if (flag4)
                        {
                            num5 = list3.Count;
                        }

                        if (num5 > 1)
                        {
                            num4--;
                            num4 += num5;
                            Debug.Log("보스 스텝 선택지 조정 : 보스 스텝 선택지에서 추가 보스를 등장시킴 : bossCount=" + num5);
                        }
                    }

                    List<EFloorMainEventType> list9 = new List<EFloorMainEventType>();
                    array2 = __instance.steps[k].possibleMainEvents;
                    foreach (EFloorMainEventType eFloorMainEventType2 in array2)
                    {
                        switch (eFloorMainEventType2)
                        {
                            case EFloorMainEventType.StoneTablet:
                            case EFloorMainEventType.Enchant:
                                if (!flag)
                                {
                                    Debug.Log("랜덤 이벤트 제거 : 석판&인챈트 노드는 운명각인 이후 나와야 하므로 넘어감");
                                    continue;
                                }

                                break;
                            case EFloorMainEventType.Dice:
                                if (!flag2)
                                {
                                    Debug.Log("랜덤 이벤트 제거 : 주사위 노드는 리롤 운명각인 이후 나와야 하므로 넘어감");
                                    continue;
                                }

                                break;
                            case EFloorMainEventType.Sapphire:
                                if (!createMiracle)
                                {
                                    Debug.Log("랜덤 이벤트 제거 : 기적 노드가 없으면 사파이어 노드도 생성 안됨");
                                    continue;
                                }

                                break;
                        }

                        list9.Add(eFloorMainEventType2);
                    }

                    List<EFloorNodeType> list10 = new List<EFloorNodeType>();
                    EFloorNodeType[] matchEvents = __instance.steps[k].matchEvents;
                    for (int i = 0; i < matchEvents.Length; i++)
                    {
                        EFloorNodeType item2 = matchEvents[i];
                        if (item2.threat != EFloorThreatType.Boss)
                        {
                            list10.Add(item2);
                            continue;
                        }

                        for (int m = 0; m < num5; m++)
                        {
                            list10.Add(item2);
                        }
                    }

                    foreach (EFloorMainEventType item4 in list9)
                    {
                        list10.Add(new EFloorNodeType
                        {
                            ev = item4,
                            threat = __instance.steps[k].possibleThreats.GetRandom(random),
                            fixedPool = false,
                            essential = (item4 == EFloorMainEventType.Unknown)
                        });
                    }

                    List<EFloorNodeType> list11 = new List<EFloorNodeType>();
                    List<EFloorNodeType> list12 = new List<EFloorNodeType>();
                    foreach (EFloorNodeType item5 in list10)
                    {
                        if (item5.essential)
                        {
                            list11.Add(item5);
                        }
                        else
                        {
                            list12.Add(item5);
                        }
                    }

                    int num6 = list11.Count + list12.Count;
                    if (num4 > num6)
                    {
                        Debug.LogWarning($"스텝 생성 문제 {k} : 선택지가 노드 풀 개수보다 많습니다. choice={num4}, currentAllChoices={num6}");
                        num4 = num6;
                    }

                    float num7 = ((float)num4 - 1f) / 2f;
                    int num8 = -1;
                    if (num4 > 0 && num5 > 0)
                    {
                        num8 = random4.Next(0, num4);
                    }

                    List<float> list13 = new List<float>();
                    for (int n = 0; n < num4; n++)
                    {
                        float num9 = (float)n - num7;
                        if (num4 <= 1)
                        {
                            num9 = 0f;
                        }

                        list13.Add(num9 * 160f + (float)random3.Next(-20, 20));
                    }

                    list13.Shuffle(sysRand);
                    List<FloorData> list14 = new List<FloorData>();
                    for (int num10 = 0; num10 < num4; num10++)
                    {
                        EFloorNodeType eFloorNodeType;
                        if (list11.Count > 0)
                        {
                            int index = random.Next(0, list11.Count);
                            eFloorNodeType = list11[index];
                            list11.RemoveAt(index);
                        }
                        else if (list12.Count == 0)
                        {
                            Debug.LogWarning($"스텝 생성 문제 {k} : 랜덤 노드 풀이 비어있습니다.");
                            EFloorNodeType eFloorNodeType2 = default(EFloorNodeType);
                            eFloorNodeType2.ev = EFloorMainEventType.EXP;
                            eFloorNodeType2.threat = EFloorThreatType.Battle;
                            eFloorNodeType2.fixedPool = false;
                            eFloorNodeType2.essential = false;
                            eFloorNodeType = eFloorNodeType2;
                        }
                        else
                        {
                            int index2 = random.Next(0, list12.Count);
                            eFloorNodeType = list12[index2];
                            list12.RemoveAt(index2);
                        }

                        EFloorThreatType eFloorThreatType = eFloorNodeType.threat;
                        EFloorMainEventType eFloorMainEventType3 = eFloorNodeType.ev;
                        FloorGenerator floorGenerator2;
                        if (eFloorNodeType.fixedPool)
                        {
                            if (eFloorThreatType == EFloorThreatType.Boss)
                            {
                                int index3 = random.Next(0, list3.Count);
                                floorGenerator2 = list3[index3];
                                list3.RemoveAt(index3);
                            }
                            else
                            {
                                floorGenerator2 = dictionary2[eFloorThreatType][eFloorMainEventType3].GetRandom(random);
                            }
                        }
                        else
                        {
                            if (enhancedDisturbance)
                            {
                                if (eFloorThreatType == EFloorThreatType.Battle)
                                {
                                    eFloorThreatType = EFloorThreatType.HardBattle;
                                }
                            }
                            else if (eFloorThreatType == EFloorThreatType.Battle && dictionary[EFloorThreatType.BattleFloor].Count > 0 && random5.NextDouble() < (double)__instance.battleZoneFloorChance)
                            {
                                eFloorThreatType = EFloorThreatType.BattleFloor;
                            }

                            floorGenerator2 = dictionary[eFloorThreatType].GetRandom(random);
                        }

                        if (!floorGenerator2)
                        {
                            num10--;
                            continue;
                        }

                        float nodeAnchoredPositionX = list13.SafeRandomAccess(num10);
                        if (eFloorThreatType == EFloorThreatType.Boss && eFloorMainEventType3 != 0)
                        {
                            Debug.LogError($"보스 노드는 무조건 None 이벤트여야 함 : current evType={eFloorMainEventType3}");
                            eFloorMainEventType3 = EFloorMainEventType.None;
                        }

                        string disturbance = "";
                        string bossHard = "";
                        switch (eFloorThreatType)
                        {
                            case EFloorThreatType.HardBattle:
                                disturbance = __instance.disturbanceList.GetRandom(sysRand2);
                                break;
                            case EFloorThreatType.Boss:
                                if (hardModeAllBoss > 0 && num8 == num10)
                                {
                                    bossHard = __instance.GetRandomHardBattleDisturbance(dungeon, random4);
                                }

                                break;
                        }

                        FloorData floorData = new FloorData
                        {
                            guid = Guid.NewGuid().ToString(),
                            name = floorGenerator2.name,
                            stageName = __instance.name,
                            seed = random2.Next(),
                            globalX = num++,
                            globalY = stageOrder,
                            nodeProgress = k + 1,
                            prefabAssetId = floorGenerator2.GetAssetId(),
                            mainEventType = eFloorMainEventType3,
                            threatType = eFloorThreatType,
                            hasTraveler = false,
                            difficulty = difficulty,
                            nodeAnchoredPositionX = nodeAnchoredPositionX,
                            nodeAnchoredPositionY = (float)(k + 1) * (0f - __instance.distanceBetweenNodes) + (float)random3.Next(-15, 15),
                            disturbance = disturbance,
                            bossHard = bossHard
                        };
                        if (__instance.steps[k].allowTravelersEncounter)
                        {
                            list7.Add(floorData);
                        }

                        if (floorGenerator2.ExplorationActivated)
                        {
                            list.Add(floorData);
                            if (k >= 1)
                            {
                                list2.Add(floorData);
                            }
                        }
                        else
                        {
                            floorData.generateMerchantType = -1;
                        }

                        if (eFloorMainEventType3 == EFloorMainEventType.Unknown)
                        {
                            list6.Add(floorData);
                        }

                        list4.Add(floorData);
                        list14.Add(floorData);
                        array4[k + 1].Add(floorData);
                    }
                }

                List<EFloorMainEventType> list15 = new List<EFloorMainEventType>(__instance.unknownUniqueEvents);
                if (list6.Count < list15.Count)
                {
                    Debug.LogWarning($"unknownNodes.Count({list6.Count}) < uniqueCandidates.Count({list15.Count})");
                    list15.Remove(EFloorMainEventType.MaxHP);
                }

                System.Random random6 = new System.Random(seed);
                while (list6.Count > 0)
                {
                    if (list15.Count > 0)
                    {
                        int index4 = random6.Next(0, list15.Count);
                        EFloorMainEventType eFloorMainEventType4 = list15[index4];
                        list15.RemoveAt(index4);
                        switch (eFloorMainEventType4)
                        {
                            case EFloorMainEventType.Miracle:
                                if (!createMiracle || SaveManager.Current.GetInt("DeathCount", 0) < 2)
                                {
                                    continue;
                                }

                                break;
                            case EFloorMainEventType.StoneTablet:
                            case EFloorMainEventType.Enchant:
                                if (!flag)
                                {
                                    Debug.Log("유니크 이벤트 제거 : 석판&인챈트 노드는 운명각인 이후 나와야 하므로 넘어감");
                                    continue;
                                }

                                break;
                            case EFloorMainEventType.Sapphire:
                                if (!createMiracle)
                                {
                                    Debug.Log("유니크 이벤트 제거 : 기적 노드가 없으면 사파이어 노드도 생성 안됨");
                                    continue;
                                }

                                break;
                            case EFloorMainEventType.Anvil:
                                if (!createAnvil)
                                {
                                    continue;
                                }

                                break;
                        }

                        int index5 = random6.Next(0, list6.Count);
                        FloorData floorData2 = list6[index5];
                        floorData2.mainEventType = eFloorMainEventType4;
                        list6.RemoveAt(index5);
                        list8.Add(floorData2);
                        continue;
                    }

                    List<EFloorMainEventType> list16 = list5.ToList();
                    int index6 = random6.Next(0, list6.Count);
                    int nodeProgress = list6[index6].nodeProgress;
                    if (nodeProgress < array4.Length)
                    {
                        foreach (FloorData item6 in array4[nodeProgress])
                        {
                            list16.Remove(item6.mainEventType);
                        }
                    }

                    if (list16.Count > 0)
                    {
                        list6[index6].mainEventType = list16.GetRandom(random6);
                        list6.RemoveAt(index6);
                    }
                    else
                    {
                        Debug.Log("adjustedNormalEvents 후보가 없으므로 중복 노드 처리");
                        list6[index6].mainEventType = list5.GetRandom(random6);
                        list6.RemoveAt(index6);
                    }
                }

                System.Random sysRand3 = new System.Random(seed + 22);
                foreach (FloorData item7 in list8)
                {
                    if (dictionary3.ContainsKey(item7.mainEventType))
                    {
                        FloorGenerator random7 = dictionary3[item7.mainEventType].GetRandom(sysRand3);
                        FloorGenerator component3 = random7.GetComponent<FloorGenerator>();
                        item7.prefabAssetId = random7.GetAssetId();
                        item7.threatType = component3.floorThreatType;
                        if (!component3.ExplorationActivated)
                        {
                            item7.disturbance = "";
                            item7.generateMerchantType = -1;
                            list.Remove(item7);
                            list2.Remove(item7);
                        }
                    }
                }

                System.Random sysRand4 = new System.Random(seed + 99);
                for (int num11 = 0; num11 < __instance.steps.Length; num11++)
                {
                    if (!__instance.steps[num11].includeMerchant || array4[num11 + 1].Count <= 0)
                    {
                        continue;
                    }

                    FloorData random8 = array4[num11 + 1].GetRandom(sysRand4);
                    if (random8.generateMerchantType >= 0)
                    {
                        if ((bool)DungeonManager.Instance && DungeonManager.Instance.dungeonEnvironment.TryGetValue("RedMerchant", out var value) && value > 0)
                        {
                            random8.generateMerchantType = 2;
                        }
                        else
                        {
                            random8.generateMerchantType = 1;
                        }

                        list.Remove(random8);
                        list2.Remove(random8);
                    }
                }

                System.Random random9 = new System.Random(seed + 1000);
                int num12 = Mathf.Min(2, list.Count);
                for (int num13 = 0; num13 < num12; num13++)
                {
                    int index7 = random9.Next(0, list.Count);
                    FloorData floorData3 = list[index7];
                    floorData3.hiddenRoomCount = 1;
                    Debug.Log($"비밀방 생성 : {floorData3.nodeProgress}, {floorData3.mainEventType}");
                }

                System.Random sysRand5 = new System.Random(seed + 1500);
                if (__instance.createMiniBossNode && list2.Count > 0)
                {
                    FloorData random10 = list2.GetRandom(sysRand5);
                    random10.threatType = EFloorThreatType.MiniBoss;
                    random10.disturbance = "";
                    random10.prefabAssetId = __instance.minibossFloorPrefabs.GetRandom(sysRand5).GetAssetId();
                }

                for (int num14 = 0; num14 < array4.Length - 1; num14++)
                {
                    for (int num15 = 0; num15 < array4[num14].Count; num15++)
                    {
                        List<string> list17 = new List<string>();
                        foreach (FloorData item8 in array4[num14 + 1])
                        {
                            list17.Add(item8.guid);
                        }

                        array4[num14][num15].connectionToOtherFloors = list17.ToArray();
                    }
                }

                if (list7.Count > 0)
                {
                    System.Random sysRand6 = new System.Random(seed + 2000);
                    list7.GetRandom(sysRand6).hasTraveler = true;
                }

                array3 = __instance.essentialFloorPrefabs;
                foreach (FloorGenerator floorGenerator3 in array3)
                {
                    FloorData item3 = new FloorData
                    {
                        guid = Guid.NewGuid().ToString(),
                        name = floorGenerator3.name,
                        stageName = __instance.name,
                        seed = random2.Next(),
                        prefabAssetId = floorGenerator3.GetAssetId(),
                        hasTraveler = false,
                        isHidden = true,
                        globalY = stageOrder,
                        globalX = num++,
                        nodeProgress = -1,
                        nodeAnchoredPositionX = -1280f,
                        nodeAnchoredPositionY = 0f,
                        disturbance = ""
                    };
                    list4.Add(item3);
                }

                __result = list4.ToArray();
                return false;
            }
        }
    }
}
