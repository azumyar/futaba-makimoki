using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Data {
	public class BoardConfig : ConfigObject {
		public static int CurrentVersion { get; } = 2021012000;

		[JsonProperty("boards", Required = Required.Always)]
		public BoardData[] Boards { get; protected set; }

		internal static BoardConfig CreateDefault() {
			return new BoardConfig() {
				Version = CurrentVersion,
				Boards = Array.Empty<BoardData>(),
			};
		}

		internal static BoardConfig From(BoardData[] boards) {
			return new BoardConfig() {
				Version = CurrentVersion,
				Boards = boards.ToArray(),
			};
		}
	}

	public class CoreBoardConfig : BoardConfig {
		public static new int CurrentVersion { get; } = BoardConfig.CurrentVersion;

		[JsonProperty("max-file-size", Required = Required.Always)]
		public int MaxFileSize { get; private set; }

		internal new static CoreBoardConfig CreateDefault() {
			return new CoreBoardConfig() {
				Version = CurrentVersion,
				Boards = Array.Empty<BoardData>(),
			};
		}

		internal static CoreBoardConfig CreateAppInstance(CoreBoardConfig config, BoardData[] margeData) {
			return new CoreBoardConfig() {
				Version = CurrentVersion,
				MaxFileSize = config.MaxFileSize,
				Boards = margeData,
			};
		}

		internal static CoreBoardConfig From(BoardData[] boards, int maxFileSize) {
			return new CoreBoardConfig() {
				Version = CurrentVersion,
				MaxFileSize = maxFileSize,
				Boards = boards.ToArray(),
			};
		}
	}

	public class BoardData : JsonObject {
		[JsonProperty("name", Required = Required.Always)]
		public string Name { get; set; }

		[JsonProperty("url", Required = Required.Always)]
		public string Url { get; set; }

		[JsonProperty("default-comment", DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue("本文無し")]
		public string DefaultComment { get; set; }

		[JsonProperty("sort-index", DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(int.MaxValue)]
		public int SortIndex { get; set; }

		[JsonProperty("display", DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(true)]
		public bool Display { get; set; }

		[JsonProperty("extra", Required = Required.Always)]
		public BoardDataExtra Extra { get; set; }


		public static BoardData From(
			string name,
			string url,
			string defaultComment,
			int sortIndex,
			BoardDataExtra extra,
			bool display = true) {

			System.Diagnostics.Debug.Assert(!string.IsNullOrWhiteSpace(name));
			System.Diagnostics.Debug.Assert(!string.IsNullOrWhiteSpace(url));
			System.Diagnostics.Debug.Assert(!string.IsNullOrWhiteSpace(defaultComment));
			System.Diagnostics.Debug.Assert(extra != null);

			return new BoardData() {
				Name = name,
				Url = url,
				DefaultComment = defaultComment,
				SortIndex = sortIndex,
				Extra = extra,
				Display = display,
			};
		}
	}

	public class BoardDataExtra : JsonObject {
		[JsonProperty("name", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(true)]
		public bool Name { get; set; }

		[JsonProperty("enable-res-image", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(true)]
		public bool ResImage { get; set; }

		[JsonProperty("enable-mail-ip", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(false)]
		public bool MailIp { get; set; }

		[JsonProperty("enable-mail-id", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(false)]
		public bool MailId { get; set; }

		[JsonProperty("always-ip", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(false)]
		public bool AlwaysIp { get; set; }

		[JsonProperty("always-id", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(false)]
		public bool AlwaysId { get; set; }

		[JsonProperty("max-stored-res", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(0)]
		public int MaxStoredRes { get; set; }

		[JsonProperty("max-stored-time", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(0)]
		public int MaxStoredTime { get; set; }

		[JsonProperty("enable-res-tegaki", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(false)]
		public bool ResTegaki { get; set; }

		public static BoardDataExtra From(
			bool name,
			bool resImage,
			bool mailIp,
			bool mailId,
			bool alwaysIp,
			bool alwaysId,
			int maxStoredRes,
			int maxStoredTime,
			bool resTegaki) {

			return new BoardDataExtra() {
				Name = name,
				ResImage = resImage,
				MailIp = mailIp,
				MailId = mailId,
				AlwaysIp = alwaysIp,
				AlwaysId = alwaysId,
				MaxStoredRes = maxStoredRes,
				MaxStoredTime = maxStoredTime,
				ResTegaki = resTegaki,
			};
		}

		[JsonIgnore]
		public bool NameValue => Name;

		[JsonIgnore]
		public bool ResImageValue => ResImage;

		[JsonIgnore]
		public bool MailIpValue => MailIp;

		[JsonIgnore]
		public bool MailIdValue => MailId;

		[JsonIgnore]
		public bool AlwaysIpValue => AlwaysIp;

		[JsonIgnore]
		public bool AlwaysIdValue => AlwaysId;
	}

	public class MimeConfig : ConfigObject {
		public static int CurrentVersion { get; } = 2020062900;
		private Dictionary<string, string> mimeDic = null;

		[JsonProperty("support-types", Required = Required.DisallowNull)]
		public MimeData[] Types { get; private set; }

		[JsonIgnore]
		public Dictionary<string, string> MimeTypes {
			get {
				if(this.mimeDic == null) {
					this.mimeDic = new Dictionary<string, string>();
					foreach(var t in this.Types) {
						this.mimeDic.Add(t.Ext, t.MimeType);
					}
				}
				return this.mimeDic;
			}
		}
	}

	public class MimeData : JsonObject {
		[JsonProperty("ext", Required = Required.Always)]
		public string Ext { get; private set; }

		[JsonProperty("mime", Required = Required.Always)]
		public string MimeType { get; private set; }

		[JsonProperty("contents", Required = Required.Default)]
		[DefaultValue(MimeContents.None)]
		public MimeContents MimeContents { get; private set; }
	}

	public enum MimeContents {
		None,
		Image,
		Video
	}

	public class UploderConfig : ConfigObject {
		public static int CurrentVersion { get; } = 2020062900;

		[JsonProperty("uploders", Required = Required.Always)]
		public UploderData[] Uploders { get; private set; }
	}

	public class UploderData : JsonObject {
		[JsonProperty("root", Required = Required.Always)]
		public string Root { get; private set; }

		[JsonProperty("file", Required = Required.Always)]
		public string File { get; private set; }
	}

	public class FutabaApiConfig : ConfigObject {
		public static int CurrentVersion { get; } = 2020062900;

		[JsonProperty("ptua", Required = Required.Always)]
		public string Ptua { get; internal set; }

		[JsonProperty("cookies", Required = Required.Always)]
		public Cookie[] Cookies { get; internal set; }

		[JsonProperty("subject", Required = Required.Always)]
		public string SavedSubject { get; internal set; }

		[JsonProperty("name", Required = Required.Always)]
		public string SavedName { get; internal set; }

		[JsonProperty("mail", Required = Required.Always)]
		public string SavedMail { get; internal set; }

		[JsonProperty("password", Required = Required.Always)]
		public string SavedPassword { get; internal set; }

		public static FutabaApiConfig CreateDefault() {
			return new FutabaApiConfig() {
				Version = CurrentVersion,
				Ptua = "",
				Cookies = new Cookie[0],
				SavedSubject = "",
				SavedName = "",
				SavedMail = "",
				SavedPassword = "",
			};
		}
	}

	public class FutabaSavedConfig : ConfigObject {
		public static int CurrentVersion { get; } = 2020062900;

		[JsonProperty("time", Required = Required.Always)]
		public long Time { get; private set; }

		[JsonProperty("catalogs", Required = Required.Always)]
		public FutabaSavedCatalogData[] Catalogs { get; private set; }

		[JsonProperty("threads", Required = Required.Always)]
		public FutabaSavedThreadData[] Threads { get; private set; }

		public static FutabaSavedConfig CreateDefault() {
			return new FutabaSavedConfig() {
				Version = CurrentVersion,
				Time = Util.TimeUtil.ToUnixTimeMilliseconds(),
				Catalogs = Array.Empty<FutabaSavedCatalogData>(),
				Threads = Array.Empty<FutabaSavedThreadData>(),
			};
		}

		public static FutabaSavedConfig From(FutabaContext[] catalogs, FutabaContext[] threads) {
			System.Diagnostics.Debug.Assert(catalogs != null);
			System.Diagnostics.Debug.Assert(threads != null);

			return new FutabaSavedConfig() {
				Version = CurrentVersion,
				Time = Util.TimeUtil.ToUnixTimeMilliseconds(),
				Catalogs = catalogs.Select(x => FutabaSavedCatalogData.From(x)).Where(x => x != null).ToArray(),
				Threads = threads.Select(x => FutabaSavedThreadData.From(x)).Where(x => x != null).ToArray(),
			};
		}
	}

	public class FutabaSavedCatalogData : JsonObject {
		[JsonProperty("url", Required = Required.Always)]
		public UrlContext Url { get; private set; }

		[JsonProperty("catalog-response", Required = Required.Always)]
		public Data.FutabaResonse Catalog { get; private set; }
		[JsonProperty("catalog-sort-index", Required = Required.Always)]
		public string[] CatalogSortRes { get; private set; }
		[JsonProperty("catalog-res-counter", Required = Required.Always)]
		public Dictionary<string, int> CatalogResCounter { get; private set; }

		public static FutabaSavedCatalogData From(FutabaContext catalog) {
			var c = catalog.GetFullResponse();
			if(c != null) {
				var u = catalog.Url;
				var idx = catalog.ResItems.Where(x => !x.ResItem.IsolateValue).Select(x => x.ResItem.No).ToArray();
				var cnt = catalog.ResItems.ToDictionary(x => x.ResItem.No, x => x.CounterCurrent);
				return new FutabaSavedCatalogData() {
					Url = u,
					Catalog = c,
					CatalogSortRes = idx,
					CatalogResCounter = cnt,
				};
			} else {
				return null;
			}
		}
	}

	public class FutabaSavedThreadData : JsonObject {
		[JsonProperty("url", Required = Required.Always)]
		public UrlContext Url { get; private set; }

		[JsonProperty("thread-response", Required = Required.Always)]
		public Data.FutabaResonse Thread { get; private set; }

		public static FutabaSavedThreadData From(FutabaContext thread) {
			var t = thread.GetFullResponse();
			if(t != null) {
				var u = thread.Url;
				return new FutabaSavedThreadData() {
					Url = u,
					Thread = t,
				};
			} else {
				return null;
			}
		}
	}

	public class FutabaPostItemConfig : ConfigObject {
		public static int CurrentVersion { get; } = 2020062900;

		[JsonProperty("res", Required = Required.Always)]
		public PostedResItem[] Items { get; private set; }

		public static FutabaPostItemConfig CreateDefault() {
			return new FutabaPostItemConfig() {
				Version = CurrentVersion,
				Items = Array.Empty<PostedResItem>(),
			};
		}

		public static FutabaPostItemConfig From(PostedResItem[] items) {
			return new FutabaPostItemConfig() {
				Version = CurrentVersion,
				Items = items,
			};
		}
	}
}