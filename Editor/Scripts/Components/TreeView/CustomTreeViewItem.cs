using UnityEditor.IMGUI.Controls;

namespace ChenPipi.CodeExecutor.Editor
{

    internal class CustomTreeViewItem : TreeViewItem
    {

        /// <summary>
        /// 是否为容器
        /// </summary>
        public bool isContainer = false;

        /// <summary>
        /// 自定义数据
        /// </summary>
        public object userData = null;

    }

}
