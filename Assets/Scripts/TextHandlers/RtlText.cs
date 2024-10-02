using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TextHandlers
{
    [AddComponentMenu("UI/RtlText")]
    public class RtlText : Text
    {
        private const char LINE_ENDING = '\n';

        public string BaseText => base.text;

        public override string text
        {
            get
            {
                var baseText = base.text;
                cachedTextGenerator.Populate(baseText, GetGenerationSettings(rectTransform.rect.size));
                var lines = cachedTextGenerator.lines as List<UILineInfo>;
                if (lines == null) return null;
                var linedText = "";
                for (var i = 0; i < lines.Count; i++)
                {
                    if (i < lines.Count - 1)
                    {
                        var startIndex = lines[i].startCharIdx;
                        var length = lines[i + 1].startCharIdx - lines[i].startCharIdx;
                        linedText += baseText.Substring(startIndex, length);
                        if (linedText.Length > 0 && linedText[^1] != '\n' && linedText[^1] != '\r')
                            linedText += LINE_ENDING;
                    }
                    else
                        linedText += baseText[lines[i].startCharIdx..];
                }

                return linedText.RtlFix();
            }
            set => base.text = value;
        }
    }
}