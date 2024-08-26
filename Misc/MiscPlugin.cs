using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Bootstrap;
using MiscMods.Config;
using MiscMods.Hooks;
using MiscMods.StolenContent;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace MiscMods
{
    [BepInDependency("com.phreel.TitansOfTheRiftSOTV", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.TeamMoonstorm.MoonstormSharedUtils", BepInDependency.DependencyFlags.SoftDependency)]

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class MiscPlugin : BaseUnityPlugin
    {
        
        public const string PluginGUID = $"com.{PluginAuthor}.{PluginName}";
        public const string PluginAuthor = "score";
        public const string PluginName = "MiscMods";
        public const string PluginVersion = "1.0.1";

        public static MiscPlugin Instance { get; private set; }

        public static bool LeagueOfLiteralGays => Chainloader.PluginInfos.ContainsKey("com.phreel.TitansOfTheRiftSOTV");
        public static bool MSUInstalled => Chainloader.PluginInfos.ContainsKey("com.TeamMoonstorm.MoonstormSharedUtils");

        public void Awake()
        {
            Instance = this;

            Log.Init(Logger);
            PluginConfig.Init(Config);
            
            if (PluginConfig.enableEnemiesPlus.Value)
                PluginConfig.EnemiesPlusConfig.Init();

            if (PluginConfig.enableCruelty.Value)
                Cruelty.Init();

            if (PluginConfig.enablePrediction.Value)
                PredictionHooks.Init();

            if (PluginConfig.enableDirector.Value)
                CombatDirector.Init();

            if (PluginConfig.enableUnholy.Value)
                UnholyHooks.Init();


            if (PluginConfig.enableShmoovement.Value)
            {
                Shmoovement.Init();
            }

            HarmonyHooks.Init();
        }
    }
}
