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
            menu.AddItem(new GUIContent("Built-in Execution Mode/C#"), CodeExecutorSettings.enableBuiltinExecModeCSharp, Menu_EnableBuiltinExecutionModeCSharp);
            menu.AddItem(new GUIContent("Built-in Execution Mode/XLua (Standalone)"), CodeExecutorSettings.enableBuiltinExecModeXLua, Menu_EnableBuiltinExecutionModeXLua);
            menu.AddItem(
                new GUIContent("Built-in Execution Mode/XLua (Custom)"),
                (CodeExecutorSettings.enableBuiltinExecModeXLua && CodeExecutorSettings.enableBuiltinExecModeXLuaCustom),
                Menu_EnableBuiltinExecutionModeXLuaCustom
            );
            menu.AddItem(new GUIContent("Document: How to register execution mode?"), false, Menu_Document);
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

        private void Menu_EnableBuiltinExecutionModeCSharp()
        {
            CodeExecutorSettings.enableBuiltinExecModeCSharp = !CodeExecutorSettings.enableBuiltinExecModeCSharp;
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
        }

        private void Menu_EnableBuiltinExecutionModeXLua()
        {
            CodeExecutorSettings.enableBuiltinExecModeXLua = !CodeExecutorSettings.enableBuiltinExecModeXLua;
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
        }

        private void Menu_EnableBuiltinExecutionModeXLuaCustom()
        {
            if (!CodeExecutorSettings.enableBuiltinExecModeXLua)
            {
                return;
            }
            CodeExecutorSettings.enableBuiltinExecModeXLuaCustom = !CodeExecutorSettings.enableBuiltinExecModeXLuaCustom;
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
        }

        private void Menu_Document()
        {
            Application.OpenURL("https://github.com/ichenpipi/unity-code-executor#readme");
        }

        private void Menu_Reload()
        {
            CodeExecutorManager.ReloadData();
            CodeExecutorManager.ReloadSettings();
            ApplySettings();
            UpdateContent();
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
