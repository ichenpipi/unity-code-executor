using System;
using System.Collections.Generic;
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

        #region Asset

        /// <summary>
        /// 视觉树资源
        /// </summary>
        [SerializeField] private VisualTreeAsset visualTree = null;

        #endregion

        #region Instance

        /// <summary>
        /// 窗口标题
        /// </summary>
        private const string k_Title = "Code Executor";

        /// <summary>
        /// 是否有已打开的窗口实例
        /// </summary>
        /// <returns></returns>
        public static bool HasOpenInstances()
        {
            return HasOpenInstances<CodeExecutorWindow>();
        }

        /// <summary>
        /// 获取已打开的窗口实例
        /// </summary>
        /// <returns></returns>
        public static CodeExecutorWindow GetOpenedInstance()
        {
            return HasOpenInstances() ? GetWindow<CodeExecutorWindow>() : null;
        }

        /// <summary>
        /// 创建窗口实例
        /// </summary>
        /// <returns></returns>
        public static CodeExecutorWindow CreateInstance()
        {
            // 销毁已存在的实例
            CodeExecutorWindow window = GetOpenedInstance();
            if (window != null)
            {
                window.Close();
            }
            // 创建新的的实例
            window = CreateWindow<CodeExecutorWindow>();
            window.titleContent = new GUIContent()
            {
                text = k_Title,
                image = PipiUtility.GetIcon("BuildSettings.Stadia"),
            };
            window.minSize = new Vector2(100, 100);
            window.SetSize(600, 500);
            window.SetCenter();
            return window;
        }

        /// <summary>
        /// 设置窗口尺寸
        /// </summary>
        /// <param name="width">宽</param>
        /// <param name="height">高</param>
        public void SetSize(int width, int height)
        {
            Rect pos = position;
            pos.width = width;
            pos.height = height;
            position = pos;
        }

        /// <summary>
        /// 使窗口居中（基于 Unity 编辑器主窗口）
        /// </summary>
        /// <param name="offsetX">水平偏移</param>
        /// <param name="offsetY">垂直偏移</param>
        public void SetCenter(int offsetX = 0, int offsetY = 0)
        {
            Rect mainWindowPos = EditorGUIUtility.GetMainWindowPosition();
            Rect pos = position;
            float centerOffsetX = (mainWindowPos.width - pos.width) * 0.5f;
            float centerOffsetY = (mainWindowPos.height - pos.height) * 0.5f;
            pos.x = mainWindowPos.x + centerOffsetX + offsetX;
            pos.y = mainWindowPos.y + centerOffsetY + offsetY;
            position = pos;
        }

        /// <summary>
        /// 展示通知
        /// </summary>
        /// <param name="content"></param>
        /// <param name="fadeoutWait"></param>
        public void ShowNotification(string content, double fadeoutWait = 1d)
        {
            ShowNotification(new GUIContent(content), fadeoutWait);
        }

        #endregion

        #region Lifecycle

        private void Awake()
        {
            CodeExecutorData.Reload();
            CodeExecutorSettings.Reload();
        }

        private void OnEnable()
        {
            CodeExecutorManager.execModeUpdated += Repaint;
            CodeExecutorManager.dataUpdated += RefreshData;
        }

        private void OnDisable()
        {
            CodeExecutorManager.execModeUpdated -= Repaint;
            CodeExecutorManager.dataUpdated -= RefreshData;
        }

        private void CreateGUI()
        {
            // 构建视觉树
            visualTree.CloneTree(rootVisualElement);
            // 生成元素
            Init();
        }

        #endregion

        private void Init()
        {
            // 初始化工具栏
            InitToolbar();
            // 初始化内容
            InitContent();

            // 监听快捷键
            RegisterHotkeys();

            // 刷新数据
            RefreshData();

            // 应用设置
            ApplySettings();
        }

        #region Data

        /// <summary>
        /// 刷新数据
        /// </summary>
        private void RefreshData()
        {
            if (!IsContentInited()) return;

            // 更新内容
            UpdateContent();
        }

        #endregion

        #region Settings

        private void ApplySettings()
        {
            if (!IsContentInited()) return;

            // 代码段列表
            ApplySettings_ShowSnippets();
            // 代码编辑器
            ApplySettings_CodeEditor();
        }

        private void ApplySettings_ShowSnippets()
        {
            if (!IsContentInited()) return;

            ToggleSidebar(CodeExecutorSettings.showSnippets);
        }

        private void ApplySettings_DragLine()
        {
            if (!IsContentInited()) return;

            float rootWidth = rootVisualElement.worldBound.width;
            float leftPaneMinWidth = m_Sidebar.style.minWidth.value.value;
            float rightPaneMinWidth = m_Detail.style.minWidth.value.value;
            float dragLinePos = CodeExecutorSettings.dragLinePos;
            if (dragLinePos < leftPaneMinWidth || dragLinePos > rootWidth - rightPaneMinWidth)
            {
                dragLinePos = leftPaneMinWidth;
            }
            else
            {
                if (m_ContentSplitView.fixedPaneIndex == 1)
                {
                    dragLinePos = rootWidth - dragLinePos;
                }
            }
            m_ContentSplitView.fixedPaneInitialDimension = dragLinePos;
        }

        private void ApplySettings_CodeEditor()
        {
            if (!IsContentInited()) return;

            SetCodeEditorFontSize(CodeExecutorSettings.fontSize);
        }

        #endregion

        #region Clipboard

        /// <summary>
        /// 保存已选择的代码段到系统剪切板
        /// </summary>
        /// <returns></returns>
        private bool TrySaveSelectedSnippetsToClipboard()
        {
            SnippetInfo[] snippetInfos = GetSelectedSnippetInfos();
            if (snippetInfos.Length == 0)
            {
                return false;
            }

            SnippetWrapper wrapper = new SnippetWrapper();
            foreach (SnippetInfo info in snippetInfos)
            {
                wrapper.snippets.Add(new SnippetInfoSimplified()
                {
                    name = info.name,
                    code = info.code,
                    mode = info.mode,
                });
            }

            string data = JsonUtility.ToJson(wrapper, true);
            PipiUtility.SaveToClipboard(data);
            return true;
        }

        /// <summary>
        /// 从系统剪切板中读取并添加代码段
        /// </summary>
        /// <returns></returns>
        private List<SnippetInfo> TryAddSnippetsFromClipboard()
        {
            string data = PipiUtility.GetClipboardContent();
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            data = data.Trim();
            if (!data.StartsWith("{") || !data.EndsWith("}"))
            {
                return null;
            }

            try
            {
                SnippetWrapper wrapper = JsonUtility.FromJson<SnippetWrapper>(data);
                if (wrapper == null || wrapper.snippets.Count == 0)
                {
                    return null;
                }

                List<SnippetInfo> list = new List<SnippetInfo>();
                for (int i = 0; i < wrapper.snippets.Count; i++)
                {
                    SnippetInfoSimplified info = wrapper.snippets[i];
                    string name = CodeExecutorManager.GetNonDuplicateName(info.name);
                    bool notify = (i == wrapper.snippets.Count - 1);
                    list.Add(CodeExecutorManager.AddSnippet(info.code, name, info.mode, notify));
                }

                return list;
            }
            catch
            {
                return null;
            }
        }

        #endregion

    }

}
