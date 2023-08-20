using UnityEditor.UIElements;
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
        /// 代码段开关
        /// </summary>
        private ToolbarToggle m_SnippetsToggle = null;

        /// <summary>
        /// 初始化
        /// </summary>
        private void InitToolbarSnippetsToggle()
        {
            m_SnippetsToggle = new ToolbarToggle()
            {
                name = "Snippets",
                tooltip = "Show snippet list",
                focusable = false,
                style =
                {
                    flexShrink = 0,
                    width = 25,
                    paddingTop = 0,
                    paddingBottom = 0,
                    paddingLeft = 2,
                    paddingRight = 2,
                    marginLeft = -1,
                    marginRight = 0,
                    left = 0,
                    alignItems = Align.Center,
                    justifyContent = Justify.Center,
                }
            };
            m_Toolbar.Add(m_SnippetsToggle);
            // 处理元素
            {
                VisualElement input = m_SnippetsToggle.Q<VisualElement>("", "unity-toggle__input");
                input.style.flexGrow = 0;
            }
            // 图标
            m_SnippetsToggle.Add(new Image()
            {
                image = PipiUtility.GetIcon("VerticalLayoutGroup Icon"),
                scaleMode = ScaleMode.ScaleToFit,
                style =
                {
                    width = 16,
                }
            });
            // 回调
            m_SnippetsToggle.RegisterValueChangedCallback(OnSnippetsToggleValueChanged);
        }

        /// <summary>
        /// 代码段开关回调
        /// </summary>
        /// <param name="evt"></param>
        private void OnSnippetsToggleValueChanged(ChangeEvent<bool> evt)
        {
            CodeExecutorSettings.showSnippets = evt.newValue;
            ToggleSidebar(evt.newValue);
        }

    }

}
