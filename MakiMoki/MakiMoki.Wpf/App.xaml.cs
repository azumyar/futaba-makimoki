using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
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
		[System.Runtime.InteropServices.DllImport("kernel32.dll")]
		private static extern bool SetDllDirectory(string lpPathName);

		private static readonly string ExeConfig = "windows.exe.json";

		public string AppSettingRootDirectory { get; private set; }
		public string AppWorkDirectory { get; private set; }
		public string AppCacheDirectory { get; private set; }
		public string UserRootDirectory { get; private set; }
		public string UserConfigDirectory { get; private set; }
		public string UserLogDirectory { get; private set; }
		public string SystemDirectory { get; private set; }

		public LibVLCSharp.Shared.LibVLC LibVLC { get; private set; }

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
			SetDllDirectory(Path.Combine(
				Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
				"Lib",
				Environment.Is64BitProcess ? "x64" : "x86"));

			LibVLCSharp.Shared.Core.Initialize();
			this.LibVLC = new LibVLCSharp.Shared.LibVLC();

			try {
				SystemDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Config.d");
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
							var exeConfPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "SingleUser"); ;
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
						Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
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
						Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
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
			UIDispatcherScheduler.Initialize();
			RemoveOldCache(AppCacheDirectory);
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
			
			return Container.Resolve<Windows.MainWindow>();
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

		private void RemoveOldCache(string cacheDir) {
			var time = DateTime.Now.AddDays(-WpfConfig.WpfConfigLoader.SystemConfig.CacheExpireDay);
#if DEBUG
			var sw = new System.Diagnostics.Stopwatch();
			sw.Start();
#endif
			var f = Directory.EnumerateFiles(cacheDir)
				.Where(x => {
					try {
						return File.GetLastWriteTime(x) < time;
					}
					catch(IOException) {
						return false;
					}
				});
			// TODO: ファイルがたくさんあると無視できないくらい重い、非同期化したほうがいいかも
			// Parallel.ForEachにしてみた
			Parallel.ForEach(f, it => {
				//System.Diagnostics.Debug.WriteLine(it);
				try {
					File.Delete(it);
				}
				catch(IOException) { /* 削除できないファイルは無視する */}
			});
#if DEBUG
			sw.Stop();
			Console.WriteLine("初期削除処理{0}ミリ秒", sw.ElapsedMilliseconds);
#endif
		}
	}
}
