using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
		public static int CurrentVersion { get; } = 2021020100;

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

		[Obsolete] // 定義を変えるとめどいのでなんか増やすまで残しておく
		[JsonProperty("post-view-min-width", Required = Required.Always)]
		public int MinWidthPostView { get; private set; }

		[JsonProperty("post-view-max-width", Required = Required.Always)]
		public int MaxWidthPostView { get; private set; }

		[JsonProperty("post-view-enable-opacity", Required = Required.Always)]
		public bool IsEnabledOpacityPostView { get; private set; }

		[JsonProperty("post-view-enable-opacity-value", Required = Required.Always)]
		public int OpacityPostView { get; private set; }

		// 2020071900
		[Obsolete] // 定義を変えるとめどいのでなんか増やすまで残しておく
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

		// 2021012000
		[JsonProperty("catalog-enable-fetch-thumbnail", Required = Required.Always)]
		public bool IsEnabledFetchThumbnail { get; private set; }

		[JsonProperty("thread-command-palette-position", Required = Required.Always)]
		public UiPosition CommandPalettePosition { get; private set; }

		[JsonProperty("thread-canvas98-position", Required = Required.Always)]
		public UiPosition Canvas98Position { get; private set; }

		// 2021020100
		[JsonProperty("thread-enable-failsafe-mistake-post", Required = Required.Always)]
		public bool IsEnabledFailsafeMistakePost { get; private set; }

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
			bool isEnabledFailsafeMistakePost,
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
				IsEnabledFailsafeMistakePost = isEnabledFailsafeMistakePost,
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

	enum FluentType {
		None,
		Auto,
		Aero,
		Acryic,
		Maica
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

	class StyleConfig : Data.ConfigObject {
		public class WeightConverter : JsonConverter {
			public override bool CanConvert(Type objectType) {
				return typeof(FontWeight) == objectType;
			}

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
				switch(reader.TokenType) {
				case JsonToken.Null:
					return FontWeights.Normal;
				case JsonToken.String:
				case JsonToken.Integer:
					return reader.Value switch {
						string s when s.ToLower() == "Thin".ToLower() => FontWeights.Thin,
						string s when s.ToLower() == "ExtraLight".ToLower() => FontWeights.ExtraLight,
						string s when s.ToLower() == "UltraLight".ToLower() => FontWeights.UltraLight,
						string s when s.ToLower() == "Light".ToLower() => FontWeights.Light,
						string s when s.ToLower() == "Normal".ToLower() => FontWeights.Normal,
						string s when s.ToLower() == "Regular".ToLower() => FontWeights.Regular,
						string s when s.ToLower() == "Medium".ToLower() => FontWeights.Medium,
						string s when s.ToLower() == "DemiBold".ToLower() => FontWeights.DemiBold,
						string s when s.ToLower() == "SemiBold".ToLower() => FontWeights.SemiBold,
						string s when s.ToLower() == "Bold".ToLower() => FontWeights.Bold,
						string s when s.ToLower() == "ExtraBold".ToLower() => FontWeights.ExtraBold,
						string s when s.ToLower() == "UltraBold".ToLower() => FontWeights.UltraBold,
						string s when s.ToLower() == "Black".ToLower() => FontWeights.Black,
						string s when s.ToLower() == "Heavy".ToLower() => FontWeights.Heavy,
						string s when s.ToLower() == "ExtraBlack".ToLower() => FontWeights.ExtraBlack,
						string s when s.ToLower() == "UltraBlack".ToLower() => FontWeights.UltraBlack,

						int i when(1 <= i) && (i <= 100) => FontWeights.Thin,
						int i when i <= 200 => FontWeights.ExtraLight,
						int i when i <= 300 => FontWeights.Light,
						int i when i <= 400 => FontWeights.Normal,
						int i when i <= 500 => FontWeights.Medium,
						int i when i <= 600 => FontWeights.SemiBold,
						int i when i <= 700 => FontWeights.Bold,
						int i when i <= 800 => FontWeights.ExtraBold,
						int i when i <= 900 => FontWeights.Black,
						int i when i <= 950 => FontWeights.ExtraBlack,
						int i when i <= 999 => FontWeights.ExtraBlack,
						_ => throw new JsonReaderException()
					};
				default:
					throw new JsonReaderException();
				}
			}

			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
				if(value is FontWeight w) {
					serializer.Serialize(writer, w.ToOpenTypeWeight());
				} else {
					serializer.Serialize(writer, 400);
				}
			}
		}
		public static int CurrentVersion { get; } = -1;

		[JsonProperty("color-white", Required = Required.Always)]
		public string WhiteColor { get; private set; }
		[JsonProperty("color-black", Required = Required.Always)]
		public string BlackColor { get; private set; }
		[JsonProperty("color-foreground", Required = Required.Always)]
		public string ForegroundColor { get; private set; }
		[JsonProperty("color-background", Required = Required.Always)]
		public string BackgroundColor { get; private set; }
		[JsonProperty("color-primary", Required = Required.Always)]
		public string PrimaryColor { get; private set; }
		[JsonProperty("color-secondary", Required = Required.Always)]
		public string SecondaryColor { get; private set; }
		[JsonProperty("color-window-frame", Required = Required.Always)]
		public string WindowFrameColor { get; private set; }
		[JsonProperty("color-window-tab", Required = Required.Always)]
		public string WindowTabColor { get; private set; }
		[JsonProperty("color-window-tab-badge", Required = Required.Always)]
		public string WindowTabBadgeColor { get; private set; }
		[JsonProperty("color-viewer-foreground", Required = Required.Always)]
		public string ViewerForegroundColor { get; private set; }
		[JsonProperty("color-viewer-background", Required = Required.Always)]
		public string ViewerBackgroundColor { get; private set; }
		[JsonProperty("color-viewer-border", Required = Required.Always)]
		public string ViewerBorderColor { get; private set; }

		[JsonProperty("color-viewer-catalog-item-background", Required = Required.Always)]
		public string CatalogItemBackgroundColor { get; private set; }
		[JsonProperty("color-viewer-catalog-item-background-search-hit", Required = Required.Always)]
		public string CatalogItemSearchHitBackgroundColor { get; private set; }
		[JsonProperty("color-viewer-catalog-item-background-watch-hit", Required = Required.Always)]
		public string CatalogItemWatchHitBackgroundColor { get; private set; }
		[JsonProperty("color-viewer-catalog-item-background-opend", Required = Required.Always)]
		public string CatalogItemOpendBackgroundColor { get; private set; }
		[JsonProperty("color-viewer-catalog-badge-count-foreground", Required = Required.Always)]
		public string CatalogBadgeCountForegroundColor { get; private set; }
		[JsonProperty("color-viewer-catalog-badge-count-background", Required = Required.Always)]
		public string CatalogBadgeCountBackgroundColor { get; private set; }
		[JsonProperty("color-viewer-catalog-badge-id-foreground", Required = Required.Always)]
		public string CatalogBadgeIdForegroundColor { get; private set; }
		[JsonProperty("color-viewer-catalog-badge-id-background", Required = Required.Always)]
		public string CatalogBadgeIdBackgroundColor { get; private set; }
		[JsonProperty("color-viewer-catalog-badge-old-foreground", Required = Required.Always)]
		public string CatalogBadgeOldForegroundColor { get; private set; }
		[JsonProperty("color-viewer-catalog-badge-old-background", Required = Required.Always)]
		public string CatalogBadgeOldBackgroundColor { get; private set; }
		[JsonProperty("color-viewer-catalog-badge-movie-foreground", Required = Required.Always)]
		public string CatalogBadgeMovieForegroundColor { get; private set; }
		[JsonProperty("color-viewer-catalog-badge-movie-background", Required = Required.Always)]
		public string CatalogBadgeMovieBackgroundColor { get; private set; }
		[JsonProperty("color-viewer-catalog-badge-isolate-foreground", Required = Required.Always)]
		public string CatalogBadgeIsolateForegroundColor { get; private set; }
		[JsonProperty("color-viewer-catalog-badge-isolate-background", Required = Required.Always)]
		public string CatalogBadgeIsolateBackgroundColor { get; private set; }

		[JsonProperty("color-viewer-thread-background", Required = Required.Always)]
		public string ThreadBackgroundColor { get; private set; }
		[JsonProperty("color-viewer-thread-link", Required = Required.Always)]
		public string ThreadLinkColor { get; private set; }
		[JsonProperty("color-viewer-thread-search-hit-background", Required = Required.Always)]
		public string ThreadSearchHitBackgroundColor { get; private set; }
		[JsonProperty("color-viewer-thread-quot-hit-background", Required = Required.Always)]
		public string ThreadQuotHitBackgroundColor { get; private set; }
		[JsonProperty("color-viewer-thread-old-foreground", Required = Required.Always)]
		public string ThreadOldForegroundColor { get; private set; }
		[JsonProperty("color-viewer-thread-header-posted-foreground", Required = Required.Always)]
		public string ThreadHeaerPostedForegroundColor { get; private set; }
		[JsonProperty("color-viewer-thread-header-subject-foreground", Required = Required.Always)]
		public string ThreadHeaerSubjectForegroundColor { get; private set; }
		[JsonProperty("color-viewer-thread-header-name-foreground", Required = Required.Always)]
		public string ThreadHeaerNameForegroundColor { get; private set; }
		[JsonProperty("color-viewer-thread-header-mail-foreground", Required = Required.Always)]
		public string ThreadHeaerMailForegroundColor { get; private set; }
		[JsonProperty("color-viewer-thread-header-res-count-foreground", Required = Required.Always)]
		public string ThreadHeaerResCountForegroundColor { get; private set; }
		[JsonProperty("color-viewer-thread-header-soudane-foreground", Required = Required.Always)]
		public string ThreadHeaerSoudaneForegroundColor { get; private set; }

		[JsonProperty("color-viewer-scrollbar-thumb", Required = Required.Always)]
		public string ViewerScollbarThumbColor { get; private set; }
		[JsonProperty("color-viewer-scrollbar-tarck", Required = Required.Always)]
		public string ViewerScollbarTrackColor { get; private set; }

		[JsonProperty("color-viewer-map-futaba", Required = Required.Always)]
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

		[JsonProperty("weight-catalog", Required = Required.Always)]
		[JsonConverter(typeof(WeightConverter))]
		public FontWeight CatalogFontWeight { get; private set; }
		[JsonProperty("weight-badge-catalog", Required = Required.Always)]
		[JsonConverter(typeof(WeightConverter))]
		public FontWeight CatalogBadgeFontWeight { get; private set; }
		[JsonProperty("weight-thread", Required = Required.Always)]
		[JsonConverter(typeof(WeightConverter))]
		public FontWeight ThreadFontWeight { get; private set; }
		[JsonProperty("weight-thread-bold", Required = Required.Always)]
		[JsonConverter(typeof(WeightConverter))]
		public FontWeight ThreadBoldFontWeight { get; private set; }
		[JsonProperty("weight-post", Required = Required.Always)]
		[JsonConverter(typeof(WeightConverter))]
		public FontWeight PostFontWeight { get; private set; }

		[JsonProperty("opacity-failsafe-thread-image", Required = Required.Always)]
		public double FailsafeThreadImageOpacity { get; private set; }
		[JsonProperty("blur-radius-failsafe-thread-image", Required = Required.Always)]
		public double FailsafeThreadImageBlurRadius { get; private set; }

		// 実験的機能
		[JsonProperty("-opt-fluent-window", Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
		[System.ComponentModel.DefaultValue(FluentType.None)]
		public FluentType OptionFluentTypeWidnow { get; private set; }
		[JsonProperty("-opt-fluent-type-window-color", Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
		[System.ComponentModel.DefaultValue("--ref:color-window-frame")]
		public string OptionFluentTypeWidnowColorOrRef { get; private set; }
		[JsonProperty("-opt-fluent-type-popup", Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
		[System.ComponentModel.DefaultValue(FluentType.Acryic)]
		public FluentType OptionFluentTypePopup { get; private set; }
		[JsonProperty("-opt-fluent-type-popup-color", Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
		[System.ComponentModel.DefaultValue("--ref:color-background")]
		public string OptionFluentTypePopupColorOrRef { get; private set; }
		[JsonProperty("-opt-fluent-blur-opacity", Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
		[System.ComponentModel.DefaultValue(0.4f)]
		public float FluentBlurOpacity { get; private set; }
		

		public (bool Successed, string ErrorText) Validate() {
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

		public Color GetSubColor(Color baseColor) {
			return WpfUtil.ImageUtil.GetMaterialSubColor(baseColor);
		}

		public Color GetTextColor(Color background, Color white, Color black) {
			return WpfUtil.ImageUtil.GetTextColor(background, white, black);
		}

		public static StyleConfig CreateDefault() {
			// ここは使われない
			return new StyleConfig() {
				Version = CurrentVersion,
			};
		}
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


	public enum MouseGestureCommand {
		Left,
		Up,
		Right,
		Down
	}
	public record class MouseGestureCommands(IEnumerable<MouseGestureCommand> Commands) {
		bool IEquatable<MouseGestureCommands>.Equals(MouseGestureCommands? other) {
			if(object.ReferenceEquals(this, other)) {
				return true;
			} else if(other != null) {
				var c1 = this.Commands.ToArray();
				var c2 = other.Commands.ToArray();
				if(c1.Length != c2.Length) {
					return false;
				}
				for(var i = 0; i < c1.Length; i++) {
					if(c1[i] != c2[i]) {
						return false;
					}
				}
				return true;
			} else {
				return false;
			}
		}

		public override int GetHashCode() => this.Commands.GetHashCode();

		public override string ToString() {
			return this.ToString(false);
		}

		public string ToString(bool isSymbol) {
			var left = isSymbol switch {
				true => "\xf0b0",
				false => "←",
			};
			var up = isSymbol switch {
				true => "\xf0ad",
				false => "↑",
			};
			var right = isSymbol switch {
				true => "\xf0af",
				false => "→",
			};
			var down = isSymbol switch {
				true => "\xf0ae",
				false => "↓",
			};
			return string.Join(' ', this.Commands.Select(x => x switch {
				MouseGestureCommand.Left => left,
				MouseGestureCommand.Up => up,
				MouseGestureCommand.Right => right,
				MouseGestureCommand.Down => down,
				_ => null
			}).Where(x => x != null).ToArray());
		}
	}

	public class GestureConfig : Data.ConfigObject {
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


		[JsonIgnore]
		public MouseGestureCommands[] MouseGestureCatalogOpenPost { get; } = new[] {
			new MouseGestureCommands(new [] {
				MouseGestureCommand.Up,
				MouseGestureCommand.Left
			})
		};
		[JsonIgnore]
		public MouseGestureCommands[] MouseGestureCatalogUpdate { get; } = new[] {
			new MouseGestureCommands(new [] {
				MouseGestureCommand.Up,
				MouseGestureCommand.Down
			})
		};
		[JsonIgnore]
		public MouseGestureCommands[] MouseGestureCatalogClose { get; } = new[] {
			new MouseGestureCommands(new [] {
				MouseGestureCommand.Up,
				MouseGestureCommand.Right
			})
		};
		[JsonIgnore]
		public MouseGestureCommands[] MouseGestureThreadOpenPost { get; } = new[] {
			new MouseGestureCommands(new [] {
				MouseGestureCommand.Down,
				MouseGestureCommand.Left
			})
		};
		[JsonIgnore]
		public MouseGestureCommands[] MouseGestureThreadUpdate { get; } = new[] {
			new MouseGestureCommands(new [] {
				MouseGestureCommand.Down,
				MouseGestureCommand.Up
			})
		};
		[JsonIgnore]
		public MouseGestureCommands[] MouseGestureThreadClose { get; } = new[] {
			new MouseGestureCommands(new [] {
				MouseGestureCommand.Down,
				MouseGestureCommand.Right
			})
		};
		[JsonIgnore]
		public MouseGestureCommands[] MouseGestureThreadPrevious { get; } = Array.Empty<MouseGestureCommands>();
		[JsonIgnore]
		public MouseGestureCommands[] MouseGestureThreadNext { get; } = Array.Empty<MouseGestureCommands>();

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