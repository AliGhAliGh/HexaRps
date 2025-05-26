using UnityEngine;
using Utilities;

namespace TextHandlers
{
	public class LuiText : MonoBehaviour
	{
		public string Text => rtlText.BaseText;

		[SerializeField] private string key;
		[SerializeField] private RtlText rtlText;

		private LString _lText;
		private bool _needForRefresh;

		public void Awake()
		{
			rtlText ??= GetComponent<RtlText>();
			_needForRefresh = true;
		}

		public void SetAlignment(TextAnchor anchor)
		{
			rtlText ??= GetComponent<RtlText>();
			rtlText.alignment = anchor;
		}

		public void SetEnabled(bool isEnabled) => rtlText.enabled = isEnabled;

		public void OnEnable()
		{
			if (_needForRefresh)
				Refresh();
		}

		public void SetText(string textKey, params string[] textParameters)
		{
			_lText = Translator.GetLString(textKey, textParameters);
			Refresh();
		}

		public void SetContext(string text)
		{
			Refresh();
			rtlText.text = text;
		}

		public void SetTextColor(Color c) => rtlText.color = new Color(c.r, c.g, c.b, rtlText.color.a);

		private void Refresh()
		{
			rtlText ??= GetComponent<RtlText>();
			if (!string.IsNullOrEmpty(key))
			{
				var f = Translator.GetLString(key);

				if (f != null)
					_lText = f;
				else LogManager.ShowMessage(Color.red, key + $" not found in {name}!");
			}

			if (_lText == null) return;
			_needForRefresh = false;
			rtlText.text = _lText.Value;
		}
	}
}
