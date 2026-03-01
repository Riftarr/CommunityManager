using HarmonyLib;
using ProjectSchoolNs;
using UnityEngine;

namespace CommunityManager
{
    [HarmonyPatch(typeof(StudentSourceInstance), nameof(StudentSourceInstance.nextStudentCount), MethodType.Getter)]
    internal static class Postfix_StudentSourceInstance_nextStudentCount
    {
        public static void Postfix(StudentSourceInstance __instance, ref int __result)
        {
            if (!ManagerConfig.StudentBatch)
            {
                __result = Mathf.CeilToInt(__instance.playerTotalStudentCount);
            }
        }
    }
}
