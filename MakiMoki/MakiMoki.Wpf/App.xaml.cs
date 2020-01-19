﻿using System;
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
		public string AppSettingRootDirectory { get; private set; }
		public string AppWorkDirectory { get; private set; }
		public string AppCacheDirectory { get; private set; }

		protected override Window CreateShell() {
			AppSettingRootDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MakiMoki");
			var userRoot = Path.Combine(AppSettingRootDirectory, "User");
			AppWorkDirectory = Directory.CreateDirectory(Path.Combine(AppSettingRootDirectory, "Work")).FullName;
			AppCacheDirectory = Directory.CreateDirectory(Path.Combine(AppSettingRootDirectory, "Work", "Cache")).FullName;
			//var userConfig = Directory.CreateDirectory(Path.Combine(userRoot, "Config.d")).FullName;

			try {
				Config.ConfigLoader.Initialize(new Config.ConfigLoader.Setting() {
					SystemDirectory = Path.Combine(
						Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
						"Config.d"),
					UserDirectory = null,
					CacheDirectory = AppCacheDirectory,
					WorkDirectory = AppWorkDirectory,
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
			ViewModelLocationProvider.Register<Controls.FutabaViewer, ViewModels.FutabaViewerViewModel>();
			ViewModelLocationProvider.Register<Controls.FutabaMediaViewer, ViewModels.FutabaMediaViewerViewModel>();
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
