using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Yarukizero.Net.MakiMoki.Wpf.PlatformData {

	class MakiMokiExeConfig : Data.ConfigObject {
		public static int CurrentVersion { get; } = 2020062900;

		[JsonProperty("single-user-data", Required = Required.AllowNull)]
		public bool? IsSingleUserData { get; private set; }

		[JsonProperty("single-user-config", Required = Required.AllowNull)]
		public bool? IsSingleUserConfig { get; private set; }

		[JsonProperty("custom-data-path", Required = Required.AllowNull)]
		public string CustomDataPathRoot { get; private set; }

		[JsonProperty("custom-config-path", Required = Required.AllowNull)]
		public string CustomConfigPathRoot { get; private set; }
	}

	class WpfConfig : Data.ConfigObject {
		public static int CurrentVersion { get; } = 2021012000;

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

		// vNext
		[JsonProperty("catalog-enable-fetch-thumbnail", Required = Required.Always)]
		public bool IsEnabledFetchThumbnail { get; private set; }

		[JsonProperty("thread-command-palette-position", Required = Required.Always)]
		public UiPosition CommandPalettePosition { get; private set; }

		[JsonProperty("thread-canvas98-position", Required = Required.Always)]
		public UiPosition Canvas98Position { get; private set; }

		public static WpfConfig CreateDefault() {
			// ここは使われない
			return new WpfConfig() {
				Version = CurrentVersion,
			};
		}

		public static WpfConfig Create(
			WindowTheme windowTheme,
			bool isEnabledFetchThumbnail,
			bool isEnabledMovieMarker, bool isEnabledIdMarker, bool isEnabledOldMarker,
			CatalogNgImage catalogNgImage, ThreadDelResVisibility threadDelResVisibility, bool isEnabledQuotLink,
			bool isVisibleCatalogIsolateThread, CatalogSearchResult catalogSearchResult,
			bool isEnabledThreadCommandPalette, UiPosition commandPalettePosition,
			UiPosition canvas98Position,
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
			System.Diagnostics.Debug.Assert(new[] { UiPosition.Left, UiPosition.Right }.Contains(commandPalettePosition));
			System.Diagnostics.Debug.Assert(new[] { UiPosition.Default, UiPosition.Right, UiPosition.Bottom }.Contains(canvas98Position));

			return new WpfConfig() {
				Version = CurrentVersion,
				WindowTheme = windowTheme,
				IsEnabledFetchThumbnail = isEnabledFetchThumbnail,
				IsEnabledMovieMarker = isEnabledMovieMarker,
				IsEnabledIdMarker = isEnabledIdMarker,
				IsEnabledOldMarker = isEnabledOldMarker,
				CatalogNgImage = catalogNgImage,
				IsVisibleCatalogIsolateThread = isVisibleCatalogIsolateThread,
				CatalogSearchResult = catalogSearchResult,
				ThreadDelResVisibility = threadDelResVisibility,
				IsEnabledThreadCommandPalette = isEnabledThreadCommandPalette,
				CommandPalettePosition = commandPalettePosition,
				Canvas98Position = canvas98Position,
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
	}

	enum WindowTheme {
		Light,
		Dark,
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

	enum UiPosition {
		Default = 0,
		Left,
		Top,
		Right,
		Bottom
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

	public class StyleConfig : Data.ConfigObject {
		public static int CurrentVersion { get; } = -1;

		[JsonProperty("style-type", Required = Required.Always)]
		public StyleType StyleType { get; private set; }

		[JsonProperty("color-white", Required = Required.Always)]
		public string WhiteColor { get; private set; }
		[JsonProperty("color-black", Required = Required.Always)]
		public string BlackColor { get; private set; }
		[JsonProperty("color-primary", Required = Required.Always)]
		public string PrimaryColor { get; private set; }
		[JsonProperty("color-secondary", Required = Required.Always)]
		public string SecondaryColor { get; private set; }
		[JsonProperty("color-window-frame", Required = Required.Always)]
		public string WindowFrameColor { get; private set; }
		[JsonProperty("color-window-tab", Required = Required.Always)]
		public string WindowTabColor { get; private set; }
		[JsonProperty("color-window-tab-badge")]
		public string WindowTabBadgeColor { get; private set; }
		[JsonProperty("color-viewer-foreground", Required = Required.Always)]
		public string ViewerForegroundColor { get; private set; }
		[JsonProperty("color-viewer-background", Required = Required.Always)]
		public string ViewerBackgroundColor { get; private set; }
		[JsonProperty("color-viewer-border", Required = Required.Always)]
		public string ViewerBorderColor { get; private set; }

		[JsonProperty("color-viewer-catalog-item-background")]
		public string CatalogItemBackgroundColor { get; private set; }
		[JsonProperty("color-viewer-catalog-item-background-search-hit")]
		public string CatalogItemSearchHitBackgroundColor { get; private set; }
		[JsonProperty("color-viewer-catalog-item-background-watch-hit")]
		public string CatalogItemWatchHitBackgroundColor { get; private set; }
		[JsonProperty("color-viewer-catalog-item-background-opend")]
		public string CatalogItemOpendBackgroundColor { get; private set; }
		[JsonProperty("color-viewer-catalog-badge-count-foreground")]
		public string CatalogBadgeCountForegroundColor { get; private set; }
		[JsonProperty("color-viewer-catalog-badge-count-background")]
		public string CatalogBadgeCountBackgroundColor { get; private set; }
		[JsonProperty("color-viewer-catalog-badge-id-foreground")]
		public string CatalogBadgeIdForegroundColor { get; private set; }
		[JsonProperty("color-viewer-catalog-badge-id-background")]
		public string CatalogBadgeIdBackgroundColor { get; private set; }
		[JsonProperty("color-viewer-catalog-badge-old-foreground")]
		public string CatalogBadgeOldForegroundColor { get; private set; }
		[JsonProperty("color-viewer-catalog-badge-old-background")]
		public string CatalogBadgeOldBackgroundColor { get; private set; }
		[JsonProperty("color-viewer-catalog-badge-movie-foreground")]
		public string CatalogBadgeMovieForegroundColor { get; private set; }
		[JsonProperty("color-viewer-catalog-badge-movie-background")]
		public string CatalogBadgeMovieBackgroundColor { get; private set; }
		[JsonProperty("color-viewer-catalog-badge-isolate-foreground")]
		public string CatalogBadgeIsolateForegroundColor { get; private set; }
		[JsonProperty("color-viewer-catalog-badge-isolate-background")]
		public string CatalogBadgeIsolateBackgroundColor { get; private set; }

		[JsonProperty("color-viewer-thread-background")]
		public string ThreadBackgroundColor { get; private set; }
		[JsonProperty("color-viewer-thread-search-hit-background")]
		public string ThreadSearchHitBackgroundColor { get; private set; }
		[JsonProperty("color-viewer-thread-quot-hit-background")]
		public string ThreadQuotHitBackgroundColor { get; private set; }
		[JsonProperty("color-viewer-thread-old-foreground")]
		public string ThreadOldForegroundColor { get; private set; }
		[JsonProperty("color-viewer-thread-header-posted-foreground")]
		public string ThreadHeaerPostedForegroundColor { get; private set; }
		[JsonProperty("color-viewer-thread-header-subject-foreground")]
		public string ThreadHeaerSubjectForegroundColor { get; private set; }
		[JsonProperty("color-viewer-thread-header-name-foreground")]
		public string ThreadHeaerNameForegroundColor { get; private set; }
		[JsonProperty("color-viewer-thread-header-mail-foreground")]
		public string ThreadHeaerMailForegroundColor { get; private set; }
		[JsonProperty("color-viewer-thread-header-soudane-foreground")]
		public string ThreadHeaerSoudaneForegroundColor { get; private set; }

		[JsonProperty("color-viewer-scrollbar-thumb", Required = Required.Always)]
		public string ViewerScollbarThumbColor { get; private set; }
		[JsonProperty("color-viewer-scrollbar-tarck", Required = Required.Always)]
		public string ViewerScollbarTrackColor { get; private set; }

		[JsonProperty("color-viewer-map-futaba")]
		public Dictionary<string, string> ViewerFutabaColorMap { get; private set; }

		[JsonProperty("size-catalog-image", Required = Required.Always)]
		public double CatalogImageSize { get; private set; }
		[JsonProperty("size-catalog-text", Required = Required.Always)]
		public double CatalogTextSize { get; private set; }
		[JsonProperty("size-catalog-bade", Required = Required.Always)]
		public double CatalogBadgeSize { get; private set; }
		[JsonProperty("size-catalog-text-font", Required = Required.Always)]
		public double CatalogTextFontSize { get; private set; }
		[JsonProperty("size-thread-header-font", Required = Required.Always)]
		public double ThreadHeaderFontSize { get; private set; }
		[JsonProperty("size-thread-text-font", Required = Required.Always)]
		public double ThreadTextFontSize { get; private set; }
		[JsonProperty("size-post-font", Required = Required.Always)]
		public double PostFontSize { get; private set; }


		[JsonProperty("font-catalog", Required = Required.Always)]
		public string CatalogFont { get; private set; }
		[JsonProperty("font-catalog-badge", Required = Required.Always)]
		public string CatalogBadgeFont { get; private set; }
		[JsonProperty("font-thread-header", Required = Required.Always)]
		public string ThreadHeaderFont { get; private set; }
		[JsonProperty("font-thread-text", Required = Required.Always)]
		public string ThreadTextFont { get; private set; }
		[JsonProperty("font-post", Required = Required.Always)]
		public string PostFont { get; private set; }



		public (bool Successed, string ErrorText) Validate() {
			if(!((this.StyleType == StyleType.Light) || (this.StyleType == StyleType.Dark))) {
				return (false, $"{ nameof(StyleType) }が不正です。");
			}

			var ps = this.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
			foreach(var p in ps.Where(x => x.CanRead && (x.PropertyType == typeof(string)) && x.Name.EndsWith("Color"))) {
				try {
					if(p.GetValue(this) is string s) {
						this.ToWpfColor(s);
					} else {
						return (false, $"{ p.Name }が不正です。");
					}
				}
				catch(InvalidOperationException) {
					return (false, $"{ p.Name }が不正です。");
				}
			}

			foreach(var p in ps.Where(x => x.CanRead && (x.PropertyType == typeof(string)) && x.Name.EndsWith("Font"))) {
				if(p.GetValue(this) is string s) {
					var f = new FontFamily(s);
					if(f.FamilyTypefaces.Count == 0) {
						return (false, $"{ p.Name }が不正です。");
					}
				} else {
					return (false, $"{ p.Name }が不正です。");
				}
			}

			return (true, "");
		}

		public Color ToWpfColor(string hex) {
			if(hex[0] != '#') {
				throw new InvalidOperationException();
			}

			var u = Convert.ToUInt32(hex.Substring(1), 16);
			switch(hex.Length) {
			case 7:
				return Color.FromRgb((byte)(u >> 16 & 0xff), (byte)(u >> 8 & 0xff), (byte)(u & 0xff));
			case 9:
				return Color.FromArgb((byte)(u >> 24 & 0xff), (byte)(u >> 16 & 0xff), (byte)(u >> 8 & 0xff), (byte)(u & 0xff));
			}
			throw new InvalidOperationException();
		}

		public Color GetSubColor(Color baseColor, StyleType? type = null) {
			return WpfUtil.ImageUtil.GetMaterialSubColor(baseColor, type ?? this.StyleType);
		}

		public Color GetTextColor(Color background, Color white, Color black) {
			return WpfUtil.ImageUtil.GetTextColor(background, white, black, this.StyleType);
		}

		public static StyleConfig CreateDefault() {
			// ここは使われない
			return new StyleConfig() {
				Version = CurrentVersion,
			};
		}
	}

	public enum StyleType {
		Light,
		Dark,
	}

	public class ColorMapCollection : System.Collections.ObjectModel.ObservableCollection<ColorMap> {
		public ColorMapCollection() : base() { }
		public ColorMapCollection(List<ColorMap> list) : base(list) { }
		public ColorMapCollection(IEnumerable<ColorMap> collection) : base(collection) { }
	}

	public class ColorMap {
		public Color Target { get; set; }
		public Color Value { get; set; }
	}

	public class GestureConfig : Data.ConfigObject {
		public static int CurrentVersion { get; } = -1;

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

		public static GestureConfig CreateDefault() {
			return new GestureConfig() {
				Version = CurrentVersion,

				KeyGestureCatalogUpdate = Array.Empty<string>(),
				KeyGestureCatalogSearch = Array.Empty<string>(),
				KeyGestureCatalogModeToggleUpdate = Array.Empty<string>(),
				KeyGestureCatalogOpenPost = Array.Empty<string>(),
				KeyGestureCatalogClose = Array.Empty<string>(),
				KeyGestureCatalogNext = Array.Empty<string>(),
				KeyGestureCatalogPrevious = Array.Empty<string>(),

				KeyGestureThreadUpdate = Array.Empty<string>(),
				KeyGestureThreadSearch = Array.Empty<string>(),
				KeyGestureThreadOpenTegaki = Array.Empty<string>(),
				KeyGestureThreadOpenPost = Array.Empty<string>(),
				KeyGestureThreadTabClose = Array.Empty<string>(),
				KeyGestureThreadTabNext = Array.Empty<string>(),
				KeyGestureThreadTabPrevious = Array.Empty<string>(),

				KeyGesturePostViewPost = Array.Empty<string>(),
				KeyGesturePostViewOpenImage = Array.Empty<string>(),
				KeyGesturePostViewOpenUploader = Array.Empty<string>(),
				KeyGesturePostViewDelete = Array.Empty<string>(),
				KeyGesturePostViewClose = Array.Empty<string>(),
				KeyGesturePostViewPasteImage = Array.Empty<string>(),
				KeyGesturePostViewPasteUploader = Array.Empty<string>(),
			};
		}

		public static GestureConfig From(
			string[] keyGestureCatalogUpdate,
			string[] keyGestureCatalogSearch,
			string[] keyGestureCatalogModeToggleUpdate,
			string[] keyGestureCatalogOpenPost,
			string[] keyGestureCatalogClose,
			string[] keyGestureCatalogNext,
			string[] keyGestureCatalogPrevious,

			string[] keyGestureThreadUpdate,
			string[] keyGestureThreadSearch,
			string[] keyGestureThreadOpenTegaki,
			string[] keyGestureThreadOpenPost,
			string[] keyGestureThreadTabClose,
			string[] keyGestureThreadTabNext,
			string[] keyGestureThreadTabPrevious,

			string[] keyGesturePostViewPost,
			string[] keyGesturePostViewOpenImage,
			string[] keyGesturePostViewOpenUploader,
			string[] keyGesturePostViewDelete,
			string[] keyGesturePostViewClose,
			string[] keyGesturePostViewPasteImage,
			string[] keyGesturePostViewPasteUploader
			) {

			System.Diagnostics.Debug.Assert(keyGestureCatalogUpdate != null);
			System.Diagnostics.Debug.Assert(keyGestureCatalogSearch != null);
			System.Diagnostics.Debug.Assert(keyGestureCatalogModeToggleUpdate != null);
			System.Diagnostics.Debug.Assert(keyGestureCatalogOpenPost != null);
			System.Diagnostics.Debug.Assert(keyGestureCatalogClose != null);
			System.Diagnostics.Debug.Assert(keyGestureCatalogNext != null);
			System.Diagnostics.Debug.Assert(keyGestureCatalogPrevious != null);

			System.Diagnostics.Debug.Assert(keyGestureThreadUpdate != null);
			System.Diagnostics.Debug.Assert(keyGestureThreadSearch != null);
			System.Diagnostics.Debug.Assert(keyGestureThreadOpenTegaki != null);
			System.Diagnostics.Debug.Assert(keyGestureThreadOpenPost != null);
			System.Diagnostics.Debug.Assert(keyGestureThreadTabClose != null);
			System.Diagnostics.Debug.Assert(keyGestureThreadTabNext != null);
			System.Diagnostics.Debug.Assert(keyGestureThreadTabPrevious != null);

			System.Diagnostics.Debug.Assert(keyGesturePostViewPost != null);
			System.Diagnostics.Debug.Assert(keyGesturePostViewOpenImage != null);
			System.Diagnostics.Debug.Assert(keyGesturePostViewOpenUploader != null);
			System.Diagnostics.Debug.Assert(keyGesturePostViewDelete != null);
			System.Diagnostics.Debug.Assert(keyGesturePostViewClose != null);
			System.Diagnostics.Debug.Assert(keyGesturePostViewPasteImage != null);
			System.Diagnostics.Debug.Assert(keyGesturePostViewPasteUploader != null);

			return new GestureConfig() {
				Version = CurrentVersion,

				KeyGestureCatalogUpdate = keyGestureCatalogUpdate,
				KeyGestureCatalogSearch = keyGestureCatalogSearch,
				KeyGestureCatalogModeToggleUpdate = keyGestureCatalogModeToggleUpdate,
				KeyGestureCatalogOpenPost = keyGestureCatalogOpenPost,
				KeyGestureCatalogClose = keyGestureCatalogClose,
				KeyGestureCatalogNext = keyGestureCatalogNext,
				KeyGestureCatalogPrevious = keyGestureCatalogPrevious,

				KeyGestureThreadUpdate = keyGestureThreadUpdate,
				KeyGestureThreadSearch = keyGestureThreadSearch,
				KeyGestureThreadOpenTegaki = keyGestureThreadOpenTegaki,
				KeyGestureThreadOpenPost = keyGestureThreadOpenPost,
				KeyGestureThreadTabClose = keyGestureThreadTabClose,
				KeyGestureThreadTabNext = keyGestureThreadTabNext,
				KeyGestureThreadTabPrevious = keyGestureThreadTabPrevious,

				KeyGesturePostViewPost = keyGesturePostViewPost,
				KeyGesturePostViewOpenImage = keyGesturePostViewOpenImage,
				KeyGesturePostViewOpenUploader = keyGesturePostViewOpenUploader,
				KeyGesturePostViewDelete = keyGesturePostViewDelete,
				KeyGesturePostViewClose = keyGesturePostViewClose,
				KeyGesturePostViewPasteImage = keyGesturePostViewPasteImage,
				KeyGesturePostViewPasteUploader = keyGesturePostViewPasteUploader,
			};
		}
	}
}