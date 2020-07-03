using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yarukizero.Net.MakiMoki.Wpf.PlatformData {

	class MakiMokiExeConfig : Data.ConfigObject {
		public static int CurrentVersion { get; } = 2020062900;

		[JsonProperty("single-user-data", Required = Required.AllowNull)]
		public bool? IsSingleUserData { get; private set; }

		[JsonProperty("single-user-config", Required = Required.AllowNull)]
		public bool? IsSingleUserConfig { get; private set; }

		[JsonProperty("custom-data-path", Required = Required.AllowNull)]
		public string CustomDataPathRoot { get; private set; }

		[JsonProperty("custom-config-path", Required =Required.AllowNull)]
		public string CustomConfigPathRoot { get; private set; }
	}

	class WpfConfig : Data.ConfigObject {
		public static int CurrentVersion { get; } = 2020070500;

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

		[JsonProperty("post-save-subject", Required = Required.Always)]
		public bool IsSavedPostSubject { get; private set; }
		[JsonProperty("post-save-name", Required = Required.Always)]
		public bool IsSavedPostName { get; private set; }
		[JsonProperty("post-save-mail", Required = Required.Always)]
		public bool IsSavedPostMail { get; private set; }

		[JsonProperty("post-view-min-width", Required = Required.Always)]
		public int MinWidthPostView { get; private set; }

		[JsonProperty("post-view-max-width", Required = Required.Always)]
		public int MaxWidthPostView { get; private set; }

		[JsonProperty("post-view-enable-opacity", Required = Required.Always)]
		public bool IsEnabledOpacityPostView { get; private set; }

		[JsonProperty("post-view-enable-opacity-value", Required = Required.Always)]
		public int OpacityPostView { get; private set; }

		public static WpfConfig CreateDefault() {
			// ここは使われない
			return new WpfConfig() {
				Version = CurrentVersion,
			};
		}

		public static WpfConfig Create(
			bool isEnabledMovieMarker, bool isEnabledOldMarker,
			CatalogNgImage catalogNgImage, ThreadDelResVisibility threadDelResVisibility,
			bool isVisibleCatalogIsolateThread, CatalogSearchResult catalogSearchResult,
			bool isSavedPostSubject, bool isSavedPostName, bool isSavedPostMail,
			int clipbordJpegQuality, bool clipbordIsEnabledUrl,
			int minWidthPostView, int maxWidthPostView, bool isEnabledOpacityPostView, int opacityPostView,
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
				IsVisibleCatalogIsolateThread = isVisibleCatalogIsolateThread,
				CatalogSearchResult = catalogSearchResult,
				ThreadDelResVisibility = threadDelResVisibility,
				ClipbordJpegQuality = clipbordJpegQuality,
				ClipbordIsEnabledUrl = clipbordIsEnabledUrl,
				IsSavedPostSubject = isSavedPostSubject,
				IsSavedPostName = isSavedPostName,
				IsSavedPostMail = isSavedPostMail,
				MinWidthPostView = minWidthPostView,
				MaxWidthPostView = maxWidthPostView,
				IsEnabledOpacityPostView = isEnabledOpacityPostView,
				OpacityPostView = opacityPostView,
				MediaExportPath = mediaExportPath,
				CacheExpireDay = cacheExpireDay,
				ExportNgRes = exportNgRes,
				ExportNgImage = exportNgImage,
				BrowserPath = browserPath,
			};
		}
	}

	enum CatalogSearchResult {
		Default,
		Nijiran,
		MaxValue = Nijiran,
	}

	enum CatalogNgImage {
		Default,
		Hidden,
		MaxValue = Hidden,
	}

	enum ThreadDelResVisibility {
		Visible,
		Hidden,
		MaxValue = Hidden,
	}

	enum ExportNgRes {
		Output,
		Mask,
		Hidden,
	}

	enum ExportNgImage {
		Output,
		Dummy,
		Hidden,
	}

	class PlacementConfig : Data.ConfigObject {
		public static int CurrentVersion { get; } = 2020062900;

		[JsonProperty("window-placement", Required = Required.Always)]
		public WinApi.WINDOWPLACEMENT WindowPlacement { get; internal set; }

		public static PlacementConfig CreateDefault() {
			return new PlacementConfig() {
				Version = CurrentVersion,
				WindowPlacement = default,
			};
		}
	}
}
