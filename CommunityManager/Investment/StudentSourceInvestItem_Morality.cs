using ProjectSchoolNs;
using ProjectSchoolNs.AssetNs;
using ProjectSchoolNs.CharacterNs;
using ProjectSchoolNs.SchoolNs;
using ProjectSchoolNs.UINs;
using System;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;

namespace CommunityManager
{
    public class StudentSourceInvestItem_Morality : StudentSourceInvestItem
    {
        public StudentSourceInvestItem_Morality()
        {
            this.nameI18n = new I18nText() { Value = "Host PTA Conference" };
        }

        public override GameObject OnGetTips(StudentSourceInstance studentSource)
        {
            if (ConvertMorality.ConvertTrue(studentSource.moralityType) == TrueMoralityType.verygood) return TipsUtility.GetTextTips("");

            return TipsUtility.GetTextTips("Host PTA Conference?\n"
                + ConvertMorality.GetDescription(ConvertMorality.ConvertTrue(studentSource.moralityType)) + " >> "
                + ConvertMorality.GetDescription((TrueMoralityType)((int)ConvertMorality.ConvertTrue(studentSource.moralityType) + 1)));
        }

        public override void OnBeginDonate(StudentSourceInstance studentSource)
        {
            string text = "Host PTA Conference";
            string text2 = ConvertMorality.GetDescription(ConvertMorality.ConvertTrue(studentSource.moralityType));
            string text3 = ConvertMorality.GetDescription((TrueMoralityType)((int)ConvertMorality.ConvertTrue(studentSource.moralityType) + 1));
            string formartText = "Host PTA Conference?";
            SchoolCurrencyGroupUI componentInChildren = CommonMessageBox.NewBox(text, formartText, "removeIcon", new CommonButton.CommonButtonConfig[]
            {
                CommonButton.NewConfirmConfigPositive(delegate
                {
                    this.OnDonate(studentSource);
                    CommonMessageBox.NewSoftMessageBoxV2(
                        "Successful Conference",
                        "Thank you for the support! The childrens morality has improved!",
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
            if (ConvertMorality.ConvertTrue(modifySource.moralityType) == TrueMoralityType.verygood) return default;

            return new CurrencyPacket()
            {
                Gold = 0,
                IntelligenceContribution = 0,
                PerceptivityContribution = 0,
                CorporeityContribution = 0,
                MemoryContribution = 0,
                PTAPoint = 250,
            };
        }

        public override void OnDonate(StudentSourceInstance modifySource)
        {
            cost = this.GetCost(modifySource);
            if (ConvertMorality.ConvertTrue(modifySource.moralityType) != TrueMoralityType.verygood)
            {
                MoralityType newValue = ConvertMorality.ConvertFalse((TrueMoralityType)((int)ConvertMorality.ConvertTrue(modifySource.moralityType) + 1));
                PropertyInfo propertyInfo = typeof(StudentSourceInstance).GetProperty("moralityType");
                if (propertyInfo != null) propertyInfo.SetValue(modifySource, newValue, null);

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
            this.post = SpriteTable.GetDisplineIcon((DisciplineStageEnum)((int)ConvertMorality.ConvertTrue(studentSource.moralityType)));

            if (ConvertMorality.ConvertTrue(studentSource.moralityType) == TrueMoralityType.verygood)
            {
                donateInfo.CanDonate = false;
                return;
            }
            donateInfo.CanDonate = true;
            donateInfo.CurrentIcon = SpriteTable.GetDisplineIcon((DisciplineStageEnum)((int)ConvertMorality.ConvertTrue(studentSource.moralityType)));
            donateInfo.NextIcon = SpriteTable.GetDisplineIcon((DisciplineStageEnum)((int)ConvertMorality.ConvertTrue(studentSource.moralityType) + 1));
            donateInfo.CurrentDisplay = ConvertMorality.GetDescription(ConvertMorality.ConvertTrue(studentSource.moralityType));
            donateInfo.NextDisplay = ConvertMorality.GetDescription((TrueMoralityType)((int)ConvertMorality.ConvertTrue(studentSource.moralityType) + 1));
        }

        public CurrencyPacket baseCurrency;
        public AnimationCurve MoneyByPerpertyValue;
        public AnimationCurve contributionByPerpertyValue;
        public AnimationCurve PTAPByPerpertyValue;
        private CurrencyPacket cost;
    }

    internal static class ConvertMorality
    {
        public static TrueMoralityType ConvertTrue(MoralityType morality)
        {
            TrueMoralityType trueMoralityType = new TrueMoralityType();

            if (morality == MoralityType.verybad) trueMoralityType = TrueMoralityType.verybad;
            if (morality == MoralityType.bad) trueMoralityType = TrueMoralityType.bad;
            if (morality == MoralityType.normal) trueMoralityType = TrueMoralityType.normal;
            if (morality == MoralityType.good) trueMoralityType = TrueMoralityType.good;
            if (morality == MoralityType.verygood) trueMoralityType = TrueMoralityType.verygood;

            return trueMoralityType;
        }

        public static MoralityType ConvertFalse(TrueMoralityType morality)
        {
            MoralityType trueMoralityType = new MoralityType();

            if (morality == TrueMoralityType.verybad) trueMoralityType = MoralityType.verybad;
            if (morality == TrueMoralityType.bad) trueMoralityType = MoralityType.bad;
            if (morality == TrueMoralityType.normal) trueMoralityType = MoralityType.normal;
            if (morality == TrueMoralityType.good) trueMoralityType = MoralityType.good;
            if (morality == TrueMoralityType.verygood) trueMoralityType = MoralityType.verygood;

            return trueMoralityType;
        }

        public static string GetDescription(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            if (field == null) return value.ToString(); // Fallback to name if no description

            DescriptionAttribute attribute = field.GetCustomAttribute<DescriptionAttribute>();
            return attribute == null ? value.ToString() : attribute.Description;
        }
    }

    internal enum TrueMoralityType
    {
        [Description("Very Bad")]
        verybad,
        [Description("Bad")]
        bad,
        [Description("Normal")]
        normal,
        [Description("Good")]
        good,
        [Description("Very Good")]
        verygood,
    }
}
