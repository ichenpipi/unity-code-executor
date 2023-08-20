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
        /// 顶栏
        /// </summary>
        private VisualElement m_Header = null;

        /// <summary>
        /// 标题标签
        /// </summary>
        private Label m_TitleLabel = null;

        /// <summary>
        /// 按钮容器
        /// </summary>
        private VisualElement m_HeaderButtonContainer = null;

        /// <summary>
        /// 保存按钮
        /// </summary>
        private ButtonWithIcon m_SaveButton = null;

        /// <summary>
        /// 编辑按钮
        /// </summary>
        private ButtonWithIcon m_EditButton = null;

        /// <summary>
        /// 拷贝按钮
        /// </summary>
        private ButtonWithIcon m_CopyButton = null;

        /// <summary>
        /// 初始化
        /// </summary>
        private void InitHeadline()
        {
            // 顶栏
            m_Header = new VisualElement()
            {
                name = "Header",
                style =
                {
                    minHeight = 30,
                    paddingTop = 2,
                    paddingBottom = 2,
                    paddingLeft = 3,
                    paddingRight = 3,
                    flexDirection = FlexDirection.Column,
                    alignItems = Align.FlexStart,
                    justifyContent = Justify.Center,
                },
            };
            m_Detail.Add(m_Header);
            // 监听元素尺寸变化
            m_Detail.RegisterCallback<GeometryChangedEvent>(OnHeaderGeometryChangedEventChanged);

            // 标题
            m_TitleLabel = new Label()
            {
                name = "Title",
                text = string.Empty,
                style =
                {
                    fontSize = 16,
                    marginLeft = 0,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    unityTextAlign = TextAnchor.UpperLeft,
                    whiteSpace = WhiteSpace.Normal,
                },
            };
            m_Header.Add(m_TitleLabel);

            m_HeaderButtonContainer = new VisualElement()
            {
                name = "Buttons",
                style =
                {
                    flexDirection = FlexDirection.Row,
                    position = Position.Absolute,
                    right = 3,
                    alignItems = Align.Center,
                    justifyContent = Justify.Center,
                }
            };
            m_Header.Add(m_HeaderButtonContainer);

            // 编辑按钮
            m_EditButton = new ButtonWithIcon()
            {
                name = "EditButton",
                tooltip = "Edit snippet",
                focusable = false,
                style =
                {
                    height = 20,
                }
            };
            m_HeaderButtonContainer.Add(m_EditButton);
            // 文本
            m_EditButton.SetText("Edit");
            m_EditButton.SetTextFontSize(11);
            // 图标
            m_EditButton.SetIcon(PipiUtility.GetIcon("editicon.sml"));
            m_EditButton.SetIconSize(12);
            m_EditButton.iconImage.style.marginLeft = 2;
            m_EditButton.iconImage.style.marginRight = 2;
            // 点击回调
            m_EditButton.clicked += OnEditButtonClick;

            // 保存按钮
            m_SaveButton = new ButtonWithIcon()
            {
                name = "SaveButton",
                tooltip = "Save as new snippet",
                focusable = false,
                style =
                {
                    height = 20,
                }
            };
            m_HeaderButtonContainer.Add(m_SaveButton);
            // 文本
            m_SaveButton.SetText("Save");
            m_SaveButton.SetTextFontSize(11);
            // 图标
            m_SaveButton.SetIcon(PipiUtility.GetIcon("SaveAs"));
            m_SaveButton.SetIconSize(16);
            // 点击回调
            m_SaveButton.clicked += OnSaveButtonClick;

            // 拷贝按钮
            m_CopyButton = new ButtonWithIcon()
            {
                name = "CopyButton",
                tooltip = "Copy as new snippet",
                style =
                {
                    height = 20,
                    paddingRight = 2,
                }
            };
            m_HeaderButtonContainer.Add(m_CopyButton);
            // 文本
            m_CopyButton.SetText("Copy");
            m_CopyButton.SetTextFontSize(11);
            // 图标
            m_CopyButton.SetIcon(PipiUtility.GetIcon("TreeEditor.Duplicate"));
            m_CopyButton.SetIconSize(16);
            m_CopyButton.iconImage.style.marginLeft = -1;
            m_CopyButton.iconImage.style.marginRight = -2;
            // 点击回调
            m_CopyButton.clicked += OnCopyButtonClick;
        }

        /// <summary>
        /// 元素尺寸变化回调
        /// </summary>
        /// <param name="evt"></param>
        private void OnHeaderGeometryChangedEventChanged(GeometryChangedEvent evt)
        {
            bool isNarrow = (m_Header.localBound.width <= 200);
            m_HeaderButtonContainer.style.display = (isNarrow ? DisplayStyle.None : DisplayStyle.Flex);
        }

        /// <summary>
        /// 保存按钮点击回调
        /// </summary>
        private void OnSaveButtonClick()
        {
            string code = m_CurrSnippetInfo.code;
            string mode = m_CurrSnippetInfo.mode;

            // 清空新代码条目
            // CodeExecutorManager.SetNewSnippetCode(string.Empty);

            // 添加新的代码段
            SaveAsNewSnippet(code, "Unnamed", mode);
        }

        /// <summary>
        /// 拷贝按钮点击回调
        /// </summary>
        private void OnCopyButtonClick()
        {
            // 添加新的代码段
            SaveAsNewSnippet(m_CurrSnippetInfo.code, m_CurrSnippetInfo.name, m_CurrSnippetInfo.mode);
        }

        /// <summary>
        /// 编辑按钮点击回调
        /// </summary>
        private void OnEditButtonClick()
        {
            SetCodeEditorEditable(true, true);
        }

        /// <summary>
        /// 设置保存按钮状态
        /// </summary>
        /// <param name="isShow"></param>
        private void SetSaveButtonStatus(bool isShow)
        {
            m_SaveButton.style.display = (isShow ? DisplayStyle.Flex : DisplayStyle.None);
        }

        /// <summary>
        /// 设置拷贝按钮状态
        /// </summary>
        /// <param name="isShow"></param>
        private void SetCopyButtonStatus(bool isShow)
        {
            m_CopyButton.style.display = (isShow ? DisplayStyle.Flex : DisplayStyle.None);
        }

        /// <summary>
        /// 设置编辑按钮状态
        /// </summary>
        /// <param name="isShow"></param>
        private void SetEditButtonStatus(bool isShow)
        {
            m_EditButton.style.display = (isShow ? DisplayStyle.Flex : DisplayStyle.None);
        }

        /// <summary>
        /// 设置标题文本
        /// </summary>
        /// <param name="text"></param>
        /// <param name="suffix"></param>
        private void SetTitleText(string text, string suffix = null)
        {
            m_TitleLabel.text = (suffix == null ? text : text + suffix);
            m_TitleLabel.tooltip = text;
        }

        /// <summary>
        /// 保存为新的代码段
        /// </summary>
        /// <param name="code"></param>
        /// <param name="name"></param>
        /// <param name="mode"></param>
        private void SaveAsNewSnippet(string code, string name = null, string mode = null)
        {
            // 避免重名
            name = CodeExecutorManager.GetNonDuplicateName(name);
            // 添加新的代码段
            SnippetInfo snippetInfo = CodeExecutorManager.AddSnippet(code, name, mode, false);
            // 刷新代码段列表
            UpdateSnippetList();
            // 确保切换到新的代码段条目
            Switch(snippetInfo.guid);
            // 延迟执行，等待列表生成
            EditorApplication.delayCall = () =>
            {
                // 确保切换到新的代码段条目
                Switch(snippetInfo.guid);
                // 展示条目的名称输入框
                ListItem listItem = GetSnippetListItem(snippetInfo.guid);
                listItem?.ShowNameTextField();
            };
        }

    }

}
