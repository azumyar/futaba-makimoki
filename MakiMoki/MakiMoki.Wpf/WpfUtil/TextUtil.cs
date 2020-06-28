using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Yarukizero.Net.MakiMoki.Wpf.WpfUtil {
	static class TextUtil {
		public static string RawComment2Text(string com) {
			var s1 = Regex.Replace(com, @"<br>", Environment.NewLine,
				RegexOptions.IgnoreCase | RegexOptions.Multiline);
			var s2 = Regex.Replace(s1, @"<[^>]*>", "",
				RegexOptions.IgnoreCase | RegexOptions.Multiline);
			var s3 = System.Net.WebUtility.HtmlDecode(s2);

			return s3;
		}
	}
}
