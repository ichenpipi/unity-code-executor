using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChenPipi.CodeExecutor.Editor
{

    /// <summary>
    /// 窗口
    /// </summary>
    public partial class CodeExecutorWindow
    {

        /// <summary>
        /// 监听快捷键
        /// </summary>
        private void RegisterHotkeys()
        {
            rootVisualElement.RegisterCallback<KeyDownEvent>((evt) =>
            {
                bool stopEvent = true;

                // Ctrl + F
                if (evt.ctrlKey && evt.keyCode == KeyCode.F)
                {
                    FocusToSearchField();
                }
                // Ctrl + C
                else if (evt.ctrlKey && evt.keyCode == KeyCode.C)
                {
                    TrySaveSelectedSnippetsToClipboard();
                }
                // Ctrl + V
                else if (evt.ctrlKey && evt.keyCode == KeyCode.V)
                {
                    List<SnippetInfo> list = TryAddSnippetsFromClipboard();
                    if (list != null && list.Count > 0)
                    {
                        string[] guids = list.Select(o => o.guid).ToArray();
                        Switch(guids.First());
                        SetSnippetListSelection(guids, false);
                    }
                }
                // Ctrl + D
                else if (evt.keyCode == KeyCode.Z) { }
                // Ctrl + Z
                else if (evt.keyCode == KeyCode.Z) { }
                // Ctrl + Shift + Z
                else if (evt.shiftKey && evt.keyCode == KeyCode.Z) { }
                // F2
                else if (evt.keyCode == KeyCode.F2)
                {
                    ListItem item = GetSelectedSnippetListItem();
                    item?.ShowNameTextField();
                }
                // F5
                else if (evt.keyCode == KeyCode.F5)
                {
                    Menu_Reload();
                }
                // Delete / Backspace
                else if (evt.keyCode == KeyCode.Delete || evt.keyCode == KeyCode.Backspace)
                {
                    string[] names = GetSelectedSnippetInfos().Select(v => $"- {v.name}").ToArray();
                    bool isOk = EditorUtility.DisplayDialog(
                        "[Code Executor] Delete snippets",
                        $"Are you sure to delete the following snippets?\n{string.Join("\n", names)}",
                        "Confirm!",
                        "Cancel"
                    );
                    if (isOk)
                    {
                        CodeExecutorManager.RemoveSnippets(GetSelectedSnippetGuids());
                    }
                }
                // 不响应
                else
                {
                    stopEvent = false;
                }

                if (stopEvent)
                {
                    // 阻止事件的默认行为，停止事件传播
                    evt.PreventDefault();
                    evt.StopImmediatePropagation();
                }
            });
        }

    }

}
