using System;
using System.Collections.Generic;

namespace ChenPipi.CodeExecutor.Editor
{

    /// <summary>
    /// 数据
    /// </summary>
    public static class CodeExecutorData
    {

        #region UserData

        [Serializable]
        private class UserData
        {
            public int version = 0;
            public List<string> categories = new List<string>();
            public SnippetInfo newSnippet = new SnippetInfo();
            public List<SnippetInfo> snippets = new List<SnippetInfo>()
            {
                new SnippetInfo()
                {
                    guid = "e48e9761-cf81-4540-8155-dde48362a5b0",
                    createTime = 0,
                    editTime = 0,
                    top = false,
                    name = "HelloWorld",
                    code = @"UnityEngine.Debug.Log(""[CodeExecutor] Hello World!"");
UnityEngine.Debug.LogWarning(""[CodeExecutor] Hello World!"");
UnityEngine.Debug.LogError(""[CodeExecutor] Hello World!"");",
                    mode = "C#",
                    category = null,
                },
                new SnippetInfo()
                {
                    guid = "6ac20611-c6ec-4971-a121-7ff01b44b84f",
                    createTime = 0,
                    editTime = 0,
                    top = false,
                    name = "CrazyThursday",
                    code = "UnityEngine.Debug.LogError(\"[CodeExecutor] Crazy Thursday\");",
                    mode = "C#",
                    category = "Test",
                },
                new SnippetInfo()
                {
                    guid = "e4db94ed-9244-492a-9f88-bdee5ed616fd",
                    createTime = 0,
                    editTime = 0,
                    top = false,
                    name = "TestImport",
                    code = "@import(\"CrazyThursday\")\r\n\nUnityEngine.Debug.LogError(\"[CodeExecutor] V Me 50\");",
                    mode = "C#",
                    category = "Test",
                },
            };
        }

        private static UserData s_UserData;

        private static UserData userData
        {
            get
            {
                if (s_UserData == null)
                {
                    s_UserData = GetLocal();
                    GenerateMapping();
                }
                return s_UserData;
            }
        }

        #endregion

        #region Categories

        public static List<string> GetCategories()
        {
            List<string> list = new List<string>(userData.categories);
            foreach (SnippetInfo snippet in snippets)
            {
                if (!string.IsNullOrEmpty(snippet.category) && !list.Contains(snippet.category))
                {
                    userData.categories.Add(snippet.category);
                    list.Add(snippet.category);
                }
            }
            return list;
        }

        private static List<string> categories => s_UserData.categories;

        public static bool HasCategory(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }
            return categories.Contains(name);
        }

        public static void AddCategory(string name)
        {
            if (string.IsNullOrEmpty(name) || HasCategory(name))
            {
                return;
            }
            categories.Add(name);
        }

        public static void RemoveCategory(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }
            categories.Remove(name);
            // 更新代码段
            foreach (SnippetInfo snippet in snippets)
            {
                if (name.Equals(snippet.category, StringComparison.OrdinalIgnoreCase))
                {
                    snippet.category = null;
                }
            }
        }

        public static void RenameCategory(string originalName, string newName)
        {
            if (string.IsNullOrEmpty(originalName))
            {
                return;
            }
            int index = categories.IndexOf(originalName);
            if (index < 0)
            {
                return;
            }
            categories[index] = newName;
            // 更新代码段
            foreach (SnippetInfo snippet in snippets)
            {
                if (originalName.Equals(snippet.category, StringComparison.OrdinalIgnoreCase))
                {
                    snippet.category = newName;
                }
            }
        }

        #endregion

        #region NewSnippet

        /// <summary>
        /// 新代码段
        /// </summary>
        public static SnippetInfo newSnippet
        {
            get => userData.newSnippet;
        }

        #endregion

        #region Snippets

        #region Snippets Mapping

        private static readonly Dictionary<string, SnippetInfo> s_GuidMap = new Dictionary<string, SnippetInfo>();

        private static void GenerateMapping()
        {
            s_GuidMap.Clear();
            foreach (SnippetInfo snippet in snippets)
            {
                s_GuidMap.Add(snippet.guid, snippet);
            }
        }

        #endregion

        public static List<SnippetInfo> GetSnippets()
        {
            return snippets;
        }

        private static List<SnippetInfo> snippets => userData.snippets;

        public static SnippetInfo GetSnippet(string guid)
        {
            if (!string.IsNullOrEmpty(guid) && s_GuidMap.TryGetValue(guid, out SnippetInfo snippet))
            {
                return snippet;
            }
            return null;
        }

        public static SnippetInfo AddSnippet(string code, string name = null, string mode = null, string category = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = "Unnamed";
            }
            long time = PipiUtility.GetTimestamp();
            SnippetInfo snippet = new SnippetInfo()
            {
                guid = PipiUtility.NewGuid(),
                createTime = time,
                editTime = time,
                code = code,
                name = name,
                mode = mode,
                category = category,
            };
            snippets.Add(snippet);
            s_GuidMap.Add(snippet.guid, snippet);
            return snippet;
        }

        public static void RemoveSnippet(string guid)
        {
            if (string.IsNullOrEmpty(guid) || !s_GuidMap.TryGetValue(guid, out SnippetInfo snippet))
            {
                return;
            }
            s_GuidMap.Remove(guid);
            snippets.Remove(snippet);
        }

        public static void RemoveSnippetsWithCategory(string category)
        {
            foreach (SnippetInfo snippet in snippets.ToArray())
            {
                if (snippet.MatchCategory(category))
                {
                    s_GuidMap.Remove(snippet.guid);
                    snippets.Remove(snippet);
                }
            }
        }

        public static bool HasSnippet(string guid)
        {
            return (!string.IsNullOrEmpty(guid) && s_GuidMap.ContainsKey(guid));
        }

        public static SnippetInfo GetSnippetWithName(string name, string mode = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            foreach (SnippetInfo snippet in snippets)
            {
                if (snippet.MatchName(name) && (string.IsNullOrEmpty(mode) || snippet.MatchMode(mode)))
                {
                    return snippet;
                }
            }
            return null;
        }

        public static bool HasSnippetWithName(string name)
        {
            return (GetSnippetWithName(name) != null);
        }

        public static List<SnippetInfo> GetSnippetsWithCategory(string category)
        {
            List<SnippetInfo> list = new List<SnippetInfo>();
            foreach (SnippetInfo snippet in snippets)
            {
                if (snippet.MatchCategory(category))
                {
                    list.Add(snippet);
                }
            }
            return list;
        }

        #endregion

        #region Basic Interface

        /// <summary>
        /// 保存到本地
        /// </summary>
        public static void Save()
        {
            SetLocal(userData);
        }

        /// <summary>
        /// 重新加载
        /// </summary>
        public static void Reload()
        {
            s_UserData = GetLocal();
        }

        /// <summary>
        /// 重置
        /// </summary>
        public static void Reset()
        {
            SetLocal(s_UserData = new UserData());
        }

        #endregion

        #region Serialization & Deserialization

        /// <summary>
        /// 本地序列化文件路径
        /// </summary>
        internal static readonly string SerializedFilePath = string.Format(CodeExecutorManager.LocalFilePathTemplate, "data");

        /// <summary>
        /// 获取本地序列化的数据
        /// </summary>
        /// <returns></returns>
        private static UserData GetLocal()
        {
            return PipiUtility.GetLocal<UserData>(SerializedFilePath);
        }

        /// <summary>
        /// 将数据序列化到本地
        /// </summary>
        /// <param name="value"></param>
        private static void SetLocal(UserData value)
        {
            PipiUtility.SetLocal(SerializedFilePath, value);
        }

        #endregion

    }

    #region Type Definition

    /// <summary>
    /// 条目信息
    /// </summary>
    [Serializable]
    public class SnippetInfo
    {
        public string guid = null;
        public long createTime = 0;
        public long editTime = 0;
        public bool top = false;
        public string name = null;
        public string code = null;
        public string mode = null;
        public string category = null;

        public bool MatchGuid(string guid) => (this.guid != null && this.guid.Equals(guid, StringComparison.OrdinalIgnoreCase));

        public bool MatchName(string name) => (this.name != null && this.name.Equals(name, StringComparison.OrdinalIgnoreCase));

        public bool MatchMode(string mode) => (this.mode != null && this.mode.Equals(mode, StringComparison.OrdinalIgnoreCase));

        public bool MatchCategory(string category) =>
        (
            (string.IsNullOrEmpty(this.category) && string.IsNullOrEmpty(category)) ||
            (this.category != null && this.category.Equals(category, StringComparison.OrdinalIgnoreCase))
        );
    }

    [Serializable]
    public class SnippetWrapper
    {
        public List<SnippetInfoSimplified> snippets = new List<SnippetInfoSimplified>();
    }

    [Serializable]
    public class SnippetInfoSimplified
    {
        public string name = null;
        public string code = null;
        public string mode = null;
        public string category = null;

        public SnippetInfoSimplified() { }

        public SnippetInfoSimplified(SnippetInfo snippet)
        {
            name = snippet.name;
            code = snippet.code;
            mode = snippet.mode;
            category = snippet.category;
        }
    }

    #endregion

}
