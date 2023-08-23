#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ChenPipi.CodeExecutor.Examples
{

    /// <summary>
    ///  CodeExecutor xLua 执行工具
    /// </summary>
    public static class ExecutionHelperXLua
    {

        /// <summary>
        /// 项目默认程序集名称
        /// </summary>
        public const string DefaultAssemblyName = "Assembly-CSharp";

        /// <summary>
        /// 初始化 xLua 环境
        /// </summary>
        /// <param name="assemblyName">XLua 所在的程序集名称</param>
        public static bool Init(string assemblyName = DefaultAssemblyName)
        {
            try
            {
                assemblyName = string.IsNullOrEmpty(assemblyName) ? DefaultAssemblyName : assemblyName;
                Assembly assembly = Assembly.Load(assemblyName);
                if (assembly != null)
                {
                    s_LuaEnvType = assembly.GetType("XLua.LuaEnv", true);
                    s_LuaTableType = assembly.GetType("XLua.LuaTable", true);
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[CodeExecutor] {e}");
            }

            if (s_LuaEnvType == null || s_LuaTableType == null)
            {
                Debug.LogError("[CodeExecutor] Unable to reach 'XLua', make sure you specify the correct assembly name!");
            }

            return false;
        }

        /// <summary>
        /// 是否就绪
        /// </summary>
        public static bool isReady => (s_LuaEnvType != null);

        /// <summary>
        /// XLua.LuaEnv
        /// </summary>
        private static Type s_LuaEnvType = null;

        /// <summary>
        /// XLua.LuaTable
        /// </summary>
        private static Type s_LuaTableType = null;

        /// <summary>
        /// 缓存 Lua 环境
        /// </summary>
        private static object s_LuaEnv = null;

        /// <summary>
        /// 执行代码
        /// </summary>
        /// <param name="code">Lua 代码</param>
        /// <param name="luaEnv">LuaEnv 对象</param>
        /// <returns></returns>
        public static object[] ExecuteCode(string code, dynamic luaEnv = null)
        {
            // 获取 XLua.LuaEnv
            if (s_LuaEnvType == null)
            {
                Debug.LogError("[CodeExecutor] Unable to reach type \'XLua.LuaEnv\'!");
                return null;
            }
            // 获取 XLua.LuaTable
            if (s_LuaTableType == null)
            {
                Debug.LogError("[CodeExecutor] Unable to reach \'XLua.LuaTable\'!");
                return null;
            }

            // Lua 虚拟机
            luaEnv ??= (s_LuaEnv ??= Activator.CreateInstance(s_LuaEnvType));

            // 为代码段创建一个临时执行环境
            dynamic codeEnv = luaEnv.NewTable();
            {
                dynamic metaTable = luaEnv.NewTable();
                metaTable.Set("__index", luaEnv.Global);
                codeEnv.SetMetaTable(metaTable);
            }

            // 包裹代码段
            string wrappedCode = WrapCodeSnippet(code);
            Debug.Log($"[CodeExecutor] Wrapped code:\r\n{wrappedCode}");
            // 执行代码
            object[] results = luaEnv.DoString(wrappedCode, "chunk", codeEnv);

            // 销毁临时执行环境
            codeEnv.Dispose();

            // 打印结果
            // if (results != null)
            // {
            //     string text = string.Join(", ", results.ToList().ConvertAll(o =>
            //     {
            //         if (o == null) return "nil";
            //         if (o is string) return $"\"{o}\"";
            //         if (o.GetType() == s_LuaTableType) return $"\"{{{o}}}\"";
            //         return o.ToString();
            //     }));
            //     Debug.Log($"[CodeExecutor] Code execution results: <color=#00FF00>{text}</color>\n");
            // }

            // 返回结果
            return results;
        }

        /// <summary>
        /// 包裹代码段
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private static string WrapCodeSnippet(string code)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string[] lines = code.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                stringBuilder.Append($"    {line}\r\n");
            }

            string wrappedCode = $@"
local ok, result = xpcall(function()
    -- ---------- Here are your codes ----------
{stringBuilder.ToString().TrimEnd('\r', '\n')}
    -- ---------- Here are your codes ----------
end, function(err)
    CS.UnityEngine.Debug.LogError(""[CodeExecutor] "" .. err)
end)

if ok then
    return result
end
";
            return wrappedCode;
        }

    }

}

#endif
