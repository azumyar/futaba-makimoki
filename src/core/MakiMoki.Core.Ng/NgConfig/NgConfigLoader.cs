using AngleSharp.Common;
using AngleSharp.Text;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Yarukizero.Net.MakiMoki.Ng.NgData;

namespace Yarukizero.Net.MakiMoki.Ng.NgConfig {
	public static class NgConfigLoader {
		private static readonly string NgConfigFile = "ng.json";
		private static readonly string NgImageConfigFile = "ng.image.json";
		private static readonly string HiddenConfigFile = "ng.hidden.json";
		private static readonly string WatchConfigFile = "watch.json";
		private static readonly string WatchImageConfigFile = "watch.image.json";

#pragma warning disable IDE0044
		private static volatile object lockObj = new object();
#pragma warning restore IDE0044
		private static List<WeakReference<Action<NgData.NgConfig>>> ngUpdateNotifyer = new List<WeakReference<Action<NgData.NgConfig>>>();
		private static List<WeakReference<Action<NgData.HiddenConfig>>> hiddenUpdateNotifyer = new List<WeakReference<Action<NgData.HiddenConfig>>>();
		private static List<WeakReference<Action<NgData.NgImageConfig>>> imageUpdateNotifyer = new List<WeakReference<Action<NgData.NgImageConfig>>>();
		public static Helpers.UpdateNotifyer<NgData.WatchConfig> WatchUpdateNotifyer { get; } = new Helpers.UpdateNotifyer<WatchConfig>();
		public static Helpers.UpdateNotifyer<NgData.WatchImageConfig> WatchImageUpdateNotifyer { get; } = new Helpers.UpdateNotifyer<WatchImageConfig>();

		public class Setting {
			public string UserDirectory { get; set; } = null;
		}

		public static void Initialize(Setting setting) {
			System.Diagnostics.Debug.Assert(setting != null);
			InitializedSetting = setting;

			if((setting.UserDirectory != null) && Directory.Exists(setting.UserDirectory)) {
				NgConfig = Util.FileUtil.LoadMigrate(
					Path.Combine(setting.UserDirectory, NgConfigFile),
					NgData.NgConfig.CreateDefault(),
					new Dictionary<int, Type>() {
						{ NgData.Compat.NgConfig2020062900.CurrentVersion, typeof(NgData.Compat.NgConfig2020062900) },
					});
				NgImageConfig = Util.FileUtil.LoadMigrate(
					Path.Combine(setting.UserDirectory, NgImageConfigFile),
					NgData.NgImageConfig.CreateDefault());
				HiddenConfig = Util.FileUtil.LoadMigrate(
					Path.Combine(setting.UserDirectory, HiddenConfigFile),
					NgData.HiddenConfig.CreateDefault());
				WatchConfig = Util.FileUtil.LoadMigrate(
					Path.Combine(setting.UserDirectory, WatchConfigFile),
					NgData.WatchConfig.CreateDefault());
				WatchImageConfig = Util.FileUtil.LoadMigrate(
					Path.Combine(setting.UserDirectory, WatchImageConfigFile),
					NgData.WatchImageConfig.CreateDefault());
				foreach(var r in NgConfig.CatalogRegex
					.Concat(NgConfig.ThreadRegex)
					.Concat(WatchConfig.CatalogRegex)) {

					if(!IsValidRegex(r)) {
						throw new Exceptions.InitializeFailedException(
							$"NG/Watch正規表現が不正です。{ Environment.NewLine }{ Environment.NewLine }{ r }");
					}
				}

				// 古い非表示レス設定を削除する
				var hiddenCount = HiddenConfig.Res.Length;
				if(0 < hiddenCount) {
					var d = DateTime.Now.AddDays(-HiddenConfig.ExpireDay);
					var t = HiddenConfig.Res.Where(x => d <= x.Res.Res.NowDateTime).ToArray();
					if(hiddenCount != t.Length) {
						HiddenConfig.Res = t;
						SaveConfig(HiddenConfigFile, HiddenConfig);
					}
				}
			}

			NgConfig ??= NgData.NgConfig.CreateDefault();
			NgImageConfig ??= NgData.NgImageConfig.CreateDefault();
			HiddenConfig ??= NgData.HiddenConfig.CreateDefault();
			WatchConfig ??= NgData.WatchConfig.CreateDefault();
			WatchImageConfig ??= NgData.WatchImageConfig.CreateDefault();
		}

		public static Setting InitializedSetting { get; private set; }

		public static NgData.NgConfig NgConfig { get; private set; }
		public static NgData.NgImageConfig NgImageConfig { get; private set; }
		public static NgData.HiddenConfig HiddenConfig { get; private set; }

		public static NgData.WatchConfig WatchConfig { get; private set; }
		public static NgData.WatchImageConfig WatchImageConfig { get; private set; }

		public static void UpdateCatalogIdNg(bool ng) {
			if(NgConfig.EnableCatalogIdNg != ng) {
				NgConfig.EnableCatalogIdNg = ng;
				SaveConfig(NgConfigFile, NgConfig);
				ngUpdateNotifyer = Notify(ngUpdateNotifyer, NgConfig);
			}
		}

		public static void UpdateThreadIdNg(bool ng) {
			if(NgConfig.EnableThreadIdNg != ng) {
				NgConfig.EnableThreadIdNg = ng;
				SaveConfig(NgConfigFile, NgConfig);
				ngUpdateNotifyer = Notify(ngUpdateNotifyer, NgConfig);
			}
		}

		public static void AddCatalogNgWord(string word) {
			System.Diagnostics.Debug.Assert(word != null);
			if(!string.IsNullOrWhiteSpace(word)) {
				var c = NgConfig.CatalogWords.Length;
				NgConfig.CatalogWords = Append(NgConfig.CatalogWords, word);
				if(c != NgConfig.CatalogWords.Length) {
					SaveConfig(NgConfigFile, NgConfig);
					ngUpdateNotifyer = Notify(ngUpdateNotifyer, NgConfig);
				}
			}
		}

		public static void RemoveCatalogNgWord(string word) {
			System.Diagnostics.Debug.Assert(word != null);
			var c = NgConfig.CatalogWords.Length;
			NgConfig.CatalogWords = NgConfig.CatalogWords.Where(x => x != word).ToArray();
			if(c != NgConfig.CatalogWords.Length) {
				SaveConfig(NgConfigFile, NgConfig);
				ngUpdateNotifyer = Notify(ngUpdateNotifyer, NgConfig);
			}
		}

		public static void ReplaceCatalogNgWord(string[] words) {
			System.Diagnostics.Debug.Assert(words != null);
			NgConfig.CatalogWords = words.ToArray();
			SaveConfig(NgConfigFile, NgConfig);
			ngUpdateNotifyer = Notify(ngUpdateNotifyer, NgConfig);
		}

		public static void AddCatalogNgRegex(string word) {
			System.Diagnostics.Debug.Assert(word != null);
			if(!string.IsNullOrWhiteSpace(word)) {
				if(IsValidRegex(word)) {
					var c = NgConfig.CatalogRegex.Length;
					NgConfig.CatalogRegex = Append(NgConfig.CatalogRegex, word);
					if(c != NgConfig.CatalogRegex.Length) {
						SaveConfig(NgConfigFile, NgConfig);
						ngUpdateNotifyer = Notify(ngUpdateNotifyer, NgConfig);
					}
				}
			}
		}

		public static void RemoveCatalogNgRegex(string word) {
			System.Diagnostics.Debug.Assert(word != null);
			var c = NgConfig.CatalogRegex.Length;
			NgConfig.CatalogRegex = NgConfig.CatalogRegex.Where(x => x != word).ToArray();
			if(c != NgConfig.CatalogRegex.Length) {
				SaveConfig(NgConfigFile, NgConfig);
				ngUpdateNotifyer = Notify(ngUpdateNotifyer, NgConfig);
			}
		}

		public static void ReplaceCatalogNgRegex(string[] words) {
			System.Diagnostics.Debug.Assert(words != null);
			NgConfig.CatalogRegex = words.ToArray();
			SaveConfig(NgConfigFile, NgConfig);
			ngUpdateNotifyer = Notify(ngUpdateNotifyer, NgConfig);
		}


		public static void AddThreadNgWord(string word) {
			System.Diagnostics.Debug.Assert(word != null);
			if(!string.IsNullOrWhiteSpace(word)) {
				var c = NgConfig.ThreadWords.Length;
				NgConfig.ThreadWords = Append(NgConfig.ThreadWords, word);
				if(c != NgConfig.ThreadWords.Length) {
					SaveConfig(NgConfigFile, NgConfig);
					ngUpdateNotifyer = Notify(ngUpdateNotifyer, NgConfig);
				}
			}
		}

		public static void RemoveThreadNgWord(string word) {
			System.Diagnostics.Debug.Assert(word != null);
			var c = NgConfig.ThreadWords.Length;
			NgConfig.ThreadWords = NgConfig.ThreadWords.Where(x => x != word).ToArray();
			if(c != NgConfig.ThreadWords.Length) {
				SaveConfig(NgConfigFile, NgConfig);
				ngUpdateNotifyer = Notify(ngUpdateNotifyer, NgConfig);
			}
		}

		public static void ReplaceThreadNgWord(string[] words) {
			System.Diagnostics.Debug.Assert(words != null);
			NgConfig.ThreadWords = words.ToArray();
			SaveConfig(NgConfigFile, NgConfig);
			ngUpdateNotifyer = Notify(ngUpdateNotifyer, NgConfig);
		}

		public static void AddThreadNgRegex(string word) {
			System.Diagnostics.Debug.Assert(word != null);
			if(!string.IsNullOrWhiteSpace(word)) {
				if(IsValidRegex(word)) {
					var c = NgConfig.ThreadRegex.Length;
					NgConfig.ThreadRegex = Append(NgConfig.ThreadRegex, word);
					if(c != NgConfig.ThreadRegex.Length) {
						SaveConfig(NgConfigFile, NgConfig);
						ngUpdateNotifyer = Notify(ngUpdateNotifyer, NgConfig);
					}
				}
			}
		}

		public static void RemoveThreadNgRegex(string word) {
			System.Diagnostics.Debug.Assert(word != null);
			var c = NgConfig.ThreadRegex.Length;
			NgConfig.ThreadRegex = NgConfig.ThreadRegex.Where(x => x != word).ToArray();
			if(c != NgConfig.ThreadRegex.Length) {
				SaveConfig(NgConfigFile, NgConfig);
				ngUpdateNotifyer = Notify(ngUpdateNotifyer, NgConfig);
			}
		}

		public static void ReplaceThreadNgRegex(string[] words) {
			System.Diagnostics.Debug.Assert(words != null);
			NgConfig.ThreadRegex = words.ToArray();
			SaveConfig(NgConfigFile, NgConfig);
			ngUpdateNotifyer = Notify(ngUpdateNotifyer, NgConfig);
		}

		public static void AddHiddenRes(NgData.HiddenData data) {
			System.Diagnostics.Debug.Assert(data != null);
			var c = HiddenConfig.Res.Length;
			HiddenConfig.Res = Append(HiddenConfig.Res, data);
			if(c != HiddenConfig.Res.Length) {
				SaveConfig(HiddenConfigFile, HiddenConfig);
				hiddenUpdateNotifyer = Notify(hiddenUpdateNotifyer, HiddenConfig);
			}
		}

		public static void RemoveHiddenRes(NgData.HiddenData data) {
			System.Diagnostics.Debug.Assert(data != null);
			var c = HiddenConfig.Res.Length;
			HiddenConfig.Res = HiddenConfig.Res.Where(x => x != data).ToArray();
			if(c != HiddenConfig.Res.Length) {
				SaveConfig(HiddenConfigFile, HiddenConfig);
				hiddenUpdateNotifyer = Notify(hiddenUpdateNotifyer, HiddenConfig);
			}
		}

		public static void AddNgImage(NgData.NgImageData data) {
			System.Diagnostics.Debug.Assert(data != null);
			if(!NgImageConfig.Images.Select(x => x.Hash).Contains(data.Hash)) {
				NgImageConfig.Images = NgImageConfig.Images.Concat(new NgImageData[] { data }).ToArray();
				SaveConfig(NgImageConfigFile, NgImageConfig);
				imageUpdateNotifyer = Notify(imageUpdateNotifyer, NgImageConfig);
			}
		}

		public static void RemoveNgImage(NgData.NgImageData data) {
			System.Diagnostics.Debug.Assert(data != null);
			var c = NgImageConfig.Images.Length;
			NgImageConfig.Images = NgImageConfig.Images.Where(x => x.Hash != data.Hash).ToArray();
			if(c != NgImageConfig.Images.Length) {
				SaveConfig(NgImageConfigFile, NgImageConfig);
				imageUpdateNotifyer = Notify(imageUpdateNotifyer, NgImageConfig);
			}
		}

		public static void ReplaceNgImage(NgData.NgImageData[] data) {
			System.Diagnostics.Debug.Assert(data != null);
			NgImageConfig.Images = data.ToArray();
			SaveConfig(NgImageConfigFile, NgImageConfig);
			imageUpdateNotifyer = Notify(imageUpdateNotifyer, NgImageConfig);
		}

		public static void UpdateNgImageMethod(ImageNgMethod m) {
			if(NgImageConfig.NgMethod != m) {
				NgImageConfig.NgMethod = m;
				SaveConfig(NgImageConfigFile, NgImageConfig);
				imageUpdateNotifyer = Notify(imageUpdateNotifyer, NgImageConfig);
			}
		}

		public static void UpdateNgImageThreshold(int t) {
			if(NgImageConfig.Threshold != t) {
				NgImageConfig.Threshold = t;
				SaveConfig(NgImageConfigFile, NgImageConfig);
				imageUpdateNotifyer = Notify(imageUpdateNotifyer, NgImageConfig);
			}
			if(WatchImageConfig.Threshold != t) {
				WatchImageConfig.Threshold = t;
				SaveConfig(WatchImageConfigFile, WatchImageConfig);
				WatchImageUpdateNotifyer.Notify(WatchImageConfig);
			}
		}

		public static void ReplaceWatchWord(string[] words) {
			System.Diagnostics.Debug.Assert(words != null);
			WatchConfig.CatalogWords = words.ToArray();
			SaveConfig(WatchConfigFile, WatchConfig);
			WatchUpdateNotifyer.Notify(WatchConfig);
		}

		public static void ReplaceWatchRegex(string[] words) {
			System.Diagnostics.Debug.Assert(words != null);
			WatchConfig.CatalogRegex = words.ToArray();
			SaveConfig(WatchConfigFile, WatchConfig);
			WatchUpdateNotifyer.Notify(WatchConfig);
		}

		public static void AddWatchImage(NgData.NgImageData data) {
			System.Diagnostics.Debug.Assert(data != null);
			if(!WatchImageConfig.Images.Select(x => x.Hash).Contains(data.Hash)) {
				WatchImageConfig.Images = WatchImageConfig.Images.Concat(new NgImageData[] { data }).ToArray();
				SaveConfig(WatchImageConfigFile, WatchImageConfig);
				WatchImageUpdateNotifyer.Notify(WatchImageConfig);
			}
		}

		public static void RemoveWatchImage(NgData.NgImageData data) {
			System.Diagnostics.Debug.Assert(data != null);
			var c = NgImageConfig.Images.Length;
			WatchImageConfig.Images = WatchImageConfig.Images.Where(x => x.Hash != data.Hash).ToArray();
			if(c != WatchImageConfig.Images.Length) {
				SaveConfig(WatchImageConfigFile, WatchImageConfig);
				WatchImageUpdateNotifyer.Notify(WatchImageConfig);
			}
		}

		public static void ReplaceWatchImage(NgData.NgImageData[] data) {
			System.Diagnostics.Debug.Assert(data != null);
			WatchImageConfig.Images = data.ToArray();
			SaveConfig(WatchImageConfigFile, WatchImageConfig);
			WatchImageUpdateNotifyer.Notify(WatchImageConfig);
		}

		public static void AddNgUpdateNotifyer(Action<NgData.NgConfig> action) {
			ngUpdateNotifyer.Add(new WeakReference<Action<NgData.NgConfig>>(action));
		}

		public static void AddHiddenUpdateNotifyer(Action<NgData.HiddenConfig> action) {
			hiddenUpdateNotifyer.Add(new WeakReference<Action<NgData.HiddenConfig>>(action));
		}

		public static void AddImageUpdateNotifyer(Action<NgData.NgImageConfig> action) {
			imageUpdateNotifyer.Add(new WeakReference<Action<NgData.NgImageConfig>>(action));
		}

		private static bool IsValidRegex(string r) {
			try {
				_ = new Regex(r);
				return true;
			}
			catch(ArgumentException) {
				return false;
			}
		}

		private static T[] Append<T>(T[] array, T s) {
			return (!array.Contains(s)) ? array.Concat(new T[] { s }).ToArray() : array;
		}


		private static void SaveConfig(string fileName, object o) {
			if((InitializedSetting.UserDirectory != null) && Directory.Exists(InitializedSetting.UserDirectory)) {
				try {
					Util.FileUtil.SaveJson(Path.Combine(InitializedSetting.UserDirectory, fileName), o);
				}
				catch(IOException e) {
					// TODO: どうしよ
					/*
					throw new Exceptions.InitializeFailedException(
						string.Format(
							"ファイルの保存に失敗しました{0}{0}{1}",
							Environment.NewLine,
							e.Message));
							*/
					throw;
				}
			}
		}

		private static List<WeakReference<Action<T>>> Notify<T>(List<WeakReference<Action<T>>> list, T instance) {
			lock(lockObj) {
				foreach(var a in list) {
					if(a.TryGetTarget(out var t)) {
						t(instance);
					}
				}
				return list.Where(x => x.TryGetTarget(out var _)).ToList();
			}
		}
	}
}
