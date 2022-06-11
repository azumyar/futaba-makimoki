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
		[System.Runtime.InteropServices.DllImport("Dwmapi.dll")]
		private static extern IntPtr DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern bool SendMessage(IntPtr hwnd, int msg, IntPtr wP, IntPtr lP);
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern bool GetCursorPos(ref POINT p);
		struct POINT {
			public int x;
			public int y;
		}

		public MainWindow() {
			InitializeComponent();

			var isWindows11RTM = false;
			if(Environment.OSVersion.Platform == PlatformID.Win32NT) {
				var win11RTM = new Version(10, 0, 22000);
				if(isWindows11RTM = (win11RTM <= Environment.OSVersion.Version)) {
					// Windows11でウインドウ角を丸くする
					var hwnd = new WindowInteropHelper(GetWindow(this)).EnsureHandle();
					int DWM_WINDOW_CORNER_PREFERENCE = 33;
					int DWMWCP_ROUND = 2;
					DwmSetWindowAttribute(hwnd, DWM_WINDOW_CORNER_PREFERENCE, ref DWMWCP_ROUND, sizeof(int));
				}
			}
			this.Loaded += (_, _) => {
				IntPtr wndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
					if(isWindows11RTM) {
						/* 無効化
						var r = this.Win11SnapLayoutProc(hwnd, msg, wParam, lParam, ref handled);
						if(handled) {
							return r;
						}
						*/
					}
					return IntPtr.Zero;
				}

				HwndSource.FromHwnd(new WindowInteropHelper(GetWindow(this)).Handle)
					.AddHook(new HwndSourceHook(wndProc));
			};

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

		private IntPtr Win11SnapLayoutProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
			bool hittest(FrameworkElement e, int x, int y) {
				var p = e.PointFromScreen(new Point(x, y));
				return (0 <= p.X) && (p.X <= e.ActualWidth)
					&& (0 <= p.Y) && (p.Y <= e.ActualHeight);
			}
			bool isMax(IntPtr lP) {
				foreach(var b in new[] {
							this.maximizeWindowButton,
							this.restoreWindowButton,
						}) {
					var l = (uint)lP.ToInt64();
					if(b.Visibility == Visibility.Visible) {
						if(hittest(b, (int)(short)(l & 0xffff), (int)(short)(l >> 16 & 0xffff))) {
							return true;
						}
					}
				}
				return false;
			}
			int makeWp() {
				const int MK_CONTROL = 0x0008;
				const int MK_LBUTTON = 0x0001;
				const int MK_MBUTTON = 0x0010;
				const int MK_RBUTTON = 0x0002;
				const int MK_SHIFT = 0x0004;
				const int MK_XBUTTON1 = 0x0020;
				const int MK_XBUTTON2 = 0x0040;

				var r = 0;
				foreach(var it in new (MouseButtonState State, int Value)[] {
					(Mouse.LeftButton, MK_LBUTTON),
					(Mouse.MiddleButton, MK_MBUTTON),
					(Mouse.RightButton, MK_RBUTTON),
					(Mouse.XButton1, MK_XBUTTON1),
					(Mouse.XButton2, MK_XBUTTON2),
				}) {
					r |= it.State switch {
						MouseButtonState.Pressed => it.Value,
						_ => 0
					};
				}
				foreach(var it in new (Key Key, int Value)[] {
					(Key.LeftCtrl, MK_CONTROL),
					(Key.RightCtrl, MK_CONTROL),
					(Key.LeftShift, MK_SHIFT),
					(Key.RightShift, MK_SHIFT),
				}) {
					r |= Keyboard.IsKeyDown(it.Key) switch {
						true => it.Value,
						false => 0,
					};
				}
				return r;
			}

			const int WM_NCHITTEST = 0x0084;
			const int WM_NCMOUSEHOVER = 0x02A0;
			const int WM_NCMOUSELEAVE = 0x02A2;
			const int WM_NCMOUSEMOVE = 0x00A0;
			const int WM_NCLBUTTONDOWN = 0x00A1;
			const int WM_NCLBUTTONUP = 0x00A2;
			const int WM_LBUTTONDOWN = 0x0201;
			const int WM_LBUTTONUP = 0x0202;
			const int WM_MOUSEHOVER = 0x02A1;
			const int WM_MOUSEMOVE = 0x0200;
			const int WM_MOUSELEAVE = 0x02A3;
			switch(msg) {
			case WM_NCHITTEST:
				if(isMax(lParam)) {
					const int HTMAXBUTTON = 9;
					handled = true;
					return (IntPtr)HTMAXBUTTON;
				}
				break;
			case WM_NCLBUTTONDOWN:
				if(isMax(lParam)) {
					if(new System.Windows.Automation.Peers.ButtonAutomationPeer(this.WindowState switch {
						WindowState ws when ws == WindowState.Maximized => this.restoreWindowButton,
						_ => this.maximizeWindowButton,
					}).GetPattern(System.Windows.Automation.Peers.PatternInterface.Invoke) is System.Windows.Automation.Provider.IInvokeProvider ip) {
						ip.Invoke();
						handled = true;
						return IntPtr.Zero;
					}
				}
				break;
			case WM_MOUSELEAVE: {
					var p = default(POINT);
					GetCursorPos(ref p);
					if(hittest(this.WindowState switch {
						WindowState ws when ws == WindowState.Maximized => this.restoreWindowButton,
						_ => this.maximizeWindowButton
					}, p.x, p.y)) {
						handled = true;
						return IntPtr.Zero;
					}
				}
				break;
			case WM_NCMOUSEMOVE:
				if(isMax(lParam)) {
					SendMessage(hwnd, WM_MOUSEMOVE, (IntPtr)makeWp(), lParam);
					handled = true;
					return IntPtr.Zero;
				}
				break;
			}
			return IntPtr.Zero;
		}
	}
}