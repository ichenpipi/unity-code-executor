using UnityEngine;
using UnityEngine.UIElements;

namespace ChenPipi.CodeExecutor.Editor
{

    /// <summary>
    /// 窗口
    /// </summary>
    public partial class CodeExecutorWindow
    {

        #region Content Initialization

        /// <summary>
        /// 内容
        /// </summary>
        private VisualElement m_Content = null;

        /// <summary>
        /// 内容分栏面板
        /// </summary>
        private TwoPaneSplitView m_ContentSplitView = null;

        /// <summary>
        /// 内容分栏拖拽线
        /// </summary>
        private VisualElement m_ContentDragLine = null;

        /// <summary>
        /// 初始化
        /// </summary>
        private void InitContent()
        {
            // 绑定元素
            m_Content = rootVisualElement.Q<VisualElement>("Content");
            {
                m_Content.style.flexBasis = Length.Percent(100);
                m_Content.style.marginTop = 0;
                m_Content.style.marginBottom = 0;
                m_Content.style.marginLeft = 0;
                m_Content.style.marginRight = 0;
                m_Content.style.paddingTop = 0;
                m_Content.style.paddingBottom = 0;
                m_Content.style.paddingLeft = 0;
                m_Content.style.paddingRight = 0;
                m_Content.style.flexDirection = FlexDirection.Column;
            }
            // 监听元素尺寸变化
            m_Content.RegisterCallback<GeometryChangedEvent>(OnContentGeometryChangedEventChanged);

            // 内容分栏
            m_ContentSplitView = new TwoPaneSplitView()
            {
                name = "ContentSplitView",
                fixedPaneIndex = 0,
                orientation = TwoPaneSplitViewOrientation.Horizontal,
                style =
                {
                    flexBasis = Length.Percent(100),
                },
            };
            m_Content.Add(m_ContentSplitView);

            // 分栏拖拽线
            {
                m_ContentDragLine = m_ContentSplitView.Q<VisualElement>("unity-dragline-anchor");
                IStyle dragLineStyle = m_ContentDragLine.style;
                // 禁止拖拽线在Hover的时候变颜色
                Color color = dragLineColor;
                m_ContentDragLine.RegisterCallback<MouseEnterEvent>((evt) => dragLineStyle.backgroundColor = color);
                // 拖动拖拽线后保存其位置
                m_ContentDragLine.RegisterCallback<MouseUpEvent>((evt) =>
                {
                    float rootWidth = rootVisualElement.worldBound.width;
                    float leftPaneMinWidth = m_Sidebar.style.minWidth.value.value;
                    float rightPaneMinWidth = m_Detail.style.minWidth.value.value;
                    float dragLinePos = dragLineStyle.left.value.value;
                    if (dragLinePos < leftPaneMinWidth || dragLinePos > rootWidth - rightPaneMinWidth)
                    {
                        dragLinePos = leftPaneMinWidth;
                        dragLineStyle.left = dragLinePos;
                        m_ContentSplitView.fixedPaneInitialDimension = dragLinePos;
                    }
                    CodeExecutorSettings.dragLinePos = dragLinePos;
                });
            }

            // 列表
            InitSidebar();
            // 详情
            InitDetail();
            // 拖放区
            InitDropArea();
        }

        /// <summary>
        /// 元素尺寸变化回调
        /// </summary>
        /// <param name="evt"></param>
        private void OnContentGeometryChangedEventChanged(GeometryChangedEvent evt) { }

        /// <summary>
        /// 内容是否初始化
        /// </summary>
        /// <returns></returns>
        private bool IsContentInited()
        {
            return (m_Content != null);
        }

        #endregion

        #region Data

        /// <summary>
        /// 当前代码段信息引用
        /// </summary>
        private SnippetInfo m_CurrSnippetInfo = null;

        /// <summary>
        /// 当前选中
        /// </summary>
        private string m_SelectedSnippetGuid = null;

        /// <summary>
        /// 更新内容
        /// </summary>
        private void UpdateContent()
        {
            if (!IsContentInited()) return;

            // 加载代码段列表
            UpdateSnippetList();

            // 恢复选中的代码段
            Switch(m_SelectedSnippetGuid);
        }

        /// <summary>
        /// 切换
        /// </summary>
        /// <param name="guid"></param>
        private void Switch(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                Switch(CodeExecutorData.newSnippet);
                return;
            }
            SnippetInfo snippetInfo = CodeExecutorManager.GetSnippet(guid);
            if (snippetInfo == null)
            {
                Switch(CodeExecutorData.newSnippet);
                return;
            }
            Switch(snippetInfo);
        }

        /// <summary>
        /// 切换
        /// </summary>
        /// <param name="snippetInfo"></param>
        private void Switch(SnippetInfo snippetInfo)
        {
            if (snippetInfo == null) return;

            // 是否为新代码条目
            bool isNew = IsNewSnippet(snippetInfo);

            // 保存引用
            m_CurrSnippetInfo = snippetInfo;
            // 记录选择
            m_SelectedSnippetGuid = isNew ? null : snippetInfo.guid;

            // 更新界面状态
            UpdateNewCodeItemStyle(isNew);
            // 更新列表状态
            if (isNew)
            {
                ClearSnippetListSelection(false);
            }
            else
            {
                SetSnippetListSelection(m_SelectedSnippetGuid, false);
            }

            // 设置标题
            if (isNew)
            {
                SetTitleText("New", "*");
            }
            else
            {
                SetTitleText(snippetInfo.name);
            }
            // 设置代码
            SetCodeEditorText(snippetInfo.code);
            // 设置执行模式
            SetExecutionModeText(snippetInfo.mode);
            // 是否可编辑
            SetCodeEditorEditable(isNew);
            // 展示保存按钮
            SetSaveButtonStatus(isNew);
            // 展示拷贝按钮
            SetCopyButtonStatus(!isNew);
        }

        /// <summary>
        /// 是否为新代码段条目
        /// </summary>
        /// <param name="snippetInfo"></param>
        /// <returns></returns>
        private bool IsNewSnippet(SnippetInfo snippetInfo)
        {
            return (snippetInfo == CodeExecutorData.newSnippet);
        }

        #endregion

    }

}
