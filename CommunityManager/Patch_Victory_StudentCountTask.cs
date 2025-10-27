using FrameworkNs;
using HarmonyLib;
using ProjectSchoolNs.EndingNS;
using UnityEngine;

namespace CommunityManager
{
    [HarmonyPatch(typeof(StudentCountTask), nameof(StudentCountTask.GetText))]
    internal static class Patch_Victory_StudentCountTask_GetText
    {
        [HarmonyPrefix]
        public static bool Prefix(StudentCountTask __instance, ref string __result)
        {
            if (ManagerConfig.StudentVictoryAdjusted == 0f) return true;

            __result = Framework.I18nMgr.GetFormartText("key学生人数达到x以上", "学生人数达到{0}以上", new object[]
            {
                Mathf.CeilToInt((float)__instance.count * ManagerConfig.StudentMultiplier)
            });
            return false;
        }
    }

    [HarmonyPatch(typeof(StudentCountTask), nameof(StudentCountTask.targetValue), MethodType.Getter)]
    internal static class Patch_Victory_StudentCountTask_targetValue
    {
        [HarmonyPrefix]
        public static bool Prefix(StudentCountTask __instance, ref int __result)
        {
            if (ManagerConfig.StudentVictoryAdjusted == 0f) return true;

            __result = Mathf.CeilToInt((float)__instance.count * ManagerConfig.StudentMultiplier);
            return false;
        }
    }
}
