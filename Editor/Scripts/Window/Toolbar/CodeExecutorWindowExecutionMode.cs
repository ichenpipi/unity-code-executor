using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace ChenPipi.CodeExecutor.Editor
{

    /// <summary>
    /// 窗口
    /// </summary>
    public partial class CodeExecutorWindow
    {

        /// <summary>
        /// 执行模式菜单
        /// </summary>
        private ToolbarMenu m_ExecutionModeMenu = null;

        /// <summary>
        /// 初始化
        /// </summary>
        private void InitToolbarExecutionModeMenu()
        {
            m_ExecutionModeMenu = new ToolbarMenu()
            {
                name = "ExecutionMode",
                tooltip = "Registered code execution modes",
                variant = ToolbarMenu.Variant.Popup,
                text = string.Empty,
                focusable = false,
                style =
                {
                    flexShrink = 0,
                    width = 205,
                    paddingTop = 0,
                    paddingBottom = 0,
                    paddingLeft = 4,
                    paddingRight = 4,
                    marginLeft = -1,
                    marginRight = 0,
                    left = 0,
                    alignItems = Align.Center,
                    justifyContent = Justify.Center,
                }
            };
            m_Toolbar.Add(m_ExecutionModeMenu);

            // 构建下拉菜单项
            BuildExecutionModeMenuItems();
        }

        #region Execution Mode Menu

        /// <summary>
        /// 构建执行模式下拉菜单项
        /// </summary>
        private void BuildExecutionModeMenuItems()
        {
            // 添加菜单项
            DropdownMenu menu = m_ExecutionModeMenu.menu;
            // 缺省模式
            menu.AppendAction(CodeExecutorManager.DefaultExecMode.name, OnExecutionModeMenuAction, GetExecutionModeMenuActionStatus);
            // 自定义模式
            foreach (var item in CodeExecutorManager.ExecutionModes)
            {
                menu.AppendAction(item.Value.name, OnExecutionModeMenuAction, GetExecutionModeMenuActionStatus);
            }
        }

        /// <summary>
        /// 获取执行模式菜单行为状态
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private DropdownMenuAction.Status GetExecutionModeMenuActionStatus(DropdownMenuAction action)
        {
            if (m_CurrSnippetInfo != null && action.name.Equals(m_CurrSnippetInfo.mode, StringComparison.OrdinalIgnoreCase))
            {
                return DropdownMenuAction.Status.Checked;
            }
            return DropdownMenuAction.Status.Normal;
        }

        /// <summary>
        /// 执行模式菜单行为回调
        /// </summary>
        /// <param name="action"></param>
        private void OnExecutionModeMenuAction(DropdownMenuAction action)
        {
            SwitchExecutionMode(action.name);
        }

        #endregion

        #region Execution Mode

        /// <summary>
        /// 设置执行模式文本
        /// </summary>
        /// <param name="mode"></param>
        private void SetExecutionModeText(string mode)
        {
            if (!CodeExecutorManager.HasExecMode(mode))
            {
                mode = CodeExecutorManager.DefaultExecMode.name;
            }
            m_ExecutionModeMenu.text = mode;
        }

        /// <summary>
        /// 切换执行模式
        /// </summary>
        /// <param name="mode"></param>
        private void SwitchExecutionMode(string mode)
        {
            if (m_CurrSnippetInfo == null)
            {
                return;
            }
            if (!CodeExecutorManager.HasExecMode(mode))
            {
                mode = CodeExecutorManager.DefaultExecMode.name;
            }
            // 更新数据
            m_CurrSnippetInfo.mode = mode;
            SetExecutionModeText(mode);
            // 序列化数据
            if (IsNewSnippet(m_CurrSnippetInfo))
            {
                CodeExecutorManager.SetNewSnippetExecMode(mode);
            }
            else
            {
                CodeExecutorManager.SetSnippetExecMode(m_CurrSnippetInfo.guid, mode);
            }
        }

        #endregion

    }

}
