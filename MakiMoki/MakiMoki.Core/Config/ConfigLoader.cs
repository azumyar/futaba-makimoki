using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;

namespace Yarukizero.Net.MakiMoki.Config {
	public static class ConfigLoader {
		private static readonly string MakiMokiConfigFile = "makimoki.json";
		private static readonly string BordConfigFile = "bord.json";
		private static readonly string MimeConfigFile = "mime.json";
		private static readonly string PtuaConfigFile = "ptua.json";
		private static readonly string CookieConfigFile = "cookie.json";
		private static readonly string PasswordConfigFile = "passwd.json";
		internal static readonly Assembly CoreAssembly = typeof(ConfigLoader).Assembly;

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

			string loadFile(Stream s) {
				using(var sr = new StreamReader(s, Encoding.UTF8)) {
					return sr.ReadToEnd();
				}
			}
			void saveFile(string path, string s) {
				using(var fs = new FileStream(path, FileMode.OpenOrCreate)) {
					var b = Encoding.UTF8.GetBytes(s);
					fs.Write(b, 0, b.Length);
					fs.Flush();
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
					saveFile(ptua, Ptua.ToString());
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
						e.Message));
			}
			catch(JsonSerializationException e) {
				throw new Exceptions.InitializeFailedException(
					string.Format(
						"JSONファイルが不正な形式です{0}{0}{1}",
						Environment.NewLine,
						e.Message));
			}
			catch(IOException e) {
				throw new Exceptions.InitializeFailedException(
					string.Format(
						"ファイルの読み込みに失敗しました{0}{0}{1}",
						Environment.NewLine,
						e.Message));
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

		public static Data.Ptua Ptua { get; private set; }

		public static Data.Cookie[] Cookies { get; private set; }

		public static Data.Password Password { get; private set; }

		private static void SaveFile(string path, string s) {
			var m = File.Exists(path) ? FileMode.Truncate : FileMode.OpenOrCreate;
			using(var fs = new FileStream(path, m)) {
				var b = Encoding.UTF8.GetBytes(s);
				fs.Write(b, 0, b.Length);
				fs.Flush();
				fs.Close();
			}
		}

		internal static void UpdateCookie(Data.Cookie[] cookies) {
			Cookies = cookies;
			var s = JsonConvert.SerializeObject(cookies);
			lock(CoreAssembly) {
				SaveFile(Path.Combine(InitializedSetting.WorkDirectory, CookieConfigFile), s);
			}
		}

		internal static void UpdateFutabaPassword(string password) {
			Password = Data.Password.FromFutaba(password);
			SaveFile(Path.Combine(InitializedSetting.WorkDirectory, PasswordConfigFile), Password.ToString());
		}

		private static Data.Ptua CreatePtua() {
			var rnd = new Random();
			long t = 0;
			for(var i = 0; i < 33; i++) {
				t |= ((long)rnd.Next() % 2) << i;
			}
			return new Data.Ptua(t.ToString());
		}
	}
}
