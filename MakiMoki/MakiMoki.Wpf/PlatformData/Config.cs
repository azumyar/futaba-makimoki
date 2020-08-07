using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
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

		[JsonProperty("custom-config-path", Required =Required.AllowNull)]
		public string CustomConfigPathRoot { get; private set; }
	}

	class WpfConfig : Data.ConfigObject {
		public static int CurrentVersion { get; } = 2020071900;

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

		public static WpfConfig CreateDefault() {
			// ここは使われない
			return new WpfConfig() {
				Version = CurrentVersion,
			};
		}

		public static WpfConfig Create(
			bool isEnabledMovieMarker, bool isEnabledOldMarker,
			CatalogNgImage catalogNgImage, ThreadDelResVisibility threadDelResVisibility, bool isEnabledQuotLink,
			bool isVisibleCatalogIsolateThread, CatalogSearchResult catalogSearchResult,
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
				IsEnabledMovieMarker = isEnabledMovieMarker,
				IsEnabledOldMarker = isEnabledOldMarker,
				CatalogNgImage = catalogNgImage,
				IsVisibleCatalogIsolateThread = isVisibleCatalogIsolateThread,
				CatalogSearchResult = catalogSearchResult,
				ThreadDelResVisibility = threadDelResVisibility,
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
		[JsonProperty("color-viewer-catalog-item-background-opend")]
		public string CatalogItemOpendBackgroundColor { get; private set; }
		[JsonProperty("color-viewer-catalog-badge-count-foreground")]
		public string CatalogBadgeCountForegroundColor { get; private set; }
		[JsonProperty("color-viewer-catalog-badge-count-background")]
		public string CatalogBadgeCountBackgroundColor { get; private set; }
		//[JsonProperty("color-viewer-catalog-badge-old-foreground")]
		//public string CatalogBadgeOldForegroundColor { get; private set; }
		//[JsonProperty("color-viewer-catalog-badge-old-background")]
		//public string CatalogBadgeOldBackgroundColor { get; private set; }
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
}
