using FrameworkNs;
using HarmonyLib;
using ProjectSchoolNs;
using ProjectSchoolNs.MajorNs;
using System;
using System.Collections.Generic;
using UnityEngine;
using static CommunityManager.ManagerConfig;

namespace CommunityManager
{
    [HarmonyPatch(typeof(TargetMajorSelectPanel), "GenMajorSelection")]
    internal static class Postfix_TargetMajorSelectPanel_GenMajorSelection
    {
        public static void Postfix(TargetMajorSelectPanel __instance)
        {
            try
            {
                MajorModifyersData foundData = new MajorModifyersData();
                ManagerConfig.MajorModifyers?.TryGetValue(__instance.modifyer, out foundData);
                MajorModifyersData ExtraData = foundData ?? new MajorModifyersData();
                if (ExtraData.State)
                {
                    TargetMajorItem targetMajor = Module<TargetMajorModule>.Instance.GetTargetMajor(__instance.studentSource.targetMajor.id);

                    List<(float, TargetMajorItem)> list = new List<(float, TargetMajorItem)>();
                    foreach (KeyValuePair<int, TargetMajorItem> item in Module<TargetMajorModule>.Instance.targetMajorItemDict)
                    {
                        if (item.Value.difficulty == targetMajor.difficulty && item.Value != targetMajor)
                        {
                            list.Add((0f, item.Value));
                        }
                    }

                    __instance.selectionMajor[0] = list[-3 + (3 * ExtraData.IdSet)].Item2;
                    __instance.selectionMajor[1] = list[-2 + (3 * ExtraData.IdSet)].Item2;
                    __instance.selectionMajor[2] = list[-1 + (3 * ExtraData.IdSet)].Item2;
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) Debug.LogError("[Community Manager] Postfix_TargetMajorSelectPanel_GenMajorSelection Inner ERROR: " + ex.InnerException.Message + "|" + ex.InnerException.Source + "|" + ex.InnerException.StackTrace);
                else Debug.LogError("[Community Manager] Postfix_TargetMajorSelectPanel_GenMajorSelection ERROR: " + ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }
        }
    }
}
