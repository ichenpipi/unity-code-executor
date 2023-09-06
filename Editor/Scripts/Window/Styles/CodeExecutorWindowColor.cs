using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ChenPipi.CodeExecutor.Editor
{

    /// <summary>
    /// 窗口
    /// </summary>
    public partial class CodeExecutorWindow
    {

        private static Color separatorColor => Theme.GetColor("SeparatorColor");

        private static Color dragLineColor => Theme.GetColor("DragLineColor");

        private static Color newItemNormalBgColor => Theme.GetColor("NewItemNormalBgColor");
        private static Color newItemActiveBgColor => Theme.GetColor("NewItemActiveBgColor");

        private static Color textFieldNormalBgColor => Theme.GetColor("TextFieldNormalBgColor");
        private static Color textFieldReadOnlyBgColor => Theme.GetColor("TextFieldReadOnlyBgColor");

        private static Color textFieldNormalBorderColor => Theme.GetColor("TextFieldNormalBorderColor");
        private static Color textFieldReadOnlyBorderColor => Theme.GetColor("TextFieldReadOnlyBorderColor");

        private static Color dropTipBgColor => Theme.GetColor("DropTipBgColor");
        private static Color dropTipBorderColor => Theme.GetColor("DropTipBorderColor");
        private static Color dropTipTextColor => Theme.GetColor("DropTipTextColor");

        private static class Theme
        {

            public static Color GetColor(string name)
            {
                if (isDarkTheme)
                {
                    if (s_DarkColor.TryGetValue(name, out Color color)) return color;
                }
                else
                {
                    if (s_LightColor.TryGetValue(name, out Color color)) return color;
                }
                return s_DefaultColor;
            }

            public static bool isDarkTheme => EditorGUIUtility.isProSkin;

            private static readonly Color s_DefaultColor = Color.black;

            private static readonly Dictionary<string, Color> s_DarkColor = new Dictionary<string, Color>()
            {
                { "SeparatorColor", new Color(35 / 255f, 35 / 255f, 35 / 255f, 1) },

                { "DragLineColor", new Color(89 / 255f, 89 / 255f, 89 / 255f, 1) },

                { "NewItemNormalBgColor", new Color(0, 0, 0, 0) },
                { "NewItemActiveBgColor", new Color(77 / 255f, 77 / 255f, 77 / 255f, 1) },

                { "TextFieldNormalBgColor", new Color(42 / 255f, 42 / 255f, 42 / 255f, 1) },
                { "TextFieldReadOnlyBgColor", new Color(63 / 255f, 63 / 255f, 63 / 255f, 1) },

                { "TextFieldNormalBorderColor", new Color(33 / 255f, 33 / 255f, 33 / 255f, 1) },
                { "TextFieldReadOnlyBorderColor", new Color(45 / 255f, 45 / 255f, 45 / 255f, 1) },

                { "DropTipBgColor", new Color(0f, 0f, 0f, 80 / 255f) },
                { "DropTipBorderColor", new Color(210 / 255f, 210 / 255f, 210 / 255f, 1) },
                { "DropTipTextColor", new Color(210 / 255f, 210 / 255f, 210 / 255f, 1) },
            };

            private static readonly Dictionary<string, Color> s_LightColor = new Dictionary<string, Color>()
            {
                { "SeparatorColor", new Color(153 / 255f, 153 / 255f, 153 / 255f, 1) },

                { "DragLineColor", new Color(153 / 255f, 153 / 255f, 153 / 255f, 1) },

                { "NewItemNormalBgColor", new Color(0, 0, 0, 0) },
                { "NewItemActiveBgColor", new Color(174 / 255f, 174 / 255f, 174 / 255f, 1) },

                { "TextFieldNormalBgColor", new Color(240 / 255f, 240 / 255f, 240 / 255f, 1) },
                { "TextFieldReadOnlyBgColor", new Color(209 / 255f, 209 / 255f, 209 / 255f, 1) },

                { "TextFieldNormalBorderColor", new Color(153 / 255f, 153 / 255f, 153 / 255f, 1) },
                { "TextFieldReadOnlyBorderColor", new Color(153 / 255f, 153 / 255f, 153 / 255f, 1) },

                { "DropTipBgColor", new Color(0f, 0f, 0f, 160 / 255f) },
                { "DropTipBorderColor", new Color(210 / 255f, 210 / 255f, 210 / 255f, 1) },
                { "DropTipTextColor", new Color(210 / 255f, 210 / 255f, 210 / 255f, 1) },
            };

        }

    }

}
