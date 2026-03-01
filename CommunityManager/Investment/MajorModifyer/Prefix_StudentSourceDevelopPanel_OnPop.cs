using HarmonyLib;
using ProjectSchoolNs;
using System;
using UnityEngine;

namespace CommunityManager.CommunityManager.Investment.MajorModifyer
{
    [HarmonyPatch(typeof(StudentSourceDevelopPanel), "OnPop")]
    internal static class Prefix_StudentSourceDevelopPanel_OnPop
    {
        public static void Postfix()
        {
            try
            {
                ManagerConfig.MajorModifyers.Clear();
                Debug.LogError("[Community Manager] ManagerConfig.MajorModifyers.Clear()");
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) Debug.LogError("[Community Manager] ManagerConfig.MajorModifyers.Clear() Inner ERROR: " + ex.InnerException.Message + "|" + ex.InnerException.Source + "|" + ex.InnerException.StackTrace);
                else Debug.LogError("[Community Manager] ManagerConfig.MajorModifyers.Clear() ERROR: " + ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }
        }
    }
}