using BepInEx.Configuration;

namespace MiscMods.StolenContent
{
    public static class CrueltyConfig
    {
        public static ConfigEntry<int> maxGeneralAffixes;
        public static ConfigEntry<bool> guaranteeSpecialBoss;
        public static ConfigEntry<float> triggerChance;
        public static ConfigEntry<float> successChance;

        public static ConfigEntry<Cruelty.ScalingMode> damageScaling;
        public static ConfigEntry<Cruelty.ScalingMode> healthScaling;
        public static ConfigEntry<Cruelty.ScalingMode> costScaling;
        public static ConfigEntry<Cruelty.ScalingMode> rewardScaling;

        public static void Init(ConfigFile config)
        {
            CrueltyConfig.maxGeneralAffixes = config.Bind(
                new ConfigDefinition("Cruelty", "Max Non-T2 Affixes"),
                3,
                new ConfigDescription("Maximum Non-T2 Affixes that Cruelty can add. Set to 0 to disable tier from being picked. Set to -1 for no limit."));

            CrueltyConfig.guaranteeSpecialBoss = config.Bind(
                new ConfigDefinition("Cruelty", "Guarantee Special Boss"),
                true,
                new ConfigDescription("Always apply Cruelty to special bosses."));

            CrueltyConfig.triggerChance = config.Bind(
                new ConfigDefinition("Cruelty", "Trigger Chance"),
                25f,
                new ConfigDescription("Chance for Cruelty to be applied to an enemy. Set to 100 to make it always apply."));

            CrueltyConfig.successChance = config.Bind(
                new ConfigDefinition("Cruelty", "Failure Chance"),
                25f,
                new ConfigDescription("Chance for Cruelty to succeed when adding each new affix. Set to 100 to make it always attempt to add as many affixes as possible."));

            //
            // scaling
            //
            CrueltyConfig.costScaling = config.Bind(
                new ConfigDefinition("Cruelty", "Cost Scaling"),
                Cruelty.ScalingMode.Additive,
                new ConfigDescription("How should director cost scale?"));

            CrueltyConfig.damageScaling = config.Bind(
                new ConfigDefinition("Cruelty", "Damage Scaling"),
                Cruelty.ScalingMode.None,
                new ConfigDescription("How should elite damage scale?"));

            CrueltyConfig.healthScaling = config.Bind(
                new ConfigDefinition("Cruelty", "Health Scaling"),
                Cruelty.ScalingMode.Additive,
                new ConfigDescription("How should elite health scale?"));

            CrueltyConfig.rewardScaling = config.Bind(
                new ConfigDefinition("Cruelty", "Reward Scaling"),
                Cruelty.ScalingMode.Additive,
                new ConfigDescription("How should elite kill rewards scale?"));
        }
    }
}
