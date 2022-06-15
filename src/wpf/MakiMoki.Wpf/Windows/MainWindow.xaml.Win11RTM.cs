using System;
using System.Collections.Generic;
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

namespace Yarukizero.Net.MakiMoki.Wpf.Windows {
	// Windows11 RTM用の互換処理
	public partial class MainWindow {
		[System.Runtime.InteropServices.DllImport("Dwmapi.dll")]
		private static extern IntPtr DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern bool SendMessage(IntPtr hwnd, int msg, IntPtr wP, IntPtr lP);
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern bool GetCursorPos(ref POINT p);
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern bool RedrawWindow(IntPtr hWnd, ref RECT lprcUpdate, IntPtr hrgnUpdate, int flags);
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, int flags);

		struct POINT {
			public int x;
			public int y;
		}
		struct RECT {
			public int left;
			public int top;
			public int right;
			public int bottom;
		}

		private bool win11SnapLayoutMouseOver = false;
		private bool win11SnapLayoutMousePress = false;
		private (
			bool MouseOver,
			bool MousePress,
			FrameworkElement Target,
			System.Windows.Media.Animation.Storyboard Storyboard) win11SnapLayoutLatestAction = (false, false, null, null);

		private void ApplyDwmRound(IntPtr hwnd, bool round) {
			int DWM_WINDOW_CORNER_PREFERENCE = 33;
			if(round) {
				int DWMWCP_ROUND = 2;
				DwmSetWindowAttribute(hwnd, DWM_WINDOW_CORNER_PREFERENCE, ref DWMWCP_ROUND, sizeof(int));
			} else {
				int DWMWCP_DONOTROUND = 1;
				DwmSetWindowAttribute(hwnd, DWM_WINDOW_CORNER_PREFERENCE, ref DWMWCP_DONOTROUND, sizeof(int));
			}
		}

		private IntPtr Win11SnapLayoutProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
			static bool hittest(FrameworkElement e, int x, int y) {
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

			void applyVisualState(bool mouseOver, bool mousePress) {
				static IEnumerable<FrameworkElement> enumrate(FrameworkElement e) {
					var q = new Queue<DependencyObject>();
					do {
						var parent = q.Any() switch {
							true => q.Dequeue(),
							false => e,
						};

						var count = VisualTreeHelper.GetChildrenCount(parent);
						for(var i = 0; i < count; i++) {
							var child = VisualTreeHelper.GetChild(parent, i);
							q.Enqueue(child);

							if(child is FrameworkElement el) {
								yield return el;
							}
						}
					}
					while(q.Count > 0);
				}

				static (FrameworkElement Obj, IEnumerable<VisualStateGroup> Grp) getGropus(FrameworkElement element) {
					if(VisualTreeHelper.GetChildrenCount(element) <= 0) {
						return (null, null);
					}

					foreach(var d in enumrate(element).OfType<FrameworkElement>()) {
						var groups = VisualStateManager.GetVisualStateGroups(d)?.Cast<VisualStateGroup>();
						if(groups != null) {
							return (d, groups);
						}
					}
					return (null, null);
				}
				var tp = getGropus(this.WindowState switch {
					WindowState ws when ws == WindowState.Maximized => this.restoreWindowButton,
					_ => this.maximizeWindowButton,
				});
				if((this.win11SnapLayoutLatestAction.MouseOver != mouseOver) || (this.win11SnapLayoutLatestAction.MousePress != mousePress)) {
					this.win11SnapLayoutLatestAction.Storyboard?.Stop(this.win11SnapLayoutLatestAction.Target);
				}
				if(tp.Grp != null) {
					var vg = tp.Grp.Where(x => x.Name == "CommonStates").FirstOrDefault();
					if(vg != null) {
						var sb = (P: mousePress, O: mouseOver) switch {
							var t when t.P => vg.States.Cast<VisualState>().Where(x => x.Name == "Pressed").FirstOrDefault()?.Storyboard,
							var t when t.O => vg.States.Cast<VisualState>().Where(x => x.Name == "MouseOver").FirstOrDefault()?.Storyboard,
							_ => vg.States.Cast<VisualState>().Where(x => x.Name == "Normal").FirstOrDefault()?.Storyboard,
						};
						if(sb != null) {
							try {
								// CurrentStateの読み込みがだいたい失敗するのでいらないかもしれない
								if(sb.GetCurrentState(tp.Obj) == System.Windows.Media.Animation.ClockState.Stopped) {
									sb.Begin(tp.Obj, true);
								}
							}
							catch(InvalidOperationException) {
								sb.Begin(tp.Obj, true);
							}
						}
						this.win11SnapLayoutLatestAction = (mouseOver, mousePress, tp.Obj, sb);
					}
				}
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
			const int HTMAXBUTTON = 9;
			switch(msg) {
			case WM_NCHITTEST:
				if(isMax(lParam)) {
					SendMessage(hwnd, WM_MOUSEMOVE, (IntPtr)makeWp(), lParam);
					if(!this.win11SnapLayoutMouseOver) {
						this.win11SnapLayoutMouseOver = true;
						applyVisualState(this.win11SnapLayoutMouseOver, this.win11SnapLayoutMousePress);
					}
					handled = true;
					return (IntPtr)HTMAXBUTTON;
				} else {
					if(this.win11SnapLayoutMouseOver || this.win11SnapLayoutMousePress) {
						applyVisualState(false, false);
					}
					this.win11SnapLayoutMouseOver = false;
					this.win11SnapLayoutMousePress = false;
				}
				break;
			case WM_NCLBUTTONDOWN:
				if(wParam.ToInt32() == HTMAXBUTTON) {
					if(!this.win11SnapLayoutMousePress) {
						this.win11SnapLayoutMousePress = true;
						applyVisualState(this.win11SnapLayoutMouseOver, this.win11SnapLayoutMousePress);
					}
					handled = true;
					return IntPtr.Zero;
				}
				break;
			case WM_NCLBUTTONUP:
				if((wParam.ToInt32() == HTMAXBUTTON) && this.win11SnapLayoutMousePress) {
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
					} else {
						/* これ入れたほうが標準のウインドウの動きになるけど他と動きが違うので除外
						if(this.win11SnapLayoutMouseOver || this.win11SnapLayoutMousePress) {
							applyVisualState(false, false);
						}
						this.win11SnapLayoutMouseOver = false;
						this.win11SnapLayoutMousePress = false;
						*/
					}
				}
				break;
			case WM_NCMOUSEMOVE:
				if(wParam.ToInt32() == HTMAXBUTTON) {
					SendMessage(hwnd, WM_MOUSEMOVE, (IntPtr)makeWp(), lParam);
					/*
					const int WM_SETREDRAW = 0x000B;
					const int RDW_ERASE = 0x004;
					const int RDW_INVALIDATE = 0x001;
					const int RDW_FRAME = 0x400;
					const int RDW_ALLCHILDREN = 0x080;
					SendMessage(hwnd, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
					RedrawWindow(hwnd, IntPtr.Zero, IntPtr.Zero,
						 RDW_INVALIDATE | RDW_FRAME | RDW_ALLCHILDREN);
					*/
					handled = true;
					return IntPtr.Zero;
				}
				break;
			case WM_MOUSEMOVE:
				if(!isMax(lParam) && (this.win11SnapLayoutMouseOver || this.win11SnapLayoutMousePress)) {
					applyVisualState(false, false);
					this.win11SnapLayoutMouseOver = false;
					this.win11SnapLayoutMousePress = false;
				}
				break;
			}
			return IntPtr.Zero;
		}
	}
}
