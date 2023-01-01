using Org.Apache.Http.Client.Utils;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Yarukizero.Net.MakiMoki.Droid.App {
	internal class MakiMokiContext {
		private string ContentType { get; set; }
		private string AppInternalRootDirectory { get; set; }
		private string AppExternalRootDirectory { get; set; }

		public string AppCacheDirectory { get; private set; }
		public string AppWorkDirectory { get; private set; }


		public System.Net.Http.HttpClient HttpClient { get; private set; }
		public DbConnection Db { get; private set; }


		public MakiMokiContext() {
			this.DoInitilize();
		}

		private void DoInitilize() {
			var platform = "Android";
			{
				var intl = Droid.MakiMokiApplication.Current.FilesDir;
				using var extl = Droid.MakiMokiApplication.Current.GetExternalFilesDir(null);
				AppInternalRootDirectory = intl.Path;
				AppExternalRootDirectory = extl.Path;
			}
			var device = $"{Android.OS.Build.Manufacturer}-{Android.OS.Build.Model}";
			var version = "alpha-00";

#if DEBUG
			// デバッグ用に書き換える
			device = $"DEBUG";
#endif
			AppCacheDirectory = MakeDirectory(AppExternalRootDirectory, "cache.d");
			AppWorkDirectory = MakeDirectory(AppExternalRootDirectory, "work.d");

			ContentType = $"MakiMoki-Test/{platform}/{version}/{device}";
			HttpClient = new System.Net.Http.HttpClient();
			HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(
				"User-Agent",
				ContentType);
			Db = new DbConnection(Path.Combine(AppInternalRootDirectory, DroidConst.DbName));

			global::Reactive.Bindings.UIDispatcherScheduler.Initialize();
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
