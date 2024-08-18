using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace MiscMods
{
    [HarmonyPatch]
    public class LogDampener
    {
        public static readonly Stack<string> recentLogs = [];
        private static IEnumerable<MethodBase> TargetMethods()
        {
            return AccessTools.GetDeclaredMethods(typeof(UnityEngine.Debug))
                .Where(m => m.IsPublic && m.IsStatic && m.ReturnType == typeof(void) && m.IsPublic &&
                    m.Name.StartsWith("Log") && !m.Name.Contains("Exception"))
                .Cast<MethodBase>();

        }
        private static bool Prefix(MethodBase __originalMethod)
        {
            return true;
        }

    }
}
