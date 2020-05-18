using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Yarukizero.Net.MakiMoki.Util {
	public static class TextUtil {
		public static string RowComment2Text(string com) {
			var s1 = Regex.Replace(com, "@<br>", Environment.NewLine,
				RegexOptions.IgnoreCase | RegexOptions.Multiline);
			var s2 = Regex.Replace(s1, @"<[^>]*>", "",
				RegexOptions.IgnoreCase | RegexOptions.Multiline);
			var s3 = System.Net.WebUtility.HtmlDecode(s2);

			return s3;
		}

		public static string RemoveCrLf(string text) {
			return Regex.Replace(text, @"[\r\n]", "", RegexOptions.Multiline);
		}

		public static string SafeSubstring(string text, int num) {
			return (num < text.Length) ? text.Substring(0, num) : text;
		}

		public static string Filter2SearchText(string input) {
			return CSharp.Japanese.Kanaxs.KanaEx.ToHiragana(CSharp.Japanese.Kanaxs.KanaEx.ToZenkakuKana(input));
		}

		public static string Comment2SearchText(string input) {
			var text = input.ToString();
			var t2 = Regex.Replace(text, @"<[^>]*>", "",
				RegexOptions.IgnoreCase | RegexOptions.Multiline);
			var t3 = System.Net.WebUtility.HtmlDecode(t2);

			return CSharp.Japanese.Kanaxs.KanaEx.ToHiragana(CSharp.Japanese.Kanaxs.KanaEx.ToZenkakuKana(t3));
		}
	}
}
