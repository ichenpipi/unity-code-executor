using System;
using System.Text.RegularExpressions;
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
        /// 元素
        /// </summary>
        private VisualElement m_Detail = null;

        /// <summary>
        /// 执行按钮
        /// </summary>
        private ButtonWithIcon m_ExecuteButton = null;

        /// <summary>
        /// 初始化
        /// </summary>
        private void InitDetail()
        {
            m_Detail = new VisualElement()
            {
                name = "Detail",
                style =
                {
                    flexBasis = Length.Percent(100),
                    minWidth = 100,
                    flexDirection = FlexDirection.Column,
                    paddingTop = 0,
                    paddingBottom = 0,
                    paddingLeft = 0,
                    paddingRight = 0,
                    marginTop = 0,
                    marginBottom = 0,
                    marginLeft = 1, // 左边 1px 是拖拽线
                    marginRight = 0,
                }
            };
            m_ContentSplitView.Add(m_Detail);
            // 监听元素尺寸变化
            m_Detail.RegisterCallback<GeometryChangedEvent>(OnDetailGeometryChangedEventChanged);

            // 标题
            InitHeadline();

            // 分割线
            m_Detail.Add(GenHorizontalSeparator());

            // 代码输入框
            InitCodeEditor();

            // 执行按钮
            m_ExecuteButton = new ButtonWithIcon()
            {
                name = "ExecuteButton",
                focusable = false,
                style =
                {
                    height = 30,
                    marginTop = 3,
                    marginBottom = 3,
                    marginLeft = 3,
                    marginRight = 3,
                },
            };
            m_Detail.Add(m_ExecuteButton);
            // 文本
            m_ExecuteButton.SetText("Execute");
            m_ExecuteButton.SetTextFontStyle(FontStyle.Bold);
            // 图标
            m_ExecuteButton.SetIcon(PipiUtility.GetIcon("PlayButton"));
            // 点击回调
            m_ExecuteButton.clicked += OnExecuteButtonClick;
        }

        /// <summary>
        /// 执行按钮点击回调
        /// </summary>
        private void OnExecuteButtonClick()
        {
            ExecuteCode(m_CurrSnippetInfo.name, m_CurrSnippetInfo.code, m_CurrSnippetInfo.mode);
        }

        /// <summary>
        /// 元素尺寸变化回调
        /// </summary>
        /// <param name="evt"></param>
        private void OnDetailGeometryChangedEventChanged(GeometryChangedEvent evt) { }

        /// <summary>
        /// 执行代码
        /// </summary>
        /// <param name="snippetName"></param>
        /// <param name="codeText"></param>
        /// <param name="modeName"></param>
        private void ExecuteCode(string snippetName, string codeText, string modeName)
        {
            if (modeName.Equals(CodeExecutorManager.DefaultExecMode.name, StringComparison.OrdinalIgnoreCase))
            {
                ShowNotification($"Please select a valid execution mode!", 1f);
                return;
            }

            if (!CodeExecutorManager.HasExecMode(modeName))
            {
                ShowNotification($"Please select a valid execution mode!", 1f);
                return;
            }

            if (!codeText.EndsWith(Environment.NewLine))
            {
                codeText += Environment.NewLine;
            }
            codeText = ParseCode(snippetName, codeText);

            CodeExecutorManager.ExecuteCode(codeText, modeName);
        }

        /// <summary>
        /// 解析代码
        /// </summary>
        /// <param name="snippetName"></param>
        /// <param name="codeText"></param>
        /// <returns></returns>
        private string ParseCode(string snippetName, string codeText)
        {
            if (string.IsNullOrEmpty(codeText))
            {
                return codeText;
            }
            // 处理导入
            return ProcessImport(snippetName, codeText);
        }

        #region Syntax: Import

        /// <summary>
        /// 导入语法模板
        /// </summary>
        private const string k_ImportSyntaxPattern = "@import\\s*\\(?[\"\']([^\"\']+)[\"\']\\)?";

        /// <summary>
        /// 处理导入
        /// </summary>
        /// <param name="snippetName"></param>
        /// <param name="codeText"></param>
        /// <returns></returns>
        private string ProcessImport(string snippetName, string codeText)
        {
            string result = Regex.Replace(codeText, k_ImportSyntaxPattern, match =>
            {
                string importName = match.Groups[1].Value.Trim();
                if (string.IsNullOrWhiteSpace(importName))
                {
                    return string.Empty;
                }

                if (importName.Equals(snippetName, StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogError($"[CodeExecutor] Cannot import snippet itself!");
                    return string.Empty;
                }

                SnippetInfo snippetInfo = CodeExecutorData.GetSnippetWithName(importName);
                if (snippetInfo == null)
                {
                    Debug.LogError($"[CodeExecutor] Failed to import snippet named '{importName}'!");
                    return string.Empty;
                }

                string importCode = ParseCode(snippetName, snippetInfo.code);
                if (!importCode.EndsWith(Environment.NewLine))
                {
                    importCode += Environment.NewLine;
                }

                return importCode;
            });

            return result;
        }

        #endregion

    }

}
