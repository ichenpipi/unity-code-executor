#if UNITY_EDITOR
using ChenPipi.CodeExecutor.Editor;
using UnityEditor;
using UnityEngine;

namespace ChenPipi.CodeExecutor.Examples
{

    /// <summary>
    /// 给 CodeExecutor 注入 XLua 代码执行模式
    /// </summary>
    public static class CodeExecutorInjectorXLua
    {

        /// <summary>
        /// XLua 所在的程序集名称，置空则使用默认程序集 "Assembly-CSharp"
        /// </summary>
        private const string k_XLuaAssemblyName = "Assembly-CSharp";

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
            InjectHelperXLua.Init(k_XLuaAssemblyName);
            // 注册
            if (InjectHelperXLua.isReady)
            {
                CodeExecutorManager.RegisterExecMode(new ExecutionMode
                {
                    name = "XLua (Standalone)",
                    executor = Executor,
                });
            }
        }

        public static object[] Executor(string code)
        {
            object[] results = InjectHelperXLua.ExecuteCode(code);

            CodeExecutorManager.ShowNotification("Executed");

            return results;
        }

    }

}

#endif
