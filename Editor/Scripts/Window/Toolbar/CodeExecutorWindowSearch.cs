using System.Collections.Generic;
using System.Text.RegularExpressions;
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
        /// 搜索栏
        /// </summary>
        private ToolbarSearchField m_SearchField = null;

        /// <summary>
        /// 初始化
        /// </summary>
        private void InitToolbarSearchField()
        {
            m_SearchField = new ToolbarSearchField()
            {
                name = "Search",
                value = m_SearchText,
                tooltip = "Search [Ctrl+F]",
                style =
                {
                    width = StyleKeyword.Auto,
                    marginLeft = 4,
                    marginRight = 4,
                    flexShrink = 1,
                }
            };
            m_Toolbar.Add(m_SearchField);
            // 值变化回调
            m_SearchField.RegisterValueChangedCallback(OnSearchFieldValueChanged);
            // 监听键盘事件
            m_SearchField.RegisterCallback<KeyDownEvent>((evt) =>
            {
                // ↑ || ↓
                if (evt.keyCode == KeyCode.UpArrow || evt.keyCode == KeyCode.DownArrow)
                {
                    FocusToSnippetList();
                }
            });
        }

        /// <summary>
        /// 搜索栏内容变化回调
        /// </summary>
        /// <param name="evt"></param>
        private void OnSearchFieldValueChanged(ChangeEvent<string> evt)
        {
            SetSearchText(evt.newValue);
        }

        #region SearchField

        /// <summary>
        /// 搜索文本
        /// </summary>
        private string m_SearchText = string.Empty;

        /// <summary>
        /// 设置搜索栏内容
        /// </summary>
        /// <param name="value"></param>
        private void SetSearchText(string value)
        {
            m_SearchText = value;
            m_SearchField.SetValueWithoutNotify(value);
            UpdateContent();
        }

        /// <summary>
        /// 聚焦到搜索框
        /// </summary>
        private void FocusToSearchField()
        {
            m_SearchField.Focus();
        }

        #endregion

        #region Searching

        /// <summary>
        /// 过滤
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private void Filter(ref List<SnippetInfo> list)
        {
            // 移除空格
            string text = m_SearchText.Trim();

            // 匹配名称
            if (!string.IsNullOrWhiteSpace(text))
            {
                string pattern = text.Trim().ToCharArray().Join(".*");
                Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
                list = list.FindAll(v => regex.Match(v.name).Success);
            }
        }

        #endregion

    }

}
