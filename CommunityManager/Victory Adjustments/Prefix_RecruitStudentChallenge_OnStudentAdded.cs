using HarmonyLib;
using ProjectSchoolNs.ChallengeNs;
using System.Reflection;

namespace CommunityManager
{
    [HarmonyPatch(typeof(RecruitStudentChallenge.Info), "OnStudentAdded")]
    internal static class Prefix_RecruitStudentChallenge_OnStudentAdded
    {
        [HarmonyPostfix]
        public static bool Prefix(RecruitStudentChallenge.Info __instance)
        {
            if (!ManagerConfig.StudentVictoryAdjusted) return true;

            ManagerConfig.StudentAddedCount++;

            if (ManagerConfig.StudentAddedCount >= (ManagerConfig.StudentMultiplier * ManagerConfig.StudentBaseAdjustRatio))
            {
                ManagerConfig.StudentAddedCount = 0;

                MethodInfo addValue = AccessTools.Method(typeof(CommonLevelChallengeBase<RecruitStudentChallenge.Info, CommonLevelChallengeConfig>.CommonLevelInfoBase<RecruitStudentChallenge>), "AddValue");
                if (addValue != null)
                {
                    addValue.Invoke(__instance, new object[] { 1 });
                }
            }
            return false;
        }
    }
}
