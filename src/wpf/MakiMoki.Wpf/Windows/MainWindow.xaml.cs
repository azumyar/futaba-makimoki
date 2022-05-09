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
				.GetEvent<PubSubEvent<ViewModels.MainWindowViewModel.WpfBugMessage>>()
				.Subscribe(async x => {
					// 現在は別アプローチで対策したので使っていない
					static async Task<(bool Sucessed, int? Index)> preRemove(FrameworkElement el) {
						if(el is Panel p) {
							var r = p.Children.IndexOf(el);
							p.Children.Remove(el);
							await Task.Yield();
							return (true, r);
						} else if(el is global::System.Windows.Controls.Decorator d) {
							d.Child = null;
							await Task.Yield();
							return (true, null);
						} if(el is Image) {
							await Task.Yield();
							return (true, null);
						} else {
							System.Diagnostics.Debug.WriteLine($"!!!!!!! 不明なWPFBugコンテナ { el.GetType() } !!!!!!!!");
							await Task.Yield();
							return (false, null);
						}
					}
					static async Task remove(FrameworkElement el, Grid removeConainer) {
						try {
							removeConainer.Children.Add(el);
							await Task.Yield();
							el.UpdateLayout();
							await Task.Yield();
							removeConainer.Children.Remove(el);
						}
						catch(InvalidOperationException) { } // 再利用されるとくる再利用されて場合は多分リークしないので無視
					}

					System.Diagnostics.Debug.WriteLine("unload bag fix");
					if(x.Remove) {
						var el = x.Element;
						var r = await preRemove(el);
						if(r.Sucessed) {
							el.DataContext = null;						
							await remove(el, this.BitmapRemoveConainer);
						}
					} else {
						var el = x.Element;
						if(el is Image im) {
							var prt = im.Parent;
							var r = await preRemove(el);
							if(r.Sucessed) {
								await remove(el, this.BitmapRemoveConainer);
								if(el is Panel p && r.Index.HasValue) {
									p.Children.Insert(r.Index.Value, el);
									await Task.Yield();
								} else if(el is global::System.Windows.Controls.Decorator d) {
									d.Child = el;
									await Task.Yield();
								}
							}
						} else {
							System.Diagnostics.Debug.WriteLine($"!!!!!!! 不明なWPFBugオブジェクト { x.GetType() } !!!!!!!!");
							await Task.Yield();
							return;
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

		private void OnImageUnloaded(object sender, RoutedEventArgs e) {
			if(sender is FrameworkElement o) {
				BindingOperations.ClearAllBindings(o);
				if(o.DataContext != null) {
					ViewModels.MainWindowViewModel.Messenger.Instance
						.GetEvent<PubSubEvent<ViewModels.MainWindowViewModel.WpfBugMessage>>()
						.Publish(new ViewModels.MainWindowViewModel.WpfBugMessage(o));
				}
			}
		}

	}
}