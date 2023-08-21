#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace ChenPipi.CodeExecutor.Examples
{

    /// <summary>
    /// CodeExecutor C# 注入工具
    /// </summary>
    public static class InjectHelperCSharp
    {

        /// <summary>
        /// 执行信息
        /// </summary>
        public struct ExecutionInfo
        {
            public string namespaceName;
            public string className;
            public string methodName;
        }

        /// <summary>
        /// 默认执行信息
        /// </summary>
        private static readonly ExecutionInfo s_DefaultExecutionInfo = new ExecutionInfo()
        {
            namespaceName = "WrapperNamespace",
            className = "WrapperClass",
            methodName = "WrapperMethod",
        };

        /// <summary>
        /// 执行代码段
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static object[] ExecuteSnippetCode(string code)
        {
            // 用临时的命名空间和类包裹代码段
            string wrappedCode = WrapCodeSnippet(code, s_DefaultExecutionInfo);
            Debug.Log($"[CodeExecutor] Wrapped code:\r\n{wrappedCode}");
            // 执行
            return ExecuteCode(wrappedCode, s_DefaultExecutionInfo);
        }

        /// <summary>
        /// 执行代码
        /// </summary>
        /// <param name="code"></param>
        /// <param name="executionInfo"></param>
        /// <returns></returns>
        public static object[] ExecuteCode(string code, ExecutionInfo executionInfo)
        {
            // 编译代码
            Assembly assembly = CompileCode(code);
            if (assembly == null)
            {
                return null;
            }

            // 获取包装类型
            string typeName = new string[] { executionInfo.namespaceName, executionInfo.className }.Join(".", true);
            object tempClassInstance = assembly.CreateInstance(typeName);
            if (tempClassInstance == null)
            {
                Debug.LogError($"[CodeExecutor] Cannot find class \'{typeName}\'");
                return null;
            }

            // 获取包装函数
            MethodInfo executeMethodInfo = tempClassInstance!.GetType().GetMethod(executionInfo.methodName);
            if (executeMethodInfo == null)
            {
                Debug.LogError($"[CodeExecutor] Cannot find method \'{executionInfo.methodName}\'");
                return null;
            }

            // 执行
            object result = executeMethodInfo!.Invoke(tempClassInstance, null);

            // 打印结果
            if (result != null)
            {
                Debug.Log($"[CodeExecutor] Code execution results: <color=#00FF00>{result}</color>\n");
            }

            // 返回结果
            return new[] { result };
        }

        /// <summary>
        /// 使用命名空间和类包裹代码段
        /// </summary>
        /// <param name="code"></param>
        /// <param name="executionInfo"></param>
        /// <returns></returns>
        private static string WrapCodeSnippet(string code, ExecutionInfo executionInfo)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("using System;\r\n");
            stringBuilder.Append("using UnityEngine;\r\n\r\n");

            if (!string.IsNullOrEmpty(executionInfo.namespaceName))
            {
                stringBuilder.Append($"namespace {executionInfo.namespaceName}\r\n");
                stringBuilder.Append("{\r\n");
            }

            stringBuilder.Append($"      public class {executionInfo.className}\r\n");
            stringBuilder.Append("      {\r\n");
            stringBuilder.Append($"          public object {executionInfo.methodName}()\r\n");
            stringBuilder.Append("          {\r\n");
            stringBuilder.Append("#pragma warning disable 0162\r\n"); // Avoid "Unreachable code detected"

            stringBuilder.Append($"              // ---------- Here are your codes ---------- \r\n");
            string[] lines = code.Split(';');
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                stringBuilder.Append($"              {line.Replace("\n", "")};\r\n");
            }
            stringBuilder.Append($"              // ---------- Here are your codes ---------- \r\n");

            stringBuilder.Append("              return null;\r\n");
            stringBuilder.Append("#pragma warning restore 0162\r\n");
            stringBuilder.Append("          }\r\n");
            stringBuilder.Append("      }\r\n");

            if (!string.IsNullOrEmpty(executionInfo.namespaceName))
            {
                stringBuilder.Append("}\r\n");
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// 编译代码
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static Assembly CompileCode(string code)
        {
            // 创建 CSharpCodeProvider 实例
            CSharpCodeProvider provider = new CSharpCodeProvider();

            // 创建编译参数容器
            CompilerParameters parameters = new CompilerParameters();

            // 添加所有程序集引用
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic || assembly.Location.Contains("mscorlib.dll")) continue;
                parameters.ReferencedAssemblies.Add(assembly.Location);
            }

            // 添加程序集引用
            // string unityAsmPath = $"{EditorApplication.applicationContentsPath}/Managed/UnityEngine";
            // string projectAsmPath = $"{Application.dataPath.Replace("Assets", "")}Library/ScriptAssemblies";
            // string[] assemblyPaths = new[]
            // {
            //     $"System.dll",
            //     $"{s_UnityAsmPath}/UnityEngine.dll",
            //     $"{s_UnityAsmPath}/UnityEngine.CoreModule.dll",
            //     $"{s_ProjectAsmPath}/Assembly-CSharp.dll",
            // };
            // foreach (string path in assemblyPaths)
            // {
            //     compilerParameters.ReferencedAssemblies.Add(path);
            // }

            // 只编译到内存中，不生成文件
            parameters.GenerateExecutable = false;
            parameters.GenerateInMemory = true;

//         // 编译代码
// #pragma warning disable CS0618
//         ICodeCompiler compiler = provider.CreateCompiler();
// #pragma warning restore CS0618
//         CompilerResults results = compiler.CompileAssemblyFromSource(parameters, code);

            // 编译代码
            CompilerResults results = provider.CompileAssemblyFromSource(parameters, code);

            // 编译失败
            if (results.Errors.Count > 0)
            {
                Debug.LogError("[CodeExecutor] Compilation failed!");
                foreach (CompilerError err in results.Errors)
                {
                    Debug.LogError(err.ErrorText);
                }
                return null;
            }

            // 返回程序集
            return results.CompiledAssembly;
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
