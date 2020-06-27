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

	public class Ptua : ConfigObject {
		public static readonly int CurrentVersion = 1;

		[JsonProperty("value")]
		public string Value { get; private set; }

		public Ptua() {
			base.Version = CurrentVersion;
		}

		public Ptua(string val) : this() {
			this.Value = val;
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

	public class Password : ConfigObject {
		public static readonly int CurrentVersion = 1;

		[JsonProperty("futaba")]
		public string Futaba { get; private set; }


		[JsonIgnore]
		public string FutabaValue => Futaba ?? "";

		public Password() {
			base.Version = CurrentVersion;
		}

		public static Password FromFutaba(string password) {
			return new Password() {
				Futaba = password,
			};
		}
	}
}
