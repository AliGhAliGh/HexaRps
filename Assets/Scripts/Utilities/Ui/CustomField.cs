using System;
using TextHandlers;
using UnityEngine;
using UnityEngine.UI;

namespace Utilities.Ui
{
    public class CustomField : MonoBehaviour
    {
        [SerializeField] private InputField myField;
        [SerializeField] private LuiText myPlaceholder;

        public Action<string> OnSubmitString
        {
            set
            {
                myField.onSubmit.RemoveAllListeners();
                myField.onSubmit.AddListener(data => value?.Invoke(data));
            }
        }

        public Action OnSubmit
        {
            set
            {
                myField.onSubmit.RemoveAllListeners();
                myField.onSubmit.AddListener(_ => value?.Invoke());
            }
        }

        public string Text
        {
            get => myField.text;
            set => myField.text = value;
        }

        public bool Interactable
        {
	        get => myField.interactable;
	        set => myField.interactable = value;
        }

        public string Placeholder
        {
            get => myPlaceholder.Text;
            set => myPlaceholder.SetText(value);
        }

        public void Select()
        {
            myField.Select();
            myField.ActivateInputField();
        }
    }
}
