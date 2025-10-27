using HarmonyLib;
using ProjectSchoolNs;
using System;
using UnityEngine;

namespace CommunityManager
{

    [HarmonyPatch(typeof(StudentSourceInstance), nameof(StudentSourceInstance.dailyFee), MethodType.Setter)]
    internal static class Patch_dailyFeeSet_AdjustTuition
    {
        public static void Prefix(StudentSourceInstance __instance, int ___baseDailyFee, ref int value)
        {
            try
            {
                //FileLog.Log("==  " + __instance.config.id.ToString() +"  ========================  " + DateTime.Now.ToString("HH:mm ffff") + "  ===========================");
                //FileLog.Log("pre-dailyFee:  " + value.ToString());

                value = Mathf.FloorToInt(((float)___baseDailyFee * __instance.config.feeCurve.Evaluate((float)(__instance.level + 1))) * ManagerConfig.TuitionMultiplier);

                //FileLog.Log("Post-dailyFee:  " + value.ToString());
            }
            catch (Exception ex)
            {
                //FileLog.Log("Patch_InitLevel_AdjustTuition ERROR: " + ex.Message);
            }
        }
    }
}