using System;
using UnityEngine;
using VInspector;

namespace Utilities.Ui
{
    public abstract class CodeSwitcher : MonoBehaviour
    {
        [SerializeField] private bool hasFather;

        [ShowIf(nameof(hasFather)), SerializeField]
        private CodeSwitcher father;

        [NonSerialized] public Action<string> OnCodeChange;
        private string _code;

        private void Awake()
        {
            if (hasFather)
            {
                father.OnCodeChange += code => Code = code;
                Code = father.Code;
            }
        }

        public string Code
        {
            get => _code;
            set
            {
                if (value != null && _code == value) return;
                _code = value;
                CodeChanged(value);
                OnCodeChange?.Invoke(value);
            }
        }

        protected abstract void CodeChanged(string value);
    }
}