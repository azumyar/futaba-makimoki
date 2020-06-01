using Prism.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Yarukizero.Net.MakiMoki.Wpf.ViewModels;

namespace Yarukizero.Net.MakiMoki.Wpf.Windows {
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window {
		private static readonly string PlacementJsonFile = "windows.placement.json";

		public MainWindow() {
			InitializeComponent();

			ViewModels.MainWindowViewModel.Messenger.Instance
				.GetEvent<PubSubEvent<ViewModels.MainWindowViewModel.CurrentCatalogChanged>>()
				.Subscribe(x => {
					/*
					foreach(var it in this.TabContainer.Items) {
						if(it is Model.TabItem ti) {
							if(ti.Url == x.FutabaContext?.Url) {
								this.TabContainer.SelectedItem = ti;
							}
						}
					}
					*/
				});
			ViewModels.MainWindowViewModel.Messenger.Instance
				.GetEvent<PubSubEvent<ViewModels.MainWindowViewModel.CurrentThreadChanged>>()
				.Subscribe(x => {
					if(this.DataContext is MainWindowViewModel vm) {
						var t = vm.Threads.Value.Where(y => y.Url == x.FutabaContext?.Url).FirstOrDefault();
						if(t != null) {
							vm.ThreadTabSelectedItem.Value = t;
						}
					}
				});
		}

		private void SystemCommandsCanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = true;
		}
		private void CommandBindingMinimizeWindowCommand(object sender, ExecutedRoutedEventArgs e) {
			SystemCommands.MinimizeWindow(this);
		}
		private void CommandBindingRestoreWindowCommand(object sender, ExecutedRoutedEventArgs e) {
			SystemCommands.RestoreWindow(this);
		}
		private void CommandBindingMaximizeWindowCommand(object sender, ExecutedRoutedEventArgs e) {
			SystemCommands.MaximizeWindow(this);
		}
		private void CommandBindingCloseWindowCommand(object sender, ExecutedRoutedEventArgs e) {
			SystemCommands.CloseWindow(this);
		}

		protected override void OnSourceInitialized(EventArgs e) {
			base.OnSourceInitialized(e);

			if(Application.Current is App app) {
				try {
					var path = System.IO.Path.Combine(app.AppWorkDirectory, PlacementJsonFile);
					if(File.Exists(path)) {
						var hwnd = new WindowInteropHelper(this).Handle;
						var placement = Util.FileUtil.LoadJson<WinApi.WINDOWPLACEMENT>(path);
						placement.flags = 0;
						placement.showCmd = (placement.showCmd == WinApi.Win32.SW_SHOWMINIMIZED)
							? WinApi.Win32.SW_SHOWNORMAL : placement.showCmd;
						WinApi.Win32.SetWindowPlacement(hwnd, ref placement);
					}
				}
				catch(IOException) {  /* 何もしない */ }
				catch(Newtonsoft.Json.JsonSerializationException) { /* 何もしない */ }
			}
		}

		protected override void OnClosing(CancelEventArgs e) {
			base.OnClosing(e);

			if(!e.Cancel) {
				if(Application.Current is App app) {
					try {
						var hwnd = new WindowInteropHelper(this).Handle;
						var placement = new WinApi.WINDOWPLACEMENT();
						placement.length = System.Runtime.InteropServices.Marshal.SizeOf(typeof(WinApi.WINDOWPLACEMENT));
						WinApi.Win32.GetWindowPlacement(hwnd, ref placement);

						var path = System.IO.Path.Combine(app.AppWorkDirectory, PlacementJsonFile);
						Util.FileUtil.SaveJson(path, placement);
					}
					catch(IOException) {  /* 何もしない */ }
				}
			}
		}
	}
}