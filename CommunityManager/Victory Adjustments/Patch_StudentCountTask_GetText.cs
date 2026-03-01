using FrameworkNs;
using HarmonyLib;
using ProjectSchoolNs.EndingNS;
using UnityEngine;

namespace CommunityManager
{
    [HarmonyPatch(typeof(StudentCountTask), nameof(StudentCountTask.GetText))]
    internal static class Patch_StudentCountTask_GetText
    {
        [HarmonyPrefix]
        public static bool Prefix(StudentCountTask __instance, ref string __result)
        {
            if (!ManagerConfig.StudentVictoryAdjusted) return true;

            __result = Framework.I18nMgr.GetFormartText("key学生人数达到x以上", "学生人数达到{0}以上", new object[]
            {
                Mathf.CeilToInt((__instance.count * ManagerConfig.StudentBaseAdjustRatio) * ManagerConfig.StudentMultiplier)
            });
            return false;
        }
    }
}
