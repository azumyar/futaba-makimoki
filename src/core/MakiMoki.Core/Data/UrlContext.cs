using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Data {
	public class UrlContext : JsonObject {
		[JsonProperty("base-url", Required = Required.Always)]
		public string BaseUrl { get; private set; }
		[JsonProperty("thread-no", Required = Required.Always)]
		public string ThreadNo { get; private set; }

		[JsonIgnore]
		public bool IsCatalogUrl => string.IsNullOrWhiteSpace(this.ThreadNo);
		[JsonIgnore]
		public bool IsThreadUrl => !string.IsNullOrWhiteSpace(this.ThreadNo);

		/// <summary>JSONシリアライザ用</summary>
		private UrlContext() { }

		public UrlContext(string baseUrl) : this(baseUrl, "") { }

		public UrlContext(string baseUrl, string threadNo) {
			this.BaseUrl = baseUrl;
			this.ThreadNo = threadNo;
		}

		public string ToUrlString() {
			return this.IsCatalogUrl
				? $"{ this.BaseUrl }futaba.php?mode=cat"
					: $"{ this.BaseUrl }res/{ this.ThreadNo }.htm";
		}

		public override bool Equals(object obj) {
			if(obj is UrlContext u) {
				return (u.BaseUrl == this.BaseUrl)
					&& (u.ThreadNo == this.ThreadNo);
			}
			return base.Equals(obj);
		}

		public override int GetHashCode() {
			return (this.BaseUrl + this.ThreadNo).GetHashCode();
		}

		public static bool operator ==(UrlContext z, UrlContext w) {
			if(z is null && w is null) {
				return true;
			}
			if(z is null || w is null) {
				return false;
			}

			return (z.BaseUrl == w.BaseUrl)
				&& (z.ThreadNo == w.ThreadNo);
		}

		public static bool operator !=(UrlContext z, UrlContext w) {
			if(z is null || w is null) {
				return true;
			}

			return !((z.BaseUrl == w.BaseUrl)
				&& (z.ThreadNo == w.ThreadNo));
		}
	}
}
