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
			// System.Net.WebUtility.HtmlEncode(x) ♡などをスルーするので自前で解析もする
			var sb = new StringBuilder(System.Net.WebUtility.HtmlEncode(input));
			for(var i = 0; i < sb.Length; i++) {
				var c = sb[i];

				// 念のためサローゲートペアの処理を入れる
				if(char.IsHighSurrogate(c)) {
					if(sb.Length <= (i + 1)) {
						// 変な文字が渡されてるので削除して終了
						sb.Remove(i, 1);
						break;
					} else {
						var c2 = sb[i + 1];
						var b = FutabaEncoding.GetBytes(new string(new[] { c, c2 }));
						var s = FutabaEncoding.GetString(b);
						if(s == FallbackUnicodeString) {
							var ss = ConvertHtmlEntityFromhSurrogateChars(c, c2);
							sb.Remove(i, 2);
							sb.Insert(i, ss);
							i += ss.Length;
						} else {
							sb.Remove(i, 2);
							sb.Insert(i, s);
							i++;
						}
					}
				} else {
					var b = FutabaEncoding.GetBytes(c.ToString());
					var s = FutabaEncoding.GetString(b);
					if(s == FallbackUnicodeString) {
						var ss = $"&#{ (uint)c };";
						sb.Remove(i, 1);
						sb.Insert(i, ss);
						i += ss.Length;
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
	}
}
