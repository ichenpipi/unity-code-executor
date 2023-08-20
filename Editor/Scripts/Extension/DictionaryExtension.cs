using System.Collections.Generic;
using System.Linq;

namespace ChenPipi.CodeExecutor.Editor
{

    public static class DictionaryExtension
    {

        public static TKey GetFirstKeyByValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TValue value)
        {
            return dict.FirstOrDefault(x => x.Value.Equals(value)).Key;
        }

    }

}
