using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yarukizero.Net.MakiMoki.Wpf.PlatformData {
	class VersionCheckResponse : Data.JsonObject {
		[JsonProperty("period")]
		public int? Period { get; private set; }


		[JsonProperty("file")]
		public string FileName { get; private set; }

		[JsonProperty("url")]
		public string Url { get; private set; }
	}
}
