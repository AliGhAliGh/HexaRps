using System.Linq;

namespace TextHandlers
{
    public static class UPersianUtils
    {
        public static string RtlFix(this string str)
        {
            str = str.Replace('ی', 'ﻱ');
            str = str.Replace('ک', 'ﻙ');
            str = ArabicSupport.ArabicFixer.Fix(str, true, false);
            str = str.Replace('ﺃ', 'آ');
            return str;
        }

        public static bool IsRtl(this string str) =>
            str.Any(@char => (@char >= 1536 && @char <= 1791) || (@char >= 65136 && @char <= 65279));
    }
}