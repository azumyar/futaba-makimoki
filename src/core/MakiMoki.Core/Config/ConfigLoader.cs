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
	public static partial class ConfigLoader {
		internal static readonly string MakiMokiConfigFile = "makimoki.json";
		internal static readonly string MakiMokiOptoutConfigFile = "makimoki.optout.json";
		internal static readonly string BoardConfigFile = "makimoki.bord.json";
		internal static readonly string MimeFutabaConfigFile = "mime-futaba.json";
		internal static readonly string MimeUp2ConfigFile = "mime-up2.json";
		internal static readonly string UploderConfigFile = "uploder.json";
		internal static readonly string FutabaApiFile = "makimoki.futaba.json";
		internal static readonly string FutabaSavedFile = "makimoki.response.json";
		internal static readonly string FutabaPostedFile = "makimoki.post.json";

#pragma warning disable IDE0044
		private static volatile object lockObj = new object();
#pragma warning restore IDE0044

		public class Setting {
			public string RestUserAgent { get; set; } = "MakiMoki/Core";
			public string SystemDirectory { get; set; } = null;
			public string UserDirectory { get; set; } = null;

			public string WorkDirectory { get; set; } = null;
			public string CacheDirectory { get; set; } = null;

			public string AppCenterSecrets { get; set; } = null;
		}

		public static void Initialize(Setting setting) {
			System.Diagnostics.Debug.Assert(setting != null);
			System.Diagnostics.Debug.Assert(setting.RestUserAgent != null);
			System.Diagnostics.Debug.Assert(setting.WorkDirectory != null);
			InitializedSetting = setting;

			var loader = new Util.ResourceLoader(typeof(ConfigLoader));
			var board = Util.FileUtil.LoadMigrate(
				loader.Get(BoardConfigFile),
				CoreBoardConfig.CreateDefault());
			var boardDic = new Dictionary<string, Data.BoardData>();
			MergeDictionary(boardDic, board.Boards);
			MakiMoki = Util.FileUtil.LoadMigrate(
				loader.Get(MakiMokiConfigFile),
				default(MakiMokiConfig));
			Optout = Util.FileUtil.LoadMigrate(
				loader.Get(MakiMokiOptoutConfigFile),
				default(MakiMokiOptout));
			MimeFutaba = Util.FileUtil.LoadMigrate(
				loader.Get(MimeFutabaConfigFile),
				default(MimeConfig));
			MimeUp2 = Util.FileUtil.LoadMigrate(
				loader.Get(MimeUp2ConfigFile),
				default(MimeConfig));
			Uploder = Util.FileUtil.LoadMigrate(
				loader.Get(UploderConfigFile),
				default(UploderConfig));
			FutabaApi = Util.FileUtil.LoadMigrate(
				Path.Combine(setting.WorkDirectory, FutabaApiFile),
				FutabaApiConfig.CreateDefault());
			System.Diagnostics.Debug.Assert(MakiMoki != null);
			System.Diagnostics.Debug.Assert(Optout != null);
			System.Diagnostics.Debug.Assert(MimeFutaba != null);
			System.Diagnostics.Debug.Assert(MimeUp2 != null);
			System.Diagnostics.Debug.Assert(Uploder != null);
			System.Diagnostics.Debug.Assert(FutabaApi != null);

			if(string.IsNullOrEmpty(FutabaApi.Ptua)) {
				FutabaApi.Ptua = CreatePtua();
			}

			foreach(var confDir in new string[] { setting.SystemDirectory, setting.UserDirectory }) {
				if(confDir != null) {
					MakiMoki = Util.FileUtil.LoadMigrate(
						Path.Combine(confDir, MakiMokiConfigFile),
						MakiMoki,
						new Dictionary<int, Type>() {
							{ Data.Compat.MakiMokiConfig2020062900.CurrentVersion, typeof(Data.Compat.MakiMokiConfig2020062900) },
						});
					Optout = Util.FileUtil.LoadMigrate(
						Path.Combine(confDir, MakiMokiOptoutConfigFile),
						Optout);
					var b = Util.FileUtil.LoadMigrate(
						Path.Combine(confDir, BoardConfigFile),
						default(BoardConfig),
						new Dictionary<int, Type>() {
							{ Data.Compat.BoardConfig2020062900.CurrentVersion, typeof(Data.Compat.BoardConfig2020062900) }
						});
					MergeDictionary(boardDic, b?.Boards);
 					if(confDir == setting.UserDirectory) {
						UserConfBoard = b;
					}
				}
			}

			Board = CoreBoardConfig.CreateAppInstance(
				board, 
				boardDic.Select(x => x.Value)
					.Where(x => x.Display)
					.OrderBy(x => x.SortIndex)
					.ToArray());
			UserConfBoard ??= BoardConfig.CreateDefault();
			SavedFutaba = Util.FileUtil.LoadMigrate(
				Path.Combine(InitializedSetting.WorkDirectory, FutabaSavedFile),
				Data.FutabaSavedConfig.CreateDefault());
			PostedItem = Util.FileUtil.LoadMigrate(
				Path.Combine(InitializedSetting.WorkDirectory, FutabaPostedFile),
				Data.FutabaPostItemConfig.CreateDefault());
			if(PostedItem.Items.Any()) {
				var time = DateTime.Now.AddDays(-MakiMoki.FutabaPostDataExpireDay);
				var t = PostedItem.Items.Where(x => time <= x.Res.Res.NowDateTime).ToArray();
				if(t.Length != PostedItem.Items.Length) {
					PostedItem = Data.FutabaPostItemConfig.From(t);
				}
			}

			StartAppCenter(InitializedSetting.AppCenterSecrets);
		}

		public static Setting InitializedSetting { get; private set; }

		public static Data.MakiMokiConfig MakiMoki { get; private set; }

		public static Data.MakiMokiOptout Optout { get; private set; }

		public static Data.CoreBoardConfig Board { get; private set; }
		public static Data.BoardConfig UserConfBoard { get; private set; }

		public static Data.MimeConfig MimeFutaba { get; private set; }

		public static Data.MimeConfig MimeUp2 { get; private set; }

		public static Data.UploderConfig Uploder { get; private set; }

		public static Data.FutabaApiConfig FutabaApi { get; private set; }

		public static Data.FutabaSavedConfig SavedFutaba { get; private set; }

		public static Data.FutabaPostItemConfig PostedItem { get; private set; }

		public static Helpers.UpdateNotifyer BoardConfigUpdateNotifyer { get; } = new Helpers.UpdateNotifyer();
		public static Helpers.UpdateNotifyer PostConfigUpdateNotifyer { get; } = new Helpers.UpdateNotifyer();

		private static void MergeDictionary(Dictionary<string, Data.BoardData> dic, Data.BoardData[] item) {
			if(item == null) {
				return;
			}

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
		public static void UpdateOptout(Data.MakiMokiOptout optout) {
			Optout = optout;
			lock(lockObj) {
				if(Directory.Exists(InitializedSetting.UserDirectory)) {
					Util.FileUtil.SaveJson(
						Path.Combine(InitializedSetting.UserDirectory, MakiMokiOptoutConfigFile),
						Optout);
				}
			}
		}

		internal static void UpdateCookie(Data.Cookie[] cookies) {
			FutabaApi.Cookies = cookies;
			lock(lockObj) {
				Util.FileUtil.SaveJson(
					Path.Combine(InitializedSetting.WorkDirectory, FutabaApiFile),
					FutabaApi);
			}
		}

		public static void UpdateFutabaInputData(BoardData bord, string subject, string name, string mail, string password) {
			System.Diagnostics.Debug.Assert(subject != null);
			System.Diagnostics.Debug.Assert(name != null);
			System.Diagnostics.Debug.Assert(mail != null);
			System.Diagnostics.Debug.Assert(password != null);

			if(bord.Extra?.NameValue ?? true) {
				FutabaApi.SavedSubject = MakiMoki.FutabaPostSavedSubject ? subject : "";
				FutabaApi.SavedName = MakiMoki.FutabaPostSavedName ? name : "";
			}
			FutabaApi.SavedMail = MakiMoki.FutabaPostSavedMail ? mail : "";
			FutabaApi.SavedPassword = password;
			Util.FileUtil.SaveJson(
				Path.Combine(InitializedSetting.WorkDirectory, FutabaApiFile),
				FutabaApi);
			PostConfigUpdateNotifyer.Notify();
		}

		internal static void UpdateFutabaPassword(string password) {
			System.Diagnostics.Debug.Assert(password != null);

			FutabaApi.SavedPassword = password;
			Util.FileUtil.SaveJson(
				Path.Combine(InitializedSetting.WorkDirectory, FutabaApiFile),
				FutabaApi);
			PostConfigUpdateNotifyer.Notify();
		}

		private static string CreatePtua() {
			var rnd = new Random();
			long t = 0;
			for(var i = 0; i < 33; i++) {
				t |= ((long)rnd.Next() % 2) << i;
			}
			return t.ToString();
		}

		public static void UpdateMakiMokiConfig(
			bool threadGetIncremental, bool responseSave, int postDataExpireDay,
			bool isSavedPostSubject, bool isSavedPostName, bool isSavedPostMail) {
			
			MakiMoki = MakiMokiConfig.From(
				threadGetIncremental: threadGetIncremental,
				responseSave: responseSave,
				postDataExpireDay: postDataExpireDay,
				isSavedPostSubject: isSavedPostSubject,
				isSavedPostName: isSavedPostName,
				isSavedPostMail: isSavedPostMail);
			lock(lockObj) {
				if(Directory.Exists(InitializedSetting.UserDirectory)) {
					Util.FileUtil.SaveJson(
						Path.Combine(InitializedSetting.UserDirectory, MakiMokiConfigFile),
						MakiMoki);
				}
			}

			// notifyerいる？
		}

		public static void UpdateUserBoardConfig(BoardData[] boardData) {
			UserConfBoard = BoardConfig.From(boardData.ToArray());
			var board = Util.FileUtil.LoadMigrate(
				new Util.ResourceLoader(typeof(ConfigLoader)).Get(BoardConfigFile),
				CoreBoardConfig.CreateDefault());
			var boardDic = new Dictionary<string, Data.BoardData>();
			MergeDictionary(boardDic, board.Boards);
			if(InitializedSetting.SystemDirectory != null) {
				var b = Util.FileUtil.LoadMigrate(
					Path.Combine(InitializedSetting.SystemDirectory, BoardConfigFile),
					default(BoardConfig),
					new Dictionary<int, Type>() {
						{ Data.Compat.BoardConfig2020062900.CurrentVersion, typeof(Data.Compat.BoardConfig2020062900) }
					});
				MergeDictionary(boardDic, b?.Boards);
			}
			MergeDictionary(boardDic, UserConfBoard.Boards);
			Board = CoreBoardConfig.CreateAppInstance(
				board,
				boardDic.Select(x => x.Value)
					.Where(x => x.Display)
					.OrderBy(x => x.SortIndex)
					.ToArray());
			lock(lockObj) {
				if(Directory.Exists(InitializedSetting.UserDirectory)) {
					Util.FileUtil.SaveJson(
						Path.Combine(InitializedSetting.UserDirectory, BoardConfigFile),
						UserConfBoard);
				}
			}
			BoardConfigUpdateNotifyer.Notify();
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
