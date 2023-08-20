#if UNITY_EDITOR
using ChenPipi.CodeExecutor.Editor;
using UnityEditor;
using UnityEngine;

namespace ChenPipi.CodeExecutor.Example
{

    /// <summary>
    /// 给 CodeExecutor 注入 XLua 代码执行模式
    /// </summary>
    public static class CodeExecutorInjectorXLuaCustom
    {

        /// <summary>
        /// 注册
        /// </summary>
        [CodeExecutorRegistration(5)]
        private static void Register()
        {
            if (!CodeExecutorSettings.enableBuiltinExecModeXLua)
            {
                return;
            }
            CodeExecutorManager.RegisterExecMode(new ExecutionMode
            {
                name = "XLua (Custom)",
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
                    EditorUtility.DisplayDialog("CodeExecutorInjectorXLuaCustom", "This execution mode is currently unavailable!", "OK");
                }
                return null;
            }

            object[] results = InjectHelperXLua.ExecuteCode(code, luaEnv);

            CodeExecutorManager.ShowNotification("Executed");

            return results;
        }

        public static object GetLuaEnv()
        {
            // Return your in-game LuaEnv object
            return null;
        }

    }

}

#endif
