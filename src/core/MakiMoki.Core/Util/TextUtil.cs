using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Yarukizero.Net.MakiMoki.Util {
	public static class TextUtil {
		private static readonly Encoding UTF32Encoding = Encoding.GetEncoding("UTF-32");
		private static readonly string FallbackUnicodeString = "\a";
		private static readonly Encoding FutabaEncoding = Encoding.GetEncoding(
			"Shift_JIS",
			new EncoderReplacementFallback(FallbackUnicodeString),
			DecoderFallback.ReplacementFallback);

		public static string RowComment2Text(string com) {
			var s1 = Regex.Replace(com, @"<br>", Environment.NewLine,
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
			return CSharp.Japanese.Kanaxs.KanaEx.ToHiragana(CSharp.Japanese.Kanaxs.KanaEx.ToZenkakuKana(input)).ToLower();
		}

		public static string Comment2SearchText(string input) {
			var text = input.ToString();
			var t2 = Regex.Replace(text, @"<[^>]*>", "",
				RegexOptions.IgnoreCase | RegexOptions.Multiline);
			var t3 = System.Net.WebUtility.HtmlDecode(t2);

			return CSharp.Japanese.Kanaxs.KanaEx.ToHiragana(CSharp.Japanese.Kanaxs.KanaEx.ToZenkakuKana(t3)).ToLower();
		}

		public static string ConvertUnicodeTextToFutabaComment(string input) {
			var sb = new StringBuilder();
			// Unicode8.0相当だけど今回の要件的には大丈夫なはず…
			var tee =System.Globalization.StringInfo.GetTextElementEnumerator(input);
			tee.Reset();
			while(tee.MoveNext()) {
				var te = tee.GetTextElement();
				if(1 < te.Length) {
					// 合字なのでまとめてエスケープする
					foreach(var c in te.ToCharArray()) {
						sb.Append($"&#{ (uint)c };");
					}
				} else {
					// 変換して失敗すればエスケープする
					// HTMLエスケープはふたば側に任せる
					var b = FutabaEncoding.GetBytes(te);
					var s = FutabaEncoding.GetString(b);
					if(s == FallbackUnicodeString) {
						sb.Append($"&#{ (uint)te[0] };");
					} else {
						sb.Append(te);
					}
				}
			}
			return sb.ToString();
		}

		public static int GetTextFutabaByteCount(string text) {
			return FutabaEncoding.GetByteCount(text);
		}

		public static string ConvertHtmlEntityFromhSurrogateChars(char high, char low) {
			var b = UTF32Encoding.GetBytes(new string(new[] { high, low }));
			var c = 0u;
			for(var i = 0; i < b.Length; i++) {
				c |= ((uint)b[i]) << i * 8;
			}
			return $"&#{ c };";
		}

		public static string GetStringYyyymmdd() => GetStringYyyymmdd(DateTime.Now);
		public static string GetStringYyyymmdd(DateTime time) {
			return time.ToString("yyyy/MM/dd", System.Globalization.CultureInfo.InvariantCulture);
		}

		public static string GetStringYyyymmddHhmmss() => GetStringYyyymmddHhmmss(DateTime.Now);
		public static string GetStringYyyymmddHhmmss(DateTime time) {
			return time.ToString("yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
		}
	}
}
