using FrameworkNs;
using HarmonyLib;
using ProjectSchoolNs;
using System;
using UnityEngine;
using static CommunityManager.ManagerConfig;

namespace CommunityManager
{
    [HarmonyPatch(typeof(MajorModifyer), nameof(MajorModifyer.OnGetTips))]
    internal class Postfix_MajorModifyer_OnGetTips
    {
        public static void Postfix(MajorModifyer __instance, ref GameObject __result, StudentSourceInstance studentSource)
        {
            try
            {
                MajorModifyersData foundData = new MajorModifyersData();
                ManagerConfig.MajorModifyers?.TryGetValue(__instance, out foundData);
                MajorModifyersData ExtraData = foundData ?? new MajorModifyersData();
                if (ExtraData.State)
                {
                    __result.DestroySelf();

                    string text = "";
                    for (int i = 0; i < studentSource.targetMajor.difficulty; i++)
                    {
                        text += "<style=Emoji>36</style>";
                    }

                    __result = TipsUtility.GetTextTips(Framework.I18nMgr.GetFormartText("key投资志愿Tips", "通过投资改变生源点的目标志愿\n<color=#248C00>{0}>>{1}</color>", text + " ", text));
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
