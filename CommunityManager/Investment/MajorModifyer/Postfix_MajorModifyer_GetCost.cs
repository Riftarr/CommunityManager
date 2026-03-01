using HarmonyLib;
using ProjectSchoolNs;
using ProjectSchoolNs.SchoolNs;
using System;
using UnityEngine;
using static CommunityManager.ManagerConfig;

namespace CommunityManager
{
    [HarmonyPatch(typeof(MajorModifyer), nameof(MajorModifyer.GetCost))]
    internal class Postfix_MajorModifyer_GetCost
    {
        public static void Postfix(MajorModifyer __instance, ref CurrencyPacket __result)
        {
            try
            {
                MajorModifyersData foundData = new MajorModifyersData();
                ManagerConfig.MajorModifyers?.TryGetValue(__instance, out foundData);
                MajorModifyersData ExtraData = foundData ?? new MajorModifyersData();
                if (ExtraData.State)
                { 
                    __result = new CurrencyPacket()
                    {
                        Gold = (int)(__result.Gold / 2f),
                        IntelligenceContribution = (int)(__result.IntelligenceContribution / 2f),
                        PerceptivityContribution = (int)(__result.PerceptivityContribution / 2f),
                        CorporeityContribution = (int)(__result.CorporeityContribution / 2f),
                        MemoryContribution = (int)(__result.MemoryContribution / 2f),
                        PTAPoint = (int)(__result.PTAPoint / 2f),
                    };
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) Debug.LogError("[Community Manager] Postfix_MajorModifyer_GetCost Inner ERROR: " + ex.InnerException.Message + "|" + ex.InnerException.Source + "|" + ex.InnerException.StackTrace);
                else Debug.LogError("[Community Manager] Postfix_MajorModifyer_GetCost ERROR: " + ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }
        }
    }
}
