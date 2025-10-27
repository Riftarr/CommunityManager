using System.Collections.Generic;

namespace CommunityManager
{
    internal static class ModUiDeclaration
    {
        public static object CreateModUiSpec()
        {
            return new ModUiDeclaration.ModUiSpecLoose
            {
                ModId = "riftarr.communitymanager",
                ButtonName = "Community Manager",
                PanelName = "Community Manager Controls",
                JsonName = "CommunityManager.json",
                Controls = ModUiDeclaration.Controls
            };
        }

        private static readonly List<ModUiDeclaration.UiControl> Controls = new List<ModUiDeclaration.UiControl>
        {
            ModUiDeclaration.UiControl.Slider("StudentMultiplier", "Student Recruit Multiplier: [value]", 1f, 20f, 1f, ManagerConfig.StudentMultiplier),
            ModUiDeclaration.UiControl.Number("StudentMultiplier", "Student Recruit Multiplier - Type Exact", 1f, 20f, ManagerConfig.StudentMultiplier),
            ModUiDeclaration.UiControl.Slider("TuitionMultiplier", "Tuition Multiplier: [value]", 0.01f, 5f, 0.01f, ManagerConfig.TuitionMultiplier),
            ModUiDeclaration.UiControl.Number("TuitionMultiplier", "Tuition Multiplier - Type Exact", 0.01f, 5f, ManagerConfig.TuitionMultiplier),
            ModUiDeclaration.UiControl.Slider("StudentVictoryAdjusted", "Enable Student Victory Requirements to be Adjusted: [value] (1 = true | 0 = false)", 0f, 1f, 1f, ManagerConfig.StudentVictoryAdjusted)
        };

        private sealed class ModUiSpecLoose
        {
            public string ModId;

            public string ButtonName;

            public string PanelName;

            public string JsonName;

            public List<ModUiDeclaration.UiControl> Controls;
        }

        private sealed class UiControl
        {
            public static ModUiDeclaration.UiControl Slider(string key, string label, double min, double max, double step, double defaultValue)
            {
                return new ModUiDeclaration.UiControl
                {
                    Kind = "slider",
                    Key = key,
                    Label = label,
                    Min = min,
                    Max = max,
                    Step = step,
                    DefaultValue = defaultValue
                };
            }

            public static ModUiDeclaration.UiControl Number(string key, string label, double min, double max, double defaultValue)
            {
                return new ModUiDeclaration.UiControl
                {
                    Kind = "number",
                    Key = key,
                    Label = label,
                    Min = min,
                    Max = max,
                    DefaultValue = defaultValue
                };
            }

            public static ModUiDeclaration.UiControl TextAlpha(string key, string label, string defaultText, int maxLength = 64)
            {
                return new ModUiDeclaration.UiControl
                {
                    Kind = "textalpha",
                    Key = key,
                    Label = label,
                    DefaultText = defaultText,
                    MaxLength = maxLength
                };
            }

            public string Kind;

            public string Key;

            public string Label;

            public double Min;

            public double Max;

            public double Step;

            public double DefaultValue;

            public string DefaultText;

            public int MaxLength;
        }
    }
}
