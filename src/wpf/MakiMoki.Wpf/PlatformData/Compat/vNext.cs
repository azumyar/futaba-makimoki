using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yarukizero.Net.MakiMoki.Wpf.PlatformData.Compat {
	class WpfConfig2020102900 : Data.ConfigObject, Data.IMigrateCompatObject {
		public static int CurrentVersion { get; } = 2020102900;

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


		// 2020070500
		[JsonProperty("catalog-search-result-type", Required = Required.Always)]
		public CatalogSearchResult CatalogSearchResult { get; private set; }

		[JsonProperty("catalog-visible-isolate-thread", Required = Required.Always)]
		public bool IsVisibleCatalogIsolateThread { get; private set; }

		[JsonProperty("post-view-min-width", Required = Required.Always)]
		public int MinWidthPostView { get; private set; }

		[JsonProperty("post-view-max-width", Required = Required.Always)]
		public int MaxWidthPostView { get; private set; }

		[JsonProperty("post-view-enable-opacity", Required = Required.Always)]
		public bool IsEnabledOpacityPostView { get; private set; }

		[JsonProperty("post-view-enable-opacity-value", Required = Required.Always)]
		public int OpacityPostView { get; private set; }

		// 2020071900
		[JsonProperty("thread-enable-quot-link", Required = Required.Always)]
		public bool IsEnabledQuotLink { get; private set; }

		[JsonProperty("platform-window-topmost", Required = Required.Always)]
		public bool IsEnabledWindowTopmost { get; private set; }

		[JsonProperty("platform-ng-reason-input", Required = Required.Always)]
		public bool IsEnabledNgReasonInput { get; private set; }


		// 2020102900
		[JsonProperty("platform-window-theme", Required = Required.Always)]
		public WindowTheme WindowTheme { get; private set; }
		[JsonProperty("catalog-enable-id-marker", Required = Required.Always)]
		public bool IsEnabledIdMarker { get; private set; }
		[JsonProperty("thread-enable-command-palette", Required = Required.Always)]
		public bool IsEnabledThreadCommandPalette { get; private set; }

		public Data.ConfigObject Migrate() {
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
				catalogSearchResult: CatalogSearchResult,
				isVisibleCatalogIsolateThread: IsVisibleCatalogIsolateThread,
				minWidthPostView: MinWidthPostView,
				maxWidthPostView: MaxWidthPostView,
				isEnabledOpacityPostView: IsEnabledOpacityPostView,
				opacityPostView: OpacityPostView,
				isEnabledQuotLink: IsEnabledQuotLink,
				windowTopmost: IsEnabledWindowTopmost,
				ngResonInput: IsEnabledNgReasonInput,
				windowTheme: WindowTheme,
				isEnabledIdMarker: IsEnabledIdMarker,
				isEnabledThreadCommandPalette: IsEnabledThreadCommandPalette,

				// vNext
				isEnabledFetchThumbnail: conf.IsEnabledFetchThumbnail,
				commandPalettePosition: conf.CommandPalettePosition,
				canvas98Position: conf.Canvas98Position
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
			WindowTheme windowTheme,
			bool isEnabledMovieMarker, bool isEnabledIdMarker, bool isEnabledOldMarker,
			CatalogNgImage catalogNgImage, ThreadDelResVisibility threadDelResVisibility, bool isEnabledQuotLink,
			bool isVisibleCatalogIsolateThread, CatalogSearchResult catalogSearchResult,
			bool isEnabledThreadCommandPalette,
			int clipbordJpegQuality, bool clipbordIsEnabledUrl,
			int minWidthPostView, int maxWidthPostView, bool isEnabledOpacityPostView, int opacityPostView,
			string[] mediaExportPath, int cacheExpireDay,
			ExportNgRes exportNgRes, ExportNgImage exportNgImage,
			bool windowTopmost, bool ngResonInput, string browserPath) {

			System.Diagnostics.Debug.Assert(catalogNgImage <= CatalogNgImage.MaxValue);
			System.Diagnostics.Debug.Assert(threadDelResVisibility <= ThreadDelResVisibility.MaxValue);
			System.Diagnostics.Debug.Assert(mediaExportPath != null);
			System.Diagnostics.Debug.Assert((0 <= cacheExpireDay) && (cacheExpireDay <= 100));
			System.Diagnostics.Debug.Assert(browserPath != null);

			return new WpfConfig() {
				Version = CurrentVersion,
				WindowTheme = windowTheme,
				IsEnabledMovieMarker = isEnabledMovieMarker,
				IsEnabledIdMarker = isEnabledIdMarker,
				IsEnabledOldMarker = isEnabledOldMarker,
				CatalogNgImage = catalogNgImage,
				IsVisibleCatalogIsolateThread = isVisibleCatalogIsolateThread,
				CatalogSearchResult = catalogSearchResult,
				ThreadDelResVisibility = threadDelResVisibility,
				IsEnabledThreadCommandPalette = isEnabledThreadCommandPalette,
				IsEnabledQuotLink = isEnabledQuotLink,
				ClipbordJpegQuality = clipbordJpegQuality,
				ClipbordIsEnabledUrl = clipbordIsEnabledUrl,
				MinWidthPostView = minWidthPostView,
				MaxWidthPostView = maxWidthPostView,
				IsEnabledOpacityPostView = isEnabledOpacityPostView,
				OpacityPostView = opacityPostView,
				MediaExportPath = mediaExportPath,
				CacheExpireDay = cacheExpireDay,
				ExportNgRes = exportNgRes,
				ExportNgImage = exportNgImage,
				IsEnabledWindowTopmost = windowTopmost,
				IsEnabledNgReasonInput = ngResonInput,
				BrowserPath = browserPath,
			};
		}
		*/
	}

}
