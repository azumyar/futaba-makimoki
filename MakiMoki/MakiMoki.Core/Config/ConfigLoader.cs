using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;
using Yarukizero.Net.MakiMoki.Data;

namespace Yarukizero.Net.MakiMoki.Config {
	public static class ConfigLoader {
		private static readonly string MakiMokiConfigFile = "makimoki.json";
		private static readonly string BordConfigFile = "bord.json";
		private static readonly string MimeConfigFile = "mime.json";
		private static readonly string UploderConfigFile = "uploder.json";
		private static readonly string PtuaConfigFile = "ptua.json";
		private static readonly string CookieConfigFile = "cookie.json";
		private static readonly string PasswordConfigFile = "passwd.json";
		private static readonly string FutabaSavedFile = "makimoki.futaba.json";
		private static readonly string FutabaPostedFile = "makimoki.post.json";
		internal static readonly Assembly CoreAssembly = typeof(ConfigLoader).Assembly;

		private static volatile object lockObj = new object();

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

			string loadFile(Stream s) {
				using(var sr = new StreamReader(s, Encoding.UTF8)) {
					return sr.ReadToEnd();
				}
			}
			void addDic(Dictionary<string, Data.BordConfig> dic, Data.BordConfig[] item) {
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

			var bordDic = new Dictionary<string, Data.BordConfig>();
			addDic(bordDic, JsonConvert.DeserializeObject<Data.BordConfig[]>(
				loadFile(CoreAssembly.GetManifestResourceStream(
					typeof(ConfigLoader).Namespace + "." + BordConfigFile))));
			MakiMoki = JsonConvert.DeserializeObject<Data.MakiMokiConfig>(
				loadFile(CoreAssembly.GetManifestResourceStream(
					typeof(ConfigLoader).Namespace + "." + MakiMokiConfigFile)));
			Mime = JsonConvert.DeserializeObject<Data.MimeConfig>(
				loadFile(CoreAssembly.GetManifestResourceStream(
					typeof(ConfigLoader).Namespace + "." + MimeConfigFile)));
			Uploder = JsonConvert.DeserializeObject<Data.UploderConfig>(
				loadFile(CoreAssembly.GetManifestResourceStream(
					typeof(ConfigLoader).Namespace + "." + UploderConfigFile)));
			try {
				if(setting.SystemDirectory != null) {
					MakiMoki = get(Path.Combine(setting.SystemDirectory, MakiMokiConfigFile), MakiMoki);
					var bord = Path.Combine(setting.SystemDirectory, BordConfigFile);
					if(File.Exists(bord)) {
						using(var fs = new FileStream(bord, FileMode.Open)) {
							addDic(bordDic, JsonConvert.DeserializeObject<Data.BordConfig[]>(loadFile(fs)));
						}
					}
				}

				if(setting.UserDirectory != null) {
					MakiMoki = get(Path.Combine(setting.UserDirectory, MakiMokiConfigFile), MakiMoki);
					var bord = Path.Combine(setting.UserDirectory, BordConfigFile);
					if(File.Exists(bord)) {
						using(var fs = new FileStream(bord, FileMode.Open)) {
							addDic(bordDic, JsonConvert.DeserializeObject<Data.BordConfig[]>(loadFile(fs)));
						}
					}
				}

				var ptua = Path.Combine(setting.WorkDirectory, PtuaConfigFile);
				if(File.Exists(ptua)) {
					using(var fs = new FileStream(ptua, FileMode.Open)) {
						var p = loadFile(fs);
						if(Data.Ptua.CurrentVersion == JsonConvert.DeserializeObject<Data.ConfigObject>(p).Version) {
							Ptua = JsonConvert.DeserializeObject<Data.Ptua>(p);
						}
					}
				}
				var cookie = Path.Combine(setting.WorkDirectory, CookieConfigFile);
				if(File.Exists(cookie)) {
					using(var fs = new FileStream(cookie, FileMode.Open)) {
						var c = loadFile(fs);
						//if (Data.Ptua.CurrentVersion == JsonConvert.DeserializeObject<Data.ConfigObject>(p).Version) {
						Cookies = JsonConvert.DeserializeObject<Data.Cookie[]>(c);
						//}
					}
				}
				var password = Path.Combine(setting.WorkDirectory, PasswordConfigFile);
				if(File.Exists(password)) {
					using(var fs = new FileStream(password, FileMode.Open)) {
						var p = loadFile(fs);
						if(Data.Password.CurrentVersion == JsonConvert.DeserializeObject<Data.ConfigObject>(p).Version) {
							Password = JsonConvert.DeserializeObject<Data.Password>(p);
						}
					}
				}

				if(Ptua == null) {
					Ptua = CreatePtua();
					Util.FileUtil.SaveJson(ptua, Ptua);
				}
				if(Cookies == null) {
					Cookies = new Data.Cookie[0];
				}
				if(Password == null) {
					Password = new Data.Password();
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

			var bordList = new List<Data.BordConfig>();
			foreach(var k in bordDic.Keys.OrderBy(x => x)) {
				bordList.Add(bordDic[k]);
			}
			Bord = bordList.Where(x => x.DisplayValue).OrderBy(x => x.SortIndexValue).ToArray();
		}

		public static Setting InitializedSetting { get; private set; }

		public static Data.MakiMokiConfig MakiMoki { get; private set; }

		public static Data.BordConfig[] Bord { get; private set; }

		public static Data.MimeConfig Mime { get; private set; }

		public static Data.UploderConfig Uploder { get; private set; }

		public static Data.Ptua Ptua { get; private set; }

		public static Data.Cookie[] Cookies { get; private set; }

		public static Data.Password Password { get; private set; }

		public static Data.FutabaSavedConfig SavedFutaba { get; private set; }

		public static Data.FutabaPostItemConfig PostedItem { get; private set; }

		internal static void UpdateCookie(Data.Cookie[] cookies) {
			Cookies = cookies;
			lock(lockObj) {
				Util.FileUtil.SaveJson(
					Path.Combine(InitializedSetting.WorkDirectory, CookieConfigFile),
					Cookies);
			}
		}

		internal static void UpdateFutabaPassword(string password) {
			Password = Data.Password.FromFutaba(password);
			Util.FileUtil.SaveJson(
				Path.Combine(InitializedSetting.WorkDirectory, PasswordConfigFile),
				Password);
		}

		private static Data.Ptua CreatePtua() {
			var rnd = new Random();
			long t = 0;
			for(var i = 0; i < 33; i++) {
				t |= ((long)rnd.Next() % 2) << i;
			}
			return new Data.Ptua(t.ToString());
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
