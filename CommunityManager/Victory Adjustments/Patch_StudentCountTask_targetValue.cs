using HarmonyLib;
using ProjectSchoolNs.EndingNS;
using UnityEngine;

namespace CommunityManager
{
    [HarmonyPatch(typeof(StudentCountTask), nameof(StudentCountTask.targetValue), MethodType.Getter)]
    internal static class Patch_StudentCountTask_targetValue
    {
        [HarmonyPrefix]
        public static bool Prefix(StudentCountTask __instance, ref int __result)
        {
            if (!ManagerConfig.StudentVictoryAdjusted) return true;

            __result = Mathf.CeilToInt((__instance.count * ManagerConfig.StudentBaseAdjustRatio) * ManagerConfig.StudentMultiplier);
            return false;
        }
    }
}
