using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Data {
	public class BordConfig : ConfigObject {
		public static int CurrentVersion { get; } = 2020062900;

		[JsonProperty("bords", Required = Required.Always)]
		public BordData[] Bords { get; protected set; }

		internal static BordConfig CreateDefault() {
			return new BordConfig() {
				Version = CurrentVersion,
				Bords = new BordData[0],
			};
		}
	}

	public class CoreBordConfig : BordConfig {
		[JsonProperty("max-file-size", Required = Required.Always)]
		public int MaxFileSize { get; private set; }

		internal new static CoreBordConfig CreateDefault() {
			return new CoreBordConfig() {
				Version = CurrentVersion,
				Bords = new BordData[0],
			};
		}

		internal static CoreBordConfig CreateAppInstance(CoreBordConfig config, BordData[] margeData) {
			return new CoreBordConfig() {
				Version = CurrentVersion,
				MaxFileSize = config.MaxFileSize,
				Bords = margeData,
			};
		}
	}

	public class BordData : JsonObject {
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
		public BordDataExtra Extra { get; set; }
	}

	public class BordDataExtra : JsonObject {
		[JsonProperty("name")]
		public bool? Name { get; set; }

		[JsonProperty("enable-res-image")]
		public bool? ResImage { get; set; }

		[JsonProperty("enable-mail-ip")]
		public bool? MailIp { get; set; }

		[JsonProperty("enable-mail-id")]
		public bool? MailId { get; set; }

		[JsonProperty("always-ip")]
		public bool? AlwaysIp { get; set; }

		[JsonProperty("always-id")]
		public bool? AlwaysId { get; set; }

		[JsonProperty("max-stored-res", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(0)]
		public int MaxStoredRes { get; set; }

		[JsonProperty("max-stored-time", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(0)]
		public int MaxStoredTime { get; set; }

		[JsonProperty("enable-res-tegaki", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(false)]
		public bool ResTegaki { get; set; }

		[JsonIgnore]
		public bool NameValue => Name ?? true;

		[JsonIgnore]
		public bool ResImageValue => ResImage ?? true;

		[JsonIgnore]
		public bool MailIpValue => MailIp ?? false;

		[JsonIgnore]
		public bool MailIdValue => MailId ?? false;

		[JsonIgnore]
		public bool AlwaysIpValue => AlwaysIp ?? false;

		[JsonIgnore]
		public bool AlwaysIdValue => AlwaysId ?? false;
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
				Catalogs = new FutabaSavedCatalogData[0],
				Threads = new FutabaSavedThreadData[0],
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
				Items = new PostedResItem[0],
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