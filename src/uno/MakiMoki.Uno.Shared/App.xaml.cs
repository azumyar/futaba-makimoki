using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Prism.Ioc;
using Prism.Unity;
using System.Reflection;

namespace Yarukizero.Net.MakiMoki.Uno {
	sealed partial class App : PrismApplication {
		static class ContainerRegistryProxy {
			public static void RegisterForNavigation<T>(IContainerRegistry containerRegistry)
				=> containerRegistry.RegisterForNavigation<T>();
		}

		public static System.Net.Http.HttpClient HttpClient { get; }
		public static readonly Data.BoardData TmpImgBoard = Data.BoardData.From(
			name: "二次元裏(img)",
			url: "https://img.2chan.net/b/",
			defaultComment: "ｷﾀ━━━━━━(ﾟ∀ﾟ)━━━━━━ !!!!!",
			sortIndex: 101,
			extra: Data.BoardDataExtra.From(
				name: false,
				resImage: false,
				mailIp: false,
				mailId: true,
				alwaysIp: false,
				alwaysId: false,
				maxStoredRes: 20000,
				maxStoredTime: 3600,
				resTegaki: true));
		private static readonly string TmpClientName = "FutaMaki/Droid-alpha";
		private static readonly string WorkDirectoryName = "makimoki-work";
		private static readonly string CacheDirectoryName = "makimoki-cache";

		static App() {
#if CANARY
#warning カナリアビルド設定です
#endif
			HttpClient = new System.Net.Http.HttpClient();
			HttpClient.DefaultRequestHeaders.Add(
				"User-Agent",
				TmpClientName);
				//WpfUtil.PlatformUtil.GetContentType());

			System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
		}

		public string AppSettingRootDirectory { get; private set; }
		public string AppWorkDirectory { get; private set; }
		public string AppCacheDirectory { get; private set; }
		public string UserRootDirectory { get; private set; }
		public string UserConfigDirectory { get; private set; }
		public string UserLogDirectory { get; private set; }
		public string SystemDirectory { get; private set; }

		public App() {
			this.InitializeComponent();
		}

		protected override UIElement CreateShell() {
			return Container.Resolve<Views.Shell>();
		}

		protected override void OnLaunched(LaunchActivatedEventArgs args) {
			base.OnLaunched(args);

			Reactive.Bindings.UIDispatcherScheduler.Initialize();
			this.Resources.MergedDictionaries.Add(new global::Uno.Material.MaterialColorPalette());
			this.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("ms-appx:///Assets/Resources/ColorPaletteOverride.xaml") });
			this.Resources.MergedDictionaries.Add(new global::Uno.Material.MaterialResources());

			try {
				/*
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
				});
				*/
#if __IOS__
				var work = Path.Combine(Directory.GetCurrentDirectory(), WorkDirectoryName);
				var cache = Path.Combine(Directory.GetCurrentDirectory(), CacheDirectoryName);
				if(!Directory.Exists(work)) {
					Directory.CreateDirectory(work);
				}
				if(!Directory.Exists(cache)) {
					Directory.CreateDirectory(cache);
				}
				AppWorkDirectory = work;
				AppCacheDirectory = cache;
#endif

#if __ANDROID__
				var work = Droid.MainActivity.ActivityContext.GetDir(WorkDirectoryName, 0).Path;
				var cache = Path.Combine(Droid.MainActivity.ActivityContext.GetExternalFilesDir(null).Path, CacheDirectoryName);
				if(!Directory.Exists(work)) {
					Directory.CreateDirectory(work);
				}
				if(!Directory.Exists(cache)) {
					Directory.CreateDirectory(cache);
				}
				AppWorkDirectory = work;
				AppCacheDirectory = cache;
#endif

				Config.ConfigLoader.Initialize(new Config.ConfigLoader.Setting() {
					RestUserAgent =TmpClientName,
					CacheDirectory = AppCacheDirectory,
					WorkDirectory = AppWorkDirectory,
				});

				// 暫定追加
				Config.ConfigLoader.UpdateUserBoardConfig(new Data.BoardData[] {
					TmpImgBoard
				});
				// 暫定追加ここまで

				Util.Futaba.Initialize(HttpClient);
				Ng.NgConfig.NgConfigLoader.Initialize(new Ng.NgConfig.NgConfigLoader.Setting() {
					UserDirectory = UserConfigDirectory,
				});

			}
			catch(Exceptions.InitializeFailedException ex) {
				//MessageBox.Show(ex.Message, "初期化エラー", MessageBoxButton.OK, MessageBoxImage.Error);
				Environment.Exit(1);
			}
			catch(Exceptions.MigrateFailedException ex) {
				//MessageBox.Show(ex.Message, "初期化エラー", MessageBoxButton.OK, MessageBoxImage.Error);
				Environment.Exit(1);
			}
			RemoveOldCache(AppCacheDirectory);

			Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += (_, e) => {
				if((global::Windows.UI.Xaml.Window.Current.Content is FrameworkElement el)
					&& (el.DataContext is ViewModels.ViewModelBase vm)
					&& vm.BackCommand.CanExecute()) {

					e.Handled = true;
					vm.BackCommand.Execute();
				} else {
#if __ANDROID__
					var root = string.IsNullOrEmpty(Droid.MainActivity.ActivityContext.Intent
						?.GetStringExtra(Droid.IntentActivity.IntentExtraFilterUri));
					if(global::Android.OS.BuildVersionCodes.Lollipop <= global::Android.OS.Build.VERSION.SdkInt) {
						Droid.MainActivity.ActivityContext.FinishAndRemoveTask();
					} else {
						Droid.MainActivity.ActivityContext.Finish();
					}
					if(root) {
						Environment.Exit(0);
					}
#endif
				}
			};
		}

		protected override void RegisterTypes(IContainerRegistry containerRegistry) {
			var nv = this.GetType().Namespace + ".Views";
			var p = new object[] { containerRegistry };

			var rn = typeof(ContainerRegistryProxy).GetMethod(
				nameof(ContainerRegistryProxy.RegisterForNavigation),
				BindingFlags.Public | BindingFlags.Static);
			foreach(var type in this.GetType().Assembly.GetTypes()
				.Where(x => (x.Namespace == nv) && (x.Name.EndsWith("Page")))) {

				System.Diagnostics.Debug.WriteLine("Register:" + type.Name);
				rn.MakeGenericMethod(type).Invoke(null, p);
			}
			containerRegistry.RegisterDialog<Views.MessageDialog, ViewModels.MessageDialogViewModel>();
		}

		protected override void OnSuspending(SuspendingEventArgs e) {
			var deferral = e.SuspendingOperation.GetDeferral();
			//TODO: Save application state and stop any background activity
			deferral.Complete();
		}

		// とりあえずここに置いておく
		private static void RemoveOldCache(string cacheDir) {
			//var time = DateTime.Now.AddDays(-WpfConfig.WpfConfigLoader.SystemConfig.CacheExpireDay);
			var time = DateTime.Now.AddDays(-3);
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
			System.Threading.Tasks.Parallel.ForEach(f, it => {
				//System.Diagnostics.Debug.WriteLine(it);
				try {
					File.Delete(it);
				}
				catch(IOException) { /* 削除できないファイルは無視する */}
			});
#if DEBUG
			sw.Stop();
			//Console.WriteLine("初期削除処理{0}ミリ秒", sw.ElapsedMilliseconds);
#endif
		}
	}
}
