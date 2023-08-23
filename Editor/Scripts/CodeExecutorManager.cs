using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ChenPipi.CodeExecutor.Editor
{

    /// <summary>
    /// 执行模式
    /// </summary>
    public struct ExecutionMode
    {
        public string name;
        public string desc;
        public Func<string, object[]> executor;
    }

    /// <summary>
    /// 管理器
    /// </summary>
    public static class CodeExecutorManager
    {

        #region File Path

        /// <summary>
        /// 基础名称
        /// </summary>
        internal const string BaseName = "CodeExecutor";

        /// <summary>
        /// 项目路径
        /// </summary>
        private static readonly string s_ProjectPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../"));

        /// <summary>
        /// 用户设置路径
        /// </summary>
        private static readonly string s_UserSettingsPath = Path.Combine(s_ProjectPath, "UserSettings");

        /// <summary>
        /// 本地序列化文件路径模板
        /// </summary>
        internal static readonly string LocalFilePathTemplate = Path.GetFullPath(Path.Combine(s_UserSettingsPath, BaseName + ".{0}.json"));

        #endregion

        #region Macro

        // [InitializeOnLoadMethod]
        // public static void SetupMacro()
        // {
        //     string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        //     List<string> define = defines.Split(';').ToList();
        //     if (!define.Contains("LUA_EXECUTOR"))
        //     {
        //         defines += ";LUA_EXECUTOR";
        //     }
        //     PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defines);
        // }

        #endregion

        #region Window

        public static void Open(bool forceReopen = false)
        {
            if (!forceReopen && CodeExecutorWindow.HasOpenInstances())
            {
                CodeExecutorWindow window = CodeExecutorWindow.GetOpenedInstance();
                window.Show(true);
                window.Focus();
            }
            else
            {
                CodeExecutorWindow.CreateInstance();
            }
        }

        /// <summary>
        /// 展示通知
        /// </summary>
        /// <param name="content"></param>
        /// <param name="fadeoutWait"></param>
        public static bool ShowNotification(string content, double fadeoutWait = 1d)
        {
            if (!CodeExecutorWindow.HasOpenInstances())
            {
                return false;
            }
            CodeExecutorWindow window = CodeExecutorWindow.GetOpenedInstance();
            window.ShowNotification(content, fadeoutWait);
            return true;
        }

        #endregion

        #region Data

        /// <summary>
        /// 数据更新
        /// </summary>
        public static event Action dataUpdated;

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="notify"></param>
        private static void SaveData(bool notify)
        {
            CodeExecutorData.Save();
            if (notify) NotifyOfDataUpdated();
        }

        /// <summary>
        /// 清除数据
        /// </summary>
        public static void ClearData()
        {
            CodeExecutorData.Reset();
            NotifyOfDataUpdated();
        }

        /// <summary>
        /// 重新加载数据
        /// </summary>
        public static void ReloadData()
        {
            CodeExecutorData.Reload();
            NotifyOfDataUpdated();
        }

        /// <summary>
        /// 通知更新
        /// </summary>
        private static void NotifyOfDataUpdated()
        {
            dataUpdated?.Invoke();
        }

        #endregion

        #region Data Operation

        /// <summary>
        /// 设置执行模式
        /// </summary>
        /// <param name="mode">执行模式名称</param>
        public static void SetNewSnippetExecMode(string mode)
        {
            // 更新数据
            CodeExecutorData.newSnippet.mode = mode;
            // 保存到本地并通知更新
            SaveData(false);
        }

        /// <summary>
        /// 设置新代码文本
        /// </summary>
        /// <param name="code">代码文本</param>
        public static void SetNewSnippetCode(string code)
        {
            // 更新数据
            CodeExecutorData.newSnippet.code = code;
            // 保存到本地并通知更新
            SaveData(false);
        }

        /// <summary>
        /// 增加代码段
        /// </summary>
        /// <param name="code">代码文本</param>
        /// <param name="name">代码段名称</param>
        /// <param name="mode">执行模式名称</param>
        /// <param name="notify">通知</param>
        /// <returns></returns>
        public static SnippetInfo AddSnippet(string code, string name = null, string mode = null, bool notify = true)
        {
            // 新增数据
            SnippetInfo snippetInfo = CodeExecutorData.AddSnippet(code, name, mode);
            // 保存到本地并通知更新
            SaveData(notify);
            return snippetInfo;
        }

        /// <summary>
        /// 克隆代码段
        /// </summary>
        /// <param name="source">源数据</param>
        /// <param name="notify">通知</param>
        /// <returns></returns>
        public static List<SnippetInfo> CloneSnippets(SnippetInfo[] source, bool notify = true)
        {
            List<SnippetInfo> list = new List<SnippetInfo>();
            for (int i = 0; i < source.Length; i++)
            {
                SnippetInfo snippetInfo = source[i];
                string name = GetNonDuplicateName(snippetInfo.name);
                bool needNotify = notify && (i == source.Length - 1);
                list.Add(AddSnippet(snippetInfo.code, name, snippetInfo.mode, needNotify));
            }
            return list;
        }

        /// <summary>
        /// 获取代码段
        /// </summary>
        /// <param name="guid">GUID</param>
        /// <returns></returns>
        public static SnippetInfo GetSnippet(string guid)
        {
            return CodeExecutorData.GetSnippet(guid);
        }

        /// <summary>
        /// 移除代码段
        /// </summary>
        /// <param name="guid">GUID</param>
        public static void RemoveSnippet(string guid)
        {
            // 移除数据
            CodeExecutorData.RemoveSnippet(guid);
            // 保存到本地并通知更新
            SaveData(true);
        }

        /// <summary>
        /// 移除代码段
        /// </summary>
        /// <param name="guids">GUID</param>
        public static void RemoveSnippets(string[] guids)
        {
            // 移除数据
            foreach (string guid in guids)
            {
                CodeExecutorData.RemoveSnippet(guid);
            }
            // 保存到本地并通知更新
            SaveData(true);
        }

        #endregion

        #region Update Snippet

        /// <summary>
        /// 代码段名称非法字符
        /// </summary>
        private static readonly char[] s_InvalidNameChars = new char[] { '*' };

        /// <summary>
        /// 设置代码段的名称
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool SetSnippetName(string guid, string name)
        {
            SnippetInfo snippetInfo = CodeExecutorData.GetSnippet(guid);
            if (snippetInfo == null)
            {
                return false;
            }

            // 禁止特殊字符
            if (name.IndexOfAny(s_InvalidNameChars) != -1)
            {
                EditorUtility.DisplayDialog(
                    "[CodeExecutor] Invalid display name",
                    $"A valid name can't contain any of the following characters: {s_InvalidNameChars.Join("")}",
                    "OK"
                );
                return false;
            }

            // 更新数据
            snippetInfo.name = name;
            // 保存到本地并通知更新
            SaveData(true);
            return true;
        }

        /// <summary>
        /// 设置代码段的代码文本
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static void SetSnippetCode(string guid, string code)
        {
            SnippetInfo snippetInfo = CodeExecutorData.GetSnippet(guid);
            if (snippetInfo == null) return;
            // 更新数据
            snippetInfo.code = code;
            snippetInfo.editTime = PipiUtility.GetTimestamp();
            // 保存到本地并通知更新
            SaveData(false);
        }

        /// <summary>
        /// 置顶代码段
        /// <param name="guid"></param>
        /// <param name="top"></param>
        /// <param name="notify"></param>
        /// </summary>
        public static void SetSnippetTop(string guid, bool top, bool notify = true)
        {
            SnippetInfo snippetInfo = CodeExecutorData.GetSnippet(guid);
            if (snippetInfo == null) return;
            // 移除数据
            snippetInfo.top = top;
            // 保存到本地并通知更新
            SaveData(notify);
        }

        /// <summary>
        /// 设置代码段的执行模式
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static void SetSnippetExecMode(string guid, string mode)
        {
            SnippetInfo snippetInfo = CodeExecutorData.GetSnippet(guid);
            if (snippetInfo == null) return;
            // 更新数据
            snippetInfo.mode = mode;
            // 保存到本地并通知更新
            SaveData(false);
        }

        #endregion

        #region Settings

        /// <summary>
        /// 重置设置
        /// </summary>
        public static void ResetSettings()
        {
            CodeExecutorSettings.Reset();
        }

        /// <summary>
        /// 重新加载设置
        /// </summary>
        public static void ReloadSettings()
        {
            CodeExecutorSettings.Reload();
        }

        #endregion

        #region Execution Mode

        /// <summary>
        /// 执行模式注册表更新
        /// </summary>
        public static event Action execModeUpdated;

        /// <summary>
        /// 默认执行模式
        /// </summary>
        public static readonly ExecutionMode DefaultExecMode = new ExecutionMode()
        {
            name = "None",
            desc = string.Empty,
            executor = null,
        };

        /// <summary>
        /// 执行模式注册表
        /// </summary>
        internal static readonly Dictionary<string, ExecutionMode> ExecutionModes = new Dictionary<string, ExecutionMode>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 注册执行模式
        /// </summary>
        /// <param name="mode">执行模式对象</param>
        public static void RegisterExecMode(ExecutionMode mode)
        {
            string modeName = mode.name;
            if (string.IsNullOrEmpty(modeName))
            {
                Debug.LogError($"[CodeExecutor] Cannot register execution mode without a name!");
                return;
            }

            if (modeName.Equals(DefaultExecMode.name, StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogError($"[CodeExecutor] Cannot register execution mode with name '{modeName}'!");
                return;
            }

            if (ExecutionModes.ContainsKey(modeName))
            {
                Debug.LogError($"[CodeExecutor] A Execution mode named '{modeName}' is already registered!");
                return;
            }

            ExecutionModes.Add(modeName, mode);
            execModeUpdated?.Invoke();

            Debug.Log($"<color=#00FF00>[CodeExecutor] Execution mode '{modeName}' registered!</color>\n");
        }

        /// <summary>
        /// 取消注册执行模式
        /// </summary>
        /// <param name="modeName"></param>
        public static void UnregisterExecMode(string modeName)
        {
            if (string.IsNullOrEmpty(modeName))
            {
                return;
            }
            ExecutionModes.Remove(modeName);
            execModeUpdated?.Invoke();
            Debug.Log($"<color=#00FF00>[CodeExecutor] Execution mode '{modeName}' is unregistered!</color>\n");
        }

        /// <summary>
        /// 取消注册所有执行模式
        /// </summary>
        public static void UnregisterAllExecModes()
        {
            ExecutionModes.Clear();
            execModeUpdated?.Invoke();
            Debug.Log($"<color=#00FF00>[CodeExecutor] All execution modes are unregistered!</color>\n");
        }

        /// <summary>
        /// 是否有指定名称的执行模式
        /// </summary>
        /// <param name="modeName"></param>
        /// <returns></returns>
        public static bool HasExecMode(string modeName)
        {
            if (string.IsNullOrEmpty(modeName))
            {
                return false;
            }
            return ExecutionModes.TryGetValue(modeName, out ExecutionMode _);
        }

        /// <summary>
        /// 获取指定名称的执行模式
        /// </summary>
        /// <param name="modeName"></param>
        /// <returns></returns>
        public static ExecutionMode? GetExecMode(string modeName)
        {
            if (string.IsNullOrEmpty(modeName))
            {
                return null;
            }
            ExecutionModes.TryGetValue(modeName, out ExecutionMode mode);
            return mode;
        }

        #endregion

        #region Execution

        /// <summary>
        /// 执行代码
        /// </summary>
        /// <param name="codeText">代码段文本</param>
        /// <param name="modeName">执行模式名称</param>
        /// <returns></returns>
        internal static object[] ExecuteCode(string codeText, string modeName)
        {
            if (!ExecutionModes.TryGetValue(modeName, out ExecutionMode mode))
            {
                Debug.LogError($"[CodeExecutor] No execution mode named '{modeName}' was found!");
                return null;
            }

            if (mode.executor == null)
            {
                Debug.LogError($"[CodeExecutor] Invalid executor for execution mode '{modeName}'!");
                return null;
            }

            return mode.executor.Invoke(codeText);
        }

        #endregion

        #region Naming

        /// <summary>
        /// 命名格式分隔符
        /// </summary>
        internal const int NameMaxLength = 30;

        /// <summary>
        /// 命名格式分隔符
        /// </summary>
        internal const string NamingSchemeSeparator = "_";

        /// <summary>
        /// 命名格式正则
        /// </summary>
        internal static readonly Regex NamingSchemeRegex = new Regex($"^(.*?){NamingSchemeSeparator}(\\d+)$");

        /// <summary>
        /// 获取不重复的名称
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static string GetNonDuplicateName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = "Unnamed";
            }

            name = name.Substring(0, Mathf.Min(name.Length, NameMaxLength));

            Match match = NamingSchemeRegex.Match(name);
            string baseName = (!match.Success ? name : match.Groups[1].Value);
            int.TryParse(match.Groups[2].Value, out int count);
            while (CodeExecutorData.HasSnippetWithName(name))
            {
                name = $"{baseName}{NamingSchemeSeparator}{++count}";
            }

            return name;
        }

        #endregion

    }

}
