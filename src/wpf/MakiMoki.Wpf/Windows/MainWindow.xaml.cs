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
using Yarukizero.Net.MakiMoki.Wpf.WpfConfig;

namespace Yarukizero.Net.MakiMoki.Wpf.Windows {
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window {
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

			// WPF バグ対策 
			// https://github.com/dotnet/wpf/issues/6091
			ViewModels.MainWindowViewModel.Messenger.Instance
				.GetEvent<PubSubEvent<System.Windows.FrameworkElement>>()
				.Subscribe(async x => {
					System.Diagnostics.Debug.WriteLine("unload bag fix");
					var p = x.Parent as Panel;
					if(p != null) {
						p.Children.Remove(x);
						await Task.Yield();
						x.DataContext = null;
						this.BitmapRemoveConainer.Children.Add(x);
						await Task.Yield();
						x.UpdateLayout();
						await Task.Yield();
						this.BitmapRemoveConainer.Children.Remove(x);
						//p.Children.Add(x);
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

			var placement = WpfConfigLoader.Placement.WindowPlacement;
			if(placement.length != 0) { // 何も設定されていない場合初期値0が入っているのでそれで判定
				var hwnd = new WindowInteropHelper(this).Handle;
				placement.flags = 0;
				placement.showCmd = (placement.showCmd == WinApi.Win32.SW_SHOWMINIMIZED)
					? WinApi.Win32.SW_SHOWNORMAL : placement.showCmd;
				WinApi.Win32.SetWindowPlacement(hwnd, ref placement);
			}
		}

		protected override void OnClosing(CancelEventArgs e) {
			base.OnClosing(e);

			if(!e.Cancel) {
				WpfConfig.WpfConfigLoader.UpdatePlacementByWindowClosing(this);
			}
		}

		private void OnLoadedGC(object sender, RoutedEventArgs e) {
#if DEBUG
			if(e.OriginalSource is UIElement el) {
				el.Visibility = Visibility.Visible;
			}
#endif
		}

		private void OnClickGC(object sender, RoutedEventArgs e) {
			// テスト用強制GC
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}
	}
}