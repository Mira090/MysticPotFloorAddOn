using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MysticPotFloor
{
    [Obsolete]
    public class StageEntity_ChoiceMod : StageEntity_Choice
    {
        public override FloorData[] GenerateStage(DungeonManager dungeon, int seed, int stageOrder, bool createMiracle, bool createAnvil, bool enhancedDisturbance, string bossGenerateParameter, int hardModeAllBoss)
        {
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
            GameObject[] array = randomEventFloorPrefabs;
            for (int i = 0; i < array.Length; i++)
            {
                FloorGenerator component = array[i].GetComponent<FloorGenerator>();
                dictionary[component.floorThreatType].Add(component);
            }

            array = fixedEventFloorPrefabs;
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
                name = firstFloor.name,
                stageName = base.name,
                seed = random2.Next(),
                globalY = stageOrder,
                globalX = num++,
                nodeProgress = 0,
                prefabAssetId = firstFloor.GetAssetId(),
                hasTraveler = false,
                difficulty = firstDifficulty,
                mainEventType = firstFloor.floorMainEventType,
                threatType = firstFloor.floorThreatType,
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
            EFloorMainEventType[] array2 = unknownNormalEvents;
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
            FloorGenerator[] array3 = uniqueFloorPrefabs;
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
            List<FloorData>[] array4 = new List<FloorData>[steps.Length + 1];
            for (int j = 0; j < array4.Length; j++)
            {
                array4[j] = new List<FloorData>();
            }

            array4[0].Add(item);
            for (int k = 0; k < steps.Length; k++)
            {
                Debug.Log("Create Step : " + k);
                bool flag3 = false;
                for (int l = 0; l < steps[k].matchEvents.Length; l++)
                {
                    if (steps[k].matchEvents[l].threat == EFloorThreatType.Boss)
                    {
                        flag3 = true;
                        break;
                    }
                }

                int num2 = steps.Length - 1;
                if (num2 <= 0)
                {
                    num2 = 1;
                }

                float num3 = (float)k / (float)num2;
                int difficulty = Mathf.CeilToInt((float)(lastDifficulty - 1 - firstDifficulty) * num3 + (float)firstDifficulty) + 1;
                int num4 = random.Next(steps[k].choiceMin, steps[k].choiceMax + 1);
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
                array2 = steps[k].possibleMainEvents;
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
                EFloorNodeType[] matchEvents = steps[k].matchEvents;
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
                        threat = steps[k].possibleThreats.GetRandom(random),
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
                        else if (eFloorThreatType == EFloorThreatType.Battle && dictionary[EFloorThreatType.BattleFloor].Count > 0 && random5.NextDouble() < (double)battleZoneFloorChance)
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
                            disturbance = disturbanceList.GetRandom(sysRand2);
                            break;
                        case EFloorThreatType.Boss:
                            if (hardModeAllBoss > 0 && num8 == num10)
                            {
                                bossHard = GetRandomHardBattleDisturbance(dungeon, random4);
                            }

                            break;
                    }

                    FloorData floorData = new FloorData
                    {
                        guid = Guid.NewGuid().ToString(),
                        name = floorGenerator2.name,
                        stageName = base.name,
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
                        nodeAnchoredPositionY = (float)(k + 1) * (0f - distanceBetweenNodes) + (float)random3.Next(-15, 15),
                        disturbance = disturbance,
                        bossHard = bossHard
                    };
                    if (steps[k].allowTravelersEncounter)
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

            List<EFloorMainEventType> list15 = new List<EFloorMainEventType>(unknownUniqueEvents);
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
            for (int num11 = 0; num11 < steps.Length; num11++)
            {
                if (!steps[num11].includeMerchant || array4[num11 + 1].Count <= 0)
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
            if (createMiniBossNode && list2.Count > 0)
            {
                FloorData random10 = list2.GetRandom(sysRand5);
                random10.threatType = EFloorThreatType.MiniBoss;
                random10.disturbance = "";
                random10.prefabAssetId = minibossFloorPrefabs.GetRandom(sysRand5).GetAssetId();
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

            array3 = essentialFloorPrefabs;
            foreach (FloorGenerator floorGenerator3 in array3)
            {
                FloorData item3 = new FloorData
                {
                    guid = Guid.NewGuid().ToString(),
                    name = floorGenerator3.name,
                    stageName = base.name,
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

            return list4.ToArray();
        }
    }
}
