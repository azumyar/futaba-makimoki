using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Data {
	public class UrlContext : JsonObject {
		[JsonProperty("base-url")]
		public string BaseUrl { get; }
		[JsonProperty("thread-no")]
		public string ThreadNo { get; }

		[JsonIgnore]
		public bool IsCatalogUrl => string.IsNullOrWhiteSpace(this.ThreadNo);
		[JsonIgnore]
		public bool IsThreadUrl => !string.IsNullOrWhiteSpace(this.ThreadNo);

		public UrlContext(string baseUrl) : this(baseUrl, "") { }

		public UrlContext(string baseUrl, string threadNo) {
			this.BaseUrl = baseUrl;
			this.ThreadNo = threadNo;
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
