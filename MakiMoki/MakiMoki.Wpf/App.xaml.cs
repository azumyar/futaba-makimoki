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

namespace Yarukizero.Net.MakiMoki.Wpf {
	/// <summary>
	/// App.xaml の相互作用ロジック
	/// </summary>
	public partial class App : PrismApplication {
		[System.Runtime.InteropServices.DllImport("kernel32.dll")]
		private static extern bool SetDllDirectory(string lpPathName);

		private static readonly string ExeConfig = "makimoki.exe.json";

		public string AppSettingRootDirectory { get; private set; }
		public string AppWorkDirectory { get; private set; }
		public string AppCacheDirectory { get; private set; }
		public string UserRootDirectory { get; private set; }
		public string SystemDirectory { get; private set; }

		public LibVLCSharp.Shared.LibVLC LibVLC { get; private set; }

		protected override Window CreateShell() {
			SetDllDirectory(Path.Combine(
				Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
				"Lib"));

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

				AppSettingRootDirectory = AppSettingRootDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MakiMoki");
				AppWorkDirectory = Directory.CreateDirectory(Path.Combine(AppSettingRootDirectory, "Work")).FullName;
				AppCacheDirectory = Directory.CreateDirectory(Path.Combine(AppSettingRootDirectory, "Work", "Cache")).FullName;
				UserRootDirectory = UserRootDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "MakiMoki");
				var userConfig = default(string);
				if(Directory.Exists(UserRootDirectory)) {
					userConfig = Directory.CreateDirectory(Path.Combine(UserRootDirectory, "Config.d")).FullName;
				}

				Config.ConfigLoader.Initialize(new Config.ConfigLoader.Setting() {
					SystemDirectory = Path.Combine(
						Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
						"Config.d"),
					UserDirectory = userConfig,
					CacheDirectory = AppCacheDirectory,
					WorkDirectory = AppWorkDirectory,
				});
				Ng.NgConfig.NgConfigLoder.Initialize(new Ng.NgConfig.NgConfigLoder.Setting() {
					UserDirectory = userConfig,
				});
			}
			catch(Exceptions.InitializeFailedException ex) {
				MessageBox.Show(ex.Message, "初期化エラー", MessageBoxButton.OK, MessageBoxImage.Error);
				Environment.Exit(1);
			}
			Util.TaskUtil.Initialize();
			Util.Futaba.Initialize();
			Reactive.Bindings.UIDispatcherScheduler.Initialize();
			RemoveOldCache(AppCacheDirectory);

			return Container.Resolve<Windows.MainWindow>();
		}

		protected override void RegisterTypes(IContainerRegistry containerRegistry) {
			base.ConfigureViewModelLocator();

			ViewModelLocationProvider.Register<Windows.MainWindow, ViewModels.MainWindowViewModel>();
			/*
			ViewModelLocationProvider.Register<Controls.FutabaViewer, ViewModels.FutabaViewerViewModel>();
			ViewModelLocationProvider.Register<Controls.FutabaCatalogViewer, ViewModels.FutabaCatalogViewerViewModel>();
			ViewModelLocationProvider.Register<Controls.FutabaMediaViewer, ViewModels.FutabaMediaViewerViewModel>();
			*/
			var controlType = typeof(Controls.FutabaCatalogViewer);
			var vmType = typeof(ViewModels.FutabaCatalogViewerViewModel);
			var ca = controlType.Assembly.GetTypes().Where(x => x.Namespace == controlType.Namespace).ToArray();
			var va = vmType.Assembly.GetTypes().Where(x => x.Namespace == vmType.Namespace).ToArray();
			var m = typeof(ViewModelLocationProvider).GetMethod("Register", new Type[0]);
			System.Diagnostics.Debug.Assert(m != null);
			foreach(var t in ca) {
				var vm = va.Where(x => x.FullName == $"{ x.Namespace }.{ t.Name }ViewModel").FirstOrDefault();
				if(vm != null) {
					System.Diagnostics.Debug.WriteLine($"Register: { vm.Name }");
					m.MakeGenericMethod(t, vm).Invoke(null, new object[0]);
				}
			}
		}

		private void RemoveOldCache(string cacheDir) {
			var now = DateTime.Now;
			var confSec = 3 * 24 * 60 * 60; // 3日以前のファイルは削除 TODO: 設定ファイルに移動
#if DEBUG
			var sw = new System.Diagnostics.Stopwatch();
			sw.Start();
#endif
			var f = Directory.EnumerateFiles(cacheDir)
				.Where(x => {
					try {
						return (now - File.GetLastWriteTime(x)).TotalSeconds > confSec;
					}
					catch(IOException) {
						return false;
					}
				});
			// TODO: ファイルがたくさんあると無視できないくらい重い、非同期化したほうがいいかも
			foreach(var it in f) {
				//System.Diagnostics.Debug.WriteLine(it);
				try {
					File.Delete(it);
				}
				catch(IOException) { /* 削除できないファイルは無視する */}
			}
#if DEBUG
			sw.Stop();
			Console.WriteLine("初期削除処理{0}ミリ秒", sw.ElapsedMilliseconds);
#endif
		}
	}
}
