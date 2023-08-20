using UnityEngine.UIElements;

namespace ChenPipi.CodeExecutor.Editor
{

    /// <summary>
    /// 窗口
    /// </summary>
    public partial class CodeExecutorWindow
    {

        /// <summary>
        /// 侧边栏
        /// </summary>
        private VisualElement m_Sidebar = null;

        /// <summary>
        /// 初始化
        /// </summary>
        private void InitSidebar()
        {
            // 代码段面板
            m_Sidebar = new VisualElement()
            {
                name = "Sidebar",
                style =
                {
                    flexDirection = FlexDirection.Column,
                    minWidth = 100,
                }
            };
            m_ContentSplitView.Add(m_Sidebar);

            // 新代码条目
            InitNewCodeItem();

            // 分割线
            m_Sidebar.Add(GenHorizontalSeparator(0));

            // 代码段列表
            InitSnippetList();
        }

        /// <summary>
        /// 开关侧栏
        /// </summary>
        /// <param name="isOn"></param>
        private void ToggleSidebar(bool isOn)
        {
            // 处理开关显示
            m_SnippetsToggle.SetValueWithoutNotify(isOn);
            // 折叠
            if (isOn)
            {
                m_ContentSplitView.UnCollapse();
                // 恢复拖拽线位置
                ApplySettings_DragLine();
            }
            else
            {
                m_ContentSplitView.CollapseChild(0);
            }
            // 再次设置面板的展示状态，确保显示无异常
            m_Sidebar.style.display = (isOn ? DisplayStyle.Flex : DisplayStyle.None);
            // 处理另一区域，显示拖拽线时左侧空出一个像素
            m_Detail.style.marginLeft = (isOn ? 1 : 0);
        }

    }

}
