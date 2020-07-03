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
			var t = typeof(Wpf.WpfConfig.WpfConfigLoader);
			var conf = JsonConvert.DeserializeObject<WpfConfig>(
				Util.FileUtil.LoadFileString(t.Assembly.GetManifestResourceStream(
					$"{ t.Namespace }.{ Wpf.WpfConfig.WpfConfigLoader.SystemConfigFile }")));

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

				// 2020070500
				catalogSearchResult: conf.CatalogSearchResult,
				isVisibleCatalogIsolateThread: conf.IsVisibleCatalogIsolateThread,
				isSavedPostSubject: conf.IsSavedPostSubject,
				isSavedPostName: conf.IsSavedPostName,
				isSavedPostMail: conf.IsSavedPostMail,
				minWidthPostView: conf.MinWidthPostView,
				maxWidthPostView: conf.MaxWidthPostView,
				isEnabledOpacityPostView: conf.IsEnabledOpacityPostView,
				opacityPostView: conf.OpacityPostView
			);
		}

		/*
		public static WpfConfig CreateDefault() {
			// ここは使われない
			return new WpfConfig() {
				Version = CurrentVersion,
			};
		}

		public static WpfConfig Create(
			bool isEnabledMovieMarker, bool isEnabledOldMarker,
			CatalogNgImage catalogNgImage, ThreadDelResVisibility threadDelResVisibility,
			int clipbordJpegQuality, bool clipbordIsEnabledUrl,
			string[] mediaExportPath, int cacheExpireDay,
			ExportNgRes exportNgRes, ExportNgImage exportNgImage,
			string browserPath) {

			System.Diagnostics.Debug.Assert(catalogNgImage <= CatalogNgImage.MaxValue);
			System.Diagnostics.Debug.Assert(threadDelResVisibility <= ThreadDelResVisibility.MaxValue);
			System.Diagnostics.Debug.Assert(mediaExportPath != null);
			System.Diagnostics.Debug.Assert((0 <= cacheExpireDay) && (cacheExpireDay <= 100));
			System.Diagnostics.Debug.Assert(browserPath != null);

			return new WpfConfig() {
				Version = CurrentVersion,
				IsEnabledMovieMarker = isEnabledMovieMarker,
				IsEnabledOldMarker = isEnabledOldMarker,
				CatalogNgImage = catalogNgImage,
				ThreadDelResVisibility = threadDelResVisibility,
				ClipbordJpegQuality = clipbordJpegQuality,
				ClipbordIsEnabledUrl = clipbordIsEnabledUrl,
				MediaExportPath = mediaExportPath,
				CacheExpireDay = cacheExpireDay,
				ExportNgRes = exportNgRes,
				ExportNgImage = exportNgImage,
				BrowserPath = browserPath,
			};
		}
		*/
	}

}
