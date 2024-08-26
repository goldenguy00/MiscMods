using System;
using System.Collections.Generic;
using HG;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace MiscMods.StolenContent.Spike
{

    namespace GrooveSaladSpikestripContent
    {
        // Token: 0x0200001D RID: 29
        public abstract class SpikestripEliteBase<T> : SpikestripEquipBase<T> where T : SpikestripEliteBase<T>
        {
            // Token: 0x0400006F RID: 111
            public static EliteTierDefinition tierOneEliteDefault = new()
            {
                tierType = TierType.TierOne,
                healthBoostCoefficient = 4f,
                damageBoostCoefficient = 2f
            };

            // Token: 0x04000070 RID: 112
            public static EliteTierDefinition tierTwoEliteDefault = new()
            {
                tierType = TierType.TierTwo,
                healthBoostCoefficient = 18f,
                damageBoostCoefficient = 6f
            };

            // Token: 0x04000071 RID: 113
            public static EliteTierDefinition honorEliteDefault = new()
            {
                tierType = TierType.Honor,
                healthBoostCoefficient = 2.5f,
                damageBoostCoefficient = 1.5f
            };

            // Token: 0x04000072 RID: 114
            public BuffDef AffixBuff;

            // Token: 0x04000073 RID: 115
            public EliteDef MainEliteDef;

            // Token: 0x04000074 RID: 116
            public List<ItemDisplayParent> itemDisplayParents = new List<ItemDisplayParent>();

            // Token: 0x04000075 RID: 117
            private List<EliteDefTierPair> eliteDefTierPairsInternal = new List<EliteDefTierPair>();

            // Token: 0x17000020 RID: 32
            // (get) Token: 0x0600008B RID: 139
            public abstract string EliteName { get; }

            // Token: 0x17000021 RID: 33
            // (get) Token: 0x0600008C RID: 140
            public abstract string AffixDescriptionMainWord { get; }

            // Token: 0x17000022 RID: 34
            // (get) Token: 0x0600008D RID: 141
            public abstract Color EliteColor { get; }

            // Token: 0x17000023 RID: 35
            // (get) Token: 0x0600008E RID: 142
            public abstract Sprite AffixBuffSprite { get; }

            // Token: 0x17000024 RID: 36
            // (get) Token: 0x0600008F RID: 143
            public abstract EliteTierDefinition MainEliteTierDefinition { get; }

            // Token: 0x17000025 RID: 37
            // (get) Token: 0x06000090 RID: 144
            public abstract Texture2D EliteRampTexture { get; }

            // Token: 0x17000026 RID: 38
            // (get) Token: 0x06000091 RID: 145 RVA: 0x0000244A File Offset: 0x0000064A
            public virtual Type AffixBuffBehaviour => null;

            // Token: 0x17000027 RID: 39
            // (get) Token: 0x06000092 RID: 146 RVA: 0x00002452 File Offset: 0x00000652
            public virtual bool ServerOnlyAffixBuffBehaviour => true;

            // Token: 0x17000028 RID: 40
            // (get) Token: 0x06000093 RID: 147 RVA: 0x0000245A File Offset: 0x0000065A
            public virtual bool HookOnHitEnemy => true;

            // Token: 0x17000029 RID: 41
            // (get) Token: 0x06000094 RID: 148 RVA: 0x00002462 File Offset: 0x00000662
            public virtual EliteTierDefinition[] ExtraEliteTierDefitions => Array.Empty<EliteTierDefinition>();

            // Token: 0x1700002C RID: 44
            // (get) Token: 0x06000097 RID: 151 RVA: 0x00002479 File Offset: 0x00000679
            public override bool CanActivate
            {
                get
                {
                    return false;
                }
            }

            // Token: 0x1700002D RID: 45
            // (get) Token: 0x06000098 RID: 152 RVA: 0x00002479 File Offset: 0x00000679
            public override bool EnigmaCompatible
            {
                get
                {
                    return false;
                }
            }

            // Token: 0x1700002E RID: 46
            // (get) Token: 0x06000099 RID: 153 RVA: 0x0000247C File Offset: 0x0000067C
            public override BuffDef PassiveBuff
            {
                get
                {
                    return this.AffixBuff;
                }
            }

            // Token: 0x1700002F RID: 47
            // (get) Token: 0x0600009A RID: 154 RVA: 0x00002479 File Offset: 0x00000679
            public override bool CanDrop
            {
                get
                {
                    return false;
                }
            }

            // Token: 0x17000030 RID: 48
            // (get) Token: 0x0600009B RID: 155 RVA: 0x00002484 File Offset: 0x00000684
            public override string EquipmentPickup
            {
                get
                {
                    return "Become an aspect of " + this.AffixDescriptionMainWord + ".";
                }
            }

            // Token: 0x17000031 RID: 49
            // (get) Token: 0x0600009C RID: 156 RVA: 0x00002484 File Offset: 0x00000684
            public override string EquipmentDescription
            {
                get
                {
                    return "Become an aspect of " + this.AffixDescriptionMainWord + ".";
                }
            }

            // Token: 0x17000032 RID: 50
            // (get) Token: 0x0600009D RID: 157 RVA: 0x0000249B File Offset: 0x0000069B
            public override string EquipmentLore
            {
                get
                {
                    return null;
                }
            }

            // Token: 0x17000033 RID: 51
            // (get) Token: 0x0600009E RID: 158 RVA: 0x0000249E File Offset: 0x0000069E
            public override string EquipmentToken
            {
                get
                {
                    return "AFFIX" + this.EliteName.ToUpper();
                }
            }

            // Token: 0x17000034 RID: 52
            // (get) Token: 0x0600009F RID: 159 RVA: 0x00002479 File Offset: 0x00000679
            public override bool CanBeRandomlyTriggered
            {
                get
                {
                    return false;
                }
            }

            // Token: 0x17000035 RID: 53
            // (get) Token: 0x060000A0 RID: 160 RVA: 0x000024B5 File Offset: 0x000006B5
            public override float DropOnDeathChance
            {
                get
                {
                    return 0.00025f;
                }
            }

            // Token: 0x060000A1 RID: 161 RVA: 0x00006718 File Offset: 0x00004918
            public override void Init()
            {
                this.MainEliteDef = ScriptableObject.CreateInstance<EliteDef>();
                this.AffixBuff = ScriptableObject.CreateInstance<BuffDef>();
                base.Init();
                this.MainEliteDef.color = this.EliteColor;
                this.MainEliteDef.eliteEquipmentDef = this.EquipmentDef;
                this.MainEliteDef.modifierToken = "ELITE_MODIFIER_" + this.EliteName.ToUpper();
                this.MainEliteDef.shaderEliteRampIndex = 0;
                this.MainEliteDef.healthBoostCoefficient = this.MainEliteTierDefinition.healthBoostCoefficient;
                this.MainEliteDef.damageBoostCoefficient = this.MainEliteTierDefinition.damageBoostCoefficient;
                //SpikestripContentBase.eliteDefContent.Add(this.MainEliteDef);
                LanguageAPI.Add(this.MainEliteDef.modifierToken, this.EliteName + " {0}");
                this.eliteDefTierPairsInternal.Add(new EliteDefTierPair
                {
                    eliteDef = this.MainEliteDef,
                    tierType = this.MainEliteTierDefinition.tierType
                });
                bool flag = this.ExtraEliteTierDefitions.Length != 0;
                if (flag)
                {
                    for (int i = 0; i < this.ExtraEliteTierDefitions.Length; i++)
                    {
                        EliteTierDefinition eliteTierDefinition = this.ExtraEliteTierDefitions[i];
                        EliteDef eliteDef = ScriptableObject.CreateInstance<EliteDef>();
                        eliteDef.color = this.MainEliteDef.color;
                        eliteDef.eliteEquipmentDef = this.MainEliteDef.eliteEquipmentDef;
                        eliteDef.modifierToken = this.MainEliteDef.modifierToken;
                        eliteDef.shaderEliteRampIndex = this.MainEliteDef.shaderEliteRampIndex;
                        eliteDef.healthBoostCoefficient = eliteTierDefinition.healthBoostCoefficient;
                        eliteDef.damageBoostCoefficient = eliteTierDefinition.damageBoostCoefficient;
                        //SpikestripContentBase.eliteDefContent.Add(eliteDef);
                        this.eliteDefTierPairsInternal.Add(new EliteDefTierPair
                        {
                            eliteDef = eliteDef,
                            tierType = eliteTierDefinition.tierType
                        });
                    }
                }
                this.AffixBuff.buffColor = this.EliteColor;
                this.AffixBuff.canStack = false;
                this.AffixBuff.eliteDef = this.MainEliteDef;
                this.AffixBuff.iconSprite = this.AffixBuffSprite;
                this.AffixBuff.isDebuff = false;
                //SpikestripContentBase.buffDefContent.Add(this.AffixBuff);
                bool flag2 = this.AffixBuffBehaviour != null;
                if (flag2)
                {
                    SpikestripBuffBehaviours.RegisterBuffBehaviour(this.AffixBuff, this.AffixBuffBehaviour, this.ServerOnlyAffixBuffBehaviour);
                }
                SpikestripEliteRamps.SpikestripEliteRamp item = default(SpikestripEliteRamps.SpikestripEliteRamp);
                item.eliteDef = this.MainEliteDef;
                item.rampTexture = this.EliteRampTexture;
                SpikestripEliteRamps.eliteRamps.Add(item);
                bool flag3 = this.itemDisplayParents.Count != 0;
                if (flag3)
                {
                    BodyCatalog.SetBodyPrefabs += new BodyCatalog.hook_SetBodyPrefabs(this.CopyItemDisplayRules);
                }
                bool hookOnHitEnemy = this.HookOnHitEnemy;
                if (hookOnHitEnemy)
                {
                    GlobalEventManager.OnHitEnemy += new GlobalEventManager.hook_OnHitEnemy(this.OnHitEffect);
                }
                CombatDirector.Init += new CombatDirector.hook_Init(this.AddToEliteTiers);
            }

            // Token: 0x060000A2 RID: 162 RVA: 0x00006A20 File Offset: 0x00004C20
            private void AddToEliteTiers(CombatDirector.orig_Init orig)
            {
                orig.Invoke();
                for (int i = 0; i < this.eliteDefTierPairsInternal.Count; i++)
                {
                    EliteDefTierPair eliteDefTierPair = this.eliteDefTierPairsInternal[i];
                    ArrayUtils.ArrayAppend<EliteDef>(ref CombatDirector.eliteTiers[(int)eliteDefTierPair.tierType].eliteTypes, eliteDefTierPair.eliteDef);
                }
            }

            // Token: 0x060000A3 RID: 163 RVA: 0x00006A7C File Offset: 0x00004C7C
            private void OnHitEffect(GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
            {
                orig.Invoke(self, damageInfo, victim);
                bool flag = damageInfo.procCoefficient == 0f || damageInfo.rejected;
                if (!flag)
                {
                    bool flag2 = !NetworkServer.active;
                    if (!flag2)
                    {
                        bool flag3 = damageInfo.attacker && damageInfo.procCoefficient > 0f;
                        if (flag3)
                        {
                            this.OnHitEnemyServer(damageInfo, victim);
                        }
                    }
                }
            }

            // Token: 0x060000A4 RID: 164 RVA: 0x000024BC File Offset: 0x000006BC
            public virtual void OnHitEnemyServer(DamageInfo damageInfo, GameObject victim)
            {
            }

            // Token: 0x060000A5 RID: 165 RVA: 0x00006AF0 File Offset: 0x00004CF0
            private void CopyItemDisplayRules(BodyCatalog.orig_SetBodyPrefabs orig, GameObject[] newBodyPrefabs)
            {
                foreach (GameObject gameObject in newBodyPrefabs)
                {
                    bool flag = gameObject;
                    if (flag)
                    {
                        this.CopyIDRFrom(gameObject);
                    }
                }
                orig.Invoke(newBodyPrefabs);
            }

            // Token: 0x060000A6 RID: 166 RVA: 0x00006B34 File Offset: 0x00004D34
            public void CopyIDRFrom(GameObject bodyPrefab)
            {
                ModelLocator component = bodyPrefab.GetComponent<ModelLocator>();
                bool flag = !component;
                if (!flag)
                {
                    Transform modelTransform = component.modelTransform;
                    bool flag2 = !modelTransform;
                    if (!flag2)
                    {
                        CharacterModel component2 = modelTransform.GetComponent<CharacterModel>();
                        bool flag3 = !component2;
                        if (!flag3)
                        {
                            ItemDisplayRuleSet itemDisplayRuleSet = component2.itemDisplayRuleSet;
                            bool flag4 = itemDisplayRuleSet;
                            if (flag4)
                            {
                                List<ItemDisplayParent> list = new List<ItemDisplayParent>(this.itemDisplayParents);
                                List<ItemDisplayRule> list2 = new List<ItemDisplayRule>();
                                int num = 0;
                                while (num < itemDisplayRuleSet.keyAssetRuleGroups.Length && list.Count > 0)
                                {
                                    ItemDisplayRuleSet.KeyAssetRuleGroup keyAssetRuleGroup = itemDisplayRuleSet.keyAssetRuleGroups[num];
                                    for (int i = list.Count - 1; i >= 0; i--)
                                    {
                                        bool flag5 = keyAssetRuleGroup.keyAsset && keyAssetRuleGroup.keyAsset == list[i].parentAsset;
                                        if (flag5)
                                        {
                                            for (int j = 0; j < keyAssetRuleGroup.displayRuleGroup.rules.Length; j++)
                                            {
                                                ItemDisplayRule itemDisplayRule = keyAssetRuleGroup.displayRuleGroup.rules[j];
                                                list2.Add(new ItemDisplayRule
                                                {
                                                    followerPrefab = list[i].prefab,
                                                    localPos = itemDisplayRule.localPos,
                                                    localAngles = itemDisplayRule.localAngles,
                                                    localScale = itemDisplayRule.localScale,
                                                    childName = itemDisplayRule.childName,
                                                    limbMask = itemDisplayRule.limbMask,
                                                    ruleType = itemDisplayRule.ruleType
                                                });
                                            }
                                            list.RemoveAt(i);
                                        }
                                    }
                                    num++;
                                }
                                bool flag6 = list2.Count != 0;
                                if (flag6)
                                {
                                    DisplayRuleGroup empty = DisplayRuleGroup.empty;
                                    empty.rules = list2.ToArray();
                                    ItemDisplayRuleSet itemDisplayRuleSet2 = itemDisplayRuleSet;
                                    ItemDisplayRuleSet.KeyAssetRuleGroup keyAssetRuleGroup2 = default(ItemDisplayRuleSet.KeyAssetRuleGroup);
                                    keyAssetRuleGroup2.keyAsset = this.EquipmentDef;
                                    keyAssetRuleGroup2.displayRuleGroup = empty;
                                    ArrayUtils.ArrayAppend<ItemDisplayRuleSet.KeyAssetRuleGroup>(ref itemDisplayRuleSet2.keyAssetRuleGroups, keyAssetRuleGroup2);
                                }
                            }
                        }
                    }
                }
            }

            // Token: 0x060000A7 RID: 167 RVA: 0x00006D7C File Offset: 0x00004F7C
            public GameObject CreateAffixModel(Color color)
            {
                GameObject gameObject = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/pickupmodels/PickupAffixRed"), "PickupAffix" + this.EliteName, false);
                Material material = Object.Instantiate<Material>(gameObject.GetComponentInChildren<MeshRenderer>().material);
                material.color = color;
                foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>())
                {
                    renderer.material = material;
                }
                return gameObject;
            }

            // Token: 0x060000A8 RID: 168 RVA: 0x00006DF4 File Offset: 0x00004FF4
            public void RegisterDisplayParent(Object parent, GameObject prefab = null)
            {
                ItemDisplayParent item = default(ItemDisplayParent);
                item.parentAsset = parent;
                item.prefab = (prefab ? prefab : this.EquipmentDisplayModel);
                this.itemDisplayParents.Add(item);
            }

            // Token: 0x060000A9 RID: 169 RVA: 0x00006E38 File Offset: 0x00005038
            public ItemDisplayRule[] AffixIDR(string childName, Vector3 position, Vector3 angles, Vector3 scale)
            {
                return new ItemDisplayRule[]
                {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = this.EquipmentDisplayModel,
                    childName = childName,
                    localPos = position,
                    localAngles = angles,
                    localScale = scale
                }
                };
            }

            // Token: 0x060000AA RID: 170 RVA: 0x000024BF File Offset: 0x000006BF
            protected SpikestripEliteBase()
            {
            }

            // Token: 0x060000AB RID: 171 RVA: 0x00006E98 File Offset: 0x00005098
            // Note: this type is marked as 'beforefieldinit'.
            static SpikestripEliteBase()
            {
            }

            // Token: 0x0200001E RID: 30
            public enum TierType
            {
                // Token: 0x04000077 RID: 119
                TierOne = 1,
                // Token: 0x04000078 RID: 120
                TierTwo = 3,
                // Token: 0x04000079 RID: 121
                Honor = 2,
                // Token: 0x0400007A RID: 122
                Lunar = 4
            }

            // Token: 0x0200001F RID: 31
            public struct EliteTierDefinition
            {
                // Token: 0x0400007B RID: 123
                public TierType tierType;

                // Token: 0x0400007C RID: 124
                public float healthBoostCoefficient;

                // Token: 0x0400007D RID: 125
                public float damageBoostCoefficient;
            }

            // Token: 0x02000020 RID: 32
            public struct EliteDefTierPair
            {
                // Token: 0x0400007E RID: 126
                public EliteDef eliteDef;

                // Token: 0x0400007F RID: 127
                public TierType tierType;
            }

            // Token: 0x02000021 RID: 33
            public struct ItemDisplayParent
            {
                // Token: 0x04000080 RID: 128
                public Object parentAsset;

                // Token: 0x04000081 RID: 129
                public GameObject prefab;
            }
        }
    }

}
