using System;

namespace ChenPipi.CodeExecutor.Editor
{

    /// <summary>
    /// 设置
    /// </summary>
    public static class CodeExecutorSettings
    {

        [Serializable]
        private class Settings
        {
            public int version = 0;
            public bool showSnippets = true;
            public float dragLinePos = 150f;
            public int fontSize = 12;
            public bool enableBuiltinExecModeCSharp = true;
            public bool enableBuiltinExecModeXLua = true;
            public bool enableBuiltinExecModeXLuaCustom = false;
        }

        private static Settings s_Settings;

        private static Settings settings => (s_Settings ??= GetLocal());

        /// <summary>
        /// 展示代码段列表
        /// </summary>
        public static bool showSnippets
        {
            get => settings.showSnippets;
            set
            {
                settings.showSnippets = value;
                Save();
            }
        }

        /// <summary>
        /// 拖拽线位置
        /// </summary>
        public static float dragLinePos
        {
            get => settings.dragLinePos;
            set
            {
                settings.dragLinePos = value;
                Save();
            }
        }

        /// <summary>
        /// 编辑器字体大小
        /// </summary>
        public static int fontSize
        {
            get => settings.fontSize;
            set
            {
                settings.fontSize = value;
                Save();
            }
        }

        /// <summary>
        /// 启用内置执行模式 C#
        /// </summary>
        public static bool enableBuiltinExecModeCSharp
        {
            get => settings.enableBuiltinExecModeCSharp;
            set
            {
                settings.enableBuiltinExecModeCSharp = value;
                Save();
            }
        }

        /// <summary>
        /// 启用内置执行模式 XLua
        /// </summary>
        public static bool enableBuiltinExecModeXLua
        {
            get => settings.enableBuiltinExecModeXLua;
            set
            {
                settings.enableBuiltinExecModeXLua = value;
                Save();
            }
        }

        /// <summary>
        /// 启用内置执行模式 XLua (Custom)
        /// </summary>
        public static bool enableBuiltinExecModeXLuaCustom
        {
            get => settings.enableBuiltinExecModeXLuaCustom;
            set
            {
                settings.enableBuiltinExecModeXLuaCustom = value;
                Save();
            }
        }

        #region Basic Interface

        /// <summary>
        /// 保存到本地
        /// </summary>
        public static void Save()
        {
            SetLocal(settings);
        }

        /// <summary>
        /// 重新加载
        /// </summary>
        public static void Reload()
        {
            s_Settings = GetLocal();
        }

        /// <summary>
        /// 重置
        /// </summary>
        public static void Reset()
        {
            SetLocal(s_Settings = new Settings());
        }

        #endregion

        #region Serialization & Deserialization

        /// <summary>
        /// 本地序列化文件路径
        /// </summary>
        internal static readonly string SerializedFilePath = string.Format(CodeExecutorManager.LocalFilePathTemplate, "settings");

        /// <summary>
        /// 获取本地序列化的设置
        /// </summary>
        /// <returns></returns>
        private static Settings GetLocal()
        {
            return PipiUtility.GetLocal<Settings>(SerializedFilePath);
        }

        /// <summary>
        /// 将设置序列化到本地
        /// </summary>
        /// <param name="value"></param>
        private static void SetLocal(Settings value)
        {
            PipiUtility.SetLocal(SerializedFilePath, value);
        }

        #endregion

    }

}
