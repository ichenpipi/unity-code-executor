using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ChenPipi.CodeExecutor.Editor
{

    /// <summary>
    /// 窗口
    /// </summary>
    public partial class CodeExecutorWindow
    {

        /// <summary>
        /// 拖拽标签
        /// </summary>
        private const string k_DragAndDropGenericType = "CodeExecutorWindow";

        /// <summary>
        /// 拖放区域
        /// </summary>
        private VisualElement m_DropArea = null;

        /// <summary>
        /// 拖放样式
        /// </summary>
        private VisualElement m_DropTip = null;

        /// <summary>
        /// 初始化
        /// </summary>
        private void InitDropArea()
        {
            // 绑定视图
            m_DropArea = new VisualElement()
            {
                name = "DropArea",
                style =
                {
                    display = DisplayStyle.None,
                    position = Position.Absolute,
                    top = 0,
                    bottom = 0,
                    left = 0,
                    right = 0,
                    backgroundColor = dropTipBgColor,
                }
            };
            rootVisualElement.Add(m_DropArea);

            // 放置样式
            const int dropTipBorderWidth = 2;
            float dropTipMarginTop = m_Toolbar.style.height.value.value;
            m_DropTip = new VisualElement()
            {
                name = "DropTip",
                style =
                {
                    display = DisplayStyle.None,
                    flexBasis = Length.Percent(100),
                    alignItems = Align.Center,
                    justifyContent = Justify.Center,
                    borderTopWidth = dropTipBorderWidth,
                    borderBottomWidth = dropTipBorderWidth,
                    borderLeftWidth = dropTipBorderWidth,
                    borderRightWidth = dropTipBorderWidth,
                    borderTopColor = dropTipBorderColor,
                    borderBottomColor = dropTipBorderColor,
                    borderLeftColor = dropTipBorderColor,
                    borderRightColor = dropTipBorderColor,
                    marginTop = dropTipMarginTop,
                }
            };
            m_DropArea.Add(m_DropTip);
            // 文本
            Label label = new Label()
            {
                name = "Label",
                text = "Drop to Add Snippet",
                style =
                {
                    paddingLeft = 10,
                    paddingRight = 10,
                    fontSize = 36,
                    color = dropTipTextColor,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    whiteSpace = WhiteSpace.Normal,
#if UNITY_2021_1_OR_NEWER
                    unityTextOutlineColor = new Color(0f, 0f, 0f, 1f),
                    unityTextOutlineWidth = 1,
#endif
                }
            };
            m_DropTip.Add(label);

            // 监听拖拽事件
            m_DropArea.RegisterCallback<DragEnterEvent>(OnDragEnter);
            m_DropArea.RegisterCallback<DragLeaveEvent>(OnDragLeave);
            m_DropArea.RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            m_DropArea.RegisterCallback<DragPerformEvent>(OnDragPerform);
            m_DropArea.RegisterCallback<DragExitedEvent>(OnDragExited);

            // 一些特殊处理
            {
                // If the mouse move quickly, DragExitedEvent will only be sent to panel.visualTree.
                // Register a callback there to get notified.
                rootVisualElement.panel?.visualTree.RegisterCallback<DragExitedEvent>(OnDragExited);

                // When opening the window, root.panel is not set yet. Use these callbacks to make
                // sure we register a DragExitedEvent callback on root.panel.visualTree.
                m_DropArea.RegisterCallback<AttachToPanelEvent>((evt) =>
                {
                    evt.destinationPanel.visualTree.RegisterCallback<DragExitedEvent>(OnDragExited);
                });
                m_DropArea.RegisterCallback<DetachFromPanelEvent>((evt) =>
                {
                    evt.originPanel.visualTree.UnregisterCallback<DragExitedEvent>(OnDragExited);
                });
            }

            // 根节点监听拖拽事件（用于启用/禁用拖放区域）
            rootVisualElement.RegisterCallback<DragEnterEvent>((evt) => EnableDropArea());
            rootVisualElement.RegisterCallback<DragLeaveEvent>((evt) => DisableDropArea());
            rootVisualElement.RegisterCallback<DragExitedEvent>((evt) => DisableDropArea());
        }

        /// <summary>
        /// 启用拖放区域
        /// </summary>
        private void EnableDropArea()
        {
            m_DropArea.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// 禁用拖放区域
        /// </summary>
        private void DisableDropArea()
        {
            m_DropArea.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// 展示拖放样式
        /// </summary>
        private void ShowDropTip()
        {
            m_DropTip.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// 隐藏拖放样式
        /// </summary>
        private void HideDropTip()
        {
            m_DropTip.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// 拖拽进入元素回调
        /// </summary>
        /// <param name="evt"></param>
        private void OnDragEnter(DragEnterEvent evt)
        {
            if (!CanDrop()) return;

            ShowDropTip();
        }

        /// <summary>
        /// 拖拽离开元素回调
        /// </summary>
        /// <param name="evt"></param>
        private void OnDragLeave(DragLeaveEvent evt)
        {
            if (!CanDrop()) return;

            HideDropTip();
        }

        /// <summary>
        /// 拖拽结束回调
        /// </summary>
        /// <param name="evt"></param>
        private void OnDragExited(DragExitedEvent evt)
        {
            if (!CanDrop()) return;

            HideDropTip();
        }

        /// <summary>
        /// 拖拽状态更新回调
        /// </summary>
        /// <param name="evt"></param>
        private void OnDragUpdated(DragUpdatedEvent evt)
        {
            if (CanDrop())
            {
                ShowDropTip();
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
            }
            else
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
            }
        }

        /// <summary>
        /// 拖拽执行回调
        /// </summary>
        /// <param name="evt"></param>
        private void OnDragPerform(DragPerformEvent evt)
        {
            DragAndDrop.AcceptDrag();

            if (!CanDrop()) return;

            PerformDrop();
        }

        /// <summary>
        /// 是否可以放下
        /// </summary>
        /// <returns></returns>
        private bool CanDrop()
        {
            object genericData = DragAndDrop.GetGenericData(k_DragAndDropGenericType);
            if (genericData != null && ReferenceEquals(genericData, this))
            {
                return false;
            }
            foreach (Object obj in DragAndDrop.objectReferences)
            {
                if (obj is MonoScript) continue;
                if (obj is TextAsset) continue;
                if (obj is DefaultAsset) continue;
                return false;
            }
            return true;
        }

        /// <summary>
        /// 放下
        /// </summary>
        private void PerformDrop()
        {
            foreach (Object obj in DragAndDrop.objectReferences)
            {
                string path = PipiUtility.GetAssetAbsolutePath(obj);
                string fileText = File.ReadAllText(path, Encoding.UTF8);
                string fileName = new FileInfo(path).Name;
                CodeExecutorManager.AddSnippet(fileText, fileName, null);
            }
        }

    }

}
