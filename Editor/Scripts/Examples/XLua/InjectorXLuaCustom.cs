#if UNITY_EDITOR
using ChenPipi.CodeExecutor.Editor;
using UnityEditor;
using UnityEngine;

namespace ChenPipi.CodeExecutor.Examples
{

    /// <summary>
    /// 给 CodeExecutor 注入代码执行模式
    /// </summary>
    public static class InjectorXLuaCustom
    {

        /// <summary>
        /// 注册 CodeExecutor 执行模式
        /// </summary>
        [CodeExecutorRegistration(5)]
        private static void Register()
        {
            if (!CodeExecutorSettings.enableBuiltinExecModeXLuaCustom)
            {
                return;
            }
            // 初始化 xLua Helper
            if (!ExecutionHelperXLua.isReady && !ExecutionHelperXLua.Init(InjectorXLua.XLuaAssemblyName))
            {
                return;
            }
            // 注册
            CodeExecutorManager.RegisterExecMode(new ExecutionMode
            {
                name = "xLua (Custom)",
                executor = Executor,
            });
        }

        public static object[] Executor(string code)
        {
            object luaEnv = GetLuaEnv();
            if (luaEnv == null)
            {
                if (!CodeExecutorManager.ShowNotification("This execution mode is currently unavailable"))
                {
                    EditorUtility.DisplayDialog("CodeExecutor", "This execution mode is currently unavailable!", "OK");
                }
                return null;
            }

            object[] results = ExecutionHelperXLua.ExecuteCode(code, luaEnv);

            CodeExecutorManager.ShowNotification("Executed");

            return results;
        }

        public static object GetLuaEnv()
        {
            // Return your LuaEnv object
            return null;
        }

    }

}

#endif
