using FrameworkNs;
using HarmonyLib;
using Lanka;
using Lanka.CharacterSystem;
using ProjectSchoolNs;
using ProjectSchoolNs.CharacterNs;
using ProjectSchoolNs.UINs;
using ProjectSchoolNs.UtilitiesNs;
using System;
using UnityEngine;

namespace CommunityManager
{
    [HarmonyPatch(typeof(StudentSourceSubscribePanel), "SetData")]
    internal static class Prefix_StudentSourceSubscribePanel_SetData
    {
        public static bool Prefix(StudentSourceSubscribePanel __instance, StudentSourceInstance studentSource, ref StudentSourceInstance ___studentSource)
        {
            try
            {
                ___studentSource = studentSource;
                __instance.nameText.text = studentSource.config.siteNameI18n.GetText();
                __instance.SourceDescribe.OverrideTipsData = studentSource.config.descriptionI18n.GetText();
                __instance.stakeText.text = ___studentSource.playerInitialInvestmentStake.SignedPercent();
                int num = Mathf.CeilToInt((float)studentSource.currentLevel.totalCount * ___studentSource.playerInitialInvestmentStake);
                //==================================================================================================================================================
                if (!ManagerConfig.StudentRelation)
                {
                    num = Mathf.CeilToInt(studentSource.currentLevel.totalCount);
                }
                //==================================================================================================================================================
                __instance.studentCountText.text = Framework.I18nMgr.GetFormartText("新生申请约{0}人/周期", "约{0}人/周期", new object[] { num });
                __instance.price.SetData(studentSource.GetInitialInvestment(___studentSource.playerInitialInvestmentStake));
                __instance.conditionParent.DestroyAllChild();
                bool flag = Module<CharacterModule>.Instance.commonRecruitSubModule.commutingConditionDict.TryGetValue(studentSource.overwrideCommutingType, out RecruitCondition data);
                if (flag)
                {
                    ObjectTable.GetUIDataInstanceComponment<StudentRecruitConditionUIBlock>("", UILayer.MiddleUI, __instance.conditionParent).SetData(data);
                }
                __instance.conditionRoot.SetActive(flag);


                return false;
            }
            catch (Exception ex)
            {
                return true;
            }
        }

        public static string SignedPercent(this float p)
        {
            return ((p < 0f) ? "" : "+") + p.ToString("P", TextUtil.IntegerPercentFormat).Replace(" ", "");
        }
    }
}
