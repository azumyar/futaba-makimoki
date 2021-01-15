using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Yarukizero.Net.MakiMoki.Wpf.Canvas98.Canvas98Data {
	public class Canvas98Bookmarklet : Data.ConfigObject {
		public static int CurrentVersion { get; } = 2021011600;

		[JsonProperty("bookmarklet", Required = Required.Always)]
		public string Bookmarklet { get; private set; }

		[JsonProperty("bookmarklet-extends", Required = Required.Always)]
		public Dictionary<string, string> ExtendBookmarklet { get; private set; }

		[JsonIgnore]
		private string CacheScript { get; set; }
		[JsonIgnore]
		private string[] CacheExtendScripts { get; set; }


		[JsonIgnore]
		public string Script {
			get {
				if(this.CacheScript != null) {
					return this.CacheScript;
				}

				if(this.Bookmarklet == null) {
					return this.CacheScript = "";
				}

				return this.CacheScript = RemovePrefix(this.Bookmarklet);
			}
		}

		[JsonIgnore]
		public string[] ExtendScripts {
			get {
				if(this.CacheExtendScripts != null) {
					return this.CacheExtendScripts;
				}

				return this.CacheExtendScripts = this.ExtendBookmarklet
					.Select(x => RemovePrefix(x.Value))
					.ToArray();
			}
		}

		private string RemovePrefix(string script) {
			return script.StartsWith("javascript:")
				? script.Substring("javascript:".Length)
					: script;
		}

		public static Canvas98Bookmarklet From(
			string bookmarklet,
			Dictionary<string, string> extends = null) {

			return new Canvas98Bookmarklet() {
				Version = CurrentVersion,
				Bookmarklet = bookmarklet ?? "",
				ExtendBookmarklet = extends ?? new Dictionary<string, string>()
			};
		}
	}

	public class StoredForm : Data.JsonObject {
		[JsonProperty("name")]
		public string Name { get; private set; }
		[JsonProperty("sub")]
		public string Subject { get; private set; }
		[JsonProperty("email")]
		public string Email { get; private set; }
		[JsonProperty("pwd")]
		public string Password { get; private set; }
	}
}