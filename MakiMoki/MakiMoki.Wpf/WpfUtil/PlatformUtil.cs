using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yarukizero.Net.MakiMoki.Wpf.WpfUtil {
	static class PlatformUtil {

		public static void StartBrowser(Uri uri) {
			var b = WpfConfig.WpfConfigLoader.SystemConfig.BrowserPath;
			try {
				if(!string.IsNullOrEmpty(b)) {
					System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(b) {
						Arguments = $"\"{ uri.AbsoluteUri }\""
					});
				} else {
					System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(uri.AbsoluteUri));
				}
			}
			catch(System.ComponentModel.Win32Exception) {
				// 関連付け実行に失敗
				Util.Futaba.PutInformation(new Data.Information(
					string.IsNullOrEmpty(b) ? "URLの関連付け設定が存在しないか無効です" : $"ブラウザ\"{ b }\"の起動に失敗しました"));
			}
		}
	}
}
