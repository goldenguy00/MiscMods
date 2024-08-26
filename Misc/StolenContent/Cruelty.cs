using System;
using System.Collections.Generic;
using System.Linq;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using MiscMods.Config;

namespace MiscMods.StolenContent
{
    /// <summary>
    /// Fixed it.
    /// </summary>
    public class Cruelty
    {
        public static List<EquipmentDef> BlacklistedElites = [];

        public static void Init()
        {
            PluginConfig.CrueltyConfig.Init();

            RoR2Application.onLoad += OnLoad;

            On.RoR2.CombatDirector.Awake += (orig, self) =>
            {
                orig(self);
                if (NetworkServer.active && PluginConfig.CrueltyConfig.enabled.Value)
                {
                    self.gameObject.AddComponent<CombatCruelty>().SetCombatListener(self);
                }
            };

            On.RoR2.ScriptedCombatEncounter.Awake += (orig, self) =>
            {
                orig(self);
                if (NetworkServer.active && PluginConfig.CrueltyConfig.enabled.Value)
                {
                    self.gameObject.AddComponent<ScriptedCruelty>().SetCombatListener(self);
                }
            };
        }

        private static void OnLoad()
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

        public class CombatCruelty : MonoBehaviour
        {
            private CombatDirector director;
            private Xoroshiro128Plus rng;

            public void SetCombatListener(CombatDirector d)
            {
                director = d;
                rng = director.rng;
                director.onSpawnedServer.AddListener(OnSpawnedServer);
            }

            private void OnSpawnedServer(GameObject masterObject)
            {
                if (Util.CheckRoll(PluginConfig.CrueltyConfig.triggerChance.Value))
                {
                    var master = masterObject.GetComponent<CharacterMaster>();
                    if (master && master.hasBody && master.inventory && master.inventory.GetItemCount(RoR2Content.Items.HealthDecay) <= 0)
                    {
                        CreateCrueltyElite(master);
                    }
                }
            }

            private void CreateCrueltyElite(CharacterMaster master)
            {
                var body = master.GetBody();
                var inventory = master.inventory;

                if (!body || !inventory || !director)
                    return;

                //Check amount of elite buffs the target has
                List<BuffIndex> currentEliteBuffs = [];
                foreach (var b in BuffCatalog.eliteBuffIndices)
                {
                    if (body.HasBuff(b) && !currentEliteBuffs.Contains(b))
                    {
                        currentEliteBuffs.Add(b);
                    }
                }

                if (PluginConfig.CrueltyConfig.onlyApplyToElites.Value && !currentEliteBuffs.Any())
                    return;

                var dr = body.GetComponent<DeathRewards>();
                uint xp = 0, gold = 0;
                if (dr)
                {
                    xp = dr.expReward;
                    gold = dr.goldReward;
                }

                while (director.monsterCredit > 0 && currentEliteBuffs.Count < PluginConfig.CrueltyConfig.maxAffixes.Value && GetRandom(director.monsterCredit, director.currentMonsterCard, rng, currentEliteBuffs, out var result))
                {
                    //Fill in equipment slot if it isn't filled
                    if (inventory.currentEquipmentIndex == EquipmentIndex.None)
                        inventory.SetEquipmentIndex(result.def.eliteEquipmentDef.equipmentIndex);

                    //Apply Elite Bonus
                    var buff = result.def.eliteEquipmentDef.passiveBuffDef.buffIndex;
                    currentEliteBuffs.Add(buff);
                    body.AddBuff(buff);

                    float affixes = currentEliteBuffs.Count;
                    director.monsterCredit -= result.cost / affixes;
                    inventory.GiveItem(RoR2Content.Items.BoostHp, Mathf.RoundToInt((result.def.healthBoostCoefficient - 1f) * 10f / affixes));
                    inventory.GiveItem(RoR2Content.Items.BoostDamage, Mathf.RoundToInt((result.def.damageBoostCoefficient - 1f) * 10f / affixes));

                    if (dr)
                    {
                        dr.expReward += Convert.ToUInt32(xp / affixes);
                        dr.goldReward += Convert.ToUInt32(gold / affixes);
                    }

                    if (!Util.CheckRoll(PluginConfig.CrueltyConfig.successChance.Value))
                        break;
                }
            }
        }

        public class ScriptedCruelty : MonoBehaviour
        {
            private Xoroshiro128Plus rng;
            private ScriptedCombatEncounter encounter;

            public void SetCombatListener(ScriptedCombatEncounter enc)
            {
                encounter = enc;
                rng = encounter.rng;
                encounter.combatSquad.onMemberAddedServer += OnMemberAddedServer;
            }

            private void OnMemberAddedServer(CharacterMaster master)
            {
                if (PluginConfig.CrueltyConfig.guaranteeSpecialBoss.Value || Util.CheckRoll(PluginConfig.CrueltyConfig.triggerChance.Value))
                {
                    if (master && master.hasBody && master.inventory && master.inventory.GetItemCount(RoR2Content.Items.HealthDecay) <= 0)
                    {
                        CreateCrueltyElite(master);
                    }
                }
            }

            private void CreateCrueltyElite(CharacterMaster master)
            {
                var body = master.GetBody();
                var inventory = master.inventory;

                if (!body || !inventory)
                    return;

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

                while (currentEliteBuffs.Count < PluginConfig.CrueltyConfig.maxAffixes.Value && GetScriptedRandom(rng, currentEliteBuffs, out var result))
                {
                    //Fill in equipment slot if it isn't filled
                    if (inventory.currentEquipmentIndex == EquipmentIndex.None)
                        inventory.SetEquipmentIndex(result.eliteEquipmentDef.equipmentIndex);

                    //Apply Elite Bonus
                    var buff = result.eliteEquipmentDef.passiveBuffDef.buffIndex;
                    currentEliteBuffs.Add(buff);
                    body.AddBuff(buff);

                    float affixes = currentEliteBuffs.Count;
                    inventory.GiveItem(RoR2Content.Items.BoostHp, Mathf.RoundToInt((result.healthBoostCoefficient - 1f) * 10f / affixes));
                    inventory.GiveItem(RoR2Content.Items.BoostDamage, Mathf.RoundToInt((result.damageBoostCoefficient - 1f) * 10f / affixes));
                    if (dr)
                    {
                        dr.expReward += Convert.ToUInt32(xp / affixes);
                        dr.goldReward += Convert.ToUInt32(gold / affixes);
                    }

                    if (!Util.CheckRoll(PluginConfig.CrueltyConfig.successChance.Value))
                        break;
                }
            }
        }

        public static bool GetScriptedRandom(Xoroshiro128Plus rng, List<BuffIndex> currentBuffs, out EliteDef result)
        {
            result = null;

            var tiers = EliteAPI.GetCombatDirectorEliteTiers();
            if (tiers == null || tiers.Length == 0)
                return false;

            var availableDefs =
                from etd in tiers
                where etd != null && !etd.canSelectWithoutAvailableEliteDef
                from ed in etd.availableDefs
                where IsValid(ed, currentBuffs)
                select ed;


            if (availableDefs.Any())
            {
                var rngIndex = rng.RangeInt(0, availableDefs.Count());
                result = availableDefs.ElementAt(rngIndex);
                return true;
            }

            return false;
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
    }
}