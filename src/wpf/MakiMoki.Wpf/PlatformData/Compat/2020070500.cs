#pragma warning disable CS0612 // 古い形式警告を除去
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yarukizero.Net.MakiMoki.Data;

namespace Yarukizero.Net.MakiMoki.Wpf.PlatformData.Compat {
	class WpfConfig2020062900 : Data.ConfigObject, Data.IMigrateCompatObject {
		public static int CurrentVersion { get; } = 2020062900;

		[JsonProperty("catalog-enable-movie-marker", Required = Required.Always)]
		public bool IsEnabledMovieMarker { get; private set; }

		[JsonProperty("catalog-enable-old-marker", Required = Required.Always)]
		public bool IsEnabledOldMarker { get; private set; }

		[JsonProperty("catalog-ng-image-hidden", Required = Required.Always)]
		public CatalogNgImage CatalogNgImage { get; private set; }

		[JsonProperty("thread-visible-del-res", Required = Required.Always)]
		public ThreadDelResVisibility ThreadDelResVisibility { get; private set; }

		[JsonProperty("post-clipbord-jpeg-quality", Required = Required.Always)]
		public int ClipbordJpegQuality { get; private set; }

		[JsonProperty("post-clipbord-enable-url", Required = Required.Always)]
		public bool ClipbordIsEnabledUrl { get; private set; }

		[JsonProperty("media-export-path", Required = Required.Always)]
		public string[] MediaExportPath { get; private set; }

		[JsonProperty("media-cache-expire-day", Required = Required.Always)]
		public int CacheExpireDay { get; private set; }

		[JsonProperty("export-output-ng-res", Required = Required.Always)]
		public ExportNgRes ExportNgRes { get; private set; }

		[JsonProperty("export-output-ng-image", Required = Required.Always)]
		public ExportNgImage ExportNgImage { get; private set; }

		[JsonProperty("platform-browser-path", Required = Required.Always)]
		public string BrowserPath { get; private set; }

		public ConfigObject Migrate() {
			var conf = JsonConvert.DeserializeObject<WpfConfig>(
				Util.FileUtil.LoadFileString(new Util.ResourceLoader(
					typeof(Wpf.WpfConfig.WpfConfigLoader))
						.Get(Wpf.WpfConfig.WpfConfigLoader.SystemConfigFile)
					));

			return WpfConfig.Create(
				isEnabledMovieMarker: IsEnabledMovieMarker,
				isEnabledOldMarker: IsEnabledOldMarker,
				catalogNgImage: CatalogNgImage,
				threadDelResVisibility: ThreadDelResVisibility,
				clipbordJpegQuality: ClipbordJpegQuality,
				clipbordIsEnabledUrl: ClipbordIsEnabledUrl,
				mediaExportPath: MediaExportPath,
				cacheExpireDay: CacheExpireDay,
				exportNgRes: ExportNgRes,
				exportNgImage: ExportNgImage,
				browserPath: BrowserPath,
				catalogSearchResult: conf.CatalogSearchResult,
				isVisibleCatalogIsolateThread: conf.IsVisibleCatalogIsolateThread,
				maxWidthPostView: conf.MaxWidthPostView,
				isEnabledOpacityPostView: conf.IsEnabledOpacityPostView,
				opacityPostView: conf.OpacityPostView,
				windowTopmost: conf.IsEnabledWindowTopmost,
				ngReasonInput: conf.IsEnabledNgReasonInput,
				windowTheme: conf.WindowTheme,
				isEnabledIdMarker: conf.IsEnabledIdMarker,
				isEnabledThreadCommandPalette: conf.IsEnabledThreadCommandPalette,
				isEnabledFetchThumbnail: conf.IsEnabledFetchThumbnail,
				commandPalettePosition: conf.CommandPalettePosition,
				canvas98Position: conf.Canvas98Position,
				isEnabledFailsafeMistakePost: conf.IsEnabledFailsafeMistakePost,
				bouyomiChanEndPoint: conf.BouyomiChanEndPoint,
				isMaskPassword: conf.IsMaskPassword,
				isVisibleNgCatalog: conf.IsVisibleCatalogViaNg,
				isVisibleNgThread: conf.IsVisibleThreadViaNg
			);
		}

	}

}
