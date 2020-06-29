using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Yarukizero.Net.MakiMoki.Wpf.WpfUtil {
	static class PlatformUtil {
		private static readonly string VersionCheckUrl = "https://dev.yarukizero.net/futamaki-version-v1.py";

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

		public static async Task<bool> CheckNewVersion() {
			if(Uri.TryCreate(VersionCheckUrl, UriKind.Absolute, out var uri)) {
				using(var client = new System.Net.Http.HttpClient() {
					Timeout = TimeSpan.FromMilliseconds(5000),
				}) {
					client.DefaultRequestHeaders.Add("User-Agent", GetContentType());
					var ret = await client.SendAsync(new System.Net.Http.HttpRequestMessage(
						System.Net.Http.HttpMethod.Get, uri));
					if(ret.StatusCode == System.Net.HttpStatusCode.OK) {
						try {
							var b = await ret.Content.ReadAsByteArrayAsync();
							var r = JsonConvert.DeserializeObject<PlatformData.VersionCheckResponse>(Encoding.UTF8.GetString(b));
							if(r.Period.HasValue) {
								var exe = System.Reflection.Assembly.GetExecutingAssembly().Location;
								var ver = System.Diagnostics.FileVersionInfo.GetVersionInfo(exe);
								return ver.FileMinorPart < r.Period.Value;
							}
						}
						catch(JsonReaderException) { /* 無視する */ }
						catch(JsonSerializationException) { /* 無視する */ }
					}
				}
			}
			return false;
		}

		public static string GetContentType() {
			var exe = System.Reflection.Assembly.GetExecutingAssembly().Location;
			var ver = System.Diagnostics.FileVersionInfo.GetVersionInfo(exe);
			return string.Format("FutaMaki/period.{0:00}", ver.FileMinorPart);

		}

		public static string GetArchiveFileName() {
			var exe = System.Reflection.Assembly.GetExecutingAssembly().Location;
			var ver = System.Diagnostics.FileVersionInfo.GetVersionInfo(exe);
			return string.Format("{0}-{1:000}", Path.GetFileNameWithoutExtension(exe), ver.FileMinorPart);
		}
	}
}
