using System.Linq;
using TextHandlers;
using UnityEditor;
using UnityEngine;


public class TranslatorWindow : EditorWindow
{
    public string key;
    [Multiline] public string persianText;
    [Multiline] public string englishText;

    private string _lastKey;
    private TranslatorConfig _translator;
    private SerializedProperty _propKey;
    private SerializedProperty _propPersianTxt;
    private SerializedProperty _propEnglishTxt;
    private SerializedObject _so;
    private TranslatorConfig.Item _foundMessage;

    private bool ItemExist { set; get; }

    private TranslatorConfig Translator =>
        _translator ??=
            AssetDatabase.LoadAssetAtPath<TranslatorConfig>("Assets/Scriptable Objects/Translator.asset");

    [MenuItem("Translator/Translator")]
    public static void OpenWindow() => GetWindow<TranslatorWindow>("TranslatorWindow");

    private TranslatorConfig.Item GetItemByKey()
    {
        var item = Translator?.items?.FirstOrDefault(message => message.key.Equals(_propKey.stringValue));
        return item == null
            ? null
            : new TranslatorConfig.Item
            {
                key = item.key,
                text = item.text
            };
    }

    private TranslatorConfig.Item GetItemByPersian()
    {
        var item = Translator.items.FirstOrDefault(
            message => message.text.Farsi.Equals(_propPersianTxt.stringValue));
        return item == null
            ? null
            : new TranslatorConfig.Item
            {
                key = item.key,
                text = item.text
            };
    }

    private void OnEnable()
    {
        _so = new SerializedObject(this);
        _propKey = _so.FindProperty(nameof(key));
        _propPersianTxt = _so.FindProperty(nameof(persianText));
        _propEnglishTxt = _so.FindProperty(nameof(englishText));
        key = persianText = englishText = "";
        ItemExist = false;
    }

    private void OnValidate()
    {
        if (!string.IsNullOrEmpty(key))
            _foundMessage = GetItemByKey();
        else if (!string.IsNullOrEmpty(persianText))
            _foundMessage = GetItemByPersian();

        if (_lastKey != key && _foundMessage != null)
        {
            _lastKey = key;
            persianText = _foundMessage.text.Farsi;
            englishText = _foundMessage.text.English;
        }

        ItemExist = _foundMessage != null;
    }

    private void OnGUI()
    {
        _so.Update();
        GUILayout.Space(20);

        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            GUILayout.Label("Amytis Translator", EditorStyles.largeLabel);
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(_propKey);
                    GUILayout.EndHorizontal();
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(_propPersianTxt);
                    GUILayout.EndHorizontal();
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(_propEnglishTxt);
                    GUILayout.EndHorizontal();
                }
            }
        }

        using (new EditorGUI.DisabledScope(ItemExist))
        {
            if (GUILayout.Button("ADD"))
            {
                var item = new TranslatorConfig.Item
                {
                    key = key,
                    text = new LString
                    {
                        Farsi = persianText,
                        English = englishText
                    }
                };
                Translator.items.Add(item);
                Save();
            }
        }

        using (new EditorGUI.DisabledScope(!ItemExist))
        {
            if (GUILayout.Button("Change"))
            {
                Translator.items.ForEach(message =>
                {
                    if (message.key.Equals(_foundMessage.key))
                    {
                        message.text.Farsi = persianText;
                        message.text.English = englishText;
                    }
                });
                Save();
            }

            if (GUILayout.Button("Remove"))
            {
                TranslatorConfig.Item temp = null;
                Translator.items.ForEach(message =>
                {
                    if (message.key.Equals(_foundMessage.key))
                    {
                        temp = message;
                    }
                });
                if (temp != null)
                {
                    Translator.items.Remove(temp);
                }

                Save();
            }
        }


        if (!ItemExist)
        {
            _so.ApplyModifiedProperties();
            return;
        }

        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            GUILayout.Label("Already Exists", EditorStyles.helpBox);
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Key", GUILayout.Width(100));
                    GUILayout.TextArea(_foundMessage.key);
                    GUILayout.EndHorizontal();
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("PersianTxt", GUILayout.Width(100));
                    GUILayout.TextArea(_foundMessage.text.Farsi);
                    GUILayout.EndHorizontal();
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("EnglishTxt", GUILayout.Width(100));
                    GUILayout.TextArea(_foundMessage.text.English);
                    GUILayout.EndHorizontal();
                }
            }
        }

        _so.ApplyModifiedProperties();
        return;

        void Save()
        {
            EditorUtility.SetDirty(Translator);
            OnValidate();
        }
    }
}