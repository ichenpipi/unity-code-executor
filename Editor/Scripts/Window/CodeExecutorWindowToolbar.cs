using System.Linq;
using UnityEngine.UIElements;

namespace ChenPipi.CodeExecutor.Editor
{

    /// <summary>
    /// 窗口
    /// </summary>
    public partial class CodeExecutorWindow
    {

        /// <summary>
        /// 工具栏
        /// </summary>
        private VisualElement m_Toolbar = null;

        /// <summary>
        /// 初始化
        /// </summary>
        private void InitToolbar()
        {
            // 绑定元素
            m_Toolbar = rootVisualElement.Q<VisualElement>("Toolbar");
            {
                m_Toolbar.style.height = 20;
                m_Toolbar.style.flexShrink = 0;
                m_Toolbar.style.flexDirection = FlexDirection.Row;
            }
            // 监听尺寸变化
            m_Toolbar.RegisterCallback<GeometryChangedEvent>(OnToolbarGeometryChangedEventChanged);

            // 分割线
            VisualElement separator = rootVisualElement.Q<VisualElement>("Separator");
            {
                separator.style.borderBottomColor = separatorColor;
            }

            // 搜索栏
            InitToolbarSearchField();
            // 代码段开关
            InitToolbarSnippetsToggle();
            // 代码段排序菜单
            InitToolbarSortingMenu();
            // 执行模式菜单
            InitToolbarExecutionModeMenu();
            // 导入按钮
            InitToolBarImportButton();

            // 特殊处理最后一个元素的样式
            VisualElement[] elements = m_Toolbar.Children().ToArray();
            for (int i = 0; i < elements.Length; i++)
            {
                if (i == elements.Length - 1)
                {
                    VisualElement element = elements[i];
                    element.style.marginRight = -1;
                }
            }
        }

        /// <summary>
        /// 尺寸变化回调
        /// </summary>
        /// <param name="evt"></param>
        private void OnToolbarGeometryChangedEventChanged(GeometryChangedEvent evt)
        {
            bool isNarrow = (m_Toolbar.localBound.width <= 180);
            m_SearchField.style.display = (isNarrow ? DisplayStyle.None : DisplayStyle.Flex);
            m_SortingMenu.style.display = (isNarrow ? DisplayStyle.None : DisplayStyle.Flex);
            m_ImportButton.style.display = (isNarrow ? DisplayStyle.None : DisplayStyle.Flex);
        }

    }

}
