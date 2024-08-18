using System;
using System.Collections.Generic;
using System.Linq;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace MiscMods.StolenContent
{
    /// <summary>
    /// Fixed it.
    /// </summary>
    public class Cruelty
    {
        public enum ScalingMode
        {
            None,
            Additive,
            Multiplicative
        }

        public static ArtifactDef CrueltyArtifact { get; private set; }

        public static List<EquipmentDef> BlacklistedElites = [];
        public static Cruelty instance { get; private set; }

        public static void Init()
        {
            instance ??= new Cruelty();
        }

        private Cruelty()
        {
            CrueltyArtifact = ScriptableObject.CreateInstance<ArtifactDef>();
            CrueltyArtifact.cachedName = "RiskyArtifactOfCruelty";
            CrueltyArtifact.nameToken = "RISKYARTIFACTS_CRUELTY_NAME";
            CrueltyArtifact.descriptionToken = "RISKYARTIFACTS_CRUELTY_DESC";
            CrueltyArtifact.smallIconDeselectedSprite = Assets.smallIconDeselectedSprite;
            CrueltyArtifact.smallIconSelectedSprite = Assets.smallIconSelectedSprite;
            (CrueltyArtifact as ScriptableObject).name = CrueltyArtifact.cachedName;
            ContentAddition.AddArtifactDef(CrueltyArtifact);

            On.RoR2.CombatDirector.Awake += CombatDirector_Awake;
            On.RoR2.ScriptedCombatEncounter.BeginEncounter += ScriptedCombatEncounter_BeginEncounter;
            RoR2Application.onLoad += OnLoad;
        }

        private void ScriptedCombatEncounter_BeginEncounter(On.RoR2.ScriptedCombatEncounter.orig_BeginEncounter orig, ScriptedCombatEncounter self)
        {
            if (NetworkServer.active && self.combatSquad && RunArtifactManager.instance && RunArtifactManager.instance.IsArtifactEnabled(Cruelty.CrueltyArtifact))
            {
                self.combatSquad.onMemberAddedServer += (master) =>
                {
                    if (CrueltyConfig.guaranteeSpecialBoss.Value || Util.CheckRoll(CrueltyConfig.triggerChance.Value))
                    {
                        if (master && master.hasBody && master.inventory && master.inventory.GetItemCount(RoR2Content.Items.HealthDecay) <= 0)
                        {
                            Cruelty.CreateCrueltyElite(master, Mathf.Infinity, null, self.rng);
                        }
                    }
                };
            }
            orig(self);
        }

        private void CombatDirector_Awake(On.RoR2.CombatDirector.orig_Awake orig, CombatDirector self)
        {
            orig(self);
            if (NetworkServer.active && Run.instance && RunArtifactManager.instance.IsArtifactEnabled(Cruelty.CrueltyArtifact))
            {
                self.onSpawnedServer.AddListener((masterObject) =>
                {
                    if (Util.CheckRoll(CrueltyConfig.triggerChance.Value))
                    {
                        var master = masterObject.GetComponent<CharacterMaster>();
                        if (master && master.hasBody && master.inventory && master.inventory.GetItemCount(RoR2Content.Items.HealthDecay) <= 0)
                        {
                            self.monsterCredit = CreateCrueltyElite(master, self.monsterCredit, self.currentMonsterCard, self.rng);
                        }
                    }
                });
            }
        }

        private void OnLoad()
        {
            var blightIndex = EquipmentCatalog.FindEquipmentIndex("AffixBlightedMoffein");
            if (blightIndex != EquipmentIndex.None)
            {
                var ed = EquipmentCatalog.GetEquipmentDef(blightIndex);
                if (ed && ed.passiveBuffDef && ed.passiveBuffDef.eliteDef)
                {
                    BlacklistedElites.Add(ed);
                }
            }

            var perfectedIndex = EquipmentCatalog.FindEquipmentIndex("EliteLunarEquipment");
            if (perfectedIndex != EquipmentIndex.None)
            {
                var ed = EquipmentCatalog.GetEquipmentDef(perfectedIndex);
                if (ed && ed.passiveBuffDef && ed.passiveBuffDef.eliteDef)
                {
                    BlacklistedElites.Add(ed);
                }
            }
        }

        public static float CreateCrueltyElite(CharacterMaster master, float availableCredits, DirectorCard card, Xoroshiro128Plus rng)
        {
            var body = master.GetBody();
            var inventory = master.inventory;

            if (!body || !inventory)
                return availableCredits;

            //Check amount of elite buffs the target has
            List<BuffIndex> currentEliteBuffs = [];
            foreach (var b in BuffCatalog.eliteBuffIndices)
            {
                if (body.HasBuff(b) && !currentEliteBuffs.Contains(b))
                {
                    currentEliteBuffs.Add(b);
                }
            }

            var dr = body.GetComponent<DeathRewards>();
            uint xp = 0, gold = 0;
            if (dr)
            {
                xp = dr.expReward;
                gold = dr.goldReward;
            }

            while (availableCredits > 0 && currentEliteBuffs.Count < CrueltyConfig.maxGeneralAffixes.Value && GetRandom(availableCredits, card, rng, currentEliteBuffs, out var result))
            {
                //Fill in equipment slot if it isn't filled
                if (inventory.currentEquipmentIndex == EquipmentIndex.None)
                    inventory.SetEquipmentIndex(result.def.eliteEquipmentDef.equipmentIndex);

                //Apply Elite Bonus
                var buff = result.def.eliteEquipmentDef.passiveBuffDef.buffIndex;
                currentEliteBuffs.Add(buff);
                body.AddBuff(buff);

                float affixes = currentEliteBuffs.Count;
                availableCredits -= result.cost / affixes;
                MultiplyHealth(inventory, (result.def.damageBoostCoefficient - 1) / affixes);
                MultiplyDamage(inventory, (result.def.healthBoostCoefficient - 1) / affixes);
                if (dr)
                {
                    dr.expReward += Convert.ToUInt32(xp / affixes);
                    dr.goldReward += Convert.ToUInt32(gold / affixes);
                }
                /*availableCredits = CrueltyConfig.costScaling.Value switch
                {
                    ScalingMode.Multiplicative => availableCredits - (cost / affixCount),
                    ScalingMode.Additive => availableCredits - cost,
                    _ => availableCredits - cost
                };

                damageMult = CrueltyConfig.damageScaling.Value switch
                {
                    ScalingMode.Multiplicative => damageMult + ((ed.damageBoostCoefficient - 1) / affixCount),
                    ScalingMode.Additive => damageMult + (ed.damageBoostCoefficient - 1),
                    _ => damageMult
                };

                healthMult = CrueltyConfig.healthScaling.Value switch
                {
                    ScalingMode.Multiplicative => healthMult + ((ed.healthBoostCoefficient - 1) / affixCount),
                    ScalingMode.Additive => healthMult + (ed.healthBoostCoefficient - 1),
                    _ => healthMult
                };*/
                if (!Util.CheckRoll(CrueltyConfig.successChance.Value))
                    break;
            }

            return availableCredits;
        }

        private static bool IsValid(EliteDef ed, List<BuffIndex> currentBuffs)
        {
            return ed && ed.IsAvailable() && ed.eliteEquipmentDef &&
                                ed.eliteEquipmentDef.passiveBuffDef &&
                                ed.eliteEquipmentDef.passiveBuffDef.isElite &&
                                !BlacklistedElites.Contains(ed.eliteEquipmentDef) &&
                                !currentBuffs.Contains(ed.eliteEquipmentDef.passiveBuffDef.buffIndex);
        }

        private static bool IsValid(CombatDirector.EliteTierDef etd, DirectorCard card, int cost, float availableCredits)
        {
            var canAfford = availableCredits >= cost * etd.costMultiplier;

            return etd != null && !etd.canSelectWithoutAvailableEliteDef && canAfford &&
                   (card == null || etd.CanSelect(card.spawnCard.eliteRules));
        }

        public static bool GetRandom(float availableCredits, DirectorCard card, Xoroshiro128Plus rng, List<BuffIndex> currentBuffs, out (EliteDef def, float cost) result)
        {
            result = default;

            var tiers = EliteAPI.GetCombatDirectorEliteTiers();
            if (tiers == null || tiers.Length == 0)
                return false;

            var cost = card?.cost ?? 0;

            var availableDefs =
                from etd in tiers
                where IsValid(etd, card, cost, availableCredits)
                from ed in etd.availableDefs
                where IsValid(ed, currentBuffs)
                select (ed, etd.costMultiplier * cost);


            if (availableDefs.Any())
            {
                var rngIndex = rng.RangeInt(0, availableDefs.Count());
                result = availableDefs.ElementAt(rngIndex);
                return true;
            }

            return false;
        }

        public static void MultiplyHealth(Inventory inventory, float multAdd)
        {
            var intMult = Mathf.RoundToInt(10f * (multAdd - 1f));
            if (inventory && intMult > 0)
                inventory.GiveItem(RoR2Content.Items.BoostHp, intMult);
        }

        public static void MultiplyDamage(Inventory inventory, float multAdd)
        {
            var intMult = Mathf.RoundToInt(10f * (multAdd - 1f));
            if (inventory && intMult > 0)
                inventory.GiveItem(RoR2Content.Items.BoostDamage, intMult);
        }
    }
}