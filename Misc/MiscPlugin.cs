using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Bootstrap;
using MiscMods.StolenContent;
using MiscMods.Hooks;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace MiscMods
{
    [BepInDependency("com.Nuxlar.EnemiesPlus", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.phreel.TitansOfTheRiftSOTV", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.rob.Hunk", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.ThinkInvisible.Dronemeld", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.TeamMoonstorm.MoonstormSharedUtils", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class MiscPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = $"com.{PluginAuthor}.{PluginName}";
        public const string PluginAuthor = "score";
        public const string PluginName = "MiscMods";
        public const string PluginVersion = "1.0.0";

        public static MiscPlugin Instance { get; private set; }
        public static bool LeagueOfLiteralGays => Chainloader.PluginInfos.ContainsKey("com.phreel.TitansOfTheRiftSOTV");
        public static bool EnemiesPlusInstalled => Chainloader.PluginInfos.ContainsKey("com.Nuxlar.EnemiesPlus");
        public static bool HunkInstalled => Chainloader.PluginInfos.ContainsKey("com.rob.Hunk");
        public static bool DronemeldInstalled => Chainloader.PluginInfos.ContainsKey("com.ThinkInvisible.Dronemeld");
        public static bool MSUInstalled => Chainloader.PluginInfos.ContainsKey("com.TeamMoonstorm.MoonstormSharedUtils");
        public static bool WRBInstalled => Chainloader.PluginInfos.ContainsKey("BALLS.WellRoundedBalance");

        public void Awake()
        {
            
            Instance = this;

            Log.Init(Logger);

            if (!WRBInstalled)
            {
                Assets.Init();
                CrueltyConfig.Init(Config);
                Cruelty.Init();
                PredictionHooks.Init();
                CombatDirector.Init();
            }
            UnholyHooks.Init();
            HarmonyHooks.Init();
            Shmoovement.Init();
            BeetleSkills.Init();
        }
    }
}
