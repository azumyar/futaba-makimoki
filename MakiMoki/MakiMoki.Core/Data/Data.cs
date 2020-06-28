using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Data {
	public class JsonObject {
		public override string ToString() {
			return JsonConvert.SerializeObject(this, Formatting.None);
		}
	}

	public class ConfigObject : JsonObject {
		[JsonProperty("version", Required = Required.Always)]
		public int Version { get; protected set; }
	}

	public class Cookie : JsonObject {
		[JsonProperty("name")]
		public string Name { get; private set; }
		[JsonProperty("value")]
		public string Value { get; private set; }

		public Cookie(string name, string value) {
			this.Name = name;
			this.Value = value;
		}
	}
}
