using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Yarukizero.Net.MakiMoki.Data;

namespace Yarukizero.Net.MakiMoki.Wpf.PlatformData.Compat {
	internal class GestureConfig2021020100: Data.ConfigObject, Data.IMigrateCompatObject {
		public static int CurrentVersion { get; } = 2021020100;

		[JsonProperty("gesture-key-catalog-update", Required = Required.Always)]
		public string[] KeyGestureCatalogUpdate { get; private set; }
		[JsonProperty("gesture-key-catalog-search", Required = Required.Always)]
		public string[] KeyGestureCatalogSearch { get; private set; }
		[JsonProperty("gesture-key-catalog-mode-change-toggle", Required = Required.Always)]
		public string[] KeyGestureCatalogModeToggleUpdate { get; private set; }
		[JsonProperty("gesture-key-catalog-open-post", Required = Required.Always)]
		public string[] KeyGestureCatalogOpenPost { get; private set; }
		[JsonProperty("gesture-key-catalog-close", Required = Required.Always)]
		public string[] KeyGestureCatalogClose { get; private set; }
		[JsonProperty("gesture-key-catalog-next", Required = Required.Always)]
		public string[] KeyGestureCatalogNext { get; private set; }
		[JsonProperty("gesture-key-catalog-previous", Required = Required.Always)]
		public string[] KeyGestureCatalogPrevious { get; private set; }


		[JsonProperty("gesture-key-thread-update", Required = Required.Always)]
		public string[] KeyGestureThreadUpdate { get; private set; }
		[JsonProperty("gesture-key-thread-search", Required = Required.Always)]
		public string[] KeyGestureThreadSearch { get; private set; }
		[JsonProperty("gesture-key-thread-open-tegaki", Required = Required.Always)]
		public string[] KeyGestureThreadOpenTegaki { get; private set; }
		[JsonProperty("gesture-key-thread-open-post", Required = Required.Always)]
		public string[] KeyGestureThreadOpenPost { get; private set; }
		[JsonProperty("gesture-key-thread-tab-close", Required = Required.Always)]
		public string[] KeyGestureThreadTabClose { get; private set; }
		[JsonProperty("gesture-key-thread-tab-next", Required = Required.Always)]
		public string[] KeyGestureThreadTabNext { get; private set; }
		[JsonProperty("gesture-key-thread-tab-previous", Required = Required.Always)]
		public string[] KeyGestureThreadTabPrevious { get; private set; }

		[JsonProperty("gesture-key-post-view-post", Required = Required.Always)]
		public string[] KeyGesturePostViewPost { get; private set; }
		[JsonProperty("gesture-key-post-view-open-image", Required = Required.Always)]
		public string[] KeyGesturePostViewOpenImage { get; private set; }
		[JsonProperty("gesture-key-post-view-open-uploader", Required = Required.Always)]
		public string[] KeyGesturePostViewOpenUploader { get; private set; }
		[JsonProperty("gesture-key-post-view-delete", Required = Required.Always)]
		public string[] KeyGesturePostViewDelete { get; private set; }
		[JsonProperty("gesture-key-post-view-close", Required = Required.Always)]
		public string[] KeyGesturePostViewClose { get; private set; }
		[JsonProperty("gesture-key-post-view-paste-image", Required = Required.Always)]
		public string[] KeyGesturePostViewPasteImage { get; private set; }
		[JsonProperty("gesture-key-post-view-paste-uploader", Required = Required.Always)]
		public string[] KeyGesturePostViewPasteUploader { get; private set; }

		public ConfigObject Migrate() {
			return GestureConfig.From(
				keyGestureCatalogUpdate: this.KeyGestureCatalogUpdate,
				keyGestureCatalogSearch: this.KeyGestureCatalogSearch,
				keyGestureCatalogOpenPost: this.KeyGestureCatalogOpenPost,
				keyGestureCatalogClose: this.KeyGestureCatalogClose,
				keyGestureCatalogNext: this.KeyGestureCatalogNext,
				keyGestureCatalogPrevious: this.KeyGestureCatalogPrevious,

				keyGestureThreadUpdate: this.KeyGestureThreadUpdate,
				keyGestureThreadSearch: this.KeyGestureThreadSearch,
				keyGestureThreadOpenTegaki:	this.KeyGestureThreadOpenTegaki,
				keyGestureThreadOpenPost: this.KeyGestureThreadOpenPost,
				keyGestureThreadTabClose: this.KeyGestureThreadTabClose,
				keyGestureThreadTabNext: this.KeyGestureThreadTabNext,
				keyGestureThreadTabPrevious: this.KeyGestureThreadTabPrevious,

				keyGesturePostViewPost: this.KeyGesturePostViewPost,
				keyGesturePostViewOpenImage: this.KeyGesturePostViewOpenImage,
				keyGesturePostViewOpenUploader: this.KeyGesturePostViewOpenUploader,
				keyGesturePostViewDelete: this.KeyGesturePostViewDelete,
				keyGesturePostViewClose: this.KeyGesturePostViewClose,
				keyGesturePostViewPasteImage: this.KeyGesturePostViewPasteImage,
				keyGesturePostViewPasteUploader: this.KeyGesturePostViewPasteUploader,

				mouseGestureCatalogOpenPost: new[] { new MouseGestureCommands(new [] {MouseGestureCommand.Up, MouseGestureCommand.Left}) },
				mouseGestureCatalogUpdate: new[] { new MouseGestureCommands(new[] { MouseGestureCommand.Up, MouseGestureCommand.Down }) },
				mouseGestureCatalogClose: new[] { new MouseGestureCommands(new[] { MouseGestureCommand.Up, MouseGestureCommand.Right }) },
				mouseGestureCatalogNext: new[] { new MouseGestureCommands(new[] { MouseGestureCommand.Right, MouseGestureCommand.Up, MouseGestureCommand.Right }) },
				mouseGestureCatalogPrevious: new[] { new MouseGestureCommands(new[] { MouseGestureCommand.Left, MouseGestureCommand.Up, MouseGestureCommand.Left }) },

				mouseGestureThreadOpenPost: new[] { new MouseGestureCommands(new[] { MouseGestureCommand.Down, MouseGestureCommand.Left }) },
				mouseGestureThreadUpdate: new[] { new MouseGestureCommands(new[] { MouseGestureCommand.Down, MouseGestureCommand.Up }) },
				mouseGestureThreadClose: new[] { new MouseGestureCommands(new[] { MouseGestureCommand.Down, MouseGestureCommand.Right }) },
				mouseGestureThreadNext: new[] { new MouseGestureCommands(new[] { MouseGestureCommand.Left, MouseGestureCommand.Down, MouseGestureCommand.Left }) },
				mouseGestureThreadPrevious: new[] { new MouseGestureCommands(new[] { MouseGestureCommand.Right, MouseGestureCommand.Down, MouseGestureCommand.Right }) }
				);

		}
	}
}
