using System;
using System.Collections.Generic;
using System.Linq;

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
            public SnippetInfo newSnippet = new SnippetInfo();
            public List<SnippetInfo> snippets = new List<SnippetInfo>();
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

        /// <summary>
        /// 新代码段
        /// </summary>
        public static SnippetInfo newSnippet
        {
            get => userData.newSnippet;
        }

        #region Snippets

        public static List<SnippetInfo> snippets => userData.snippets;

        private static readonly Dictionary<string, SnippetInfo> s_GuidMap = new Dictionary<string, SnippetInfo>();

        private static void GenerateMapping()
        {
            s_GuidMap.Clear();
            foreach (SnippetInfo snippetInfo in snippets)
            {
                s_GuidMap.Add(snippetInfo.guid, snippetInfo);
            }
        }

        public static SnippetInfo GetSnippet(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }
            if (s_GuidMap.TryGetValue(guid, out SnippetInfo snippetInfo))
            {
                return snippetInfo;
            }
            return null;
        }

        public static SnippetInfo AddSnippet(string code, string name = null, string mode = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = "Unnamed";
            }
            long time = PipiUtility.GetTimestamp();
            SnippetInfo snippetInfo = new SnippetInfo()
            {
                guid = PipiUtility.NewGuid(),
                createTime = time,
                editTime = time,
                code = code,
                name = name,
                mode = mode,
            };
            snippets.Add(snippetInfo);
            s_GuidMap.Add(snippetInfo.guid, snippetInfo);
            return snippetInfo;
        }

        public static void RemoveSnippet(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return;
            }
            if (!s_GuidMap.TryGetValue(guid, out SnippetInfo snippetInfo))
            {
                return;
            }
            s_GuidMap.Remove(guid);
            snippets.Remove(snippetInfo);
        }

        public static bool HasSnippet(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return false;
            }
            return s_GuidMap.ContainsKey(guid);
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

    #region ItemInfo

    /// <summary>
    /// 条目信息
    /// </summary>
    [Serializable]
    public class SnippetInfo
    {

        public string guid = string.Empty;
        public long createTime = 0;
        public long editTime = 0;
        public bool top = false;
        public string name = string.Empty;
        public string code = string.Empty;
        public string mode = string.Empty;

        public bool MatchGuid(string guid)
        {
            return this.guid.Equals(guid, StringComparison.OrdinalIgnoreCase);
        }

        public bool MatchName(string name)
        {
            return this.name.Equals(name, StringComparison.OrdinalIgnoreCase);
        }

        public bool MatchMode(string mode)
        {
            return this.mode.Equals(mode, StringComparison.OrdinalIgnoreCase);
        }

    }

    [Serializable]
    public class SnippetWrapper
    {
        public List<SnippetInfoSimplified> snippets = new List<SnippetInfoSimplified>();
    }

    [Serializable]
    public class SnippetInfoSimplified
    {
        public string name = string.Empty;
        public string code = string.Empty;
        public string mode = string.Empty;
    }

    #endregion

}
