using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using Prism.Mvvm;
using Prism.Ioc;
using Prism.Unity;
using Newtonsoft.Json;
using System.Reactive.Linq;
using Yarukizero.Net.MakiMoki.Wpf.WpfUtil;
using Reactive.Bindings;

namespace Yarukizero.Net.MakiMoki.Wpf {
	/// <summary>
	/// App.xaml の相互作用ロジック
	/// </summary>
	public partial class App : PrismApplication {
		public class CompatValue {
			public bool IsWindows10Threshold1 { get; init; }
			public bool IsWindows10Threshold2 { get; init; }
			public bool IsWindows10Redstone1 { get; init; }
			public bool IsWindows10Redstone2 { get; init; }
			public bool IsWindows10Redstone3 { get; init; }
			public bool IsWindows10Redstone4 { get; init; }
			public bool IsWindows10Redstone5 { get; init; }
			public bool IsWindows10Ver19H1 { get; init; }
			public bool IsWindows10Ver19H2 { get; init; }
			public bool IsWindows10Ver20H1 { get; init; }
			public bool IsWindows10Ver20H2 { get; init; }
			public bool IsWindows10Ver21H1 { get; init; }
			public bool IsWindows10Ver21H2 { get; init; }
			public bool IsWindows11Rtm { get; init; }
			public bool IsWindows11Ver22H2 { get; init; }
		}
		private static readonly string ExeConfig = "windows.exe.json";

		public static System.Net.Http.HttpClient HttpClient  { get; }
		public static CompatValue OsCompat { get; }

		static App() {
#if CANARY
#warning カナリアビルド設定です
#endif
			HttpClient = new System.Net.Http.HttpClient() {
				Timeout = TimeSpan.FromMilliseconds(5000),
			};
			HttpClient.DefaultRequestHeaders.Add(
				"User-Agent",
				WpfUtil.PlatformUtil.GetContentType());

			System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
			if(Environment.OSVersion.Platform == PlatformID.Win32NT) {
				OsCompat = new CompatValue() {
					IsWindows10Threshold1 = new Version(10, 0, 10240) <= Environment.OSVersion.Version,
					IsWindows10Threshold2 = new Version(10, 0, 10586) <= Environment.OSVersion.Version,
					IsWindows10Redstone1 = new Version(10, 0, 14393) <= Environment.OSVersion.Version,
					IsWindows10Redstone2 = new Version(10, 0, 15063) <= Environment.OSVersion.Version,
					IsWindows10Redstone3 = new Version(10, 0, 16299) <= Environment.OSVersion.Version,
					IsWindows10Redstone4 = new Version(10, 0, 17134) <= Environment.OSVersion.Version,
					IsWindows10Redstone5 = new Version(10, 0, 17763) <= Environment.OSVersion.Version,
					IsWindows10Ver19H1 = new Version(10, 0, 18362) <= Environment.OSVersion.Version,
					IsWindows10Ver19H2 = new Version(10, 0, 18363) <= Environment.OSVersion.Version,
					IsWindows10Ver20H1 = new Version(10, 0, 19041) <= Environment.OSVersion.Version,
					IsWindows10Ver20H2 = new Version(10, 0, 19042) <= Environment.OSVersion.Version,
					IsWindows10Ver21H1 = new Version(10, 0, 19042) <= Environment.OSVersion.Version,
					IsWindows10Ver21H2 = new Version(10, 0, 19044) <= Environment.OSVersion.Version,
					IsWindows11Rtm = new Version(10, 0, 17763) <= Environment.OSVersion.Version,
					IsWindows11Ver22H2 = new Version(10, 0, 22621) <= Environment.OSVersion.Version,
				};
			} else {
				OsCompat = new CompatValue();
			}

		}

		public string AppSettingRootDirectory { get; private set; }
		public string AppWorkDirectory { get; private set; }
		public string AppCacheDirectory { get; private set; }
		public string UserRootDirectory { get; private set; }
		public string UserConfigDirectory { get; private set; }
		public string UserLogDirectory { get; private set; }
		public string SystemDirectory { get; private set; }

		public LibVLCSharp.Shared.LibVLC LibVLC { get; private set; }

		//private Action<PlatformData.WpfConfig> systemUpdateAction;
		protected override void OnStartup(StartupEventArgs e) {
			AppDomain.CurrentDomain.UnhandledException += delegate (object sender, UnhandledExceptionEventArgs e) {
				if((e.ExceptionObject is Exception ex) && !string.IsNullOrEmpty(UserLogDirectory) && Directory.Exists(UserLogDirectory)) {
					var pid = Environment.ProcessId;
					var tid = System.Threading.Thread.CurrentThread.ManagedThreadId;
					var d = DateTime.Now;

					File.AppendAllText(
						Path.Combine(UserLogDirectory, $"crash-{ d.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) }.txt"),
						$"[{ d.ToString("yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) }][{ pid }][{ tid }]{ Environment.NewLine }"
							+ $"{ ex }{ Environment.NewLine }",
						System.Text.Encoding.UTF8);
				}
			};
			WpfUtil.MediaFoundationUtil.StratUp();
			var arch = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString().ToLower();
			WinApi.Win32.SetDllDirectory(Path.Combine(
				AppContext.BaseDirectory,
				"runtimes",
				"libwebp",
				$"win-{ arch }"));

			UIDispatcherScheduler.Initialize();
			ReactivePropertyScheduler.SetDefault(
				new global::Reactive.Bindings.Schedulers.ReactivePropertyWpfScheduler(
					this.Dispatcher));
			// ARM64未対応
			LibVLCSharp.Shared.Core.Initialize(Path.Combine(
				AppContext.BaseDirectory,
#if !DEBUG
				"runtimes",
#endif
				"libvlc",
				$"win-{ arch }"));
			this.LibVLC = new LibVLCSharp.Shared.LibVLC();

			try {
				SystemDirectory = Path.Combine(System.AppContext.BaseDirectory, "Config.d");
				var exeConf = Path.Combine(SystemDirectory, ExeConfig);
				if(File.Exists(exeConf)) {
					Util.FileUtil.LoadConfigHelper(exeConf,
						(json) => {
							static string get(bool? enable, string exePath, string custamPath) {
								if(enable ?? false) {
									if(custamPath != null) {
										if(!Directory.Exists(custamPath)) {
											throw new Exceptions.InitializeFailedException($"設定ディレクトリ{ custamPath }が見つかりません");
										}
										return custamPath;
									}
									return exePath;
								}
								return null;
							}

							if(PlatformData.MakiMokiExeConfig.CurrentVersion != JsonConvert.DeserializeObject<Data.ConfigObject>(json).Version) {
								throw new Exceptions.InitializeFailedException($"{ exeConf }のバージョンが不正です");
							}
							var conf = JsonConvert.DeserializeObject<PlatformData.MakiMokiExeConfig>(json);
							var exeConfPath = Path.Combine(System.AppContext.BaseDirectory, "SingleUser"); ;
							AppSettingRootDirectory = get(conf.IsSingleUserData, exeConfPath, conf.CustomDataPathRoot);
							UserRootDirectory = get(conf.IsSingleUserConfig, exeConfPath, conf.CustomConfigPathRoot);
						},
						(e, m) => throw new Exceptions.InitializeFailedException(m, e));
				}

				AppSettingRootDirectory ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FutaMaki");
				AppWorkDirectory = Directory.CreateDirectory(Path.Combine(AppSettingRootDirectory, "Work")).FullName;
				AppCacheDirectory = Directory.CreateDirectory(Path.Combine(AppSettingRootDirectory, "Work", "Cache")).FullName;
				UserRootDirectory ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FutaMaki");
				UserConfigDirectory = Directory.CreateDirectory(Path.Combine(UserRootDirectory, "Config.d")).FullName;
				UserLogDirectory = Directory.CreateDirectory(Path.Combine(UserRootDirectory, "Log")).FullName;

				Config.ConfigLoader.Initialize(new Config.ConfigLoader.Setting() {
					RestUserAgent = WpfUtil.PlatformUtil.GetContentType(),
					SystemDirectory = Path.Combine(
						System.AppContext.BaseDirectory,
						"Config.d"),
					UserDirectory = UserConfigDirectory,
					CacheDirectory = AppCacheDirectory,
					WorkDirectory = AppWorkDirectory,
					AppCenterSecrets = AppCenterSecrets,
					HttpClient = HttpClient,
				});
				Util.Futaba.Initialize(HttpClient);
				Ng.NgConfig.NgConfigLoader.Initialize(new Ng.NgConfig.NgConfigLoader.Setting() {
					UserDirectory = UserConfigDirectory,
				});
				WpfConfig.WpfConfigLoader.Initialize(new WpfConfig.WpfConfigLoader.Setting() {
					SystemDirectory = Path.Combine(
						System.AppContext.BaseDirectory,
						"Config.d"),
					UserDirectory = UserConfigDirectory,
					WorkDirectory = AppWorkDirectory,
				});
				Canvas98.Canvas98Config.Canvas98ConfigLoader.Initialize(new Canvas98.Canvas98Config.Canvas98ConfigLoader.Setting() {
					SystemDirectory = Path.Combine(
						System.AppContext.BaseDirectory,
						"Config.d"),
					UserDirectory = UserConfigDirectory,
					MaskPassword = () => WpfConfig.WpfConfigLoader.SystemConfig.IsMaskPassword,
				});
			}
			catch(Exceptions.InitializeFailedException ex) {
				MessageBox.Show(ex.Message, "初期化エラー", MessageBoxButton.OK, MessageBoxImage.Error);
				Environment.Exit(1);
			}
			catch(Exceptions.MigrateFailedException ex) {
				MessageBox.Show(ex.Message, "初期化エラー", MessageBoxButton.OK, MessageBoxImage.Error);
				Environment.Exit(1);
			}

			{
				var v = WpfConfig.WpfConfigLoader.Style.Validate();
				if(!v.Successed) {
					MessageBox.Show(v.ErrorText, "初期化エラー", MessageBoxButton.OK, MessageBoxImage.Error);
					Environment.Exit(1);
				}
			}
			ApplyStyle();
			PlatformUtil.RemoveOldCache(AppCacheDirectory);
			//WpfConfig.WpfConfigLoader.AddSystemConfigUpdateNotifyer(systemUpdateAction = (x) => ApplyStyle());

#if !DEBUG
			Observable.Create<bool>(async o => {
				o.OnNext(await PlatformUtil.CheckNewVersion());
				return System.Reactive.Disposables.Disposable.Empty;
			}).ObserveOn(UIDispatcherScheduler.Default)
				.Subscribe(x => {
					if(x) {
						var r = MessageBox.Show(
							"新しいバージョンが公開されています。\r\n配布サイトをブラウザで表示しますか？",
							"バージョンチェック通知",
							MessageBoxButton.YesNo,
							MessageBoxImage.Information);
						if(r == MessageBoxResult.Yes) {
							WpfUtil.PlatformUtil.StartBrowser(new Uri(PlatformConst.WebPageUrl));
						}
					}
				});
#endif
			base.OnStartup(e);
		}

		protected override void OnExit(ExitEventArgs e) {
			try {
				WpfUtil.MediaFoundationUtil.Shutdown();
			}
			catch(InvalidOperationException) { /* どうしようもないので無視する */ }
			base.OnExit(e);
		}

		protected override Window CreateShell() {
			return Container.Resolve<Windows.MainWindow>();
		}

		private void ApplyStyle(PlatformData.StyleConfig styleConfig = null) {
			static Color border(Color c1, Color c2) {
				var a = ((int)c1.A + c2.A) / 2; 
				var r = ((int)c1.R + c2.R) / 2; 
				var g = ((int)c1.G + c2.G) / 2; 
				var b = ((int)c1.B + c2.B) / 2;
				var hsv = WpfUtil.ImageUtil.ToHsv(Color.FromRgb((byte)r, (byte)g, (byte)b));
				var c = WpfUtil.ImageUtil.HsvToRgb(hsv.H, hsv.S * 0.4, hsv.V);
				return Color.FromArgb((byte)a, c.R, c.G, c.B);
			}

			var style = styleConfig ?? WpfConfig.WpfConfigLoader.Style;
			{
				var white = style.ToWpfColor(style.WhiteColor);
				var black = style.ToWpfColor(style.BlackColor);
				var foreground = style.ToWpfColor(style.ForegroundColor);
				var background = style.ToWpfColor(style.BackgroundColor);
				var primary = style.ToWpfColor(style.PrimaryColor);
				var primarySub = style.GetSubColor(primary);
				var secondary = style.ToWpfColor(style.SecondaryColor);
				var secondarySub = style.GetSubColor(secondary);
				var windowFrame = style.ToWpfColor(style.WindowFrameColor);
				var windowBorder = style.GetSubColor(windowFrame);
				var windowTabBackground = style.ToWpfColor(style.WindowTabColor);
				var windowTabForeground = style.GetTextColor(windowFrame, white, black);
				var windowTabActiveForeground = style.GetTextColor(windowTabBackground, white, black);
				var windowTabBadge = style.ToWpfColor(style.WindowTabBadgeColor);
				var viewerForeground = style.ToWpfColor(style.ViewerForegroundColor);
				var viewerBackground = style.ToWpfColor(style.ViewerBackgroundColor);
				var viewerBorder = style.ToWpfColor(style.ViewerBorderColor);
				var viewerScrollBarThumb = style.ToWpfColor(style.ViewerScollbarThumbColor);
				var viewerScrollBarThumbSub = style.GetSubColor(viewerScrollBarThumb);
				var viewerScrollBarThumbSubSub = style.GetSubColor(viewerScrollBarThumbSub);
				var viewerScrollBarTrack = style.ToWpfColor(style.ViewerScollbarTrackColor);

				this.Resources["MakimokiWhiteColor"] = white;
				this.Resources["MakimokiBlackColor"] = black;
				this.Resources["MakimokiForegroundColor"] = foreground;
				this.Resources["MakimokiBackgroundColor"] = background;
				this.Resources["MakimokiBorderColor"] = border(foreground, background);
				this.Resources["MakimokiPrimaryColor"] = primary;
				this.Resources["MakimokiSecondaryColor"] = secondary;

				this.Resources["WindowFrameColor"] = this.Resources["WindowSplitterColor"] =  windowFrame;
				this.Resources["WindowFrameBorderColor"] = windowBorder;
				this.Resources["WindowTabBackgroundColor"] = windowTabBackground;
				this.Resources["WindowTabForegroundColor"] = windowTabForeground;
				this.Resources["WindowTabSelectedForegroundColor"] = windowTabActiveForeground;
				this.Resources["WindowThreadTabNewColor"] = windowTabBadge;

				this.Resources["ViewerForegroundColor"] = white;
				this.Resources["ViewerBackgroundColor"] = black;
				this.Resources["ViewerPrimaryColor"] = primary;
				this.Resources["ViewerPrimaryDarkColor"] = primarySub;
				this.Resources["ViewerSecondaryColor"] = secondary;
				this.Resources["ViewerSecondaryDarkColor"] = secondarySub;
				this.Resources["ViewerForegroundColor"] = viewerForeground;
				this.Resources["ViewerBackgroundColor"] = viewerBackground;
				this.Resources["ViewerBorderColor"] = viewerBorder;

				this.Resources["ViewerScrollBarColor"] = viewerScrollBarThumb;
				this.Resources["ViewerScrollBarBorderColor"] = viewerScrollBarThumb;
				this.Resources["ViewerScrollBarMouseOverColor"] = viewerScrollBarThumbSub;
				this.Resources["ViewerScrollBarPressedColor"] = viewerScrollBarThumbSubSub;
				this.Resources["ViewerScrollBarTrackColor"] = viewerScrollBarTrack;
				this.Resources["ViewerScrollButtonColor"] = viewerScrollBarThumb;
				this.Resources["ViewerScrollButtonMouseOverColor"] = primary;
				this.Resources["ViewerScrollButtonPressedColor"] = primarySub;

				var catalogItemBackground = style.ToWpfColor(style.CatalogItemBackgroundColor);
				var catalogItemSearchHitBackground = style.ToWpfColor(style.CatalogItemSearchHitBackgroundColor);
				var catalogItemWatchHitBackgroundColor = style.ToWpfColor(style.CatalogItemWatchHitBackgroundColor);
				var catalogItemOpendBackground = style.ToWpfColor(style.CatalogItemOpendBackgroundColor);
				var catalogBadgeCountForeground = style.ToWpfColor(style.CatalogBadgeCountForegroundColor);
				var catalogBadgeCountBackground = style.ToWpfColor(style.CatalogBadgeCountBackgroundColor);
				var catalogBadgeOldForeground = style.ToWpfColor(style.CatalogBadgeOldForegroundColor);
				var catalogBadgeOldBackground = style.ToWpfColor(style.CatalogBadgeOldBackgroundColor);
				var catalogBadgeIdForeground = style.ToWpfColor(style.CatalogBadgeIdForegroundColor);
				var catalogBadgeIdBackground = style.ToWpfColor(style.CatalogBadgeIdBackgroundColor);
				var catalogBadgeMovieForeground = style.ToWpfColor(style.CatalogBadgeMovieForegroundColor);
				var catalogBadgeMovieBackground = style.ToWpfColor(style.CatalogBadgeMovieBackgroundColor);
				var catalogBadgeIsolateForeground = style.ToWpfColor(style.CatalogBadgeIsolateForegroundColor);
				var catalogBadgeIsolateBackground = style.ToWpfColor(style.CatalogBadgeIsolateBackgroundColor);
				this.Resources["CatalogItemBackgroundColor"] = catalogItemBackground;
				this.Resources["CatalogBackgroundSerachHitColor"] = catalogItemSearchHitBackground;
				this.Resources["CatalogItemWatchBackgroundColor"] = catalogItemWatchHitBackgroundColor;
				this.Resources["CatalogItemOpendBackgroundColor"] = catalogItemOpendBackground;
				this.Resources["CatalogBadgeCountForegroundColor"] = catalogBadgeCountForeground;
				this.Resources["CatalogBadgeCountBackgroundColor"] = catalogBadgeCountBackground;
				this.Resources["CatalogBadgeOldForegroundColor"] = catalogBadgeOldForeground;
				this.Resources["CatalogBadgeOldBackgroundColor"] = catalogBadgeOldBackground;
				this.Resources["CatalogBadgeIdForegroundColor"] = catalogBadgeIdForeground;
				this.Resources["CatalogBadgeIdBackgroundColor"] = catalogBadgeIdBackground;
				this.Resources["CatalogBadgeMovieForegroundColor"] = catalogBadgeMovieForeground;
				this.Resources["CatalogBadgeMovieBackgroundColor"] = catalogBadgeMovieBackground;
				this.Resources["CatalogBadgeIsolateForegroundColor"] = catalogBadgeIsolateForeground;
				this.Resources["CatalogBadgeIsolateBackgroundColor"] = catalogBadgeIsolateBackground;

				var threadBackground = style.ToWpfColor(style.ThreadBackgroundColor);
				var threadLink = style.ToWpfColor(style.ThreadLinkColor);
				var threadSearchHitBackground = style.ToWpfColor(style.ThreadSearchHitBackgroundColor);
				var threadQuotHitBackground = style.ToWpfColor(style.ThreadQuotHitBackgroundColor);
				var threadOldForeground = style.ToWpfColor(style.ThreadOldForegroundColor);
				var threadHeaderPostedForeground = style.ToWpfColor(style.ThreadHeaerPostedForegroundColor);
				var threadHeaderSubjectForeground = style.ToWpfColor(style.ThreadHeaerSubjectForegroundColor);
				var threadHeaderNameForeground = style.ToWpfColor(style.ThreadHeaerNameForegroundColor);
				var threadHeaderMailForeground = style.ToWpfColor(style.ThreadHeaerMailForegroundColor);
				var threadHeaerResCountForegroundColor = style.ToWpfColor(style.ThreadHeaerResCountForegroundColor);
				var threadHeaderSoudaneForeground = style.ToWpfColor(style.ThreadHeaerSoudaneForegroundColor);
				this.Resources["ThreadBackgroundColor"] = threadBackground;
				this.Resources["ThreadLinkColor"] = threadLink;
				this.Resources["ThreadBackgroundSerachHitColor"] = threadSearchHitBackground;
				this.Resources["ThreadBackgroundQuotHitColor"] = threadQuotHitBackground;
				this.Resources["ThreadTextOldColor"] = threadOldForeground;
				this.Resources["ThreadHeaderPostedColor"] = threadHeaderPostedForeground;
				this.Resources["ThreadHeaderSubjectColor"] = threadHeaderSubjectForeground;
				this.Resources["ThreadHeaderNameColor"] = threadHeaderNameForeground;
				this.Resources["ThreadHeaderMailColor"] = threadHeaderMailForeground;
				this.Resources["ThreadHeaderResCountColor"] = threadHeaerResCountForegroundColor;
				this.Resources["ThreadHeaderSoudaneColor"] = threadHeaderSoudaneForeground;

				var mapArray = style.ViewerFutabaColorMap
					?.Select(x => new PlatformData.ColorMap() {
						Target = style.ToWpfColor(x.Key),
						Value = style.ToWpfColor(x.Value),
					}).ToArray() ?? Array.Empty<PlatformData.ColorMap>();
				this.Resources["FutabaCommentColorMap"] = new PlatformData.ColorMapCollection(mapArray);
			}

			this.Resources["CatalogImageSize"] = style.CatalogImageSize;
			this.Resources["CatalogTextSize"] = style.CatalogTextSize;
			this.Resources["CatalogBadgeSize"] = style.CatalogBadgeSize;
			this.Resources["CatalogTextFontSize"] = style.CatalogTextFontSize;
			this.Resources["ThreadHeaderFontSize"] = style.ThreadHeaderFontSize;
			this.Resources["ThreadTextFontSize"] = style.ThreadTextFontSize;
			this.Resources["PostFontSize"] = style.PostFontSize;

			this.Resources["CatalogTextFont"] = new FontFamily(style.CatalogFont);
			this.Resources["CatalogBadgeFont"] = new FontFamily(style.CatalogBadgeFont);
			this.Resources["ThreadHeaderFont"] = new FontFamily(style.ThreadHeaderFont);
			this.Resources["ThreadTextFont"] = new FontFamily(style.ThreadTextFont);
			this.Resources["PostFont"] = new FontFamily(style.PostFont);

			this.Resources["CatalogTextFontWeight"] = style.CatalogFontWeight;
			this.Resources["CatalogBadgeFontWeight"] = style.CatalogBadgeFontWeight;
			this.Resources["ThreadFontWeight"] = style.ThreadFontWeight;
			this.Resources["ThreadBoldFontWeight"] = style.ThreadBoldFontWeight;
			this.Resources["PostFontWeight"] = style.PostFontWeight;

			{ // 実験的機能
				static Color blend(float a, Color @base) {
					return Color.FromArgb((byte)(Math.Min(Math.Max(0f, a), 1f) * 255), @base.R, @base.G, @base.B);
				}
				static string refValue(PlatformData.StyleConfig style, string p) {
					return style.GetType()
						.GetProperties()
						.Where(x => x.GetCustomAttributes<JsonPropertyAttribute>(true).Where(y => y.PropertyName == p).Any())
						.FirstOrDefault()
						?.GetValue(style) as string;
				}

				this.Resources["FluentWindowType"] = style.OptionFluentTypeWidnow;
				this.Resources["FluentPopupType"] = style.OptionFluentTypePopup;
				this.Resources["FluentBlurOpacity"] = style.FluentBlurOpacity;
				this.Resources["FluentWindowAccentColor"] = blend(style.FluentBlurOpacity, style.OptionFluentTypeWidnowColorOrRef switch {
					var s when s.StartsWith("--ref:") => style.ToWpfColor(refValue(style, s.Substring("--ref:".Length))),
					var s => style.ToWpfColor(s),
				});
				this.Resources["FluentPopupAccentColor"] = blend(style.FluentBlurOpacity, style.OptionFluentTypePopupColorOrRef switch {
					var s when s.StartsWith("--ref:") => style.ToWpfColor(refValue(style, s.Substring("--ref:".Length))),
					var s => style.ToWpfColor(s),
				});

				if(style.OptionFluentTypeWidnow switch {
					PlatformData.FluentType.None => false,
					PlatformData.FluentType.Auto when !OsCompat.IsWindows10Redstone5 => false,
					_ => true
				}) {
					this.Resources["WindowFrameColor"] = Colors.Transparent;
					this.Resources["WindowFrameBorderColor"] = Colors.Transparent;
				}
			}
		}

		protected override void RegisterTypes(IContainerRegistry containerRegistry) {
			base.ConfigureViewModelLocator();

			/*
			ViewModelLocationProvider.Register<Windows.MainWindow, ViewModels.MainWindowViewModel>();
			ViewModelLocationProvider.Register<Windows.ConfigWindow, ViewModels.ConfigWindowViewModel>();
			ViewModelLocationProvider.Register<Controls.FutabaViewer, ViewModels.FutabaViewerViewModel>();
			ViewModelLocationProvider.Register<Controls.FutabaCatalogViewer, ViewModels.FutabaCatalogViewerViewModel>();
			ViewModelLocationProvider.Register<Controls.FutabaMediaViewer, ViewModels.FutabaMediaViewerViewModel>();
			*/
			var nm = new[] {
				typeof(Windows.MainWindow).Namespace,
				typeof(Windows.Dialogs.ConfigDialog).Namespace,
				typeof(Controls.FutabaCatalogViewer).Namespace,
			};
			var vmType = typeof(ViewModels.MainWindowViewModel);
			var va = vmType.Assembly.GetTypes().Where(x => x.Namespace == vmType.Namespace).ToArray();
			var m = typeof(ViewModelLocationProvider).GetMethod(nameof(ViewModelLocationProvider.Register), Array.Empty<Type>());
			System.Diagnostics.Debug.Assert(m != null);
			foreach(var t in typeof(App).Assembly.GetTypes().Where(x => nm.Contains(x.Namespace))) {
				var vm = va.Where(x => x.FullName == $"{ x.Namespace }.{ t.Name }ViewModel").FirstOrDefault();
				if(vm != null) {
					System.Diagnostics.Debug.WriteLine($"Register: { vm.Name }");
					m.MakeGenericMethod(t, vm).Invoke(null, Array.Empty<object>());
				}
			}
			//ViewModelLocationProvider.Register<Canvas98.Controls.FutabaCanvas98View, Canvas98.ViewModels.FutabaCanvas98ViewViewModel>();
			containerRegistry.RegisterForNavigation<Controls.FutabaMediaViewer, ViewModels.FutabaMediaViewerViewModel>();
			containerRegistry.RegisterForNavigation<Canvas98.Controls.FutabaCanvas98View, Canvas98.ViewModels.FutabaCanvas98ViewViewModel>();
			containerRegistry.RegisterDialog<Windows.Dialogs.ConfigDialog, ViewModels.ConfigDialogViewModel>();
			containerRegistry.RegisterDialog<Windows.Dialogs.BoardEditDialog, ViewModels.BoardEditDialogViewModel>();

			containerRegistry.RegisterInstance(this.Container);
		}
	}
}
