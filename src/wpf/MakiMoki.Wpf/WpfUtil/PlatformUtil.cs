using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Yarukizero.Net.MakiMoki.Wpf.WpfUtil {
	static class PlatformUtil {
		[System.Runtime.InteropServices.DllImport("Kernel32.dll")]
		private static extern bool DeleteFile(string lpFileName);

		public static string GetExePath() {
#if DEBUG
			var exe = System.Reflection.Assembly.GetExecutingAssembly().Location;
			return string.IsNullOrEmpty(exe) ? Environment.GetCommandLineArgs()[0] : exe;
#else
			return Environment.GetCommandLineArgs()[0];
#endif
		}

		public static void StartBrowser(Uri uri) {
			var b = WpfConfig.WpfConfigLoader.SystemConfig.BrowserPath;
			try {
				if(!string.IsNullOrEmpty(b)) {
					System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(b) {
						Arguments = $"\"{ uri.AbsoluteUri }\""
					});
				} else {
					//.Net Coreでは動かない
					//System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(uri.AbsoluteUri));
					WinApi.Win32.ShellExecute(IntPtr.Zero, null, uri.AbsoluteUri, null, null, WinApi.Win32.SW_SHOW);
				}
			}
			catch(System.ComponentModel.Win32Exception) {
				// 関連付け実行に失敗
				Util.Futaba.PutInformation(new Data.Information(
					string.IsNullOrEmpty(b) ? "URLの関連付け設定が存在しないか無効です" : $"ブラウザ\"{ b }\"の起動に失敗しました"));
			}
		}

		public static async Task<bool> CheckNewVersion() {
			if(Uri.TryCreate(PlatformConst.VersionCheckUrl, UriKind.Absolute, out var uri)) {
				try {
					var ret = await App.HttpClient.SendAsync(new System.Net.Http.HttpRequestMessage(
						System.Net.Http.HttpMethod.Get, uri));
					if(ret.StatusCode == System.Net.HttpStatusCode.OK) {
						try {
							var b = await ret.Content.ReadAsByteArrayAsync();
							var r = JsonConvert.DeserializeObject<PlatformData.VersionCheckResponse>(Encoding.UTF8.GetString(b));
#if CANARY
							if(!string.IsNullOrEmpty(r.CanaryVersion)) {
								var exe = GetExePath();
								var fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(exe);
								if(Version.TryParse(fvi.FileVersion, out var thisVer)
									&& Version.TryParse(r.CanaryVersion, out var webVer)) {

									return thisVer < webVer;
								}
							}
#else
							if(r.Period.HasValue) {
								var exe = GetExePath();
								var ver = System.Diagnostics.FileVersionInfo.GetVersionInfo(exe);
								return ver.FileMinorPart < r.Period.Value;
							}
#endif
						}
						catch(JsonReaderException) { /* 無視する */ }
						catch(JsonSerializationException) { /* 無視する */ }
					}
				}
				catch(System.Net.Http.HttpRequestException) { /* 無視する */ }
				catch(System.Net.Sockets.SocketException) { /* 無視する */ }
			}
			return false;
		}

		public static string GetContentType() {
			var exe = GetExePath();
			var ver = System.Diagnostics.FileVersionInfo.GetVersionInfo(exe);
			return string.Format("FutaMaki/period.{0:00}", ver.FileMinorPart);

		}

		public static string GetArchiveFileName() {
			var exe = GetExePath();
			var ver = System.Diagnostics.FileVersionInfo.GetVersionInfo(exe);
			return string.Format("{0}-{1:000}", Path.GetFileNameWithoutExtension(exe), ver.FileMinorPart);
		}
		public static void RemoveOldCache(string cacheDir) {
			var time = DateTime.Now.AddDays(-WpfConfig.WpfConfigLoader.SystemConfig.CacheExpireDay);
#if DEBUG
			var sw = new System.Diagnostics.Stopwatch();
			sw.Start();
#endif
			var f = Directory.EnumerateFiles(cacheDir)
				.Where(x => {
					try {
						return File.GetLastWriteTime(x) < time;
					}
					catch(UnauthorizedAccessException) {
						return false;
					}
					catch(IOException) {
						return false;
					}
				});
			var rmdir = Path.Combine(cacheDir, "temp");
			if(!Directory.Exists(rmdir)) {
				try {
					Directory.CreateDirectory(rmdir);
				}
				catch(Exception e) when(e is UnauthorizedAccessException || e is IOException) { 
#if DEBUG
				sw.Stop();
					Console.WriteLine("初期削除処理失敗");
#endif
					return;
				}
			}
			foreach(var it in f) {
				try {
					var name = Path.GetFileName(it);
					File.Move(it, Path.Combine(rmdir, name));
				}
				catch(Exception e) when(e is UnauthorizedAccessException || e is IOException) { }
			}

			Observable.Return(rmdir)
				.ObserveOn(System.Reactive.Concurrency.ThreadPoolScheduler.Instance)
				.Subscribe(x => {
					foreach(var it in Directory.EnumerateFiles(x)) {
						try {
							File.Delete(it);
						}
						catch(Exception e) when(e is UnauthorizedAccessException || e is IOException) { }
					}
					try {
						Directory.Delete(x);
					}
					catch(Exception e) when(e is UnauthorizedAccessException || e is IOException) { }
				});

#if false
			/*
			// TODO: ファイルがたくさんあると無視できないくらい重い、非同期化したほうがいいかも
			// Parallel.ForEachにしてみた
			Parallel.ForEach(f, it => {
				// .NET例外が投げられないようにWin32 APIたたく
				DeleteFile(it);
#if false
				try {
					File.Delete(it);
				}
				catch(UnauthorizedAccessException) { /* 削除できないファイルは無視する */
		}
				catch(IOException) { /* 削除できないファイルは無視する */}
#endif
			});
#endif
#if DEBUG
			sw.Stop();
			Console.WriteLine("初期削除処理{0}ミリ秒", sw.ElapsedMilliseconds);
#endif
		}
	}
}
