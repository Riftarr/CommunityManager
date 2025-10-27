using HarmonyLib;
using ProjectSchoolNs;
using System;
using UnityEngine;

namespace CommunityManager
{
    [HarmonyPatch(typeof(StudentSourceInstance), nameof(StudentSourceInstance.Init))]
    internal static class Patch_Init_AdjustTotalStudentCount
    {
        public static void Postfix(StudentSourceInstance __instance)
        {
            //FileLog.Log("==  " + __instance.config.id.ToString() + "  ========================  " + DateTime.Now.ToString("HH:mm ffff") + "  ===========================");
            //FileLog.Log("pre-currentLevel.totalCount: " + __instance.playerTotalStudentCount.ToString());
            //FileLog.Log("StudentMultiplier: " + ManagerConfig.StudentMultiplier.ToString());

            __instance.currentLevel.totalCount = Mathf.CeilToInt((float)__instance.currentLevel.totalCount * ManagerConfig.StudentMultiplier);

            //FileLog.Log("Post-currentLevel.totalCount: " + __instance.playerTotalStudentCount.ToString());
        }
    }
}
