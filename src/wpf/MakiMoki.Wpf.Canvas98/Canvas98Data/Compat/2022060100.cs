using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yarukizero.Net.MakiMoki.Data;

namespace Yarukizero.Net.MakiMoki.Wpf.Canvas98.Canvas98Data.Compat {
	public class Canvas98Bookmarklet2022060100 : ConfigObject, IMigrateCompatObject {
		public static int CurrentVersion { get; } = 2022060100;

		[JsonProperty("bookmarklet", Required = Required.Always)]
		public string Bookmarklet { get; private set; }

		[JsonProperty("bookmarklet-layer", Required = Required.Always)]
		public string BookmarkletLayer { get; private set; }
		[JsonProperty("bookmarklet-albam", Required = Required.Always)]
		public string BookmarkletAlbam { get; private set; }
		[JsonProperty("bookmarklet-menu", Required = Required.Always)]
		public string BookmarkletMenu { get; private set; }
		[JsonProperty("bookmarklet-timelapse", Required = Required.Always)]
		public string BookmarkletTimelapse { get; private set; }

		public ConfigObject Migrate() {
			return Canvas98Bookmarklet.From(
				bookmarklet: Bookmarklet,
				bookmarkletLayer: BookmarkletLayer,
				bookmarkletAlbam: BookmarkletAlbam,
				bookmarkletMenu: BookmarkletMenu,
				bookmarkletTimelapse: BookmarkletTimelapse
			);
		}
	}
}