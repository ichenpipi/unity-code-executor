#if UNITY_EDITOR
using ChenPipi.CodeExecutor.Editor;
using UnityEditor;
using UnityEngine;

namespace ChenPipi.CodeExecutor.Examples
{

    /// <summary>
    /// 给 CodeExecutor 注入代码执行模式
    /// </summary>
    public static class InjectorXLua
    {

        /// <summary>
        /// XLua 所在的程序集名称，置空则使用默认程序集 "Assembly-CSharp"
        /// </summary>
        public const string XLuaAssemblyName = "Assembly-CSharp";

        /// <summary>
        /// 注册 CodeExecutor 执行模式
        /// </summary>
        [CodeExecutorRegistration(1)]
        private static void Register()
        {
            if (!CodeExecutorSettings.enableBuiltinExecModeXLua)
            {
                return;
            }
            // 初始化 xLua Helper
            if (!ExecutionHelperXLua.isReady && !ExecutionHelperXLua.Init(XLuaAssemblyName))
            {
                return;
            }
            // 注册
            CodeExecutorManager.RegisterExecMode(new ExecutionMode
            {
                name = "xLua (Standalone)",
                executor = Executor,
            });
        }

        public static object[] Executor(string code)
        {
            object[] results = ExecutionHelperXLua.ExecuteCode(code);

            CodeExecutorManager.ShowNotification("Executed");

            return results;
        }

    }

}

#endif
