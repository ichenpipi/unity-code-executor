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
        /// 新代码条目
        /// </summary>
        private ListItem m_NewItem = null;

        /// <summary>
        /// 初始化
        /// </summary>
        private void InitNewCodeItem()
        {
            // 临时代码条目
            m_NewItem = new ListItem()
            {
                name = "New",
                style =
                {
                    flexShrink = 0,
                    height = 30,
                    marginTop = 0,
                    marginBottom = 0,
                }
            };
            // 文本
            m_NewItem.SetText("New");
            m_NewItem.SetTextFontStyle(FontStyle.Bold);
            // 图标
            m_NewItem.SetIcon(PipiUtility.GetIcon("CreateAddNew"));
            // 点击回调
            m_NewItem.RegisterCallback<MouseDownEvent>(OnNewCodeItemMouseDown);
            m_Sidebar.Add(m_NewItem);
        }

        private void OnNewCodeItemMouseDown(MouseDownEvent e)
        {
            Switch(CodeExecutorData.newSnippet);
        }

        private void UpdateNewCodeItemStyle(bool isSelected)
        {
            m_NewItem.style.backgroundColor = (isSelected ? newItemActiveBgColor : newItemNormalBgColor);
        }

    }

}
