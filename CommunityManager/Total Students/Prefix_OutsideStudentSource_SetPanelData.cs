using HarmonyLib;
using ProjectSchoolNs;
using System;
using System.Collections.Generic;
using UnityEngine;
using static CommunityManager.ManagerConfig;

namespace CommunityManager
{
    [HarmonyPatch(typeof(OutsideStudentSource), "I_CustomInspectorObject.SetPanelData")]
    internal static class Prefix_OutsideStudentSource_SetPanelData
    {
        public static void Prefix(OutsideStudentSource __instance)
        {
            try
            {
                //This will eventually be where we differentiate saves...
                string schoolName = "School";
    
                Dictionary<long, StudentSource> StudentSources = new Dictionary<long, StudentSource>();
                StudentSource studentSource = new StudentSource();
                bool SchoolExists = ManagerConfig.StudentSourceData.StudentSorces.TryGetValue(schoolName, out StudentSources);
                bool studentSourceExists = false;
                if (SchoolExists) studentSourceExists = StudentSources.TryGetValue(__instance.source.config.id, out studentSource);
                if (studentSourceExists && studentSource.Equals(Mathf.CeilToInt((ManagerConfig.StudentBase + studentSource.StudentRandom) * (ManagerConfig.StudentMultiplier + studentSource.HousingSubsidiesMultiplier))))
                {
                    __instance.source.currentLevel.totalCount = Mathf.CeilToInt(studentSource.TotalStudentCount);
                }
                else
                {
                    System.Random randomGen = new System.Random();
                    float random = Mathf.CeilToInt(((float)randomGen.NextDouble() * (ManagerConfig.StudentRandom * 2)) - ManagerConfig.StudentRandom);
    
                    __instance.source.currentLevel.totalCount = Mathf.CeilToInt((ManagerConfig.StudentBase + random) * ManagerConfig.StudentMultiplier);
    
                    studentSource = new StudentSource()
                    {
                        SourceDescription = __instance.source.config.siteNameI18n.GetText(),
                        StudentBase = ManagerConfig.StudentBase,
                        StudentMultiplier = ManagerConfig.StudentMultiplier,
                        StudentRandom = random,
                        HousingSubsidiesMultiplier = 0,
                    };
    
                    ManagerConfig.StudentSourceData.UpdateSchoolSource(schoolName, __instance.source.config.id, studentSource);
                    ManagerConfig.StudentSourceData.Save();
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) Debug.LogError("[Community Manager] ManagerConfig.Postfix(StudentSourceInstance __instance) Inner ERROR: " + ex.InnerException.Message + "|" + ex.InnerException.Source + "|" + ex.InnerException.StackTrace);
                else Debug.LogError("[Community Manager] ManagerConfig.Postfix(StudentSourceInstance __instance) ERROR: " + ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }
    
        }
    }
}
