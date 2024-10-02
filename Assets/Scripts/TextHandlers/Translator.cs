using UnityEngine;
using Utilities;

namespace TextHandlers
{
	public class Translator : Singleton<Translator>
	{
		[SerializeField] private TranslatorConfig config;

		public static LString GetLString(string key, params string[] lParam)
		{
			if (key == "")
				return new LString("");

			var i = Instance?.config?.Get(key);

			if (i == null) return null;
			var word = new LString
			{
				Arabic = i.Arabic, Deutsch = i.Deutsch, English = i.English, Farsi = i.Farsi, French = i.French,
				Russian = i.Russian, Spanish = i.Spanish
			};
			word.SetParam(lParam);
			return word;
		}
	}
}
