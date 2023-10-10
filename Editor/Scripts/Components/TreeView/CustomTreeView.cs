using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ChenPipi.CodeExecutor.Editor
{

    internal class CustomTreeView : TreeView
    {

        public Func<TreeViewItem, IList<TreeViewItem>> buildRows;

        public Action<TreeViewItem> buildItems;

        public event Action<CustomTreeViewItem> beforeDrawRowGUI;

        public Action<CustomTreeViewItem, Rect> drawRowGUIUnderlay;

        public Action<CustomTreeViewItem, Rect> drawRowGUI;

        public Action<CustomTreeViewItem, Rect> drawRowGUIOverlay;

        public Action<GenericMenu> buildMenu;

        public Action<GenericMenu, int> buildItemMenu;

        public event Action<int[]> onSelectionChange;

        public event Action<int> onItemClicked;

        public event Action<int> onItemDoubleClicked;

        public event Action onContextClicked;

        public event Action<int> onItemContextClicked;

        public event Action<int, string, string> onItemRenamed;

        public event Action<int[]> onItemDragged;

        public event Action<int[], int> onItemDropped;

        public event Action<Event> onKeyDown;

        public CustomTreeView(TreeViewState treeViewState) : base(treeViewState)
        {
            showBorder = false;
            depthIndentWidth = 14f;
        }

        #region Build Items

        protected override TreeViewItem BuildRoot()
        {
            // 创建根节点
            TreeViewItem root = new TreeViewItem()
            {
                id = 0,
                depth = -1,
            };
            // 创建子节点
            buildItems?.Invoke(root);
            // 返回根节点
            return root;
        }

        #endregion

        #region Rows

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            return (buildRows != null ? buildRows.Invoke(root) : base.BuildRows(root));
        }

        protected override void BeforeRowsGUI()
        {
            base.BeforeRowsGUI();
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            CustomTreeViewItem item = (CustomTreeViewItem)args.item;

            beforeDrawRowGUI?.Invoke(item);

            drawRowGUIUnderlay?.Invoke(item, args.rowRect);

            if (drawRowGUI != null)
            {
                drawRowGUI.Invoke(item, args.rowRect);
            }
            else
            {
                base.RowGUI(args);
            }

            drawRowGUIOverlay?.Invoke(item, args.rowRect);
        }

        protected override void AfterRowsGUI()
        {
            base.AfterRowsGUI();
        }

        #endregion

        #region KeyEvent

        protected override void KeyEvent()
        {
            Event current = Event.current;
            if (current.isKey && current.type == EventType.KeyDown)
            {
                onKeyDown?.Invoke(current);
            }
        }

        #endregion

        #region Selection

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return true;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            onSelectionChange?.Invoke(selectedIds.ToArray());
        }

        #endregion

        #region Context Clicked

        protected override void ContextClicked()
        {
            onContextClicked?.Invoke();

            if (buildMenu != null)
            {
                GenericMenu menu = new GenericMenu();
                buildMenu.Invoke(menu);
                menu.ShowAsContext();
                // 确保立刻显示菜单
                Event.current.Use();
            }
        }

        protected override void ContextClickedItem(int id)
        {
            onItemContextClicked?.Invoke(id);

            if (buildItemMenu != null)
            {
                GenericMenu menu = new GenericMenu();
                buildItemMenu.Invoke(menu, id);
                menu.ShowAsContext();
                // 确保立刻显示菜单
                Event.current.Use();
            }
        }

        #endregion

        #region Item Clicked

        protected override void SingleClickedItem(int id)
        {
            onItemClicked?.Invoke(id);
        }

        protected override void DoubleClickedItem(int id)
        {
            onItemDoubleClicked?.Invoke(id);
        }

        #endregion

        #region Item Rename

        protected override bool CanRename(TreeViewItem item)
        {
            return (onItemRenamed != null);
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            onItemRenamed?.Invoke(args.itemID, args.newName, args.originalName);
        }

        #endregion

        #region Item DragAndDrop

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            foreach (int id in args.draggedItemIDs)
            {
                CustomTreeViewItem item = FindItem(id);
                if (item == null || item.isContainer)
                {
                    return false;
                }
            }
            return true;
        }

        protected override bool CanBeParent(TreeViewItem item)
        {
            return true;
        }

        protected readonly List<int> m_DraggedItemIDs = new List<int>();

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            PerformDrag(args);
        }

        protected bool IsValidDragDrop(DragAndDropArgs args)
        {
            IList<int> data = (IList<int>)DragAndDrop.GetGenericData("SnippetTreeView.DraggedItemIDs");
            if (data == null)
            {
                return false;
            }
            return true;
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            if (!IsValidDragDrop(args))
            {
                return DragAndDropVisualMode.Rejected;
            }

            if (args.performDrop)
            {
                PerformDrop(args);
            }

            return DragAndDropVisualMode.Link;
        }

        protected void PerformDrag(SetupDragAndDropArgs args)
        {
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.SetGenericData("SnippetTreeView.DraggedItemIDs", args.draggedItemIDs);
            DragAndDrop.StartDrag("SnippetTreeView");
            DragAndDrop.objectReferences = new UnityEngine.Object[] { };
            // 记录
            m_DraggedItemIDs.Clear();
            m_DraggedItemIDs.AddRange(args.draggedItemIDs);
            // 回调
            onItemDragged?.Invoke(m_DraggedItemIDs.ToArray());
        }

        protected void PerformDrop(DragAndDropArgs args)
        {
            // 放下
            int parentItemID = (args.parentItem?.id ?? -1);
            onItemDropped?.Invoke(m_DraggedItemIDs.ToArray(), parentItemID);
            // 清除记录
            m_DraggedItemIDs.Clear();
        }

        #endregion

        #region Interfaces

        public CustomTreeViewItem FindItem(int id)
        {
            return (CustomTreeViewItem)FindItem(id, rootItem);
        }

        public new void SetSelection(IList<int> selectedIDs)
        {
            base.SetSelection(selectedIDs, TreeViewSelectionOptions.FireSelectionChanged);
        }

        public void SetSelectionWithoutNotify(IList<int> selectedIDs)
        {
            base.SetSelection(selectedIDs, TreeViewSelectionOptions.None);
        }

        public void SetupDepths(TreeViewItem root)
        {
            SetupDepthsFromParentsAndChildren(root);
        }

        public void SetupParentsAndChildren(TreeViewItem root, IList<TreeViewItem> rows)
        {
            SetupParentsAndChildrenFromDepths(root, rows);
        }

        public void RevealAndFrameSelectedItem()
        {
            base.SetSelection(GetSelection(), TreeViewSelectionOptions.RevealAndFrame);
        }

        public new float GetContentIndent(TreeViewItem item)
        {
            return base.GetContentIndent(item);
        }

        #endregion

    }

}
