using HarmonyLib;
using ProjectSchoolNs.ChallengeNs;
using System.Reflection;

namespace CommunityManager
{
    [HarmonyPatch(typeof(RecruitStudentChallenge.Info), "OnStudentAdded")]
    internal static class Patch_Victory_RecruitStudentChallenge
    {
        [HarmonyPostfix]
        public static bool Prefix(RecruitStudentChallenge.Info __instance)
        {
            ManagerConfig.StudentAddedCount++;
            //FileLog.Log("StudentAddedCount: " + ManagerConfig.StudentAddedCount);

            if (ManagerConfig.StudentAddedCount == ManagerConfig.StudentMultiplier)
            {
                ManagerConfig.StudentAddedCount = 0;
                //FileLog.Log("StudentAddedCount: " + ManagerConfig.StudentAddedCount);

                MethodInfo addValue = AccessTools.Method(typeof(CommonLevelChallengeBase<RecruitStudentChallenge.Info, CommonLevelChallengeConfig>.CommonLevelInfoBase<RecruitStudentChallenge>), "AddValue");
                if (addValue != null)
                {
                    //FileLog.Log("addValue invoked");
                    addValue.Invoke(__instance, new object[] { 1 });
                }
            }
            return false;
        }
    }
}
