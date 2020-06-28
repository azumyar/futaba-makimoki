using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;
using Yarukizero.Net.MakiMoki.Data;
using Yarukizero.Net.MakiMoki.Util;

namespace Yarukizero.Net.MakiMoki.Config {
	public static class ConfigLoader {
		private static readonly string MakiMokiConfigFile = "makimoki.json";
		private static readonly string BordConfigFile = "makimoki.bord.json";
		private static readonly string MimeConfigFile = "mime.json";
		private static readonly string UploderConfigFile = "uploder.json";
		private static readonly string FutabaApiFile = "makimoki.futaba.json";
		private static readonly string FutabaSavedFile = "makimoki.response.json";
		private static readonly string FutabaPostedFile = "makimoki.post.json";
		internal static readonly Assembly CoreAssembly = typeof(ConfigLoader).Assembly;

		private static volatile object lockObj = new object();

		public class Compat {
			public static bool IsValid(ConfigObject obj, int currentVersion) {
				if(obj == null) {
					return false;
				}

				return obj.Version == currentVersion;
			}

			public static T Migrate<T>(string path, ConfigObject obj, Func<int, string, T> migrate) where T : ConfigObject {
				var r = default(T);
				Util.FileUtil.LoadConfigHelper(path,
					(json) => r = migrate(obj.Version, json),
					(e, m) => throw new Exceptions.InitializeFailedException(m, e));
				if(r == null) {
					throw new Exceptions.InitializeFailedException($"ファイル[{ path }]のVersion[{ obj.Version }]は不正な設定です。");
				}

				return r;
			}
		}

		public class Setting {
			public string SystemDirectory { get; set; } = null;
			public string UserDirectory { get; set; } = null;

			public string WorkDirectory { get; set; } = null;
			public string CacheDirectory { get; set; } = null;
		}

		public static void Initialize(Setting setting) {
			System.Diagnostics.Debug.Assert(setting != null);
			System.Diagnostics.Debug.Assert(setting.WorkDirectory != null);
			InitializedSetting = setting;

			T get<T>(string path, T defaultValue) {
				if(File.Exists(path)) {
					using(var fs = new FileStream(path, FileMode.Open)) {
						return JsonConvert.DeserializeObject<T>(loadFile(fs));
					}
				}
				return defaultValue;
			}
			T getPath<T>(string path, T defaultValue) {
				var r = defaultValue;
				if(File.Exists(path)) {
					Util.FileUtil.LoadConfigHelper(path,
						(json) => r = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json),
						(e, m) => throw new Exceptions.InitializeFailedException(m, e));
				}
				return r;
			}
			T getStream<T>(Stream path, T defaultValue) {
				var r = defaultValue;
				Util.FileUtil.LoadConfigHelper(path,
					(json) => r = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json),
					(e, m) => throw new Exceptions.InitializeFailedException(m, e));
				return r;
			}
			string getResPath(string name) => $"{ typeof(ConfigLoader).Namespace }.{ name }";

			string loadFile(Stream s) {
				using(var sr = new StreamReader(s, Encoding.UTF8)) {
					return sr.ReadToEnd();
				}
			}
			void addDic(Dictionary<string, Data.BordData> dic, Data.BordData[] item) {
				foreach(var b in item) {
					if(string.IsNullOrWhiteSpace(b.Name)) {
						throw new Exceptions.InitializeFailedException(
							string.Format("JSON{0}は不正な形式です", b.ToString()));
					}

					if(dic.ContainsKey(b.Name)) {
						dic[b.Name] = b;
					} else {
						dic.Add(b.Name, b);
					}
				}
			}

			var bord = getStream(
				CoreAssembly.GetManifestResourceStream(getResPath(BordConfigFile)),
				CoreBordConfig.CreateDefault());
			var bordDic = new Dictionary<string, Data.BordData>();
			addDic(bordDic, bord.Bords);
			MakiMoki = JsonConvert.DeserializeObject<Data.MakiMokiConfig>(
				loadFile(CoreAssembly.GetManifestResourceStream(
					typeof(ConfigLoader).Namespace + "." + MakiMokiConfigFile)));
			Mime = JsonConvert.DeserializeObject<Data.MimeConfig>(
				loadFile(CoreAssembly.GetManifestResourceStream(
					typeof(ConfigLoader).Namespace + "." + MimeConfigFile)));
			Uploder = JsonConvert.DeserializeObject<Data.UploderConfig>(
				loadFile(CoreAssembly.GetManifestResourceStream(
					typeof(ConfigLoader).Namespace + "." + UploderConfigFile)));
			{
				var path = Path.Combine(setting.WorkDirectory, FutabaApiFile);
				var j = string.Empty;
				var o = default(FutabaApiConfig);
				if(File.Exists(path)) {
					var co = default(ConfigObject);
					Util.FileUtil.LoadConfigHelper(path,
						(json) => { j = json; co = JsonConvert.DeserializeObject<ConfigObject>(json); },
						(e, m) => throw new Exceptions.InitializeFailedException(m, e));
					if(Compat.IsValid(co, BordConfig.CurrentVersion)) {
						Util.FileUtil.LoadJsonHelper(j,
							(json) => o = JsonConvert.DeserializeObject<FutabaApiConfig>(json),
							(e, m) => throw new Exceptions.InitializeFailedException(m, e));
					} else {
						o = Compat.Migrate<FutabaApiConfig>(path, co, (version, json) => null);
					}
				} else {
					o = FutabaApiConfig.CreateDefault();
				}
				FutabaApi = o;
				if(string.IsNullOrEmpty(FutabaApi.Ptua)) {
					FutabaApi.Ptua = CreatePtua();
				}
			}

			try {
				foreach(var confDir in new string[] { setting.SystemDirectory, setting.UserDirectory }) {
					if(confDir != null) {
						MakiMoki = get(Path.Combine(confDir, MakiMokiConfigFile), MakiMoki);
						var b = Path.Combine(confDir, BordConfigFile);
						if(File.Exists(b)) {
							var j = string.Empty;
							var o = default(BordConfig);
							var co = default(ConfigObject);
							Util.FileUtil.LoadConfigHelper(b,
								(json) => { j = json; co = JsonConvert.DeserializeObject<ConfigObject>(json); },
								(e, m) => throw new Exceptions.InitializeFailedException(m, e));
							if(Compat.IsValid(co, BordConfig.CurrentVersion)) {
								Util.FileUtil.LoadJsonHelper(j,
									(json) => o = JsonConvert.DeserializeObject<BordConfig>(json),
									(e, m) => throw new Exceptions.InitializeFailedException(m, e));
							} else {
								o = Compat.Migrate<BordConfig>(b, co, (version, json) => null);
							}
							addDic(bordDic, o.Bords);
						}
					}
				}
			}
			catch(JsonReaderException e) {
				throw new Exceptions.InitializeFailedException(
					string.Format(
						"JSONファイルが不正な形式です{0}{0}{1}",
						Environment.NewLine,
						e.Message), e);
			}
			catch(JsonSerializationException e) {
				throw new Exceptions.InitializeFailedException(
					string.Format(
						"JSONファイルが不正な形式です{0}{0}{1}",
						Environment.NewLine,
						e.Message), e);
			}
			catch(IOException e) {
				throw new Exceptions.InitializeFailedException(
					string.Format(
						"ファイルの読み込みに失敗しました{0}{0}{1}",
						Environment.NewLine,
						e.Message), e);
			}

			SavedFutaba = getPath(
				Path.Combine(InitializedSetting.WorkDirectory, FutabaSavedFile),
				Data.FutabaSavedConfig.CreateDefault());
			PostedItem = getPath(
				Path.Combine(InitializedSetting.WorkDirectory, FutabaPostedFile),
				Data.FutabaPostItemConfig.CreateDefault());
			if(0 < PostedItem.Items.Length) {
				var time = DateTime.Now.AddDays(-MakiMoki.FutabaPostDataExpireDay);
				var t = PostedItem.Items.Where(x => time <= x.Res.Res.NowDateTime).ToArray();
				if(t.Length != PostedItem.Items.Length) {
					PostedItem = Data.FutabaPostItemConfig.From(t);
				}
			}

			var bordList = new List<Data.BordData>();
			foreach(var k in bordDic.Keys.OrderBy(x => x)) {
				bordList.Add(bordDic[k]);
			}
			Bord = CoreBordConfig.CreateAppInstance(bord, bordList.Where(x => x.DisplayValue).OrderBy(x => x.SortIndexValue).ToArray());
		}

		public static Setting InitializedSetting { get; private set; }

		public static Data.MakiMokiConfig MakiMoki { get; private set; }

		public static Data.CoreBordConfig Bord { get; private set; }

		public static Data.MimeConfig Mime { get; private set; }

		public static Data.UploderConfig Uploder { get; private set; }

		public static Data.FutabaApiConfig FutabaApi { get; private set; }

		public static Data.FutabaSavedConfig SavedFutaba { get; private set; }

		public static Data.FutabaPostItemConfig PostedItem { get; private set; }

		internal static void UpdateCookie(Data.Cookie[] cookies) {
			FutabaApi.Cookies = cookies;
			lock(lockObj) {
				Util.FileUtil.SaveJson(
					Path.Combine(InitializedSetting.WorkDirectory, FutabaApiFile),
					FutabaApi);
			}
		}

		internal static void UpdateFutabaPassword(string password) {
			FutabaApi.SavedPassword = password;
			Util.FileUtil.SaveJson(
				Path.Combine(InitializedSetting.WorkDirectory, FutabaApiFile),
				FutabaApi);
		}

		private static string CreatePtua() {
			var rnd = new Random();
			long t = 0;
			for(var i = 0; i < 33; i++) {
				t |= ((long)rnd.Next() % 2) << i;
			}
			return t.ToString();
		}

		public static void UpdateMakiMokiConfig(bool threadGetIncremental, bool responseSave, int postDataExpireDay) {
			MakiMoki = MakiMokiConfig.From(
				threadGetIncremental: threadGetIncremental,
				responseSave: responseSave,
				postDataExpireDay: postDataExpireDay);
			lock(lockObj) {
				if(Directory.Exists(InitializedSetting.UserDirectory)) {
					Util.FileUtil.SaveJson(
						Path.Combine(InitializedSetting.UserDirectory, MakiMokiConfigFile),
						MakiMoki);
				}
			}

			// notifyerいる？
		}

		public static void SaveFutabaResponse(Data.FutabaContext[] catalogs, Data.FutabaContext[] threads) {
			if(MakiMoki.FutabaResponseSave) {
				lock(lockObj) {
					SavedFutaba = Data.FutabaSavedConfig.From(catalogs, threads);
					Util.FileUtil.SaveJson(
						Path.Combine(InitializedSetting.WorkDirectory, FutabaSavedFile),
						SavedFutaba);
				}
			}
		}
		public static void RemoveSaveFutabaResponseFile() {
			var f = Path.Combine(InitializedSetting.WorkDirectory, FutabaSavedFile);
			if(File.Exists(f)) {
				try {
					File.Delete(f);
				}
				catch(IOException) { /* TODO: どうする？ */}
			}
		}

		public static void SavePostItems(Data.PostedResItem[] items) {
			lock(lockObj) {
				PostedItem = Data.FutabaPostItemConfig.From(items);
				Util.FileUtil.SaveJson(
					Path.Combine(InitializedSetting.WorkDirectory, FutabaPostedFile),
					PostedItem);
			}
		}
	}
}
