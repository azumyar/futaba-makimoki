using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Data {
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
		[JsonProperty("ext", Required = Required.DisallowNull)]
		public string Ext { get; private set; }

		[JsonProperty("mime", Required = Required.DisallowNull)]
		public string MimeType { get; private set; }
	}
}
