using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Data {
	public class MakiMokiConfig : ConfigObject {
		public static int CurrentVersion = -1;

		[JsonProperty("futaba-thread-get-incremental", Required = Required.Always)]
		public bool FutabaThreadGetIncremental { get; private set; }

		[JsonProperty("futaba-response-data-save", Required = Required.Always)]
		public bool FutabaResponseSave { get; private set; }

		[JsonProperty("futaba-post-data-expire-day", Required = Required.Always)]
		public int FutabaPostDataExpireDay { get; private set; }

		public static MakiMokiConfig From(bool threadGetIncremental, bool responseSave, int postDataExpireDay) {
			return new MakiMokiConfig() {
				Version = CurrentVersion,
				FutabaThreadGetIncremental = threadGetIncremental,
				FutabaResponseSave = responseSave,
				FutabaPostDataExpireDay = postDataExpireDay,
			};
		}
	}

	public class BordConfig : JsonObject {
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("url")]
		public string Url { get; set; }

		// intにして必須項目にするべきか
		[JsonProperty("max-file-size")]
		public int? MaxFileSize { get; set; }


		[JsonProperty("sort-index")]
		public int? SortIndex { get; set; }

		[JsonProperty("display")]
		public bool? Display { get; set; }

		[JsonProperty("extra", Required = Required.DisallowNull)]
		public BordConfigExtra Extra { get; set; }

		[JsonIgnore]
		public string MaxFileSzieValue => MaxFileSize?.ToString() ?? "";

		[JsonIgnore]
		public bool DisplayValue => (Display ?? true);

		[JsonIgnore]
		public int SortIndexValue => (SortIndex ?? int.MaxValue);
	}

	public class BordConfigExtra : JsonObject {
		[JsonProperty("name")]
		public bool? Name { get; set; }

		[JsonProperty("enable-res-image")]
		public bool? ResImage { get; set; }

		[JsonProperty("enable-mail-ip")]
		public bool? MailIp { get; set; }

		[JsonProperty("enable-mail-id")]
		public bool? MailId { get; set; }

		[JsonIgnore]
		public bool NameValue => Name ?? true;

		[JsonIgnore]
		public bool ResImageValue => ResImage ?? true;

		[JsonIgnore]
		public bool MailIpValue => MailIp ?? false;

		[JsonIgnore]
		public bool MailIdValue => MailId ?? false;
	}

	public class MimeConfig : ConfigObject {
		public static int CurrentVersion = -1;
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

		[JsonProperty("contents", Required = Required.Always)]
		public MimeContents MimeContents { get; private set; }
	}

	public enum MimeContents {
		None,
		Image,
		Video
	}

	public class UploderConfig : ConfigObject {
		public static int CurrentVersion = -1;

		[JsonProperty("uploders", Required = Required.Always)]
		public UploderData[] Uploders { get; private set; }
	}

	public class UploderData : JsonObject {
		[JsonProperty("root", Required = Required.Always)]
		public string Root { get; private set; }

		[JsonProperty("file", Required = Required.Always)]
		public string File { get; private set; }
	}

	public class FutabaSavedConfig : ConfigObject {
		public static int CurrentVersion = -1;

		[JsonProperty("time", Required = Required.Always)]
		public long Time { get; private set; }

		[JsonProperty("catalogs", Required = Required.Always)]
		public FutabaSavedCatalogData[] Catalogs { get; private set; }

		[JsonProperty("threads", Required = Required.Always)]
		public FutabaSavedThreadData[] Threads { get; private set; }

		public static FutabaSavedConfig CreateDefault() {
			return new FutabaSavedConfig() {
				Version = CurrentVersion,
				Time = new DateTimeOffset(DateTime.Now, new TimeSpan(+09, 00, 00)).ToUnixTimeMilliseconds(),
				Catalogs = new FutabaSavedCatalogData[0],
				Threads = new FutabaSavedThreadData[0],
			};
		}

		public static FutabaSavedConfig From(FutabaContext[] catalogs, FutabaContext[] threads) {
			System.Diagnostics.Debug.Assert(catalogs != null);
			System.Diagnostics.Debug.Assert(threads != null);

			return new FutabaSavedConfig() {
				Version = CurrentVersion,
				Time = new DateTimeOffset(DateTime.Now, new TimeSpan(+09, 00, 00)).ToUnixTimeMilliseconds(),
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
				var idx = catalog.ResItems.Select(x => x.ResItem.No).ToArray();
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
		public static int CurrentVersion = -1;

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