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

        #region Initialization

        /// <summary>
        /// 代码编辑器
        /// </summary>
        private VisualElement m_CodeEditor = null;

        /// <summary>
        /// 代码编辑器滚动视图
        /// </summary>
        private ScrollView m_CodeScrollView = null;

        /// <summary>
        /// 代码编辑器输入框
        /// </summary>
        private TextField m_CodeTextField = null;

        /// <summary>
        /// 代码编辑器输入框
        /// </summary>
        private VisualElement m_CodeTextFieldTextInput = null;

        /// <summary>
        /// 代码编辑器复制到剪切板按钮
        /// </summary>
        private ButtonWithIcon m_CodeEditorClipboardButton = null;

        /// <summary>
        /// 初始化
        /// </summary>
        private void InitCodeEditor()
        {
            // 滚动视图
            m_CodeEditor = new VisualElement()
            {
                name = "CodeEditor",
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    marginTop = 3,
                    marginLeft = 3,
                    marginRight = 3,
                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderTopColor = textFieldNormalBorderColor,
                    borderBottomColor = textFieldNormalBorderColor,
                    borderLeftColor = textFieldNormalBorderColor,
                    borderRightColor = textFieldNormalBorderColor,
                    borderTopLeftRadius = 3,
                    borderTopRightRadius = 3,
                    borderBottomLeftRadius = 3,
                    borderBottomRightRadius = 3,
                }
            };
            m_Detail.Add(m_CodeEditor);

            // 滚动视图
            m_CodeScrollView = new ScrollView()
            {
                name = "ScrollView",
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                }
            };
            m_CodeEditor.Add(m_CodeScrollView);

            // 代码输入框
            m_CodeTextField = new TextField()
            {
                name = "TextField",
                value = string.Empty,
                multiline = true,
                doubleClickSelectsWord = true,
                tripleClickSelectsLine = false,
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    marginTop = 0,
                    marginBottom = 0,
                    marginLeft = 0,
                    marginRight = 0,
                }
            };
            m_CodeScrollView.Add(m_CodeTextField);

            {
                // 输入框文本设为左上对齐
                VisualElement textInput = m_CodeTextFieldTextInput = m_CodeTextField.Q<VisualElement>("unity-text-input");
                textInput.style.unityTextAlign = TextAnchor.UpperLeft;
                textInput.style.whiteSpace = WhiteSpace.Normal;
                // 移除输入框默认的边框
                textInput.style.borderTopWidth = 0;
                textInput.style.borderBottomWidth = 0;
                textInput.style.borderLeftWidth = 0;
                textInput.style.borderRightWidth = 0;
                textInput.style.borderTopLeftRadius = 0;
                textInput.style.borderTopRightRadius = 0;
                textInput.style.borderBottomLeftRadius = 0;
                textInput.style.borderBottomRightRadius = 0;
            }

            // 滚动视图变化回调
            m_CodeScrollView.RegisterCallback<GeometryChangedEvent>(OnCodeScrollViewGeometryChangedEventChanged);

            // 输入框内容变化回调
            m_CodeTextField.RegisterValueChangedCallback(OnCodeTextFieldValueChanged);
            // 输入框键盘按下回调
            m_CodeTextField.RegisterCallback<KeyDownEvent>(OnCodeTextFieldKeyDown);
            // 输入框鼠标滚轮回调
            m_CodeTextField.RegisterCallback<WheelEvent>(OnCodeTextFieldMouseWheel);

            // 复制到剪切板按钮
            m_CodeEditorClipboardButton = new ButtonWithIcon()
            {
                name = "CopyButton",
                tooltip = "Copy code text to clipboard",
                focusable = false,
                style =
                {
                    display = DisplayStyle.None,
                    position = Position.Absolute,
                    top = 1,
                    right = 1,
                    width = 18,
                    height = 18,
                    paddingTop = 1,
                    paddingBottom = 1,
                    paddingLeft = 1,
                    paddingRight = 1,
                    opacity = 0.8f,
                }
            };
            m_CodeEditor.Add(m_CodeEditorClipboardButton);
            // 图标
            m_CodeEditorClipboardButton.SetIcon(PipiUtility.GetIcon("TextAsset Icon"));
            // 点击回调
            m_CodeEditorClipboardButton.clicked += OnCodeEditorClipboardButtonClick;

            // 监听鼠标进入事件
            m_CodeEditor.RegisterCallback<MouseEnterEvent>(OnCodeEditorMouseEnter);
            // 监听鼠标离开事件
            m_CodeEditor.RegisterCallback<MouseLeaveEvent>(OnCodeEditorMouseLeave);
        }

        /// <summary>
        /// 滚动视图变化回调
        /// </summary>
        /// <param name="evt"></param>
        private void OnCodeScrollViewGeometryChangedEventChanged(GeometryChangedEvent evt)
        {
            EditorApplication.delayCall += UpdateCodeTextFieldHeight;

            void UpdateCodeTextFieldHeight()
            {
                if (m_CodeScrollView == null) return;
                float height = m_CodeScrollView.contentViewport.localBound.height;
                m_CodeTextField.style.minHeight = height;
            }
        }

        /// <summary>
        /// 代码输入框内容变化回调
        /// </summary>
        /// <param name="evt"></param>
        private void OnCodeTextFieldValueChanged(ChangeEvent<string> evt)
        {
            if (m_CodeTextField.isReadOnly) return;
            if (m_CurrSnippetInfo == null) return;
            // 更新数据
            string code = m_CodeTextField.value;
            m_CurrSnippetInfo.code = code;
            // 序列化数据
            if (IsNewSnippet(m_CurrSnippetInfo))
            {
                CodeExecutorManager.SetNewSnippetCode(code);
            }
            else
            {
                CodeExecutorManager.SetSnippetCode(m_CurrSnippetInfo.guid, code);
            }
        }

        /// <summary>
        /// 代码输入框键盘按下回调
        /// </summary>
        /// <param name="evt"></param>
        private void OnCodeTextFieldKeyDown(KeyDownEvent evt)
        {
            bool stopEvent = true;

            // 组合键
            if (evt.ctrlKey)
            {
                // Ctrl + Enter
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    OnExecuteButtonClick();
                }
                // Ctrl + Z
                else if (evt.keyCode == KeyCode.Z) { }
                // Ctrl + Shift + Z
                else if (evt.shiftKey && evt.keyCode == KeyCode.Z) { }
                // 不响应
                else
                {
                    stopEvent = false;
                }
            }
            // Esc
            else if (evt.keyCode == KeyCode.Escape) { }
            // Tab
            else if (evt.keyCode == KeyCode.Tab) { }
            // 其他
            else
            {
                // 只读
                if (m_CodeTextField.isReadOnly)
                {
                    ShowNotification("Currently not editable", 0.5f);
                }
                stopEvent = false;
            }

            if (stopEvent)
            {
                // 阻止事件的默认行为，停止事件传播
                evt.PreventDefault();
                evt.StopImmediatePropagation();
            }
        }

        /// <summary>
        /// 代码输入框鼠标滚轮回调
        /// </summary>
        /// <param name="evt"></param>
        private void OnCodeTextFieldMouseWheel(WheelEvent evt)
        {
            // Ctrl + MouseWheel
            if (evt.ctrlKey)
            {
                // 改变字体大小
                const int step = 1;
                float oldSize = m_CodeTextField.style.fontSize.value.value;
                int newSize = (int)(evt.delta.y < 0 ? oldSize + step : oldSize - step);
                newSize = SetCodeEditorFontSize(newSize);
                CodeExecutorSettings.fontSize = newSize;
                ShowNotification($"Font size: {newSize}");
                // 阻止事件的默认行为，停止事件传播
                evt.PreventDefault();
                evt.StopImmediatePropagation();
            }
        }

        /// <summary>
        /// 代码编辑器复制按钮点击回调
        /// </summary>
        private void OnCodeEditorClipboardButtonClick()
        {
            PipiUtility.SaveToClipboard(m_CurrSnippetInfo.code);
            ShowNotification("Copied to clipboard", 1f);
        }

        /// <summary>
        /// 鼠标进入回调
        /// </summary>
        /// <param name="evt"></param>
        private void OnCodeEditorMouseEnter(MouseEnterEvent evt)
        {
            m_CodeEditorClipboardButton.style.display = (
                m_CodeTextField.isReadOnly ? DisplayStyle.Flex : DisplayStyle.None
            );
        }

        /// <summary>
        /// 鼠标离开回调
        /// </summary>
        /// <param name="evt"></param>
        private void OnCodeEditorMouseLeave(MouseLeaveEvent evt)
        {
            m_CodeEditorClipboardButton.style.display = DisplayStyle.None;
        }

        #endregion

        private void SetCodeEditorText(string text, bool notify = false)
        {
            if (notify)
            {
                m_CodeTextField.value = text;
            }
            else
            {
                m_CodeTextField.SetValueWithoutNotify(text);
            }
        }

        private int SetCodeEditorFontSize(int size)
        {
            size = Mathf.Clamp(size, 8, 40);
            m_CodeTextField.style.fontSize = size;
            return size;
        }

        private void SetCodeEditorEditable(bool isEditable, bool focus = false)
        {
            // 输入框只读
            m_CodeTextField.isReadOnly = !isEditable;
            // 输入框背景颜色
            m_CodeTextFieldTextInput.style.backgroundColor = (isEditable ? textFieldNormalBgColor : textFieldReadOnlyBgColor);
            // 边框颜色
            Color borderColor = (isEditable ? textFieldNormalBorderColor : textFieldReadOnlyBorderColor);
            m_CodeScrollView.style.borderTopColor = borderColor;
            m_CodeScrollView.style.borderBottomColor = borderColor;
            m_CodeScrollView.style.borderLeftColor = borderColor;
            m_CodeScrollView.style.borderRightColor = borderColor;

            // 展示编辑按钮
            SetEditButtonStatus(!isEditable);

            // 聚焦并选中文本内容
            if (isEditable && focus)
            {
                m_CodeTextField.Focus();
                m_CodeTextField.SelectAll();
            }
        }

    }

}
