using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yarukizero.Net.MakiMoki.Data;

namespace Yarukizero.Net.MakiMoki.Wpf.PlatformData.Compat {

	class WpfConfig2021020100 : Data.ConfigObject, Data.IMigrateCompatObject {
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

		public ConfigObject Migrate() {
			var conf = JsonConvert.DeserializeObject<WpfConfig>(
				Util.FileUtil.LoadFileString(new Util.ResourceLoader(
					typeof(Wpf.WpfConfig.WpfConfigLoader))
						.Get(Wpf.WpfConfig.WpfConfigLoader.SystemConfigFile)
					));

			return WpfConfig.Create(
				windowTheme: this.WindowTheme,
				isEnabledFetchThumbnail: this.IsEnabledFetchThumbnail,
				isEnabledMovieMarker: this.IsEnabledMovieMarker,
				isEnabledIdMarker: this.IsEnabledIdMarker,
				isEnabledOldMarker: this.IsEnabledOldMarker,
				catalogNgImage: this.CatalogNgImage,
				isVisibleCatalogIsolateThread: this.IsVisibleCatalogIsolateThread,
				catalogSearchResult: this.CatalogSearchResult,
				threadDelResVisibility: this.ThreadDelResVisibility,
				isEnabledThreadCommandPalette: this.IsEnabledThreadCommandPalette,
				commandPalettePosition: this.CommandPalettePosition,
				canvas98Position: this.Canvas98Position	,
				isEnabledFailsafeMistakePost: this.IsEnabledFailsafeMistakePost,
				clipbordJpegQuality: this.ClipbordJpegQuality,
				clipbordIsEnabledUrl: this.ClipbordIsEnabledUrl,
				maxWidthPostView: this.MaxWidthPostView,
				isEnabledOpacityPostView: this.IsEnabledOpacityPostView,
				opacityPostView: this.OpacityPostView,
				mediaExportPath: this.MediaExportPath,
				cacheExpireDay: this.CacheExpireDay,
				exportNgRes: this.ExportNgRes,
				exportNgImage: this.ExportNgImage,
				windowTopmost: this.IsEnabledWindowTopmost,
				ngReasonInput: this.IsEnabledNgReasonInput,
				browserPath: this.BrowserPath,
				bouyomiChanEndPoint: conf.BouyomiChanEndPoint
			);;
			throw new NotImplementedException();
		}
	}
}
