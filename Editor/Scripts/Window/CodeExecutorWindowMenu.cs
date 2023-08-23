using System.IO;
using UnityEditor;
using UnityEngine;

namespace ChenPipi.CodeExecutor.Editor
{

    /// <summary>
    /// 窗口
    /// </summary>
    public partial class CodeExecutorWindow : EditorWindow, IHasCustomMenu
    {

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Built-in Execution Mode/C#"), enableBuiltinExecModeCSharp, Menu_BuiltinExecutionModeCSharp);
            menu.AddItem(new GUIContent("Built-in Execution Mode/XLua (Standalone)"), enableBuiltinExecModeXLua, Menu_BuiltinExecutionModeXLua);
            menu.AddItem(new GUIContent("Document: How to register execution modes?"), false, Menu_Document);
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Reload"), false, Menu_Reload);
            menu.AddItem(new GUIContent("Show Serialized Data File"), false, Menu_ShowSerializedDataFile);
            menu.AddItem(new GUIContent("Show Serialized Settings File"), false, Menu_ShowSerializedSettingsFile);
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Clear Data ⚠️"), false, Menu_ClearData);
            menu.AddItem(new GUIContent("Reset Settings ⚠️"), false, Menu_ResetSettings);
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("About/陈皮皮 (ichenpipi)"), false, Menu_HomePage);
            menu.AddItem(new GUIContent("About/Project Home Page (Github)"), false, Menu_ProjectHomePageGithub);
            menu.AddItem(new GUIContent("About/Project Home Page (Gitee)"), false, Menu_ProjectHomePageGitee);
        }

        public static void ReRegisterExecModes()
        {
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
        }

        public static bool enableBuiltinExecModeCSharp
        {
            get => CodeExecutorSettings.enableBuiltinExecModeCSharp;
            set
            {
                CodeExecutorSettings.enableBuiltinExecModeCSharp = value;
                ReRegisterExecModes();
            }
        }

        public static bool enableBuiltinExecModeXLua
        {
            get => CodeExecutorSettings.enableBuiltinExecModeXLua;
            set
            {
                CodeExecutorSettings.enableBuiltinExecModeXLua = value;
                ReRegisterExecModes();
            }
        }

        private void Menu_BuiltinExecutionModeCSharp()
        {
            enableBuiltinExecModeCSharp = !enableBuiltinExecModeCSharp;
        }

        private void Menu_BuiltinExecutionModeXLua()
        {
            enableBuiltinExecModeXLua = !enableBuiltinExecModeXLua;
        }

        private void Menu_Document()
        {
            Application.OpenURL("https://github.com/ichenpipi/unity-code-executor#readme");
        }

        private void Menu_Reload()
        {
            // 加载
            CodeExecutorManager.ReloadData();
            CodeExecutorManager.ReloadSettings();
            // 应用
            ApplySettings();
            UpdateContent();
            // 刷新注册模式
            ReRegisterExecModes();
        }

        private void Menu_ShowSerializedDataFile()
        {
            if (!File.Exists(CodeExecutorData.SerializedFilePath))
            {
                EditorUtility.DisplayDialog(
                    "[Code Executor]",
                    $"Serialized file not found: \'{CodeExecutorData.SerializedFilePath}\'",
                    "OK"
                );
                return;
            }
            EditorUtility.RevealInFinder(CodeExecutorData.SerializedFilePath);
        }

        private void Menu_ShowSerializedSettingsFile()
        {
            if (!File.Exists(CodeExecutorSettings.SerializedFilePath))
            {
                EditorUtility.DisplayDialog(
                    "[Code Executor]",
                    $"Serialized file not found: \'{CodeExecutorData.SerializedFilePath}\'",
                    "OK"
                );
                return;
            }
            EditorUtility.RevealInFinder(CodeExecutorSettings.SerializedFilePath);
        }

        private void Menu_ClearData()
        {
            bool isOk = EditorUtility.DisplayDialog(
                "[Code Executor] Clear Data",
                "Are you sure to clear the data? This operation cannot be undone!",
                "Confirm!",
                "Cancel"
            );
            if (isOk) CodeExecutorManager.ClearData();
        }

        private void Menu_ResetSettings()
        {
            CodeExecutorManager.ResetSettings();
            ApplySettings();
        }

        private void Menu_HomePage()
        {
            Application.OpenURL("https://github.com/ichenpipi");
        }

        private void Menu_ProjectHomePageGithub()
        {
            Application.OpenURL("https://github.com/ichenpipi/unity-code-executor");
        }

        private void Menu_ProjectHomePageGitee()
        {
            Application.OpenURL("https://gitee.com/ichenpipi/unity-code-executor");
        }

    }

}
