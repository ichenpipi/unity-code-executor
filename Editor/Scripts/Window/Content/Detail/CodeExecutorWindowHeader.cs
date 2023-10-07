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
        private TextField m_TitleTextField = null;

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
        /// 复制按钮
        /// </summary>
        private ButtonWithIcon m_DuplicateButton = null;

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
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    justifyContent = Justify.SpaceBetween,
                },
            };
            m_Detail.Add(m_Header);
            // 监听元素尺寸变化
            m_Detail.RegisterCallback<GeometryChangedEvent>(OnHeaderGeometryChangedEventChanged);

            m_TitleTextField = new TextField()
            {
                name = "Title",
                value = string.Empty,
                multiline = true,
                isReadOnly = true,
                style =
                {
                    flexShrink = 1,
                    fontSize = 16,
                    marginLeft = 0,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    unityTextAlign = TextAnchor.UpperLeft,
                    whiteSpace = WhiteSpace.Normal,
                },
            };
            m_Header.Add(m_TitleTextField);
            {
                // 移除输入框的背景和边框
                VisualElement textInput = m_TitleTextField.Q<VisualElement>("unity-text-input");
                textInput.style.backgroundColor = StyleKeyword.None;
                textInput.style.borderTopWidth = 0;
                textInput.style.borderBottomWidth = 0;
                textInput.style.borderLeftWidth = 0;
                textInput.style.borderRightWidth = 0;
            }

            m_HeaderButtonContainer = new VisualElement()
            {
                name = "Buttons",
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexShrink = 0,
                    alignItems = Align.Center,
                    justifyContent = Justify.Center,
                }
            };
            m_Header.Add(m_HeaderButtonContainer);

            // 编辑按钮
            m_EditButton = new ButtonWithIcon()
            {
                name = "EditButton",
                tooltip = "Edit current snippet",
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

            // 复制按钮
            m_DuplicateButton = new ButtonWithIcon()
            {
                name = "DuplicateButton",
                tooltip = "Duplicate current snippet",
                style =
                {
                    height = 20,
                    paddingRight = 2,
                }
            };
            m_HeaderButtonContainer.Add(m_DuplicateButton);
            // 文本
            m_DuplicateButton.SetText("Duplicate");
            m_DuplicateButton.SetTextFontSize(11);
            // 图标
            m_DuplicateButton.SetIcon(PipiUtility.GetIcon("TreeEditor.Duplicate"));
            m_DuplicateButton.SetIconSize(16);
            m_DuplicateButton.iconImage.style.marginRight = -1;
            // 点击回调
            m_DuplicateButton.clicked += OnDuplicateButtonClick;
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
            // 添加新的代码段
            string code = m_CurrSnippetInfo.code;
            string mode = m_CurrSnippetInfo.mode;
            SaveAsNewSnippet(code, "Unnamed", mode);
        }

        /// <summary>
        /// 复制按钮点击回调
        /// </summary>
        private void OnDuplicateButtonClick()
        {
            DuplicateSnippet(m_CurrSnippetInfo);
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
            m_DuplicateButton.style.display = (isShow ? DisplayStyle.Flex : DisplayStyle.None);
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
            m_TitleTextField.value = (suffix == null ? text : text + suffix);
            m_TitleTextField.tooltip = text;
        }

        /// <summary>
        /// 复制代码段
        /// </summary>
        /// <param name="source"></param>
        private void DuplicateSnippet(SnippetInfo source)
        {
            SaveAsNewSnippet(source.code, source.name, source.mode, source.category);
        }

        /// <summary>
        /// 保存为新的代码段
        /// </summary>
        /// <param name="code"></param>
        /// <param name="name"></param>
        /// <param name="mode"></param>
        /// <param name="category"></param>
        /// <param name="triggerRename"></param>
        private void SaveAsNewSnippet(string code, string name = null, string mode = null, string category = null, bool triggerRename = true)
        {
            // 避免重名
            name = CodeExecutorManager.GetNonDuplicateSnippetName(name);
            // 添加新的代码段
            SnippetInfo snippet = CodeExecutorManager.AddSnippet(code, name, mode, category, false);
            // 刷新代码段列表
            UpdateSnippetTreeView();
            // 确保切换到新的代码段条目
            Switch(snippet.guid);
            // 展示条目的名称输入框
            if (triggerRename) BeginSnippetTreeViewItemRename(snippet.guid);
        }

    }

}
