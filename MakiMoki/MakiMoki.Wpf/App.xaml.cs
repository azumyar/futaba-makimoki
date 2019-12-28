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

namespace Yarukizero.Net.MakiMoki.Wpf {
	/// <summary>
	/// App.xaml の相互作用ロジック
	/// </summary>
	public partial class App : PrismApplication {
		protected override Window CreateShell() {
			var appRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MakiMoki");
			var userRoot = Path.Combine(appRoot, "User");
			var work = Directory.CreateDirectory(Path.Combine(appRoot, "Work")).FullName;
			var cache = Directory.CreateDirectory(Path.Combine(appRoot, "Work", "Cache")).FullName;
			//var userConfig = Directory.CreateDirectory(Path.Combine(userRoot, "Config.d")).FullName;

			try {
				Config.ConfigLoader.Initialize(new Config.ConfigLoader.Setting() {
					SystemDirectory = Path.Combine(
						Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
						"Config.d"),
					UserDirectory = null,
					CacheDirectory = cache,
					WorkDirectory = work,
				});
			}
			catch (Exceptions.InitializeFailedException ex) {
				MessageBox.Show(ex.Message, "初期化エラー", MessageBoxButton.OK, MessageBoxImage.Error);
				Environment.Exit(1);
			}
			Util.TaskUtil.Initialize();
			Util.Futaba.Initialize();
			RemoveOldCache(cache);

			return Container.Resolve<Windows.MainWindow>();
		}

		protected override void RegisterTypes(IContainerRegistry containerRegistry) {
			base.ConfigureViewModelLocator();

			ViewModelLocationProvider.Register<Windows.MainWindow, ViewModels.MainWindowViewModel>();
			ViewModelLocationProvider.Register<Controls.FutabaViewer, ViewModels.FutabaViewerViewModel>();
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
					catch (IOException) {
						return false;
					}
				});
			// TODO: ファイルがたくさんあると無視できないくらい重い、非同期化したほうがいいかも
			foreach(var it in f) {
				//System.Diagnostics.Debug.WriteLine(it);
				try {
					File.Delete(it);
				}
				catch (IOException) { /* 削除できないファイルは無視する */}
			}
#if DEBUG
			sw.Stop();
			Console.WriteLine("初期削除処理{0}ミリ秒", sw.ElapsedMilliseconds);
#endif
		}
	}
}
