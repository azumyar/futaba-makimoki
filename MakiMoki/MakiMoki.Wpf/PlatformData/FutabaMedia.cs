using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yarukizero.Net.MakiMoki.Wpf.PlatformData {
	public class FutabaMedia {
		public bool IsExternalUrl { get; private set; }
		public Data.UrlContext BaseUrl { get; private set; }
		public Data.ResItem Res { get; private set; }

		public string ExternalUrl { get; private set; }

		private FutabaMedia() { }

		public static FutabaMedia FromFutabaUrl(Data.UrlContext baseUrl, Data.ResItem res) {
			return new FutabaMedia() {
				IsExternalUrl = false,
				BaseUrl = baseUrl,
				Res = res,
			};
		}

		public static FutabaMedia FromExternalUrl(string url) {
			return new FutabaMedia() {
				IsExternalUrl = true,
				ExternalUrl = url,
			};
		}
	}
}
