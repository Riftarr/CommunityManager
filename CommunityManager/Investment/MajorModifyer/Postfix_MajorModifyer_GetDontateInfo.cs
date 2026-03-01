using HarmonyLib;
using ProjectSchoolNs;
using ProjectSchoolNs.MajorNs;
using System;
using UnityEngine;
using static CommunityManager.ManagerConfig;

namespace CommunityManager
{
    [HarmonyPatch(typeof(MajorModifyer), nameof(MajorModifyer.GetDontateInfo))]
    internal class Postfix_MajorModifyer_GetDontateInfo
    {
        public static void Postfix(MajorModifyer __instance, StudentSourceInstance studentSource, ref DonateInfo donateInfo)
        {
            try
            {
                MajorModifyersData foundData = new MajorModifyersData();
                ManagerConfig.MajorModifyers?.TryGetValue(__instance, out foundData);
                MajorModifyersData ExtraData = foundData ?? new MajorModifyersData();
                if (ExtraData.State)
                {
                    TargetMajorItem targetMajor = studentSource.targetMajor;
                    donateInfo.CanDonate = (targetMajor != null);
                    if (targetMajor != null)
                    {
                        string text = "";
                        for (int i = 0; i < studentSource.targetMajor.difficulty; i++)
                        {
                            text += "<style=Emoji>36</style>";
                        }
                        donateInfo.CurrentDisplay = text;
                        donateInfo.NextDisplay = text;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) Debug.LogError("[Community Manager] Postfix_MajorModifyer_OnGetTips Inner ERROR: " + ex.InnerException.Message + "|" + ex.InnerException.Source + "|" + ex.InnerException.StackTrace);
                else Debug.LogError("[Community Manager] Postfix_MajorModifyer_OnGetTips ERROR: " + ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }
        }
    }
}
