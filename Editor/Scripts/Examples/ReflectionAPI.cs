#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ChenPipi.CodeExecutor.Example
{

    /// <summary>
    /// CodeExecutor 反射 API 工具
    /// </summary>
    public static class ReflectionAPI
    {

        #region Propeties

        private static Assembly s_CodeExecutorAssembly = null;

        public static Assembly CodeExecutorAssembly
        {
            get
            {
                if (s_CodeExecutorAssembly != null)
                {
                    return s_CodeExecutorAssembly;
                }
                try
                {
                    return (s_CodeExecutorAssembly = Assembly.Load("ChenPipi.CodeExecutor.Editor"));
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    return null;
                }
            }
        }

        private static Type s_CodeExecutorManagerType = null;

        public static Type CodeExecutorManagerType
        {
            get
            {
                if (s_CodeExecutorManagerType != null)
                {
                    return s_CodeExecutorManagerType;
                }
                try
                {
                    return (s_CodeExecutorManagerType = CodeExecutorAssembly?.GetType("ChenPipi.CodeExecutor.Editor.CodeExecutorManager", true));
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    return null;
                }
            }
        }

        private static Type s_ExecutionModeType = null;

        public static Type ExecutionModeType
        {
            get
            {
                if (s_ExecutionModeType != null)
                {
                    return s_ExecutionModeType;
                }
                try
                {
                    return (s_ExecutionModeType = CodeExecutorAssembly?.GetType("ChenPipi.CodeExecutor.Editor.CodeExecutorManager+ExecutionMode", true));
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    return null;
                }
            }
        }

        #endregion

        /// <summary>
        /// 注册执行模式
        /// </summary>
        /// <param name="modeName">模式名称</param>
        /// <param name="executor">代码执行器</param>
        /// <param name="modeDesc">模式描述</param>
        public static void RegisterExecMode(string modeName, Func<string, object[]> executor, string modeDesc = "")
        {
            // ChenPipi.CodeExecutor.Editor.CodeExecutorManager.RegisterExecMode(new ChenPipi.CodeExecutor.Editor.CodeExecutorManager.ExecutionMode
            // {
            //     name = modeName,
            //     desc = modeDesc,
            //     executor = executor,
            // });

            if (CodeExecutorAssembly == null)
            {
                Debug.LogError("[CodeExecutorInjectorCSharp] Unable to reach \'CodeExecutor\'!");
                return;
            }

            // 获取注册函数 RegisterExecMode
            MethodInfo registerExecModeMethod = CodeExecutorManagerType.GetMethod("RegisterExecMode", BindingFlags.Public | BindingFlags.Static);

            // 注册执行模式
            {
                object executionMode = Activator.CreateInstance(ExecutionModeType);
                {
                    // 名称
                    FieldInfo nameFieldInfo = ExecutionModeType.GetField("name");
                    nameFieldInfo.SetValue(executionMode, modeName);
                    // 名称
                    FieldInfo descFieldInfo = ExecutionModeType.GetField("desc");
                    descFieldInfo.SetValue(executionMode, modeDesc);
                    // 代码执行函数
                    FieldInfo executorFieldInfo = ExecutionModeType.GetField("executor");
                    executorFieldInfo.SetValue(executionMode, executor);
                }
                registerExecModeMethod!.Invoke(null, new object[] { executionMode, });
            }
        }

        /// <summary>
        /// 取消注册所有执行模式
        /// </summary>
        public static void UnregisterAllExecModes()
        {
            // ChenPipi.CodeExecutor.Editor.CodeExecutorManager.UnregisterAllExecModes();

            if (CodeExecutorAssembly == null)
            {
                Debug.LogError("[CodeExecutorInjectorCSharp] Unable to reach \'CodeExecutor\'!");
                return;
            }

            // 获取并执行反注册函数 UnregisterAllExecModes
            MethodInfo unregisterAllExecModesMethod = CodeExecutorManagerType.GetMethod("UnregisterAllExecModes", BindingFlags.Public | BindingFlags.Static);
            unregisterAllExecModesMethod!.Invoke(null, null);
        }

        /// <summary>
        /// 是否有指定名称的执行模式
        /// </summary>
        /// <param name="modeName"></param>
        /// <returns></returns>
        public static bool HasExecMode(string modeName)
        {
            // return ChenPipi.CodeExecutor.Editor.CodeExecutorManager.HasExecMode(modeName);

            if (CodeExecutorAssembly == null)
            {
                Debug.LogError("[CodeExecutorInjectorCSharp] Unable to reach \'CodeExecutor\'!");
                return false;
            }

            // 获取并执行反注册函数 UnregisterAllExecModes
            MethodInfo unregisterAllExecModesMethod = CodeExecutorManagerType.GetMethod("HasExecMode", BindingFlags.Public | BindingFlags.Static);
            return (bool)unregisterAllExecModesMethod!.Invoke(null, new object[] { modeName });
        }

        #region List Extension

        private static string Join<T>(this IList<T> list, string separator = "", bool ignoreEmptyOrNull = true)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < list.Count; ++i)
            {
                if (ignoreEmptyOrNull && string.IsNullOrEmpty(list[i].ToString()))
                {
                    continue;
                }
                builder.Append(list[i]);
                if (i < list.Count - 1)
                {
                    builder.Append(separator);
                }
            }
            return builder.ToString();
        }

        #endregion

    }

}

#endif
