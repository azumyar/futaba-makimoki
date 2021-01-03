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
		private static readonly string ExeConfig = "windows.exe.json";

		static App() {
			System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
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

		protected override Window CreateShell() {
			AppDomain.CurrentDomain.UnhandledException += delegate (object sender, UnhandledExceptionEventArgs e) {
				if((e.ExceptionObject is Exception ex) && !string.IsNullOrEmpty(UserLogDirectory) && Directory.Exists(UserLogDirectory)) {
					var pid = System.Diagnostics.Process.GetCurrentProcess().Id;
					var tid = System.Threading.Thread.CurrentThread.ManagedThreadId;
					var d = DateTime.Now;

					File.AppendAllText(
						Path.Combine(UserLogDirectory, $"crash-{ d.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) }.txt"),
						$"[{ d.ToString("yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) }][{ pid }][{ tid }]{ Environment.NewLine }"
							+ $"{ ex.ToString() }{ Environment.NewLine }",
						System.Text.Encoding.UTF8);
				}
			};
			WinApi.Win32.SetDllDirectory(Path.Combine(
				AppContext.BaseDirectory,
				"Lib",
				Environment.Is64BitProcess ? "x64" : "x86"));

			UIDispatcherScheduler.Initialize();
			LibVLCSharp.Shared.Core.Initialize(Path.Combine(
				AppContext.BaseDirectory,
				"libvlc",
				Environment.Is64BitProcess ? "win-x64" : "win-x86"));
			this.LibVLC = new LibVLCSharp.Shared.LibVLC();

			try {
				SystemDirectory = Path.Combine(System.AppContext.BaseDirectory, "Config.d");
				var exeConf = Path.Combine(SystemDirectory, ExeConfig);
				if(File.Exists(exeConf)) {
					Util.FileUtil.LoadConfigHelper(exeConf,
						(json) => {
							if (PlatformData.MakiMokiExeConfig.CurrentVersion != JsonConvert.DeserializeObject<Data.ConfigObject>(json).Version) {
								throw new Exceptions.InitializeFailedException($"{ exeConf }のバージョンが不正です");
							}
							var conf = JsonConvert.DeserializeObject<PlatformData.MakiMokiExeConfig>(json);
							string get(bool? enable, string exePath, string customPath) {
								if(enable ?? false) {
									if(customPath != null) {
										if(!Directory.Exists(conf.CustomDataPathRoot)) {
											throw new Exceptions.InitializeFailedException($"設定ディレクトリ{ customPath }が見つかりません");
										}
										return customPath;
									}
									return exePath;
								}
								return null;
							}
							var exeConfPath = Path.Combine(System.AppContext.BaseDirectory, "SingleUser"); ;
							AppSettingRootDirectory = get(conf.IsSingleUserData, exeConfPath, conf.CustomDataPathRoot);
							UserRootDirectory = get(conf.IsSingleUserConfig, exeConfPath, conf.CustomConfigPathRoot);
						},
						(e, m) => throw new Exceptions.InitializeFailedException(m, e));
				}

				AppSettingRootDirectory = AppSettingRootDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FutaMaki");
				AppWorkDirectory = Directory.CreateDirectory(Path.Combine(AppSettingRootDirectory, "Work")).FullName;
				AppCacheDirectory = Directory.CreateDirectory(Path.Combine(AppSettingRootDirectory, "Work", "Cache")).FullName;
				UserRootDirectory = UserRootDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FutaMaki");
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
				});
				Util.Futaba.Initialize();
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
			}
			catch(Exceptions.InitializeFailedException ex) {
				MessageBox.Show(ex.Message, "初期化エラー", MessageBoxButton.OK, MessageBoxImage.Error);
				Environment.Exit(1);
			}
			catch(Exceptions.MigrateFailedException ex) {
				MessageBox.Show(ex.Message, "初期化エラー", MessageBoxButton.OK, MessageBoxImage.Error);
				Environment.Exit(1);
			}
			//Util.TaskUtil.Initialize();
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
			WpfConfig.WpfConfigLoader.Style.Validate();
			ApplyStyle();
			PlatformUtil.RemoveOldCache(AppCacheDirectory);
			//WpfConfig.WpfConfigLoader.AddSystemConfigUpdateNotifyer(systemUpdateAction = (x) => ApplyStyle());

			return Container.Resolve<Windows.MainWindow>();
		}

		private void ApplyStyle(PlatformData.StyleConfig styleConfig = null) {
			var style = styleConfig ?? WpfConfig.WpfConfigLoader.Style;
			this.Resources["StyleType"] = style.StyleType;
			{
				var white = style.ToWpfColor(style.WhiteColor);
				var black = style.ToWpfColor(style.BlackColor);
				var primary = style.ToWpfColor(style.PrimaryColor);
				var primarySub = style.GetSubColor(primary);
				var secondary = style.ToWpfColor(style.SecondaryColor);
				var secondarySub = style.GetSubColor(secondary);
				var windowFrame = style.ToWpfColor(style.WindowFrameColor);
				var windowBorder = style.GetSubColor(windowFrame, PlatformData.StyleType.Light);
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

				this.Resources["WindowFrameColor"] = windowFrame;
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
				var threadSearchHitBackground = style.ToWpfColor(style.ThreadSearchHitBackgroundColor);
				var threadQuotHitBackground = style.ToWpfColor(style.ThreadQuotHitBackgroundColor);
				var threadOldForeground = style.ToWpfColor(style.ThreadOldForegroundColor);
				var threadHeaderPostedForeground = style.ToWpfColor(style.ThreadHeaerPostedForegroundColor);
				var threadHeaderSubjectForeground = style.ToWpfColor(style.ThreadHeaerSubjectForegroundColor);
				var threadHeaderNameForeground = style.ToWpfColor(style.ThreadHeaerNameForegroundColor);
				var threadHeaderMailForeground = style.ToWpfColor(style.ThreadHeaerMailForegroundColor);
				var threadHeaderSoudaneForeground = style.ToWpfColor(style.ThreadHeaerSoudaneForegroundColor);
				this.Resources["ThreadBackgroundColor"] = threadBackground;
				this.Resources["ThreadBackgroundSerachHitColor"] = threadSearchHitBackground;
				this.Resources["ThreadBackgroundQuotHitColor"] = threadQuotHitBackground;
				this.Resources["ThreadTextOldColor"] = threadOldForeground;
				this.Resources["ThreadHeaderPostedColor"] = threadHeaderPostedForeground;
				this.Resources["ThreadHeaderSubjectColor"] = threadHeaderSubjectForeground;
				this.Resources["ThreadHeaderNameColor"] = threadHeaderNameForeground;
				this.Resources["ThreadHeaderMailColor"] = threadHeaderMailForeground;
				this.Resources["ThreadHeaderSoudaneColor"] = threadHeaderSoudaneForeground;

				var mapArray = style.ViewerFutabaColorMap
					?.Select(x => new PlatformData.ColorMap() {
						Target = style.ToWpfColor(x.Key),
						Value = style.ToWpfColor(x.Value),
					}).ToArray() ?? new PlatformData.ColorMap[0];
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
			var windowType = typeof(Windows.MainWindow);
			var controlType = typeof(Controls.FutabaCatalogViewer);
			var vmType = typeof(ViewModels.MainWindowViewModel);
			var va = vmType.Assembly.GetTypes().Where(x => x.Namespace == vmType.Namespace).ToArray();
			var m = typeof(ViewModelLocationProvider).GetMethod("Register", new Type[0]);
			System.Diagnostics.Debug.Assert(m != null);
			foreach(var t in controlType.Assembly.GetTypes().Where(x => (x.Namespace == windowType.Namespace) || (x.Namespace == controlType.Namespace))) {
				var vm = va.Where(x => x.FullName == $"{ x.Namespace }.{ t.Name }ViewModel").FirstOrDefault();
				if(vm != null) {
					System.Diagnostics.Debug.WriteLine($"Register: { vm.Name }");
					m.MakeGenericMethod(t, vm).Invoke(null, new object[0]);
				}
			}
		}
	}
}
