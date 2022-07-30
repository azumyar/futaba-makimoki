using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace Yarukizero.Net.MakiMoki.Uno {
	partial class App {
		private static string ContentType { get; set; }
		private static string AppInternalRootDirectory { get; set; }
		private static string AppExternalRootDirectory { get; set; }

		public static System.Net.Http.HttpClient HttpClient { get; private set; }
		
		public static string AppCacheDirectory { get; private set; }
		public static string AppWorkDirectory { get; private set; }

		static App() {
			System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
		}

		partial void DoInitilize() {
#if __ANDROID__
			var platform = "Android";
			{
				var intl = Droid.MakiMokiApplication.Current.FilesDir;
				using var extl = Droid.MakiMokiApplication.Current.GetExternalFilesDir(null);
				AppInternalRootDirectory = intl.Path;
				AppExternalRootDirectory = extl.Path;
			}
#elif __IOS__
			var platform = "iOS";
			AppInternalRootDirectory
				= AppExternalRootDirectory
				= Directory.GetCurrentDirectory();
#else
#error "非対応のプラットフォーム"
#endif
			var device = UnoUtils.PlatformInfo.UserAgentDeviceName;
			var version = "alpha-0x";

#if DEBUG
			// デバッグ用に書き換える
			device = $"DEBUG";
#endif
			AppCacheDirectory = MakeDirectory(AppExternalRootDirectory, "cache.d");
			AppWorkDirectory = MakeDirectory(AppExternalRootDirectory, "work.d");

			ContentType = $"FutaMaki/{ platform }/{ version }/{ device }";
			HttpClient = new System.Net.Http.HttpClient();
			HttpClient.DefaultRequestHeaders.Add(
				"User-Agent",
				ContentType);

			Reactive.Bindings.UIDispatcherScheduler.Initialize();
			Config.ConfigLoader.Initialize(new Config.ConfigLoader.Setting() {
				RestUserAgent = ContentType,
				/*
				SystemDirectory = Path.Combine(
					System.AppContext.BaseDirectory,
					"Config.d"),
				UserDirectory = UserConfigDirectory,
				AppCenterSecrets = AppCenterSecrets,
				*/
				CacheDirectory = AppCacheDirectory,
				WorkDirectory = AppWorkDirectory
			});

			var imgBoard = Data.BoardData.From(
				name: "二次元裏(img)",
				url: "https://img.2chan.net/b/",
				defaultComment: "ｷﾀ━━━━━━(ﾟ∀ﾟ)━━━━━━ !!!!!",
				sortIndex: 101,
				extra: Data.BoardDataExtra.From(
					name: false,
					resImage: false,
					mailIp: false,
					mailId: true,
					alwaysIp: false,
					alwaysId: false,
					maxStoredRes: 20000,
					maxStoredTime: 3600,
					resTegaki: true),
				makimokiExtra: Data.MakiMokiBoardDataExtra.From(
					isEnabledPassiveReload: false));
			Config.ConfigLoader.UpdateUserBoardConfig(new[] { imgBoard });
		}

		private string MakeDirectory(string baseDir, string dir) {
			var p = Path.Combine(baseDir, dir);
			if(!Directory.Exists(p)) {
				Directory.CreateDirectory(p);
			}
			return p;
		}
	}
}
