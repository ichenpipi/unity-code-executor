using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
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
        /// 代码段列表容器
        /// </summary>
        private IMGUIContainer m_SnippetTreeViewContainer = null;

        /// <summary>
        /// 代码段列表状态
        /// </summary>
        [SerializeField]
        private TreeViewState m_SnippetTreeViewState = new TreeViewState();

        /// <summary>
        /// 代码段列表
        /// </summary>
        private CustomTreeView m_SnippetTreeView;

        /// <summary>
        /// 初始化
        /// </summary>
        private void InitSnippetTreeView()
        {
            m_SnippetTreeViewContainer = new IMGUIContainer()
            {
                name = "TreeViewContainer",
                style =
                {
                    flexDirection = FlexDirection.Column,
                    minWidth = 100,
                    flexBasis = Length.Percent(100),
                }
            };
            m_Sidebar.Add(m_SnippetTreeViewContainer);

            // 创建树视图
            m_SnippetTreeView = new CustomTreeView(m_SnippetTreeViewState);
            m_SnippetTreeView.buildItems = BuildSnippetTreeViewItems;
            m_SnippetTreeView.beforeDrawRowGUI += OnSnippetTreeViewBeforeDrawRowGUI;
            m_SnippetTreeView.drawRowGUIOverlay = OnSnippetTreeViewDrawRowGUIOverlay;
            m_SnippetTreeView.buildMenu = BuildSnippetTreeViewMenu;
            m_SnippetTreeView.buildItemMenu = BuildSnippetTreeViewItemMenu;
            m_SnippetTreeView.onSelectionChange += OnSnippetTreeViewSelectionChanged;
            m_SnippetTreeView.onItemClicked += OnSnippetTreeViewItemClicked;
            m_SnippetTreeView.onItemDoubleClicked += OnSnippetTreeViewItemDoubleClicked;
            m_SnippetTreeView.onItemContextClicked += OnSnippetTreeViewItemContextClicked;
            m_SnippetTreeView.onItemRenamed += OnSnippetTreeViewItemRenamed;
            m_SnippetTreeView.onItemDragged += OnSnippetTreeViewItemDragged;
            m_SnippetTreeView.onItemDropped += OnSnippetTreeViewItemDropped;
            m_SnippetTreeView.onKeyDown += OnSnippetTreeViewKeyDown;
            m_SnippetTreeView.Reload();

            // 代理GUI绘制调用
            m_SnippetTreeViewContainer.onGUIHandler = OnSnippetTreeViewContainerGUI;
            // 元素失焦回调
            m_SnippetTreeViewContainer.RegisterCallback<BlurEvent>(OnSnippetTreeViewContainerBlur);
        }

        private void OnSnippetTreeViewContainerGUI()
        {
            m_SnippetTreeView.OnGUI(m_SnippetTreeViewContainer.contentRect);
        }

        private void OnSnippetTreeViewContainerBlur(BlurEvent evt)
        {
            m_SnippetTreeView.EndRename();
        }

        private void OnSnippetTreeViewKeyDown(Event evt)
        {
            // Ctrl + F
            if (evt.control && evt.keyCode == KeyCode.F)
            {
                FocusToSearchField();
            }
            // Ctrl + C
            else if (evt.control && evt.keyCode == KeyCode.C)
            {
                DoCopyToClipboard();
            }
            // Ctrl + V
            else if (evt.control && evt.keyCode == KeyCode.V)
            {
                DoPasteFromClipboard();
            }
            // Ctrl + D
            else if (evt.control && evt.keyCode == KeyCode.D)
            {
                DuplicateSelectedSnippets();
            }
            // Ctrl + Numpad- / Ctrl + [
            else if (evt.control && (evt.keyCode == KeyCode.KeypadMinus || evt.keyCode == KeyCode.LeftBracket))
            {
                SnippetTreeViewCollapseAllCategories();
            }
            // Ctrl + Numpad+ / Ctrl + ]
            else if (evt.control && (evt.keyCode == KeyCode.KeypadPlus || evt.keyCode == KeyCode.RightBracket))
            {
                SnippetTreeViewExpandAllCategories();
            }
            // Delete / Backspace
            else if (evt.keyCode == KeyCode.Delete || evt.keyCode == KeyCode.Backspace)
            {
                DeleteSelectedSnippetsAndCategories();
            }
            // F5
            else if (evt.keyCode == KeyCode.F5)
            {
                Reload();
            }
        }

        #endregion

        #region Build Items

        private static Texture2D s_CategoryCollapseIcon = null;
        private static Texture2D categoryCollapseIcon => (s_CategoryCollapseIcon ??= (Texture2D)PipiUtility.GetIcon("Folder Icon"));

        private static Texture2D s_CategoryExpandedIcon = null;
        private static Texture2D categoryExpandedIcon => (s_CategoryExpandedIcon ??= (Texture2D)PipiUtility.GetIcon("FolderOpened Icon"));

        private static Texture2D s_SnippetIcon = null;
        private static Texture2D snippetIcon => (s_SnippetIcon ??= (Texture2D)PipiUtility.GetIcon("TextAsset Icon"));

        private static Texture2D s_SnippetTopIcon = null;
        private static Texture2D snippetTopIcon => (s_SnippetTopIcon ??= (Texture2D)PipiUtility.GetIcon("CollabPush"));

        private void BuildSnippetTreeViewItems(TreeViewItem root)
        {
            // 唯一标识符
            int itemID = root.id + 1;

            // 添加类别条目
            m_ItemId2CategoryMap.Clear();
            m_Category2ItemIdMap.Clear();
            Dictionary<string, CustomTreeViewItem> categories = new Dictionary<string, CustomTreeViewItem>(StringComparer.OrdinalIgnoreCase);
            foreach (string category in m_Categories)
            {
                CustomTreeViewItem item = new CustomTreeViewItem
                {
                    isContainer = true,
                    userData = null,
                    id = itemID++,
                    displayName = category,
                    icon = categoryCollapseIcon,
                };
                // 添加条目
                item.depth = root.depth + 1;
                root.AddChild(item);
                // 记录条目
                categories.Add(category, item);
                // 记录映射
                m_ItemId2CategoryMap.Add(item.id, category);
                m_Category2ItemIdMap.Add(category, item.id);
            }

            // 添加代码段条目
            m_ItemId2SnippetGuidMap.Clear();
            m_SnippetGuid2ItemIdMap.Clear();

            foreach (SnippetInfo snippet in m_Snippets)
            {
                CustomTreeViewItem item = new CustomTreeViewItem
                {
                    isContainer = false,
                    userData = snippet,
                    id = itemID++,
                    displayName = snippet.name,
                    icon = snippetIcon,
                };
                // 添加条目
                if (!string.IsNullOrEmpty(snippet.category) && categories.TryGetValue(snippet.category, out CustomTreeViewItem category))
                {
                    item.depth = category.depth + 1;
                    category.AddChild(item);
                }
                else
                {
                    item.depth = root.depth + 1;
                    root.AddChild(item);
                }
                // 记录映射
                m_ItemId2SnippetGuidMap.Add(item.id, snippet.guid);
                m_SnippetGuid2ItemIdMap.Add(snippet.guid, item.id);
            }
        }

        #endregion

        #region Draw Item GUI

        private void OnSnippetTreeViewBeforeDrawRowGUI(CustomTreeViewItem item)
        {
            if (item.isContainer)
            {
                item.icon = (m_SnippetTreeView.IsExpanded(item.id) ? categoryExpandedIcon : categoryCollapseIcon);
            }
        }

        private void OnSnippetTreeViewDrawRowGUIOverlay(CustomTreeViewItem item, Rect rect)
        {
            if (item.isContainer || !(item.userData is SnippetInfo snippet))
            {
                return;
            }

            // 置顶图标
            if (snippet.top)
            {
                float indent = m_SnippetTreeView.GetContentIndent(item);

                Rect iconRect = new Rect(rect);
                iconRect.x += indent + 6;
                iconRect.y += 3;
                iconRect.width = 8f;

                GUI.DrawTexture(iconRect, snippetTopIcon, ScaleMode.ScaleToFit);
            }
        }

        #endregion

        #region Selection Change

        private void OnSnippetTreeViewSelectionChanged(int[] itemIDs)
        {
            // 未选中代码段条目，不做切换
            IList<string> snippetGuids = GetSnippetTreeViewSelectedSnippetGuids(false);
            if (!snippetGuids.Any())
            {
                return;
            }
            // 选中列表中包含该条目，不做切换
            if (m_CurrSnippet != null && snippetGuids.Contains(m_CurrSnippet.guid))
            {
                return;
            }
            // 切换代码段
            SnippetInfo snippet = CodeExecutorData.GetSnippet(snippetGuids.First());
            Switch(snippet);
        }

        #endregion

        #region Item Click

        private void OnSnippetTreeViewItemClicked(int itemID)
        {
            CustomTreeViewItem item = m_SnippetTreeView.FindItem(itemID);
            m_SelectedCategory = item.isContainer ? item.displayName : null;
        }

        private void OnSnippetTreeViewItemDoubleClicked(int itemID)
        {
            CustomTreeViewItem item = m_SnippetTreeView.FindItem(itemID);
            if (item.isContainer)
            {
                bool isExpanded = m_SnippetTreeView.IsExpanded(itemID);
                m_SnippetTreeView.SetExpanded(itemID, !isExpanded);
            }
            else
            {
                SnippetInfo snippet = GetSnippetInfoBySnippetTreeViewItemId(itemID);
                if (snippet != null) ExecuteSnippet(snippet.name, snippet.code, snippet.mode);
            }
        }

        private void OnSnippetTreeViewItemContextClicked(int itemID) { }

        #endregion

        #region Item Rename

        private void OnSnippetTreeViewItemRenamed(int itemID, string newName, string originalName)
        {
            if (string.IsNullOrWhiteSpace(newName) || newName.Equals(originalName))
            {
                return;
            }

            CustomTreeViewItem item = m_SnippetTreeView.FindItem(itemID);
            if (item == null)
            {
                return;
            }

            if (item.isContainer)
            {
                // 类别
                newName = CodeExecutorManager.GetNonDuplicateCategoryName(newName);
                CodeExecutorManager.RenameCategory(originalName, newName);
            }
            else
            {
                // 代码段
                newName = CodeExecutorManager.GetNonDuplicateSnippetName(newName);
                string snippetGuid = GetSnippetGuidBySnippetTreeViewItemId(itemID);
                SnippetInfo snippet = CodeExecutorData.GetSnippet(snippetGuid);
                CodeExecutorManager.SetSnippetName(snippet.guid, newName);
            }
        }

        #endregion

        #region Item DragAndDrop

        private void OnSnippetTreeViewItemDragged(int[] draggedItemIDs)
        {
            m_SelectedSnippetGuid = GetSnippetGuidBySnippetTreeViewItemId(draggedItemIDs.First());
        }

        private void OnSnippetTreeViewItemDropped(int[] draggedItemIDs, int parentItemID)
        {
            string category = (parentItemID > 0 ? GetSnippetCategoryBySnippetTreeViewItemId(parentItemID) : null);
            if (parentItemID > 0 && category == null)
            {
                string snippetGuid = GetSnippetGuidBySnippetTreeViewItemId(parentItemID);
                SnippetInfo snippet = CodeExecutorData.GetSnippet(snippetGuid);
                category = snippet?.category;
            }
            for (int i = 0; i < draggedItemIDs.Length; i++)
            {
                string guid = GetSnippetGuidBySnippetTreeViewItemId(draggedItemIDs[i]);
                bool notify = (i == draggedItemIDs.Length - 1);
                CodeExecutorManager.SetSnippetCategory(guid, category, notify);
            }
        }

        #endregion

        #region Item Info Mapping

        private readonly Dictionary<int, string> m_ItemId2CategoryMap = new Dictionary<int, string>();

        private readonly Dictionary<string, int> m_Category2ItemIdMap = new Dictionary<string, int>();

        private readonly Dictionary<int, string> m_ItemId2SnippetGuidMap = new Dictionary<int, string>();

        private readonly Dictionary<string, int> m_SnippetGuid2ItemIdMap = new Dictionary<string, int>();

        private string GetSnippetCategoryBySnippetTreeViewItemId(int itemID)
        {
            return m_ItemId2CategoryMap.TryGetValue(itemID, out string category) ? category : null;
        }

        private int GetSnippetTreeViewItemIdByCategory(string category)
        {
            return m_Category2ItemIdMap.TryGetValue(category, out int itemID) ? itemID : -1;
        }

        private string GetSnippetGuidBySnippetTreeViewItemId(int itemID)
        {
            return m_ItemId2SnippetGuidMap.TryGetValue(itemID, out string guid) ? guid : null;
        }

        private int GetSnippetTreeViewItemIdBySnippetGuid(string guid)
        {
            return m_SnippetGuid2ItemIdMap.TryGetValue(guid, out int itemID) ? itemID : -1;
        }

        private SnippetInfo GetSnippetInfoBySnippetTreeViewItemId(int itemID)
        {
            return m_ItemId2SnippetGuidMap.TryGetValue(itemID, out string guid) ? CodeExecutorData.GetSnippet(guid) : null;
        }

        #endregion

        #region Interface

        /// <summary>
        /// 类别
        /// </summary>
        private List<string> m_Categories = new List<string>();

        /// <summary>
        /// 代码段
        /// </summary>
        private List<SnippetInfo> m_Snippets = new List<SnippetInfo>();

        /// <summary>
        /// 更新列表
        /// </summary>
        private void UpdateSnippetTreeView()
        {
            // 克隆数据副本
            m_Categories.Clear();
            m_Categories.AddRange(CodeExecutorData.GetCategories());
            m_Snippets.Clear();
            m_Snippets.AddRange(CodeExecutorData.GetSnippets());

            // 处理数据
            if (m_Snippets.Count > 0)
            {
                // 过滤
                Filter(ref m_Snippets, ref m_Categories);
                // 排序
                Sort(ref m_Snippets, ref m_Categories);
            }

            m_SnippetTreeView.Reload();
        }

        /// <summary>
        /// 聚焦到列表
        /// </summary>
        private void FocusToSnippetTreeView()
        {
            m_SnippetTreeView.SetFocusAndEnsureSelectedItem();
        }

        /// <summary>
        /// 折叠所有类别
        /// </summary>
        private void SnippetTreeViewCollapseAllCategories()
        {
            m_SnippetTreeView.CollapseAll();
        }

        /// <summary>
        /// 展开所有类别
        /// </summary>
        private void SnippetTreeViewExpandAllCategories()
        {
            m_SnippetTreeView.ExpandAll();
        }

        /// <summary>
        /// 获取选中的代码段GUID
        /// </summary>
        /// <param name="includeCategories"></param>
        /// <returns></returns>
        private List<string> GetSnippetTreeViewSelectedSnippetGuids(bool includeCategories)
        {
            List<string> guids = new List<string>();
            Dictionary<string, bool> map = new Dictionary<string, bool>();
            foreach (int itemID in m_SnippetTreeView.GetSelection())
            {
                string guid = GetSnippetGuidBySnippetTreeViewItemId(itemID);
                if (guid != null)
                {
                    if (!map.ContainsKey(guid))
                    {
                        guids.Add(guid);
                        map.Add(guid, true);
                    }
                }
                else if (includeCategories)
                {
                    string category = GetSnippetCategoryBySnippetTreeViewItemId(itemID);
                    List<SnippetInfo> snippets = CodeExecutorData.GetSnippetsWithCategory(category);
                    foreach (string g in snippets.Select(v => v.guid))
                    {
                        guids.Add(g);
                        map.Add(g, true);
                    }
                }
            }
            return guids;
        }

        /// <summary>
        /// 获取选中的代码段GUID
        /// </summary>
        /// <param name="includeCategories"></param>
        /// <returns></returns>
        private List<SnippetInfo> GetSnippetTreeViewSelectedSnippets(bool includeCategories)
        {
            List<SnippetInfo> snippets = new List<SnippetInfo>();
            Dictionary<string, bool> map = new Dictionary<string, bool>();
            foreach (int itemID in m_SnippetTreeView.GetSelection())
            {
                string guid = GetSnippetGuidBySnippetTreeViewItemId(itemID);
                if (guid != null)
                {
                    if (!map.ContainsKey(guid))
                    {
                        snippets.Add(CodeExecutorData.GetSnippet(guid));
                        map.Add(guid, true);
                    }
                }
                else if (includeCategories)
                {
                    string category = GetSnippetCategoryBySnippetTreeViewItemId(itemID);
                    List<SnippetInfo> list = CodeExecutorData.GetSnippetsWithCategory(category);
                    foreach (SnippetInfo s in list)
                    {
                        if (!map.ContainsKey(s.guid))
                        {
                            snippets.Add(s);
                            map.Add(s.guid, true);
                        }
                    }
                }
            }
            return snippets;
        }

        /// <summary>
        /// 选中条目
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="notify"></param>
        public void SetSnippetTreeViewSelection(string guid, bool notify = true)
        {
            if (string.IsNullOrWhiteSpace(guid))
            {
                return;
            }
            int itemID = GetSnippetTreeViewItemIdBySnippetGuid(guid);
            if (itemID < 0)
            {
                return;
            }
            // 选中条目
            SetSnippetTreeViewSelection(itemID, notify);
        }

        /// <summary>
        /// 选中条目
        /// </summary>
        /// <param name="guids"></param>
        /// <param name="notify"></param>
        public void SetSnippetTreeViewSelection(IEnumerable<string> guids, bool notify = true)
        {
            List<int> selection = new List<int>();
            foreach (string guid in guids)
            {
                if (string.IsNullOrWhiteSpace(guid))
                {
                    continue;
                }
                int itemID = GetSnippetTreeViewItemIdBySnippetGuid(guid);
                if (itemID < 0)
                {
                    continue;
                }
                selection.Add(itemID);
            }
            // 选中条目
            SetSnippetTreeViewSelection(selection, notify);
        }

        /// <summary>
        /// 选中条目
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="notify"></param>
        public void SetSnippetTreeViewSelection(int itemID, bool notify = true)
        {
            if (itemID < 0)
            {
                return;
            }
            SetSnippetTreeViewSelection(new int[] { itemID }, notify);
            // 确保展示条目
            m_SnippetTreeView.RevealAndFrameSelectedItem();
        }

        /// <summary>
        /// 选中条目
        /// </summary>
        /// <param name="itemIDs"></param>
        /// <param name="notify"></param>
        public void SetSnippetTreeViewSelection(IList<int> itemIDs, bool notify = true)
        {
            if (notify)
            {
                m_SnippetTreeView.SetSelection(itemIDs);
            }
            else
            {
                m_SnippetTreeView.SetSelectionWithoutNotify(itemIDs);
            }
            // 确保展示条目
            m_SnippetTreeView.RevealAndFrameSelectedItem();
        }

        /// <summary>
        /// 清除选择
        /// </summary>
        /// <param name="notify"></param>
        private void ClearSnippetTreeViewSelection(bool notify = true)
        {
            if (notify)
            {
                m_SnippetTreeView.SetSelection(new int[] { });
            }
            else
            {
                m_SnippetTreeView.SetSelectionWithoutNotify(new int[] { });
            }
        }

        /// <summary>
        /// 获取条目
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        private CustomTreeViewItem GetSnippetTreeViewItem(string guid)
        {
            int itemID = GetSnippetTreeViewItemIdBySnippetGuid(guid);
            return (itemID < 0 ? null : m_SnippetTreeView.FindItem(itemID));
        }

        /// <summary>
        /// 重命名条目
        /// </summary>
        private void BeginSnippetTreeViewItemRename(string guid)
        {
            CustomTreeViewItem item = GetSnippetTreeViewItem(guid);
            if (item != null) m_SnippetTreeView.BeginRename(item);
        }

        /// <summary>
        /// 重命名条目
        /// </summary>
        private void BeginSnippetTreeViewItemRename(int itemID)
        {
            CustomTreeViewItem item = m_SnippetTreeView.FindItem(itemID);
            if (item != null) m_SnippetTreeView.BeginRename(item);
        }

        /// <summary>
        /// 复制选中的代码段
        /// </summary>
        private void DuplicateSelectedSnippets()
        {
            List<SnippetInfo> snippets = GetSnippetTreeViewSelectedSnippets(true);
            if (snippets.Count == 0)
            {
                return;
            }
            IEnumerable<SnippetInfo> list = CodeExecutorManager.CloneSnippets(snippets);
            string[] guids = list.Select(o => o.guid).ToArray();
            Switch(guids.First());
            SetSnippetTreeViewSelection(guids, false);
            // 提示
            ShowNotification("Duplicated");
        }

        /// <summary>
        /// 删除代码段
        /// </summary>
        /// <param name="snippets"></param>
        /// <returns></returns>
        private bool DeleteSnippets(IList<SnippetInfo> snippets)
        {
            bool isOk = EditorUtility.DisplayDialog(
                "[Code Executor] Delete snippets",
                $"Are you sure to delete the following snippets?\n{string.Join("\n", snippets.Select(v => $"- {v}"))}",
                "Confirm!",
                "Cancel"
            );
            if (!isOk) return false;

            CodeExecutorManager.RemoveSnippets(snippets.Select(v => v.guid));
            return true;
        }

        /// <summary>
        /// 删除类别
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        private bool DeleteCategory(string category)
        {
            List<SnippetInfo> snippets = CodeExecutorData.GetSnippetsWithCategory(category);
            if (snippets.Count > 0)
            {
                int dialogResult = EditorUtility.DisplayDialogComplex(
                    "[Code Executor] Delete category",
                    $"Whether to delete snippets under category '{category}'?\n{string.Join("\n", snippets.Select(v => $"- {v.name}"))}",
                    "Keep snippets!",
                    "Cancel",
                    "Delete!"
                );
                if (dialogResult == 1)
                {
                    return false;
                }
                if (dialogResult == 2)
                {
                    CodeExecutorManager.RemoveSnippetsWithCategory(category, false);
                }
            }

            CodeExecutorManager.RemoveCategory(category, true);
            return true;
        }

        /// <summary>
        /// 删除选中的代码段和类别
        /// </summary>
        private void DeleteSelectedSnippetsAndCategories()
        {
            bool hasSelectedSnippets = false;

            Dictionary<string, bool> snippetRecord = new Dictionary<string, bool>();
            List<SnippetInfo> snippets = new List<SnippetInfo>();
            List<string> categories = new List<string>();
            foreach (int itemID in m_SnippetTreeView.GetSelection())
            {
                string guid = GetSnippetGuidBySnippetTreeViewItemId(itemID);
                if (guid != null)
                {
                    hasSelectedSnippets = true;
                    if (!snippetRecord.ContainsKey(guid))
                    {
                        snippets.Add(CodeExecutorData.GetSnippet(guid));
                        snippetRecord.Add(guid, true);
                    }
                }
                else
                {
                    // 类别
                    string category = GetSnippetCategoryBySnippetTreeViewItemId(itemID);
                    if (string.IsNullOrEmpty(category))
                    {
                        continue;
                    }
                    categories.Add(category);
                    // 类别下的代码段
                    foreach (SnippetInfo s in CodeExecutorData.GetSnippetsWithCategory(category))
                    {
                        if (!snippetRecord.ContainsKey(s.guid))
                        {
                            snippets.Add(s);
                            snippetRecord.Add(s.guid, true);
                        }
                    }
                }
            }

            if (hasSelectedSnippets && snippets.Count > 0 && !DeleteSnippets(snippets))
            {
                return;
            }

            foreach (string category in categories)
            {
                DeleteCategory(category);
            }
        }

        /// <summary>
        /// 创建新的类别
        /// </summary>
        private void CreateNewCategory()
        {
            // 新增类别
            string category = CodeExecutorManager.GetNonDuplicateCategoryName("NewCategory");
            CodeExecutorManager.AddCategory(category);
            // 重命名类别
            int itemID = GetSnippetTreeViewItemIdByCategory(category);
            BeginSnippetTreeViewItemRename(itemID);
        }

        #endregion

    }

}
