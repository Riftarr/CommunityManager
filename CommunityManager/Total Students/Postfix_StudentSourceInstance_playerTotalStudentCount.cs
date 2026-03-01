using HarmonyLib;
using ProjectSchoolNs;
using UnityEngine;

namespace CommunityManager
{
    [HarmonyPatch(typeof(StudentSourceInstance), nameof(StudentSourceInstance.playerTotalStudentCount), MethodType.Getter)]
    internal static class Postfix_StudentSourceInstance_playerTotalStudentCount
    {
        public static void Postfix(StudentSourceInstance __instance, ref int __result)
        {
            if (!ManagerConfig.StudentRelation)
            {
                __result = Mathf.CeilToInt(__instance.currentLevel.totalCount);
            }
        }
    }
}
