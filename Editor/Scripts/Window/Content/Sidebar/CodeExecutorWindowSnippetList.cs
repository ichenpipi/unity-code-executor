using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        /// 代码段列表
        /// </summary>
        private ListView m_SnippetList = null;

        /// <summary>
        /// 初始化
        /// </summary>
        private void InitSnippetList()
        {
            // 代码段列表
            m_SnippetList = new ListView()
            {
                name = "SnippetList",
#if UNITY_2021_1_OR_NEWER
                fixedItemHeight = 18,
#else
                itemHeight = 18,
#endif
                selectionType = SelectionType.Multiple,
                makeItem = CreateListItem,
                bindItem = BindListItem,
                unbindItem = UnbindListItem,
                style =
                {
                    marginTop = 0,
                    marginBottom = 0,
                    flexBasis = Length.Percent(100),
                }
            };
            m_Sidebar.Add(m_SnippetList);

            // 列表选择变化回调
            m_SnippetList.onSelectionChange += OnSnippetListSelectionChange;
            // 列表条目选择（双击）回调
            m_SnippetList.onItemsChosen += OnSnippetListItemsChosen;
        }

        /// <summary>
        /// 列表选择变化回调
        /// </summary>
        /// <param name="objs"></param>
        private void OnSnippetListSelectionChange(IEnumerable<object> objs)
        {
            object[] objects = objs as object[] ?? objs.ToArray();
            if (!objects.Any())
            {
                return;
            }
            // 选中条目列表中包含当前条目，不做切换
            if (objects.Contains(m_CurrSnippetInfo))
            {
                return;
            }
            // 切换
            SnippetInfo snippetInfo = (SnippetInfo)objects.First();
            Switch(snippetInfo);
        }

        /// <summary>
        /// 列表条目选择（双击）回调
        /// </summary>
        /// <param name="objs"></param>
        private void OnSnippetListItemsChosen(IEnumerable<object> objs) { }

        #endregion

        #region Interface

        /// <summary>
        /// 当前列表数据
        /// </summary>
        private List<SnippetInfo> m_SnippetListData = new List<SnippetInfo>();

        /// <summary>
        /// 更新列表
        /// </summary>
        private void UpdateSnippetList()
        {
            // 克隆数据副本
            m_SnippetListData.Clear();
            m_SnippetListData.AddRange(CodeExecutorData.snippets);

            // 处理数据
            if (m_SnippetListData.Count > 0)
            {
                // 过滤
                Filter(ref m_SnippetListData);
                // 排序
                Sort(ref m_SnippetListData);
            }

            // 列表数据
            m_SnippetList.itemsSource = m_SnippetListData;
#if UNITY_2021_2_OR_NEWER
            m_SnippetList.Rebuild();
#else
            m_SnippetList.Refresh();
#endif
        }

        /// <summary>
        /// 聚焦到列表
        /// </summary>
        public void FocusToSnippetList()
        {
            m_SnippetList.Focus();
        }

        /// <summary>
        /// 选中条目
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="notify"></param>
        public void SetSnippetListSelection(string guid, bool notify = true)
        {
            if (string.IsNullOrWhiteSpace(guid))
            {
                return;
            }
            int index = GetSnippetListItemIndex(guid);
            if (index < 0)
            {
                return;
            }
            // 选中条目
            if (notify)
            {
                m_SnippetList.SetSelection(new int[] { index });
            }
            else
            {
                m_SnippetList.SetSelectionWithoutNotify(new int[] { index });
            }
            // 滚动列表
            m_SnippetList.ScrollToItem(index);
        }

        /// <summary>
        /// 选中条目
        /// </summary>
        /// <param name="guids"></param>
        /// <param name="notify"></param>
        public void SetSnippetListSelection(string[] guids, bool notify = true)
        {
            List<int> selection = new List<int>();
            foreach (string guid in guids)
            {
                if (string.IsNullOrWhiteSpace(guid))
                {
                    continue;
                }
                int index = GetSnippetListItemIndex(guid);
                if (index < 0)
                {
                    continue;
                }
                selection.Add(index);
            }
            // 选中条目
            if (notify)
            {
                m_SnippetList.SetSelection(selection.ToArray());
            }
            else
            {
                m_SnippetList.SetSelectionWithoutNotify(selection.ToArray());
            }
            // 滚动列表
            m_SnippetList.ScrollToItem(selection.First());
        }

        /// <summary>
        /// 清除列表选择
        /// </summary>
        /// <param name="notify"></param>
        private void ClearSnippetListSelection(bool notify = true)
        {
            if (notify)
            {
                m_SnippetList.ClearSelection();
            }
            else
            {
                m_SnippetList.SetSelectionWithoutNotify(new int[] { });
            }
        }

        /// <summary>
        /// 获取条目元素
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private ListItem GetSnippetListItem(int index)
        {
#if UNITY_2021_1_OR_NEWER
            return (ListItem)m_SnippetList.GetRootElementForIndex(index);
#else
            Type type = m_SnippetList.GetType();
            FieldInfo fieldInfo = type.GetField("m_ScrollView", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo == null)
            {
                return null;
            }
            ScrollView scrollView = (ScrollView)fieldInfo.GetValue(m_SnippetList);
            if (scrollView == null)
            {
                return null;
            }
            VisualElement[] elements = scrollView.Children().ToArray();
            foreach (VisualElement element in elements)
            {
                ListItem listItem = (ListItem)element;
                if (listItem.index == index)
                {
                    return listItem;
                }
            }
            return null;
#endif
        }

        /// <summary>
        /// 获取条目元素
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        private ListItem GetSnippetListItem(string guid)
        {
            int index = GetSnippetListItemIndex(guid);
            return (index < 0 ? null : GetSnippetListItem(index));
        }

        /// <summary>
        /// 获取当前选中的条目元素
        /// </summary>
        /// <returns></returns>
        private ListItem GetSelectedSnippetListItem()
        {
            return GetSnippetListItem(m_SnippetList.selectedIndex);
        }

        /// <summary>
        /// 获取条目下标
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        private int GetSnippetListItemIndex(string guid)
        {
            if (string.IsNullOrWhiteSpace(guid))
            {
                return -1;
            }
            return m_SnippetListData.FindIndex(v => v.MatchGuid(guid));
        }

        /// <summary>
        /// 获取选中的条目信息
        /// </summary>
        /// <returns></returns>
        private SnippetInfo[] GetSelectedSnippetInfos()
        {
            return m_SnippetList.selectedItems.Select(o => (SnippetInfo)o).ToArray();
        }

        /// <summary>
        /// 获取选中的条目GUID列表
        /// </summary>
        /// <returns></returns>
        private string[] GetSelectedSnippetGuids()
        {
            return m_SnippetList.selectedItems.Select(o => ((SnippetInfo)o).guid).ToArray();
        }

        #endregion

        #region ListItem

        /// <summary>
        /// 创建条目元素
        /// </summary>
        /// <returns></returns>
        private ListItem CreateListItem()
        {
            ListItem listItem = new ListItem();
            // 图标
            listItem.SetIcon(PipiUtility.GetIcon("TextAsset Icon"));
            // 浮动按钮
            listItem.enableFloatButton = true;
            listItem.floatButton.SetIcon(PipiUtility.GetIcon("PlayButton"));
            listItem.floatButton.tooltip = "Execute snippet";
            listItem.floatButtonClicked += OnListItemFloatButtonClicked;
            // 注册右键菜单
            listItem.AddManipulator(new ContextualMenuManipulator(ItemMenuBuilder));
            return listItem;
        }

        /// <summary>
        /// 条目浮动按钮点击回调
        /// </summary>
        /// <param name="listItem"></param>
        private void OnListItemFloatButtonClicked(ListItem listItem)
        {
            if (listItem.userData == null) return;
            SnippetInfo snippetInfo = listItem.userData;
            ExecuteCode(snippetInfo.name, snippetInfo.code, snippetInfo.mode);
        }

        /// <summary>
        /// 绑定条目
        /// </summary>
        /// <param name="element"></param>
        /// <param name="index"></param>
        private void BindListItem(VisualElement element, int index)
        {
            if (index >= m_SnippetListData.Count)
            {
                element.RemoveFromHierarchy();
                return;
            }
            SnippetInfo snippetInfo = m_SnippetListData[index];
            // 应用数据
            ListItem listItem = (ListItem)element;
            listItem.index = index;
            listItem.userData = snippetInfo;
            // 名称
            listItem.SetText(snippetInfo.name);
            // 置顶
            listItem.SetTop(snippetInfo.top);
            // 重命名
            listItem.renameCallback = OnListItemRenamed;
            // 监听事件
            element.RegisterCallback<MouseDownEvent>(OnListItemMouseDown);
        }

        /// <summary>
        /// 条目重命名回调
        /// </summary>
        /// <param name="listItem"></param>
        /// <param name="newName"></param>
        private bool OnListItemRenamed(ListItem listItem, string newName)
        {
            SnippetInfo itemInfo = listItem.userData;
            if (!string.IsNullOrWhiteSpace(newName) && !newName.Equals(itemInfo.name))
            {
                newName = CodeExecutorManager.GetNonDuplicateName(newName);
                CodeExecutorManager.SetSnippetName(itemInfo.guid, newName);
            }
            // 聚焦到列表
            m_SnippetList.Focus();
            return true;
        }

        /// <summary>
        /// 取消条目绑定
        /// </summary>
        /// <param name="element"></param>
        /// <param name="index"></param>
        private void UnbindListItem(VisualElement element, int index)
        {
            ListItem listItem = (ListItem)element;
            // 恢复状态
            listItem.HideNameTextField();
            // 移除数据
            listItem.index = -1;
            listItem.userData = null;
            // 取消事件监听
            element.UnregisterCallback<MouseDownEvent>(OnListItemMouseDown);
        }

        /// <summary>
        /// 列表条目鼠标点击回调
        /// </summary>
        /// <param name="evt"></param>
        private void OnListItemMouseDown(MouseDownEvent evt)
        {
            if (!(evt.target is ListItem listItem))
            {
                return;
            }
            // 鼠标右键
            if (evt.button == 1)
            {
                int[] selectedIndices = m_SnippetList.selectedIndices.ToArray();
                // 在 Unity 2020 中选中条目后快速点击鼠标右键会出现显示异常：列表会自动选择上一次选中的条目
                // 所以在这里主动选择已选中的条目来覆盖异常情况
                m_SnippetList.SetSelectionWithoutNotify(selectedIndices);
                // 已选中条目中不包含当前条目时，清除已选中条目并选中当前条目
                if (!selectedIndices.Contains(listItem.index))
                {
                    m_SnippetList.SetSelection(listItem.index);
                }
            }
        }

        #endregion

        #region ListItem Menu

        /// <summary>
        /// 条目菜单名称
        /// </summary>
        private static class ListItemMenuItemName
        {
            public const string Execute = "Execute";
            public const string Edit = "Edit";
            public const string Rename = "Rename";
            public const string Duplicate = "Duplicate";
            public const string Top = "Top";
            public const string UnTop = "Un-top";
            public const string Delete = "Delete";
        }

        /// <summary>
        /// 创建条目菜单
        /// </summary>
        /// <param name="evt"></param>
        private void ItemMenuBuilder(ContextualMenuPopulateEvent evt)
        {
            object listItem = evt.target;
            DropdownMenu menu = evt.menu;
            menu.AppendAction(ListItemMenuItemName.Execute, OnItemMenuAction, EnabledOnSingleSelection, listItem);
            menu.AppendSeparator();
            menu.AppendAction(ListItemMenuItemName.Edit, OnItemMenuAction, EnabledOnSingleSelection, listItem);
            menu.AppendAction(ListItemMenuItemName.Rename, OnItemMenuAction, EnabledOnSingleSelection, listItem);
            menu.AppendAction(ListItemMenuItemName.Duplicate, OnItemMenuAction, AlwaysEnabled, listItem);
            menu.AppendSeparator();
            menu.AppendAction(ListItemMenuItemName.Top, OnItemMenuAction, AlwaysEnabled, listItem);
            menu.AppendAction(ListItemMenuItemName.UnTop, OnItemMenuAction, AlwaysEnabled, listItem);
            menu.AppendSeparator();
            menu.AppendAction(ListItemMenuItemName.Delete, OnItemMenuAction, AlwaysEnabled, listItem);

            DropdownMenuAction.Status AlwaysEnabled(DropdownMenuAction a) => DropdownMenuAction.AlwaysEnabled(a);

            DropdownMenuAction.Status EnabledOnSingleSelection(DropdownMenuAction a)
            {
                return (m_SnippetList.selectedItems.Count() == 1 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }
        }

        /// <summary>
        /// 条目菜单行为回调
        /// </summary>
        /// <param name="action"></param>
        private void OnItemMenuAction(DropdownMenuAction action)
        {
            if (!(action.userData is ListItem listItem))
            {
                return;
            }
            switch (action.name)
            {
                case ListItemMenuItemName.Execute:
                {
                    SnippetInfo snippetInfo = listItem.userData;
                    ExecuteCode(snippetInfo.name, snippetInfo.code, snippetInfo.mode);
                    break;
                }
                case ListItemMenuItemName.Edit:
                {
                    SnippetInfo snippetInfo = listItem.userData;
                    Switch(snippetInfo);
                    SetCodeEditorEditable(true, true);
                    break;
                }
                case ListItemMenuItemName.Rename:
                {
                    listItem.ShowNameTextField();
                    break;
                }
                case ListItemMenuItemName.Duplicate:
                {
                    SnippetInfo[] snippetInfos = GetSelectedSnippetInfos();
                    List<SnippetInfo> list = CodeExecutorManager.CloneSnippets(snippetInfos);
                    string[] guids = list.Select(o => o.guid).ToArray();
                    Switch(guids.First());
                    SetSnippetListSelection(guids, false);
                    break;
                }
                case ListItemMenuItemName.Top:
                {
                    SnippetInfo[] snippetInfos = GetSelectedSnippetInfos();
                    for (int i = 0; i < snippetInfos.Length; i++)
                    {
                        SnippetInfo snippetInfo = snippetInfos[i];
                        bool isLast = (i == snippetInfos.Length - 1);
                        CodeExecutorManager.SetSnippetTop(snippetInfo.guid, true, isLast);
                    }
                    break;
                }
                case ListItemMenuItemName.UnTop:
                {
                    SnippetInfo[] snippetInfos = GetSelectedSnippetInfos();
                    for (int i = 0; i < snippetInfos.Length; i++)
                    {
                        SnippetInfo snippetInfo = snippetInfos[i];
                        bool isLast = (i == snippetInfos.Length - 1);
                        CodeExecutorManager.SetSnippetTop(snippetInfo.guid, false, isLast);
                    }
                    break;
                }
                case ListItemMenuItemName.Delete:
                {
                    CodeExecutorManager.RemoveSnippets(GetSelectedSnippetGuids());
                    break;
                }
            }
        }

        #endregion

    }

}
