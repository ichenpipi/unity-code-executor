using UnityEngine.UIElements;

namespace ChenPipi.CodeExecutor.Editor
{

    /// <summary>
    /// 窗口
    /// </summary>
    public partial class CodeExecutorWindow
    {

        /// <summary>
        /// 生成水平分割线
        /// </summary>
        /// <param name="margin"></param>
        /// <returns></returns>
        private VisualElement GenHorizontalSeparator(float margin = 0)
        {
            return new VisualElement()
            {
                name = "Separator",
                style =
                {
                    height = 1,
                    borderBottomWidth = 1,
                    borderBottomColor = separatorColor,
                    marginTop = margin,
                    marginBottom = margin,
                    flexShrink = 0,
                },
            };
        }

    }

}
