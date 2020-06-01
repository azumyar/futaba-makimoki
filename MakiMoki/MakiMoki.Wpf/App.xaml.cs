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

		public LibVLCSharp.Shared.LibVLC LibVLC { get; private set; }

		protected override Window CreateShell() {
			LibVLCSharp.Shared.Core.Initialize();
			this.LibVLC = new LibVLCSharp.Shared.LibVLC();

			AppSettingRootDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MakiMoki");
			AppWorkDirectory = Directory.CreateDirectory(Path.Combine(AppSettingRootDirectory, "Work")).FullName;
			AppCacheDirectory = Directory.CreateDirectory(Path.Combine(AppSettingRootDirectory, "Work", "Cache")).FullName;
			var userRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "MakiMoki");
			var userConfig = default(string);
			if(Directory.Exists(userRoot)) {
				userConfig = Directory.CreateDirectory(Path.Combine(userRoot, "Config.d")).FullName;
			}

			try {
				Config.ConfigLoader.Initialize(new Config.ConfigLoader.Setting() {
					SystemDirectory = Path.Combine(
						Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
						"Config.d"),
					UserDirectory = userConfig,
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
