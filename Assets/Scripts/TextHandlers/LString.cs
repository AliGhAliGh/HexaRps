using System.Linq;
using UnityEngine;

namespace TextHandlers
{
    [System.Serializable]
    public class LString
    {
        [Multiline] public string Farsi = "";
        [Multiline] public string English = "";
        [Multiline] public string Arabic = "";
        [Multiline] public string Russian = "";
        [Multiline] public string Deutsch = "";
        [Multiline] public string French = "";
        [Multiline] public string Spanish = "";


        private LParam[] Params;

        public LString()
        {
        }

        public LString(string value)
        {
            Farsi = English = Arabic = Russian = Deutsch = French = Spanish = value;
        }

        public static LString operator +(LString a, LString b)
        {
            a.Arabic += b.Arabic;
            a.Farsi += b.Farsi;
            a.English += b.English;
            a.Spanish += b.Spanish;
            a.Deutsch += b.Deutsch;
            a.French += b.French;
            a.Russian += b.Russian;
            return a;
        }

        public static LString operator +(LString a, string b)
        {
            a.Arabic += b;
            a.Farsi += b;
            a.English += b;
            a.Spanish += b;
            a.Deutsch += b;
            a.French += b;
            a.Russian += b;
            return a;
        }

        public void SetParam(params LParam[] Params) => this.Params = Params;

        public void SetParam(params string[] Params)
        {
            this.Params = Params.Select(i => new LParam(i)).ToArray();
        }

        public string Value
        {
            get
            {
                var fStr = Farsi;

	            if (fStr == "")
                    fStr = English;

                if (Params != null)
                {
                    var index = 0;

                    foreach (var lp in Params)
                    {
                        fStr = fStr.Replace("[P" + index + "]", lp.value);
                        index++;
                    }
                }

                // if (TetraServer.language is TetraServer.Language.Arabic or TetraServer.Language.Farsi)
                // {
                //     for (var i = 0; i <= 9; i++)
                //         fStr = fStr.Replace((char)(48 + i), (char)(1776 + i));
                // }

                return fStr;
            }
        }
    }

    [System.Serializable]
    public class LParam
    {
        public string value;

        public LParam(string value)
        {
            this.value = value;
        }
    }
}
