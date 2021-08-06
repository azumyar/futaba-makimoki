using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Yarukizero.Net.MakiMoki.Data;

namespace Yarukizero.Net.MakiMoki.Wpf.Canvas98.Canvas98Data.Compat {
	class Canvas98Bookmarklet2021011600 : ConfigObject, IMigrateCompatObject {
		public static int CurrentVersion { get; } = 2021011600;

		[JsonProperty("bookmarklet", Required = Required.Always)]
		public string Bookmarklet { get; private set; }

		[JsonProperty("bookmarklet-extends", Required = Required.Always)]
		public Dictionary<string, string> ExtendBookmarklet { get; private set; }

		public ConfigObject Migrate() {
			return Canvas98Bookmarklet.From(
				bookmarklet: Bookmarklet

				// "bookmarklet-extends" は廃止
			);
		}
	}
}
