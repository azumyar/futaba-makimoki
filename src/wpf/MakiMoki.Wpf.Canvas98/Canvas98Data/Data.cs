using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Yarukizero.Net.MakiMoki.Wpf.Canvas98.Canvas98Data {
	public class Canvas98Bookmarklet : Data.ConfigObject {
		public static int CurrentVersion { get; } = 2021080700;

		[JsonProperty("bookmarklet", Required = Required.Always)]
		public string Bookmarklet { get; private set; }

		[JsonProperty("bookmarklet-layer", Required = Required.Always)]
		public string BookmarkletLayer { get; private set; }
		[JsonProperty("bookmarklet-albam", Required = Required.Always)]
		public string BookmarkletAlbam { get; private set; }
		[JsonProperty("bookmarklet-menu", Required = Required.Always)]
		public string BookmarkletMenu { get; private set; }



		[JsonIgnore]
		private string CacheScript { get; set; }
		[JsonIgnore]
		private string[] CacheExtendScripts { get; set; }
		[JsonIgnore]
		private string CacheScriptAlbam { get; set; }

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
		public string ScriptAlbam {
			get {
				if(this.CacheScriptAlbam != null) {
					return this.CacheScriptAlbam;
				}

				if(this.BookmarkletAlbam == null) {
					return this.CacheScriptAlbam = "";
				}

				return this.CacheScriptAlbam = RemovePrefix(this.BookmarkletAlbam);
			}
		}


		[JsonIgnore]
		public string[] ExtendScripts {
			get {
				if(this.CacheExtendScripts != null) {
					return this.CacheExtendScripts;
				}

				return this.CacheExtendScripts = new [] {
					this.BookmarkletLayer,
					this.BookmarkletMenu,
				}.Where(x => !string.IsNullOrEmpty(x))
					.Select(x => RemovePrefix(x))
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
			string bookmarkletLayer = null,
			string bookmarkletAlbam = null,
			string bookmarkletMenu = null) {

			return new Canvas98Bookmarklet() {
				Version = CurrentVersion,
				Bookmarklet = bookmarklet ?? "",
				BookmarkletLayer = bookmarkletLayer ?? "",
				BookmarkletAlbam = bookmarkletAlbam ?? "",
				BookmarkletMenu = bookmarkletMenu ?? "",
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