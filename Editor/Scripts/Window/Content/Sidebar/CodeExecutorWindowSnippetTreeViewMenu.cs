using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ChenPipi.CodeExecutor.Editor
{

    /// <summary>
    /// 窗口
    /// </summary>
    public partial class CodeExecutorWindow
    {

        private static class SnippetTreeViewMenuContent
        {
            public static readonly GUIContent Execute = new GUIContent("Execute");
            public static readonly GUIContent Edit = new GUIContent("Edit");
            public static readonly GUIContent Rename = new GUIContent("Rename");
            public static readonly GUIContent Duplicate = new GUIContent("Duplicate");
            public static readonly GUIContent Top = new GUIContent("Top");
            public static readonly GUIContent UnTop = new GUIContent("Un-top");
            public static readonly GUIContent Delete = new GUIContent("Delete");
            public static readonly GUIContent CreateNewSnippet = new GUIContent("Create New Snippet");
            public static readonly GUIContent CreateNewSnippetUnderCategory = new GUIContent("Create New Snippet Under Category");
            public static readonly GUIContent CreateNewCategory = new GUIContent("Create New Category");
            public static readonly GUIContent CollapseAllCategories = new GUIContent("Collapse All");
            public static readonly GUIContent ExpandAllCategories = new GUIContent("Expand All");
            public static readonly GUIContent CopyToClipboard = new GUIContent("Copy Snippets To Clipboard (Ctrl+C)");
            public static readonly GUIContent PasteFromClipboard = new GUIContent("Paste Snippets From Clipboard (Ctrl+V)");
        }

        /// <summary>
        /// 创建菜单
        /// </summary>
        /// <param name="menu"></param>
        private void BuildSnippetTreeViewMenu(GenericMenu menu)
        {
            menu.AddItem(SnippetTreeViewMenuContent.CreateNewSnippet, false, SnippetTreeViewMenu_CreateNewSnippet);
            menu.AddItem(SnippetTreeViewMenuContent.CreateNewCategory, false, SnippetTreeViewMenu_CreateNewCategory);

            menu.AddSeparator(string.Empty);
            menu.AddItem(SnippetTreeViewMenuContent.CollapseAllCategories, false, SnippetTreeViewMenu_CollapseAllCategories);
            menu.AddItem(SnippetTreeViewMenuContent.ExpandAllCategories, false, SnippetTreeViewMenu_ExpandAllCategories);

            menu.AddSeparator(string.Empty);
            menu.AddItem(SnippetTreeViewMenuContent.PasteFromClipboard, false, SnippetTreeViewMenu_PasteFromClipboard);
        }

        /// <summary>
        /// 创建条目菜单
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="itemID"></param>
        private void BuildSnippetTreeViewItemMenu(GenericMenu menu, int itemID)
        {
            CustomTreeViewItem item = m_SnippetTreeView.FindItem(itemID);
            if (item.isContainer)
            {
                BuildSnippetTreeViewItemMenuCategory(menu, itemID);
            }
            else
            {
                BuildSnippetTreeViewItemMenuSnippet(menu, itemID);
            }
        }

        private void BuildSnippetTreeViewItemMenuSnippet(GenericMenu menu, int itemID)
        {
            bool isMultiSelection = (m_SnippetTreeView.GetSelection().Count > 1);

            if (isMultiSelection) menu.AddDisabledItem(SnippetTreeViewMenuContent.Execute);
            else menu.AddItem(SnippetTreeViewMenuContent.Execute, false, SnippetTreeViewMenu_ExecuteSnippet, itemID);

            menu.AddSeparator(string.Empty);
            if (isMultiSelection) menu.AddDisabledItem(SnippetTreeViewMenuContent.Edit);
            else menu.AddItem(SnippetTreeViewMenuContent.Edit, false, SnippetTreeViewMenu_EditSnippet, itemID);
            if (isMultiSelection) menu.AddDisabledItem(SnippetTreeViewMenuContent.Rename);
            else menu.AddItem(SnippetTreeViewMenuContent.Rename, false, SnippetTreeViewMenu_RenameSnippet, itemID);
            menu.AddItem(SnippetTreeViewMenuContent.Duplicate, false, SnippetTreeViewMenu_DuplicateSelectedSnippets);
            menu.AddItem(SnippetTreeViewMenuContent.Delete, false, SnippetTreeViewMenu_DeleteSelectedSnippetsAndCategories);

            menu.AddSeparator(string.Empty);
            if (isMultiSelection)
            {
                menu.AddItem(SnippetTreeViewMenuContent.Top, false, SnippetTreeViewMenu_TopSelectedSnippets);
                menu.AddItem(SnippetTreeViewMenuContent.UnTop, false, SnippetTreeViewMenu_UnTopSelectedSnippets);
            }
            else
            {
                SnippetInfo snippet = GetSnippetInfoBySnippetTreeViewItemId(itemID);
                if (snippet != null)
                {
                    if (snippet.top) menu.AddDisabledItem(SnippetTreeViewMenuContent.Top);
                    else menu.AddItem(SnippetTreeViewMenuContent.Top, false, SnippetTreeViewMenu_TopSelectedSnippets);
                    if (!snippet.top) menu.AddDisabledItem(SnippetTreeViewMenuContent.UnTop);
                    else menu.AddItem(SnippetTreeViewMenuContent.UnTop, false, SnippetTreeViewMenu_UnTopSelectedSnippets);
                }
            }

            menu.AddSeparator(string.Empty);
            menu.AddItem(SnippetTreeViewMenuContent.CreateNewSnippetUnderCategory, false, SnippetTreeViewMenu_CreateNewSnippetUnderCategory, itemID);
            menu.AddItem(SnippetTreeViewMenuContent.CreateNewCategory, false, SnippetTreeViewMenu_CreateNewCategory);

            menu.AddSeparator(string.Empty);
            menu.AddItem(SnippetTreeViewMenuContent.CopyToClipboard, false, SnippetTreeViewMenu_CopyToClipboard);
        }

        private void BuildSnippetTreeViewItemMenuCategory(GenericMenu menu, int itemID)
        {
            bool isMultiSelection = (m_SnippetTreeView.GetSelection().Count > 1);

            if (isMultiSelection) menu.AddDisabledItem(SnippetTreeViewMenuContent.Rename);
            else menu.AddItem(SnippetTreeViewMenuContent.Rename, false, SnippetTreeViewMenu_RenameCategory, itemID);
            menu.AddItem(SnippetTreeViewMenuContent.Delete, false, SnippetTreeViewMenu_DeleteSelectedSnippetsAndCategories);

            menu.AddSeparator(string.Empty);
            menu.AddItem(SnippetTreeViewMenuContent.CreateNewSnippetUnderCategory, false, SnippetTreeViewMenu_CreateNewSnippetUnderCategory, itemID);
            menu.AddItem(SnippetTreeViewMenuContent.CreateNewCategory, false, SnippetTreeViewMenu_CreateNewCategory);

            menu.AddSeparator(string.Empty);
            menu.AddItem(SnippetTreeViewMenuContent.CopyToClipboard, false, SnippetTreeViewMenu_CopyToClipboard);
        }

        private void SnippetTreeViewMenu_ExecuteSnippet(object itemID)
        {
            SnippetInfo snippet = GetSnippetInfoBySnippetTreeViewItemId((int)itemID);
            if (snippet != null)
            {
                ExecuteSnippet(snippet.name, snippet.code, snippet.mode);
            }
        }

        private void SnippetTreeViewMenu_EditSnippet(object itemID)
        {
            SnippetInfo snippet = GetSnippetInfoBySnippetTreeViewItemId((int)itemID);
            if (snippet == null)
            {
                return;
            }
            Switch(snippet);
            SetCodeEditorEditable(true, true);
        }

        private void SnippetTreeViewMenu_RenameSnippet(object itemID)
        {
            BeginSnippetTreeViewItemRename((int)itemID);
        }

        private void SnippetTreeViewMenu_DuplicateSelectedSnippets()
        {
            DuplicateSelectedSnippets();
        }

        private void SnippetTreeViewMenu_TopSelectedSnippets()
        {
            List<SnippetInfo> snippets = GetSnippetTreeViewSelectedSnippets(true);
            for (int i = 0; i < snippets.Count; i++)
            {
                SnippetInfo snippet = snippets[i];
                CodeExecutorManager.SetSnippetTop(snippet.guid, true, (i == snippets.Count - 1));
            }
        }

        private void SnippetTreeViewMenu_UnTopSelectedSnippets()
        {
            List<SnippetInfo> snippets = GetSnippetTreeViewSelectedSnippets(true);
            for (int i = 0; i < snippets.Count; i++)
            {
                SnippetInfo snippet = snippets[i];
                CodeExecutorManager.SetSnippetTop(snippet.guid, false, (i == snippets.Count - 1));
            }
        }

        private void SnippetTreeViewMenu_DeleteSelectedSnippetsAndCategories()
        {
            DeleteSelectedSnippetsAndCategories();
        }

        private void SnippetTreeViewMenu_RenameCategory(object itemID)
        {
            BeginSnippetTreeViewItemRename((int)itemID);
        }

        private void SnippetTreeViewMenu_CollapseAllCategories()
        {
            SnippetTreeViewCollapseAllCategories();
        }

        private void SnippetTreeViewMenu_ExpandAllCategories()
        {
            SnippetTreeViewExpandAllCategories();
        }

        private void SnippetTreeViewMenu_CopyToClipboard()
        {
            DoCopyToClipboard();
        }

        private void SnippetTreeViewMenu_PasteFromClipboard()
        {
            DoPasteFromClipboard();
        }

        private void SnippetTreeViewMenu_CreateNewSnippet()
        {
            SaveAsNewSnippet(string.Empty, null, CodeExecutorManager.DefaultExecMode.name, null);
        }

        private void SnippetTreeViewMenu_CreateNewSnippetUnderCategory(object itemID)
        {
            string category = GetSnippetCategoryBySnippetTreeViewItemId((int)itemID);
            SaveAsNewSnippet(string.Empty, null, CodeExecutorManager.DefaultExecMode.name, category);
        }

        private void SnippetTreeViewMenu_CreateNewCategory()
        {
            CreateNewCategory();
        }

    }

}
