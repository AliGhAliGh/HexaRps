using System.Linq;
using System.Collections.Generic;
using UnityEngine;
// using static VInspector.Libs.VUtils;
// using static VInspector.Libs.VGUI;
// 


namespace VInspector
{
    [System.Serializable]
    public class SerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        public List<SerializedKeyValuePair<TKey, TValue>> serializedKvps = new List<SerializedKeyValuePair<TKey, TValue>>();

        public float dividerPos = .33f;

        public void OnBeforeSerialize()
        {
            foreach (var kvp in this)
                if (serializedKvps.FirstOrDefault(r => Comparer.Equals(r.Key, kvp.Key)) is SerializedKeyValuePair<TKey, TValue> serializedKvp)
                    serializedKvp.Value = kvp.Value;
                else
                    serializedKvps.Add(kvp);

            serializedKvps.RemoveAll(r => !ContainsKey(r.Key));

            for (var i = 0; i < serializedKvps.Count; i++)
                serializedKvps[i].index = i;

        }

        public void OnAfterDeserialize()
        {
            Clear();

            serializedKvps.RemoveAll(r => r.Key == null);

            foreach (var serializedKvp in serializedKvps)
                if (!(serializedKvp.isKeyRepeated = ContainsKey(serializedKvp.Key)))
                    Add(serializedKvp.Key, serializedKvp.Value);

        }



        [System.Serializable]
        public class SerializedKeyValuePair<TKey_, TValue_>
        {
            public TKey_ Key;
            public TValue_ Value;

            public int index;
            public bool isKeyRepeated;


            public SerializedKeyValuePair(TKey_ key, TValue_ value) { Key = key; Value = value; }

            public static implicit operator SerializedKeyValuePair<TKey_, TValue_>(KeyValuePair<TKey_, TValue_> kvp) => new SerializedKeyValuePair<TKey_, TValue_>(kvp.Key, kvp.Value);
            public static implicit operator KeyValuePair<TKey_, TValue_>(SerializedKeyValuePair<TKey_, TValue_> kvp) => new KeyValuePair<TKey_, TValue_>(kvp.Key, kvp.Value);
        }

    }

}