﻿#if UNITY_EDITOR
using ChenPipi.CodeExecutor.Editor;
using UnityEditor;
using UnityEngine;

namespace ChenPipi.CodeExecutor.Examples
{

    /// <summary>
    /// 给 CodeExecutor 注入代码执行模式
    /// </summary>
    public static class InjectorCSharp
    {

        /// <summary>
        /// 注册 CodeExecutor 执行模式
        /// </summary>
        [CodeExecutorRegistration(0)]
        private static void Register()
        {
            if (!CodeExecutorSettings.enableBuiltinExecModeCSharp)
            {
                return;
            }
            CodeExecutorManager.RegisterExecMode(new ExecutionMode
            {
                name = "C#",
                executor = Executor,
            });
        }

        public static object[] Executor(string code)
        {
            object[] results = ExecutionHelperCSharp.ExecuteSnippetCode(code);

            CodeExecutorManager.ShowNotification("Executed");

            return results;
        }

    }

}

#endif
