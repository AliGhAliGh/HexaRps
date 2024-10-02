using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TextHandlers
{
    [CreateAssetMenu(fileName = "Translator", menuName = "ScriptableObjects/Translator")]
    public class TranslatorConfig : ScriptableObject
    {
        public List<Item> items = new();
        [System.Serializable]
        public class Item
        {
            public string key = "";
            public LString text;
        }

        public LString Get(string key) => items.FirstOrDefault(c => c.key == key)?.text;
    }
}
