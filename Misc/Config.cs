using System;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using MiscMods.StolenContent.Beetle;
using MiscMods.StolenContent.Donger;
using MiscMods.StolenContent.Imp;
using MiscMods.StolenContent.Lemur;
using MiscMods.StolenContent.Lunar;
using MiscMods.StolenContent.Wisp;
using MiscMods.StolenContent.Worm;
using UnityEngine;

namespace MiscMods.Config
{
    public static class PluginConfig
    {
        public static ConfigFile MainConfig;
        public static string MainDirectory = Path.Combine(Paths.ConfigPath, "com.score.MiscMods");

        public static ConfigEntry<bool> info;
        public static ConfigEntry<bool> enableShmoovement;
        public static ConfigEntry<bool> enableCruelty;
        public static ConfigEntry<bool> enableEnemiesPlus;
        public static ConfigEntry<bool> enablePrediction;
        public static ConfigEntry<bool> enableUnholy;
        public static ConfigEntry<bool> enableDirector;

        public static ConfigEntry<bool> enableCreditRefund;
        public static ConfigEntry<bool> enableSpawnDiversity;
        public static ConfigEntry<bool> enableDirectorTweaks;
        public static ConfigEntry<float> minimumRerollSpawnIntervalMultiplier;
        public static ConfigEntry<float> maximumRerollSpawnIntervalMultiplier;
        public static ConfigEntry<float> creditMultiplier;
        public static ConfigEntry<float> eliteBiasMultiplier;
        public static ConfigEntry<float> creditMultiplierForEachMountainShrine;
        public static ConfigEntry<float> goldAndExperienceMultiplierForEachMountainShrine;
        public static ConfigEntry<int> maximumNumberToSpawnBeforeSkipping;
        public static ConfigEntry<int> maxConsecutiveCheapSkips;

        public static void Init(ConfigFile cfg)
        {
            MainConfig = cfg;
            RiskOfOptions.ModSettingsManager.SetModDescription("Misc Mods. My version is always better.");

            string sect = "Modules", desc = "Enable disable stuff from being loaded entirely.";
            info = cfg.BindOption(sect, "INFO",
                true,
                "Enables or disables the loading of these sections entirely." +
                "\r\nSome sections have their own configs that can allow for changing stuff during the run.",
                true);

            enableCruelty = cfg.BindOption(sect,
                nameof(enableCruelty),
                true,
                "it gives multiple elite affixes and has its own config", true);

            enableEnemiesPlus = cfg.BindOption(sect,
                nameof(enableEnemiesPlus),
                true,
                "the majority is from nuxlar's enemies plus and also beetle burrow cuz its funny. has additional config.", true);

            enablePrediction = cfg.BindOption(sect,
                nameof(enablePrediction),
                true,
                "enemies predict your movement. still dodgable but they will fuck you up big time.", true);

            enableDirector = cfg.BindOption(sect,
                nameof(enableDirector),
                true,
                "combat director stuff. see other section.", true);

            enableUnholy = cfg.BindOption(sect,
                nameof(enableUnholy),
                true,
                "enables some random crash fixes for various mods. keep this on.", true);

            enableShmoovement = cfg.BindOption(sect,
                nameof(enableShmoovement),
                false,
                "**** YOU MUST ADD ----- KinematicCharacterController.dll ----- TO YOUR MMHOOK CONFIG ****" +
                "\r\nthis fucks with physics and tickrate but game runs better", true);



            sect = "Combat Director";
            desc = "Combat director values. vanilla is 1. have fun n fuck around idk what they do.";

            enableSpawnDiversity = cfg.BindOption(sect,
                nameof(enableSpawnDiversity),
                true,
                "spawns multiple enemy types per wave, multiple boss types during teleporter, etc." +
                "\r\nDONT USE with other stuff doing these things. (director rework specifically)" +
                "\r\nuse mine. i promise to god its better. i know because ive seen literally every alternative. thats why i made this. its better. trust me.", true);

            enableCreditRefund = cfg.BindOption(sect,
                nameof(enableCreditRefund),
                true,
                "gives combat director back some credits spent on spawns. math is sus tho.", true);

            enableDirectorTweaks = cfg.BindOption(sect,
                nameof(enableDirectorTweaks),
                true,
                "enables the hooks that make all the following options work.", true);

            minimumRerollSpawnIntervalMultiplier = cfg.BindOptionSlider(sect,
                nameof(minimumRerollSpawnIntervalMultiplier),
                1.65f,
                desc);

            maximumRerollSpawnIntervalMultiplier = cfg.BindOptionSlider(sect,
                nameof(maximumRerollSpawnIntervalMultiplier),
                1.65f,
                desc);

            creditMultiplier = cfg.BindOptionSlider(sect,
                nameof(creditMultiplier),
                1.3f,
                desc);

            eliteBiasMultiplier = cfg.BindOptionSlider(sect,
                nameof(eliteBiasMultiplier),
                0.9f,
                desc);

            creditMultiplierForEachMountainShrine = cfg.BindOptionSlider(sect,
                nameof(creditMultiplierForEachMountainShrine),
                1.05f,
                desc);

            goldAndExperienceMultiplierForEachMountainShrine = cfg.BindOptionSlider(sect,
                nameof(goldAndExperienceMultiplierForEachMountainShrine),
                0.9f,
                desc);

            maximumNumberToSpawnBeforeSkipping = cfg.BindOptionSlider(sect,
                nameof(maximumNumberToSpawnBeforeSkipping),
                4,
                "vanilla is 6");

            maxConsecutiveCheapSkips = cfg.BindOptionSlider(sect,
                nameof(maxConsecutiveCheapSkips),
                4,
                "vanilla is int.MaxValue so i dont fuckin know man");
        }


        public static class CrueltyConfig
        {
            public static ConfigEntry<bool> enabled;
            public static ConfigEntry<int> maxAffixes;
            public static ConfigEntry<bool> guaranteeSpecialBoss;
            public static ConfigEntry<bool> onlyApplyToElites;
            public static ConfigEntry<int> triggerChance;
            public static ConfigEntry<int> successChance;

            private static ConfigFile CConfig;

            public static void Init()
            {
                CConfig = new ConfigFile(Path.Combine(MainDirectory, "Cruelty.cfg"), true);
                string section = "Cruelty";

                enabled = CConfig.BindOption(section,
                    "Enabled",
                    true,
                    "Enables Cruelty. Dont use with Risky Artifacts. I havent tested it and dont care if things break.");

                maxAffixes = CConfig.BindOptionSlider(section, 
                    "Max Additional Affixes",
                    4,
                    "Maximum Affixes that Cruelty can add.",
                    1, 10);

                guaranteeSpecialBoss = CConfig.BindOption(section, 
                    "Guarantee Special Boss",
                    true,
                    "Always apply Cruelty to special bosses. Applies to void cradles.");

                triggerChance = CConfig.BindOptionSlider(section,
                    "Trigger Chance",
                    15,
                    "Chance for Cruelty to be applied to an enemy. Set to 100 to make it always apply.",
                    0, 100);

                successChance = CConfig.BindOptionSlider(section, 
                    "Additional Affix Chance",
                    65,
                    "Chance for Cruelty to succeed when adding each new affix. Set to 100 to make it always attempt to add as many affixes as possible.",
                    0, 100);

                onlyApplyToElites = CConfig.BindOption(section, 
                    "Only Apply to Elites",
                    true,
                    "Only applies additional affixes to enemies that are already elite.");
            }
        }

        public static class EnemiesPlusConfig
        {
            public static ConfigEntry<bool> enableSkills;
            public static ConfigEntry<bool> bellSkills;
            public static ConfigEntry<bool> impSkills;
            public static ConfigEntry<bool> wormSkills;
            public static ConfigEntry<bool> lunarGolemSkills;

            public static ConfigEntry<bool> enableTweaks;
            public static ConfigEntry<bool> lemChanges;
            public static ConfigEntry<bool> wispChanges;
            public static ConfigEntry<bool> greaterWispChanges;
            public static ConfigEntry<bool> helfireChanges;
            public static ConfigEntry<bool> lunarWispChanges;

            public static ConfigEntry<bool> enableBeetleFamilyChanges;
            public static ConfigEntry<bool> lilBeetleSkills;
            public static ConfigEntry<bool> beetleBurrow;
            public static ConfigEntry<bool> bgChanges;
            public static ConfigEntry<bool> queenChanges;

            private static ConfigFile EPConfig { get; set; }

            public static void Init()
            {
                EPConfig = new ConfigFile(Path.Combine(MainDirectory, "EnemiesPlus.cfg"), true);

                var section = "Skills";
                enableSkills = EPConfig.BindOption(section,
                    "Enable New Skills",
                    true,
                    "Allows any of the skills within this section to be toggled on and off.", true);

                bellSkills = EPConfig.BindOption(section,
                    "Enable Brass Contraptions Buff Beam Skill",
                    true,
                    "Adds a new skill that gives an ally increased armor.", true);

                impSkills = EPConfig.BindOption(section,
                    "Enable Imps Void Spike Skill",
                    true,
                    "Adds a new skill for Imps to throw void spikes at range, similarly to the Imp OverLord.", true);

                lunarGolemSkills = EPConfig.BindOption(section,
                    "Enable Lunar Golems Lunar Shell Skill",
                    true,
                    "Adds a new ability that gives them some stuff idk its vanilla but unused", true);

                wormSkills = EPConfig.BindOption(section,
                    "Enable Worms Leap Skill",
                    true,
                    "Adds a new leap skill and changes Worms to have better targeting.", true);

                
                section = "Tweaks";
                enableTweaks = EPConfig.BindOption(section,
                    "Enable Enemy Tweaks",
                    true,
                    "Allows any of the skills within this section to be toggled on and off.", true);

                helfireChanges = EPConfig.BindOption(section,
                    "Enable Lunar Helfire Debuff",
                    true,
                    "Enables Lunar enemies to apply the new Helfire debuff.", true);

                lunarWispChanges = EPConfig.BindOption(section,
                    "Enable Lunar Wisp Changes",
                    true,
                    "Increases Lunar Wisp movement speed and acceleration, orb applies helfire", true);

                wispChanges = EPConfig.BindOption(section,
                    "Enable Wisp Changes",
                    true,
                    "Increases Wisp bullet count", true);

                greaterWispChanges = EPConfig.BindOption(section,
                    "Enable Greater Wisp Changes",
                    true,
                    "Decreases Greater Wisp credit cost", true);

                lemChanges = EPConfig.BindOption(section,
                    "Enable Lemurian Bite Changes",
                    true,
                    "Adds slight leap to Lemurian bite", true);


                section = "Beetles";
                enableBeetleFamilyChanges = EPConfig.BindOption(section,
                    "Enable Beetle Family Changes",
                    true,
                    "Enables all beetle related changes. Yes, they needed their own section. Unaffected by other config sections.", true);

                lilBeetleSkills = EPConfig.BindOption(section,
                    "Enable Lil Beetles Spit Skill",
                    true,
                    "Adds a new projectile attack to beetles", true);

                beetleBurrow = EPConfig.BindOption(section,
                    "Enable Lil Beetles Burrow Skill",
                    true,
                    "Adds a new mobility skill that allows beetles to burrow into the ground and reappear near the player.", true);

                bgChanges = EPConfig.BindOption(section,
                    "Enable Beetle Guards Rally Cry Skill",
                    true,
                    "Adds a new skill that gives them and nearby allies increased attack speed and movement speed", true);

                queenChanges = EPConfig.BindOption(section,
                    "Enable Beetle Queens Debuff",
                    true,
                    "Adds a new debuff, Beetle Juice, to Beetle Queen and ally beetles attacks and makes spit explode mid air", true);


                if (enableSkills.Value)
                {
                    if (bellSkills.Value) BellChanges.Init();
                    if (impSkills.Value) ImpChanges.Init();
                    if (wormSkills.Value) WormChanges.Init();
                    if (lunarGolemSkills.Value) LunarChanges.Init();
                }

                if (enableTweaks.Value)
                {
                    if (lemChanges.Value) LemurChanges.Init();
                    if (wispChanges.Value || greaterWispChanges.Value) WispChanges.Init();
                    if (helfireChanges.Value || lunarWispChanges.Value) LunarChanges.Init();
                }

                if (enableBeetleFamilyChanges.Value) BeetleChanges.Init();
            }
        }

        #region Config Binding
        public static ConfigEntry<T> BindOption<T>(this ConfigFile myConfig, string section, string name, T defaultValue, string description = "", bool restartRequired = false)
        {
            if (string.IsNullOrEmpty(description))
                description = name;

            if (restartRequired)
                description += " (restart required)";

            var configEntry = myConfig.Bind(section, name, defaultValue, description);
            TryRegisterOption(configEntry, restartRequired);

            return configEntry;
        }

        public static ConfigEntry<T> BindOptionSlider<T>(this ConfigFile myConfig, string section, string name, T defaultValue, string description = "", float min = 0, float max = 20, bool restartRequired = false)
        {
            if (string.IsNullOrEmpty(description))
                description = name;

            description += " (Default: " + defaultValue + ")";

            if (restartRequired)
                description += " (restart required)";

            var configEntry = myConfig.Bind(section, name, defaultValue, description);

            TryRegisterOptionSlider(configEntry, min, max, restartRequired);

            return configEntry;
        }
        #endregion

        #region RoO
        public static void TryRegisterOption<T>(ConfigEntry<T> entry, bool restartRequired)
        {
            if (entry is ConfigEntry<string> stringEntry)
            {
                RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.StringInputFieldOption(stringEntry, restartRequired));
                return;
            }
            if (entry is ConfigEntry<float> floatEntry)
            {
                RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.SliderOption(floatEntry, new RiskOfOptions.OptionConfigs.SliderConfig()
                {
                    min = 0,
                    max = 20,
                    formatString = "{0:0.00}",
                    restartRequired = restartRequired
                }));
                return;
            }
            if (entry is ConfigEntry<int> intEntry)
            {
                RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.IntSliderOption(intEntry, restartRequired));
                return;
            }
            if (entry is ConfigEntry<bool> boolEntry)
            {
                RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.CheckBoxOption(boolEntry, restartRequired));
                return;
            }
            if (entry is ConfigEntry<KeyboardShortcut> shortCutEntry)
            {
                RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.KeyBindOption(shortCutEntry, restartRequired));
                return;
            }
            if (typeof(T).IsEnum)
            {
                RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.ChoiceOption(entry, restartRequired));
                return;
            }
        }

        public static void TryRegisterOptionSlider<T>(ConfigEntry<T> entry, float min, float max, bool restartRequired)
        {
            if (entry is ConfigEntry<int> intEntry)
            {
                RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.IntSliderOption(intEntry, new RiskOfOptions.OptionConfigs.IntSliderConfig()
                {
                    min = (int)min,
                    max = (int)max,
                    formatString = "{0:0.00}",
                    restartRequired = restartRequired
                }));
                return;
            }

            if (entry is ConfigEntry<float> floatEntry)
                RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.SliderOption(floatEntry, new RiskOfOptions.OptionConfigs.SliderConfig()
                {
                    min = min,
                    max = max,
                    formatString = "{0:0.00}",
                    restartRequired = restartRequired
                }));
        }
        #endregion
    }
}
