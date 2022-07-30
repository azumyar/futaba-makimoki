using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Data {
	public class JsonObject {
		public override string ToString() {
			return this.ToString(Formatting.None);
		}

		public virtual string ToString(Formatting format, params JsonConverter[] @params) {
			return JsonConvert.SerializeObject(this, format, @params);
		}
	}

	public class ConfigObject : JsonObject {
		[JsonProperty("version", Required = Required.Always)]
		public int Version { get; protected set; }

		public override string ToString() {
			return this.ToString(Formatting.Indented);
		}
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

	public class Cookie2 : JsonObject {
		[JsonProperty("name")]
		public string Name { get; private set; }
		[JsonProperty("value")]
		public string Value { get; private set; }
		[JsonProperty("path")]
		public string Path { get; private set; }
		[JsonProperty("domain")]
		public string Domain { get; private set; }
		[JsonProperty("expire")]
		public DateTime Expire { get; private set; }

		public Cookie2(string name, string value, string path, string domain, DateTime expire) {
			this.Name = name;
			this.Value = value;
			this.Path = path;
			this.Domain = domain;
			this.Expire = expire switch {
				DateTime d when d < DateTime.Now => DateTime.MaxValue,
				DateTime d => d,
			};
		}
	}
}
