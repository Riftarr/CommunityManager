using HarmonyLib;
using ProjectSchoolNs;
using UnityEngine;

namespace CommunityManager
{

    [HarmonyPatch(typeof(StudentSourceInstance), nameof(StudentSourceInstance.dailyFee), MethodType.Setter)]
    internal static class Prefix_StudentSourceInstance_dailyFee
    {
        public static void Prefix(StudentSourceInstance __instance, int ___baseDailyFee, ref int value)
        {
            value = Mathf.FloorToInt(((float)___baseDailyFee * __instance.config.feeCurve.Evaluate((float)(__instance.level + 1))) * ManagerConfig.TuitionMultiplier);
        }
    }
}