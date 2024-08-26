using System;
using System.Collections.Generic;
using System.Text;
using R2API;
using RoR2;
using UnityEngine.Networking;
using UnityEngine;
using System.Linq;

namespace MiscMods.StolenContent.Spike
{
    // Token: 0x02000027 RID: 39
    public abstract class SpikestripEquipBase<T>
    {
        // Token: 0x040000AC RID: 172
        public EquipmentDef EquipmentDef;

        // Token: 0x040000AD RID: 173
        public GameObject EquipmentPickupModel;

        // Token: 0x040000AE RID: 174
        public GameObject EquipmentDisplayModel;

        // Token: 0x040000AF RID: 175
        public ItemDisplayRuleDict idrs = new([]);

        // Token: 0x1700003D RID: 61
        // (get) Token: 0x060000C8 RID: 200
        public abstract string EquipmentName { get; }

        // Token: 0x1700003E RID: 62
        // (get) Token: 0x060000C9 RID: 201
        public abstract string EquipmentToken { get; }

        // Token: 0x1700003F RID: 63
        // (get) Token: 0x060000CA RID: 202
        public abstract string EquipmentPickup { get; }

        // Token: 0x17000040 RID: 64
        // (get) Token: 0x060000CB RID: 203
        public abstract string EquipmentDescription { get; }

        // Token: 0x17000041 RID: 65
        // (get) Token: 0x060000CC RID: 204
        public abstract string EquipmentLore { get; }

        // Token: 0x17000042 RID: 66
        // (get) Token: 0x060000CD RID: 205
        public abstract Sprite EquipmentIcon { get; }

        // Token: 0x17000043 RID: 67
        // (get) Token: 0x060000CE RID: 206 RVA: 0x000025BC File Offset: 0x000007BC
        public virtual bool AppearsInSinglePlayer => true;

        // Token: 0x17000044 RID: 68
        // (get) Token: 0x060000CF RID: 207 RVA: 0x000025C4 File Offset: 0x000007C4
        public virtual bool AppearsInMultiPlayer => true;

        // Token: 0x17000045 RID: 69
        // (get) Token: 0x060000D0 RID: 208 RVA: 0x000025CC File Offset: 0x000007CC
        public virtual bool CanDrop => true;

        // Token: 0x17000046 RID: 70
        // (get) Token: 0x060000D1 RID: 209 RVA: 0x000025D4 File Offset: 0x000007D4
        public virtual float Cooldown => 60f;

        // Token: 0x17000047 RID: 71
        // (get) Token: 0x060000D2 RID: 210 RVA: 0x000025DC File Offset: 0x000007DC
        public virtual bool EnigmaCompatible => true;

        // Token: 0x17000048 RID: 72
        // (get) Token: 0x060000D3 RID: 211 RVA: 0x000025E4 File Offset: 0x000007E4
        public virtual bool CanBeRandomlyTriggered => true;

        // Token: 0x17000049 RID: 73
        // (get) Token: 0x060000D4 RID: 212 RVA: 0x000025EC File Offset: 0x000007EC
        public virtual bool IsBoss => false;

        // Token: 0x1700004A RID: 74
        // (get) Token: 0x060000D5 RID: 213 RVA: 0x000025F4 File Offset: 0x000007F4
        public virtual bool IsLunar => false;

        // Token: 0x1700004B RID: 75
        // (get) Token: 0x060000D6 RID: 214 RVA: 0x000025FC File Offset: 0x000007FC
        public virtual BuffDef PassiveBuff => null;

        // Token: 0x1700004C RID: 76
        // (get) Token: 0x060000D7 RID: 215 RVA: 0x00002604 File Offset: 0x00000804
        public virtual bool CanActivate => true;

        // Token: 0x1700004D RID: 77
        // (get) Token: 0x060000D8 RID: 216 RVA: 0x0000260C File Offset: 0x0000080C
        public virtual float DropOnDeathChance => 0f;

        // Token: 0x1700004E RID: 78
        // (get) Token: 0x060000D9 RID: 217 RVA: 0x00002614 File Offset: 0x00000814
        public virtual Type EquipmentBehaviour => null;

        // Token: 0x1700004F RID: 79
        // (get) Token: 0x060000DA RID: 218 RVA: 0x0000261C File Offset: 0x0000081C
        public virtual bool ServerOnlyEquipmentBehaviour => false;

        // Token: 0x060000DB RID: 219
        public abstract void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict);

        // Token: 0x060000DC RID: 220
        public abstract void AssignEquipmentValues();

        // Token: 0x060000DD RID: 221 RVA: 0x00007350 File Offset: 0x00005550
        public virtual bool OnActivationServer(EquipmentSlot equipmentSlot)
        {
            return false;
        }

        // Token: 0x060000DE RID: 222 RVA: 0x00007364 File Offset: 0x00005564
        public void Init()
        {
            if (this.CanActivate)
            {
                On.RoR2.EquipmentSlot.PerformEquipmentAction += this.ActivationEffect;
            }

            if (this.EquipmentBehaviour != null)
            {
                On.RoR2.CharacterBody.OnInventoryChanged += this.UpdateEquipmentBehaviour;
            }
            this.AssignEquipmentValues();
            this.PopulateItemDisplayRules(this.idrs);
            this.EquipmentDef = ScriptableObject.CreateInstance<EquipmentDef>();
            this.EquipmentDef.name = "EQUIPMENT_" + this.EquipmentToken;
            this.EquipmentDef.nameToken = "EQUIPMENT_" + this.EquipmentToken + "_NAME";
            this.EquipmentDef.pickupToken = "EQUIPMENT_" + this.EquipmentToken + "_PICKUP";
            this.EquipmentDef.descriptionToken = "EQUIPMENT_" + this.EquipmentToken + "_DESCRIPTION";
            this.EquipmentDef.loreToken = "EQUIPMENT_" + this.EquipmentToken + "_LORE";
            this.EquipmentDef.pickupModelPrefab = this.EquipmentPickupModel;
            this.EquipmentDef.pickupIconSprite = this.EquipmentIcon;
            this.EquipmentDef.appearsInSinglePlayer = this.AppearsInSinglePlayer;
            this.EquipmentDef.appearsInMultiPlayer = this.AppearsInMultiPlayer;
            this.EquipmentDef.canDrop = this.CanDrop;
            this.EquipmentDef.cooldown = this.Cooldown;
            this.EquipmentDef.enigmaCompatible = this.EnigmaCompatible;
            this.EquipmentDef.passiveBuffDef = this.PassiveBuff;
            this.EquipmentDef.isBoss = this.IsBoss;
            this.EquipmentDef.isLunar = this.IsLunar;
            this.EquipmentDef.canBeRandomlyTriggered = this.CanBeRandomlyTriggered;
            this.EquipmentDef.dropOnDeathChance = this.DropOnDeathChance;
            this.EquipmentDef.requiredExpansion = null;
            this.EquipmentDef.colorIndex = ColorCatalog.ColorIndex.Equipment;
            if (this.IsBoss)
            {
                this.EquipmentDef.colorIndex = ColorCatalog.ColorIndex.BossItem;
            }
            if (this.IsLunar)
            {
                this.EquipmentDef.colorIndex = ColorCatalog.ColorIndex.LunarItem;
            }
            ItemAPI.Add(new CustomEquipment(this.EquipmentDef, this.idrs));
            LanguageAPI.Add("EQUIPMENT_" + this.EquipmentToken + "_NAME", this.EquipmentName);
            LanguageAPI.Add("EQUIPMENT_" + this.EquipmentToken + "_PICKUP", this.EquipmentPickup);
            LanguageAPI.Add("EQUIPMENT_" + this.EquipmentToken + "_DESCRIPTION", this.EquipmentDescription);
            if (!string.IsNullOrEmpty(this.EquipmentLore))
            {
                LanguageAPI.Add("EQUIPMENT_" + this.EquipmentToken + "_LORE", this.EquipmentLore);
            }
        }

        // Token: 0x060000DF RID: 223 RVA: 0x00007644 File Offset: 0x00005844
        private bool ActivationEffect(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (NetworkServer.active && equipmentDef == this.EquipmentDef)
            {
                return this.OnActivationServer(self);
            }
            return orig.Invoke(self, equipmentDef);
        }

        // Token: 0x060000E0 RID: 224 RVA: 0x00007684 File Offset: 0x00005884
        private void UpdateEquipmentBehaviour(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig.Invoke(self);
            if (NetworkServer.active || !this.ServerOnlyEquipmentBehaviour)
            {
                this.AddEquipBehaviour(self);
            }
        }

        // Token: 0x060000E1 RID: 225 RVA: 0x000076BC File Offset: 0x000058BC
        public void AddEquipBehaviour(CharacterBody characterBody)
        {
            var itemBehavior = characterBody.gameObject.GetComponent(this.EquipmentBehaviour) as CharacterBody.ItemBehavior;
            if (characterBody.inventory.GetEquipment(characterBody.inventory.activeEquipmentSlot).equipmentDef == this.EquipmentDef)
            {
                if (!itemBehavior)
                {
                    itemBehavior = (CharacterBody.ItemBehavior)characterBody.gameObject.AddComponent(this.EquipmentBehaviour);
                    if (itemBehavior is EquipmentTargetBase behaviour)
                    {
                        behaviour.equipmentSlot = characterBody.GetComponent<EquipmentSlot>();
                    }
                    itemBehavior.body = characterBody;
                    itemBehavior.enabled = true;
                }
            }
            else if (itemBehavior)
            {
                UnityEngine.Object.Destroy(itemBehavior);
            }
        }

        // Token: 0x02000028 RID: 40
        public class EquipmentTargetBase : CharacterBody.ItemBehavior
        {
            // Token: 0x040000B4 RID: 180
            public EquipmentSlot equipmentSlot;

            // Token: 0x040000B5 RID: 181
            public BullseyeSearch targetFinder = new BullseyeSearch();

            // Token: 0x17000050 RID: 80
            // (get) Token: 0x060000E3 RID: 227 RVA: 0x00002624 File Offset: 0x00000824
            public virtual float MaxDistance => 0f;

            // Token: 0x17000051 RID: 81
            // (get) Token: 0x060000E4 RID: 228 RVA: 0x0000262C File Offset: 0x0000082C
            public virtual float MaxAngleFilter => 10f;

            // Token: 0x17000052 RID: 82
            // (get) Token: 0x060000E5 RID: 229 RVA: 0x00002634 File Offset: 0x00000834
            public virtual BullseyeSearch.SortMode SortMode => BullseyeSearch.SortMode.Angle;

            // Token: 0x17000053 RID: 83
            // (get) Token: 0x060000E6 RID: 230 RVA: 0x0000263C File Offset: 0x0000083C
            public virtual TargetOption TargetType => 0;

            // Token: 0x060000E7 RID: 231 RVA: 0x00007814 File Offset: 0x00005A14
            public virtual GameObject GetTargetIndicatorPrefab(EquipmentSlot.UserTargetInfo info)
            {
                return LegacyResourcesAPI.Load<GameObject>("Prefabs/LightningIndicator");

            }
            // Token: 0x060000EE RID: 238 RVA: 0x00007830 File Offset: 0x00005A30
            public void ConfigureTargetFinderBase()
            {
                this.targetFinder.teamMaskFilter = TeamMask.allButNeutral;
                this.targetFinder.teamMaskFilter.RemoveTeam(this.equipmentSlot.teamComponent.teamIndex);
                this.targetFinder.sortMode = this.SortMode;
                this.targetFinder.filterByLoS = true;
                var ray = CameraRigController.ModifyAimRayIfApplicable(this.equipmentSlot.GetAimRay(), gameObject, out var num);
                this.targetFinder.searchOrigin = ray.origin;
                this.targetFinder.searchDirection = ray.direction;
                this.targetFinder.maxAngleFilter = this.MaxAngleFilter;
                this.targetFinder.maxDistanceFilter = this.MaxDistance;
                this.targetFinder.viewer = this.equipmentSlot.characterBody;
            }

            // Token: 0x060000EF RID: 239 RVA: 0x00007904 File Offset: 0x00005B04
            public void ConfigureTargetFinderForEnemies()
            {
                this.ConfigureTargetFinderBase();
                this.targetFinder.teamMaskFilter = TeamMask.GetUnprotectedTeams(this.equipmentSlot.teamComponent.teamIndex);
                this.targetFinder.RefreshCandidates();
                this.targetFinder.FilterOutGameObject(gameObject);
            }

            // Token: 0x060000F0 RID: 240 RVA: 0x00007958 File Offset: 0x00005B58
            public void ConfigureTargetFinderForFriendlies()
            {
                this.ConfigureTargetFinderBase();
                this.targetFinder.teamMaskFilter = TeamMask.none;
                this.targetFinder.teamMaskFilter.AddTeam(this.equipmentSlot.teamComponent.teamIndex);
                this.targetFinder.RefreshCandidates();
                this.targetFinder.FilterOutGameObject(gameObject);
            }

            // Token: 0x02000029 RID: 41
            public enum TargetOption
            {
                // Token: 0x040000B7 RID: 183
                Enemy,
                // Token: 0x040000B8 RID: 184
                Ally
            }
        }

        // Token: 0x0200002A RID: 42
        public class EquipmentSingleTargetBehaviour : EquipmentTargetBase
        {
            // Token: 0x040000B9 RID: 185
            public Indicator targetIndicator;

            // Token: 0x040000BA RID: 186
            public EquipmentSlot.UserTargetInfo currentTarget;

            // Token: 0x060000F2 RID: 242 RVA: 0x0000269A File Offset: 0x0000089A
            public void Awake()
            {
                this.targetIndicator = new Indicator(gameObject, null)
                {
                    active = false
                };
            }

            // Token: 0x060000F3 RID: 243 RVA: 0x000079BC File Offset: 0x00005BBC
            public void Update()
            {
                if (this.equipmentSlot)
                {
                    this.UpdateTargets();
                }
            }

            // Token: 0x060000F4 RID: 244 RVA: 0x000026C3 File Offset: 0x000008C3
            public void OnDestroy()
            {
                this.targetIndicator.active = false;
            }

            // Token: 0x060000F5 RID: 245 RVA: 0x000079EC File Offset: 0x00005BEC
            public void UpdateTargets()
            {
                var flag = this.equipmentSlot.stock > 0;
                var flag2 = flag;
                if (flag2)
                {
                    var targetType = this.TargetType;
                    var targetOption = targetType;
                    if (targetOption != TargetOption.Enemy)
                    {
                        if (targetOption == TargetOption.Ally)
                        {
                            ConfigureTargetFinderForFriendlies();
                        }
                    }
                    else
                    {
                        ConfigureTargetFinderForEnemies();
                    }
                    this.currentTarget = new EquipmentSlot.UserTargetInfo(this.targetFinder.GetResults().FirstOrDefault());
                }
                else
                {
                    this.InvalidateCurrentTarget();
                }
                bool flag3 = this.currentTarget.transformToIndicateAt;
                var flag4 = flag3;
                if (flag4)
                {
                    this.targetIndicator.visualizerPrefab = this.GetTargetIndicatorPrefab(this.currentTarget);
                }
                this.targetIndicator.active = flag3;
                this.targetIndicator.targetTransform = (flag3 ? this.currentTarget.transformToIndicateAt : null);
            }

            // Token: 0x060000F6 RID: 246 RVA: 0x000026DA File Offset: 0x000008DA
            public void InvalidateCurrentTarget()
            {
                this.currentTarget = default;
            }
        }

        // Token: 0x0200002B RID: 43
        public class EquipmentMultiTargetBehaviour : EquipmentTargetBase
        {
            public List<Indicator> indicators = [];

            public List<EquipmentSlot.UserTargetInfo> currentTargets = [];

            public float updateStopwatch;

            public virtual float UpdateInterval => 0.05f;

            public void Update()
            {
                bool flag = this.equipmentSlot;
                if (flag)
                {
                    this.updateStopwatch += Time.fixedDeltaTime;
                    var flag2 = this.updateStopwatch >= this.UpdateInterval;
                    if (flag2)
                    {
                        this.updateStopwatch -= this.UpdateInterval;
                        this.UpdateTargets();
                    }
                }
            }

            // Token: 0x060000FB RID: 251 RVA: 0x00007B28 File Offset: 0x00005D28
            public void OnDestroy()
            {
                for (var i = this.indicators.Count - 1; i >= 0; i--)
                {
                    this.indicators[i].active = false;
                }
            }

            // Token: 0x060000FC RID: 252 RVA: 0x00007B74 File Offset: 0x00005D74
            public Indicator NewIndicator()
            {
                var indicator = new Indicator(gameObject, null)
                {
                    active = false
                };
                this.indicators.Add(indicator);
                return indicator;
            }

            // Token: 0x060000FD RID: 253 RVA: 0x00007BAC File Offset: 0x00005DAC
            public void UpdateTargets()
            {
                this.InvalidateCurrentTargets();
                var flag = this.equipmentSlot.stock > 0;
                var flag2 = flag;
                if (flag2)
                {
                    var targetType = this.TargetType;
                    var targetOption = targetType;
                    if (targetOption != TargetOption.Enemy)
                    {
                        if (targetOption == TargetOption.Ally)
                        {
                            ConfigureTargetFinderForFriendlies();
                        }
                    }
                    else
                    {
                        ConfigureTargetFinderForEnemies();
                    }
                    var list = this.targetFinder.GetResults().Reverse().ToList();
                    for (var i = 0; i < list.Count; i++)
                    {
                        var userTargetInfo = new EquipmentSlot.UserTargetInfo(list[i]);
                        this.currentTargets.Add(userTargetInfo);
                        var indicator = (i < this.indicators.Count) ? this.indicators[i] : this.NewIndicator();
                        indicator.visualizerPrefab = this.GetTargetIndicatorPrefab(userTargetInfo);
                        indicator.targetTransform = userTargetInfo.transformToIndicateAt;
                        indicator.active = true;
                    }
                }
                for (var j = this.indicators.Count - 1; j >= this.currentTargets.Count; j--)
                {
                    this.indicators[j].active = false;
                    this.indicators.RemoveAt(j);
                }
            }

            // Token: 0x060000FE RID: 254 RVA: 0x00007CF8 File Offset: 0x00005EF8
            public void InvalidateCurrentTargets()
            {
                for (var i = this.currentTargets.Count - 1; i >= 0; i--)
                {
                    this.currentTargets.Clear();
                }
            }
        }
    }
}
