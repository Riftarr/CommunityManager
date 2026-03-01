using ProjectSchoolNs;
using ProjectSchoolNs.AssetNs;
using ProjectSchoolNs.CharacterNs;
using ProjectSchoolNs.SchoolNs;
using ProjectSchoolNs.UINs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommunityManager
{
    public class StudentSourceInvestItem_Housing : StudentSourceInvestItem
    {
        private ManagerConfig.StudentSource studentSource;

        public StudentSourceInvestItem_Housing()
        {
            this.nameI18n = new I18nText() { Value = "Housing Subsidies" };
            this.post = SpriteTable.GetFamilySprite(2);
            this.studentSource = new ManagerConfig.StudentSource();
        }

        public override GameObject OnGetTips(StudentSourceInstance studentSource)
        {
            return TipsUtility.GetTextTips("Assist community with housing subsidies?\n"
                + this.studentSource.TotalStudentCount.ToString() + " Expected Students >> "
                + this.studentSource.NextTotalStudentCount.ToString() + " Expected Students");
        }

        public override void OnBeginDonate(StudentSourceInstance studentSource)
        {
            string text = "Assist community with housing subsidies";
            string text2 = this.studentSource.TotalStudentCount.ToString();
            string text3 = this.studentSource.NextTotalStudentCount.ToString();
            string formartText = "Assist community with housing subsidies?";
            SchoolCurrencyGroupUI componentInChildren = CommonMessageBox.NewBox(text, formartText, "removeIcon", new CommonButton.CommonButtonConfig[]
            {
                CommonButton.NewConfirmConfigPositive(delegate
                {
                    this.OnDonate(studentSource);
                    CommonMessageBox.NewSoftMessageBoxV2(
                        "Successful Investment",
                        "Thank you for the support! New homes have been built in the community!",
                        SpriteTable.GetDisplineIcon((DisciplineStageEnum)((int)ConvertMorality.ConvertTrue(studentSource.moralityType))),
                        null, null, 5f, true);
                }),
                CommonButton.NewCancelConfig()
            }, true, null, "CommonMessageBox_cost", true, true, 999f).GetComponentInChildren<SchoolCurrencyGroupUI>();
            if (componentInChildren)
            {
                componentInChildren.SetData(this.GetCost(studentSource));
            }
        }

        public override CurrencyPacket GetCost(StudentSourceInstance modifySource)
        {
            return new CurrencyPacket()
            {
                Gold = (int)(100000f * (this.studentSource.HousingSubsidiesMultiplier + 1)),
                IntelligenceContribution = 0,
                PerceptivityContribution = 0,
                CorporeityContribution = 0,
                MemoryContribution = 0,
                PTAPoint = 0,
            };
        }

        public override void OnDonate(StudentSourceInstance modifySource)
        {
            cost = this.GetCost(modifySource);
            if (ConvertMorality.ConvertTrue(modifySource.moralityType) != TrueMoralityType.verygood)
            {
                this.studentSource.HousingSubsidiesMultiplier += 1;
                ManagerConfig.StudentSourceData.UpdateSchoolSource("School", modifySource.config.id, this.studentSource);
                ManagerConfig.StudentSourceData.Save();

                modifySource.PlayerInvest(cost, stake);

                Action onDonate = this.onDonate;
                if (onDonate != null)
                {
                    onDonate();
                }
                Action onDataChanged = modifySource.onDataChanged;
                if (onDataChanged == null)
                {
                    return;
                }
                onDataChanged();
            }
        }

        protected override void GetDontateInfo(StudentSourceInstance studentSource, DonateInfo donateInfo)
        {
            try
            {
                donateInfo.CanDonate = true;
                donateInfo.CurrentIcon = SpriteTable.GetFamilySprite(1);
                donateInfo.NextIcon = SpriteTable.GetFamilySprite(3);
                donateInfo.CurrentDisplay = this.studentSource.TotalStudentCount.ToString();
                donateInfo.NextDisplay = this.studentSource.NextTotalStudentCount.ToString();

                Dictionary<long, ManagerConfig.StudentSource> StudentSources = new Dictionary<long, ManagerConfig.StudentSource>();
                ManagerConfig.StudentSourceData.StudentSorces?.TryGetValue("School", out StudentSources);
                StudentSources?.TryGetValue(studentSource.config.id, out this.studentSource);

                if (this.studentSource == null)
                {
                    ManagerConfig.StudentSourceData.UpdateSchoolSource("School", studentSource.config.id, this.studentSource);
                    ManagerConfig.StudentSourceData.Save();
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) Debug.LogError("[Community Manager] StudentSourceInvestItem_Housing Inner ERROR: " + ex.InnerException.Message + "|" + ex.InnerException.Source + "|" + ex.InnerException.StackTrace);
                else Debug.LogError("[Community Manager] StudentSourceInvestItem_Housing ERROR: " + ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }
        }

        public CurrencyPacket baseCurrency;
        public AnimationCurve MoneyByPerpertyValue;
        public AnimationCurve contributionByPerpertyValue;
        public AnimationCurve PTAPByPerpertyValue;
        private CurrencyPacket cost;
    }
}
