using FrameworkNs;
using ProjectSchoolNs;
using ProjectSchoolNs.AssetNs;
using ProjectSchoolNs.MajorNs;
using ProjectSchoolNs.SchoolNs;
using ProjectSchoolNs.UINs;
using System;
using UnityEngine;

namespace CommunityManager
{
    public class StudentSourceInvestItem_Festival : StudentSourceInvestItem
    {
        public StudentSourceInvestItem_Festival()
        {
            this.nameI18n = new I18nText() { Value = "Festival Sponsorship" };
            this.post = SpriteTable.GetSprite("Icon_SmallReputation");
        }

        public override GameObject OnGetTips(StudentSourceInstance studentSource)
        {
            return TipsUtility.GetTextTips("Sponsor Next Local Festival?\n" 
                + Math.Round(studentSource.playerStake * 100).ToString() + "%" + " >> "
                + Math.Round(Math.Min(studentSource.playerStake + 0.2f, 1) * 100).ToString() + "%");
        }

        public override void OnBeginDonate(StudentSourceInstance studentSource)
        {
            Module<TargetMajorModule>.Instance.GetTargetMajor(studentSource.targetMajor.id + 100);
            string text = "Sponsor Festival";
            string text2 = Math.Round(studentSource.playerStake * 100).ToString() + "%";
            string text3 = Math.Round(Math.Min(studentSource.playerStake + 0.2f, 1) * 100).ToString() + "%";
            string formartText = "Sponsor Next Local Festival?";
            SchoolCurrencyGroupUI componentInChildren = CommonMessageBox.NewBox(text, formartText, "removeIcon", new CommonButton.CommonButtonConfig[]
            {
                CommonButton.NewConfirmConfigPositive(delegate
                {
                    this.OnDonate(studentSource);
                    CommonMessageBox.NewSoftMessageBoxV2(
                        "Successful Investment!", 
                        "Thank you for the funding! Your relation has improved!", 
                        SpriteTable.GetSatisfactionIcon(Math.Min(studentSource.playerStake + 0.2f, 1)), 
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
            if (modifySource.playerStake >= 1) return default;

            CurrencyPacket result = this.baseCurrency;
            result.Gold = 50000;
            return result;
        }

        public override void OnDonate(StudentSourceInstance modifySource)
        {
            CurrencyPacket cost = this.GetCost(modifySource);
            if (modifySource.playerStake < 1)
            {
                modifySource.PlayerInvest(cost, Math.Min(0.2f, 1 - modifySource.playerStake));
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
            if (studentSource.playerStake >= 1)
            {
                donateInfo.CanDonate = false;
                return;
            }
            donateInfo.CanDonate = true;
            donateInfo.CurrentIcon = SpriteTable.GetSprite("Icon_SmallReputation");
            donateInfo.NextIcon = SpriteTable.GetSprite("Icon_SmallReputation"); 
            donateInfo.CurrentDisplay = Math.Round(studentSource.playerStake * 100).ToString() + "%";
            donateInfo.NextDisplay = Math.Round(Math.Min(studentSource.playerStake + 0.2f, 1) * 100).ToString() + "%";
        }

        public CurrencyPacket baseCurrency;
        public AnimationCurve MoneyByPerpertyValue;
        public AnimationCurve contributionByPerpertyValue;
        public AnimationCurve PTAPByPerpertyValue;
        private CurrencyPacket cost;
    }
}
