using System.IO;
using System.Text;
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
            menu.AddItem(new GUIContent("Built-in Execution Mode/C#"), CodeExecutorManager.enableBuiltinExecModeCSharp, Menu_BuiltinExecutionModeCSharp);
            menu.AddItem(new GUIContent("Built-in Execution Mode/xLua (Standalone)"), CodeExecutorManager.enableBuiltinExecModeXLua, Menu_BuiltinExecutionModeXLua);
            menu.AddItem(new GUIContent("Re-register Execution Modes"), false, Menu_ReRegisterExecutionModes);
            menu.AddItem(new GUIContent("Document: How to register execution modes?"), false, Menu_Document);
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Reload Data & Settings"), false, Menu_Reload);
            menu.AddItem(new GUIContent("Show Serialized Data File"), false, Menu_ShowSerializedDataFile);
            menu.AddItem(new GUIContent("Show Serialized Settings File"), false, Menu_ShowSerializedSettingsFile);
            menu.AddSeparator(string.Empty);
            menu.AddDisabledItem(new GUIContent($"Code Editor Font Size/Current Font Size: {GetCodeEditorFontSize()}pt"));
            menu.AddItem(new GUIContent("Code Editor Font Size/> + 1pt (Ctrl+MouseUp)"), false, Menu_CodeEditorFontSizeUp);
            menu.AddItem(new GUIContent("Code Editor Font Size/> - 1pt (Ctrl+MouseDown)"), false, Menu_CodeEditorFontSizeDown);
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Import From File"), false, Menu_ImportFromFile);
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Clear Data ⚠️"), false, Menu_ClearData);
            menu.AddItem(new GUIContent("Reset Settings ⚠️"), false, Menu_ResetSettings);
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("About/陈皮皮 (ichenpipi)"), false, Menu_HomePage);
            menu.AddItem(new GUIContent("About/Project Home Page (Github)"), false, Menu_ProjectHomePageGithub);
            menu.AddItem(new GUIContent("About/Project Home Page (Gitee)"), false, Menu_ProjectHomePageGitee);
        }

        private void Menu_BuiltinExecutionModeCSharp()
        {
            CodeExecutorManager.enableBuiltinExecModeCSharp = !CodeExecutorManager.enableBuiltinExecModeCSharp;
        }

        private void Menu_BuiltinExecutionModeXLua()
        {
            CodeExecutorManager.enableBuiltinExecModeXLua = !CodeExecutorManager.enableBuiltinExecModeXLua;
        }

        private void Menu_ReRegisterExecutionModes()
        {
            CodeExecutorManager.ReRegisterExecModes();
        }

        private void Menu_Document()
        {
            Application.OpenURL("https://github.com/ichenpipi/unity-code-executor#readme");
        }

        private void Menu_Reload()
        {
            Reload();
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

        private void Menu_CodeEditorFontSizeUp()
        {
            int newSize = GetCodeEditorFontSize() + 1;
            SetCodeEditorFontSize(newSize, true);
        }

        private void Menu_CodeEditorFontSizeDown()
        {
            int newSize = GetCodeEditorFontSize() - 1;
            SetCodeEditorFontSize(newSize, true);
        }

        private void Menu_ImportFromFile()
        {
            const string title = "Import code from file";
            const string extension = "";
            string directory = Application.dataPath;
            string path = EditorUtility.OpenFilePanel(title, directory, extension);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            string fileText = File.ReadAllText(path, Encoding.UTF8);
            string fileName = new FileInfo(path).Name;
            CodeExecutorManager.AddSnippet(fileText, fileName);
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
