using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using UnityEditor;

namespace ChenPipi.CodeExecutor.Editor
{

    /// <summary>
    /// 用于注册 Code Executor 执行模式的 Attribute，提供了控制函数执行顺序的能力，来达到控制执行模式注册顺序的目的
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    [MeansImplicitUse]
    public class CodeExecutorRegistrationAttribute : Attribute
    {

        /// <summary>
        /// 执行顺序（该值越小则函数越先执行）
        /// </summary>
        public int order { get; }

        /// <param name="order">执行顺序（该值越小则函数越先执行）</param>
        public CodeExecutorRegistrationAttribute(int order = 10)
        {
            this.order = order;
        }

        /// <summary>
        /// 注册器
        /// </summary>
        [InitializeOnLoadMethod]
        private static void Registrar()
        {
            List<(MethodInfo method, int order)> list = new List<(MethodInfo method, int order)>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                    {
                        object[] attributes = method.GetCustomAttributes(typeof(CodeExecutorRegistrationAttribute), false);
                        if (attributes.Length == 0)
                        {
                            continue;
                        }
                        CodeExecutorRegistrationAttribute attribute = attributes[0] as CodeExecutorRegistrationAttribute;
                        list.Add((method, attribute!.order));
                    }
                }
            }

            list.Sort((a, b) => a.order - b.order);

            foreach ((MethodInfo method, int order) item in list)
            {
                item.method.Invoke(null, null);
            }
        }

    }

}
