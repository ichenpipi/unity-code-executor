using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
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
        /// 导入按钮
        /// </summary>
        private ToolbarButton m_ImportButton = null;

        /// <summary>
        /// 初始化
        /// </summary>
        private void InitToolBarImportButton()
        {
            m_ImportButton = new ToolbarButton()
            {
                name = "Import",
                tooltip = "Import code from file",
                focusable = false,
                style =
                {
                    flexShrink = 0,
                    width = 25,
                    paddingTop = 0,
                    paddingBottom = 0,
                    paddingLeft = 2,
                    paddingRight = 2,
                    marginLeft = -1,
                    marginRight = 0,
                    left = 0,
                    alignItems = Align.Center,
                    justifyContent = Justify.Center,
                }
            };
            m_Toolbar.Add(m_ImportButton);
            // 图标
            m_ImportButton.Add(new Image()
            {
                image = PipiUtility.GetIcon("Import"),
                scaleMode = ScaleMode.ScaleToFit,
                style =
                {
                    width = 16,
                }
            });
            // 回调
            m_ImportButton.clicked += OnImportButtonClicked;
        }

        /// <summary>
        /// 导入按钮点击回调
        /// </summary>
        private void OnImportButtonClicked()
        {
            ImportFromFile();
        }

        /// <summary>
        /// 导入
        /// </summary>
        private void ImportFromFile()
        {
            const string title = "Import code from file";
            const string extension = "";
            string directory = Application.dataPath;
            string path = EditorUtility.OpenFilePanel(title, directory, extension);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            string fileText = File.ReadAllText(path, Encoding.UTF8);
            string fileName = new FileInfo(path).Name;
            CodeExecutorManager.AddSnippet(fileText, fileName, null);
        }

    }

}
