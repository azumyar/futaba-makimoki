using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Android.Content;
using Newtonsoft.Json;
using Yarukizero.Net.MakiMoki.Util;

namespace Yarukizero.Net.MakiMoki.Droid.App {
	internal class MakiMokiContext {
		internal class DroidDataProvider : Config.ConfigLoader.IDataProvider {
			private readonly Context context;

			public DroidDataProvider(Context context) {
				this.context = context;
			}

			private T Load<T>(string name, T defaultValue, Dictionary<int, Type>? migrateTable = null, Func<string, Exception, Exception>? exception = null) where T : Data.ConfigObject {
				var r = defaultValue;
				exception ??= (m, e) => new Exceptions.InitializeFailedException(m, e);
				try {
					var json = this.context.GetSharedPreferences(DroidConst.PreferencesName, FileCreationMode.Private).GetString(name, null);
					if(json != null) {
						r = CompatUtil.Migrate<T>(json, migrateTable);
					}
				}
				catch(JsonReaderException e) {
					throw exception($"JSONファイル[{name}]が不正な形式です{Environment.NewLine}{Environment.NewLine}{e.Message}", e);
				}
				catch(JsonSerializationException e) {
					throw exception($"JSONファイル[{name}]が不正な形式です{Environment.NewLine}{Environment.NewLine}{e.Message}", e);
				}

				return r;
			}
			public void Save(string name, Data.JsonObject config)  => this.context.GetSharedPreferences(DroidConst.PreferencesName, FileCreationMode.Private)
				.Edit()
				.PutString(name, config.ToString())
				.Commit();
			public void Remove(string name) => this.context.GetSharedPreferences(DroidConst.PreferencesName, FileCreationMode.Private)
				.Edit()
				.Remove(name)
				.Commit();

			public T LoadSystem<T>(string name, T defaultValue, Dictionary<int, Type>? migrateTable = null, Func<string, Exception, Exception>? exception = null) where T : Data.ConfigObject => defaultValue;


			public T LoadUser<T>(string name, T defaultValue, Dictionary<int, Type>? migrateTable = null, Func<string, Exception, Exception>? exception = null) where T : Data.ConfigObject
				=> Load(name, defaultValue, migrateTable, exception);
			public void SaveUser(string name, Data.JsonObject config) => Save(name, config);
			public void RemoveUser(string name) => Remove(name);

			public T LoadWork<T>(string name, T defaultValue, Dictionary<int, Type>? migrateTable = null, Func<string, Exception, Exception>? exception = null) where T : Data.ConfigObject
				=> Load(name, defaultValue, migrateTable, exception);
			public void SaveWork(string name, Data.JsonObject config) => Save(name, config);
			public void RemoveWork(string name) => Remove(name);
		}

		private string ContentType { get; set; }
		private string AppInternalRootDirectory { get; set; }
		private string AppExternalRootDirectory { get; set; }

		public string AppCacheDirectory { get; private set; }
		public string AppWorkDirectory { get; private set; }


		public System.Net.Http.HttpClient HttpClient { get; private set; }
		public DbConnection Db { get; private set; }

		public long InitUnixTime { get; private set; }

		public MakiMokiContext() {
			this.DoInitilize();
		}

		private void DoInitilize() {
			AppDomain.CurrentDomain.UnhandledException += (_, e) => {
				if(e.ExceptionObject is Exception ex) {
					File.WriteAllText(Path.Combine(AppCacheDirectory, "crash.log"), ex.ToString());
				}
			};
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

			this.InitUnixTime = MakiMokiApplication.Current.GetSharedPreferences(DroidConst.PreferencesName, FileCreationMode.Private)
				.GetLong(DroidConst.PreferencesKeyInitTime, 0);
			if(this.InitUnixTime == 0) {
				this.InitUnixTime = Util.TimeUtil.ToUnixTimeSeconds();
#if DEBUG
				this.InitUnixTime = Util.TimeUtil.ToUnixTimeSeconds(DateTime.Now.AddHours(-2));
#endif
				MakiMokiApplication.Current.GetSharedPreferences(DroidConst.PreferencesName, FileCreationMode.Private)
					.Edit()
					.PutLong(DroidConst.PreferencesKeyInitTime, this.InitUnixTime)
					.Commit();
			}

			global::Reactive.Bindings.UIDispatcherScheduler.Initialize();
			Config.ConfigLoader.Initialize(new Config.ConfigLoader.Setting() {
				RestUserAgent = ContentType,
				/*
				SystemDirectory = Path.Combine(
					System.AppContext.BaseDirectory,
					"Config.d"),
				*/
				UserDirectory = AppInternalRootDirectory,
				CacheDirectory = AppCacheDirectory,
				WorkDirectory = AppWorkDirectory,
				DataProvider = new DroidDataProvider(MakiMokiApplication.Current),
				AppCenterSecrets = DroidConst.AppCenterSecrets,
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
					maxStoredRes: 30000,
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
