using UnityEditor;

namespace ChenPipi.CodeExecutor.Editor
{

    /// <summary>
    /// 菜单
    /// </summary>
    public static class CodeExecutorMenu
    {

        private const string k_MenuName = "Code Executor";

        #region Window Menu

        private const string k_WindowMenuName = "Window/" + k_MenuName;

        [MenuItem(k_WindowMenuName)]
        private static void WindowMenu_Open()
        {
            CodeExecutorManager.Open(true);
        }

        #endregion

    }

}
