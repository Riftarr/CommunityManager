using System.Collections.Generic;

namespace CommunityManager
{
    internal static class ModUiDeclaration
    {
        // ========== CONFIG START ==========
        private const string ModId = "riftarr.communitymanager";
        private const string ButtonName = "Community Manager";
        private const string PanelName = "Community Manager Settings";
        private const string JsonName = "CommunityManager.json";
        // ========== CONFIG END ==========



        // ========== UI CONTROLS ==========
        private static readonly List<UiControl> Controls = new List<UiControl>
        {
            UiControl.Number("StudentBase", "Student Base Number", 1f, 50f, 1f, ManagerConfig.StudentBase),
            UiControl.Number("StudentMultiplier", "Student Recruit Multiplier", 1f, 20f, 0.1f, ManagerConfig.StudentMultiplier),
            UiControl.Number("StudentRandom", "Student Random Number", 0f, 10f, 1f, ManagerConfig.StudentRandom, null, 1f, 0f, "StudentBase", 0.5f, 0f),
            UiControl.Checkbox("StudentRelation", "Enable the relation system affecting the student spawn limit", ManagerConfig.StudentRelation),
            UiControl.Checkbox("StudentBatch", "Enable the batch recruitment system", ManagerConfig.StudentBatch),
            UiControl.Number("TuitionMultiplier", "Tuition Multiplier", 0.01f, 5f, 0.01f, ManagerConfig.TuitionMultiplier),
            UiControl.Checkbox("StudentVictoryAdjusted", "Enable Student Victory Requirements to be Adjusted", ManagerConfig.StudentVictoryAdjusted),
        };



        public static object CreateModUiSpec()
        {
            return new ModUiSpecLoose
            {
                ModId = ModId,
                ButtonName = ButtonName,
                PanelName = PanelName,
                JsonName = JsonName,
                Controls = Controls
            };
        }

        private sealed class ModUiSpecLoose
        {
            public string ModId;
            public string ButtonName;
            public string PanelName;
            public string JsonName;
            public List<UiControl> Controls;
        }

        private sealed class UiControl
        {
            public string Kind; // slider, number, checkbox, text, textalpha, button
            public string Key;
            public string Label;

            public bool FullRow = true;

            // numeric inputs (also used by checkbox behind the scenes: 0/1)
            public double Min;
            public double Max;
            public double Step;
            public double DefaultValue;

            // optional dynamic bounds (for numeric controls only)
            // effectiveMin = State[MinFromKey] * MinMul + MinAdd
            // effectiveMax = State[MaxFromKey] * MaxMul + MaxAdd
            public string MinFromKey;
            public double MinMul;
            public double MinAdd;

            public string MaxFromKey;
            public double MaxMul;
            public double MaxAdd;

            // text inputs
            public string DefaultText;
            public int MaxLength;

            // buttons
            public string Call;
            public bool Primary;
            public bool Danger;

            public static UiControl Slider(string key, string label, double min, double max, double step, double defaultValue,
                string minFromKey = null, double minMul = 1.0, double minAdd = 0.0,
                string maxFromKey = null, double maxMul = 1.0, double maxAdd = 0.0)
                => new UiControl
                {
                    Kind = "slider",
                    Key = key,
                    Label = label,
                    Min = min,
                    Max = max,
                    Step = step,
                    DefaultValue = defaultValue,

                    MinFromKey = minFromKey,
                    MinMul = minMul,
                    MinAdd = minAdd,
                    MaxFromKey = maxFromKey,
                    MaxMul = maxMul,
                    MaxAdd = maxAdd
                };

            public static UiControl Number(string key, string label, double min, double max, double step, double defaultValue,
                string minFromKey = null, double minMul = 1.0, double minAdd = 0.0,
                string maxFromKey = null, double maxMul = 1.0, double maxAdd = 0.0)
                => new UiControl
                {
                    Kind = "number",
                    Key = key,
                    Label = label,
                    Min = min,
                    Max = max,
                    Step = step,
                    DefaultValue = defaultValue,

                    MinFromKey = minFromKey,
                    MinMul = minMul,
                    MinAdd = minAdd,
                    MaxFromKey = maxFromKey,
                    MaxMul = maxMul,
                    MaxAdd = maxAdd
                };

            // Checkbox: stored as numeric 0/1 via DefaultValue
            public static UiControl Checkbox(string key, string label, bool defaultOn = false)
                => new UiControl
                {
                    Kind = "checkbox",
                    Key = key,
                    Label = label,
                    DefaultValue = defaultOn ? 1.0 : 0.0
                };

            public static UiControl Text(string key, string label, string defaultText, int maxLength = 256)
                => new UiControl { Kind = "text", Key = key, Label = label, DefaultText = defaultText, MaxLength = maxLength };

            public static UiControl TextAlpha(string key, string label, string defaultText, int maxLength = 64)
                => new UiControl { Kind = "textalpha", Key = key, Label = label, DefaultText = defaultText, MaxLength = maxLength };

            public static UiControl Button(string key, string label, string call = null, bool primary = false, bool danger = false, bool fullRow = true)
                => new UiControl { Kind = "button", Key = key, Label = label, Call = call, Primary = primary, Danger = danger, FullRow = fullRow };
        }
    }
}