using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace Yarukizero.Net.MakiMoki.Wpf.WpfHelpers {
	internal class MouseGesture : IDisposable {
		private const int WH_MOUSE = 7;
		private const int WH_MOUSE_LL = 14;
		private const int HC_ACTION = 0;
		private const int WM_LBUTTONDOWN = 0x201;
		private const int WM_LBUTTONUP = 0x202;
		private const int WM_MBUTTONDOWN = 0x207;
		private const int WM_MBUTTONUP = 0x208;
		private const int WM_RBUTTONDOWN = 0x204;
		private const int WM_RBUTTONUP = 0x205;
		private const int WM_MOUSEMOVE = 0x200;
		private const int WM_MOUSEWHEEL = 0x20A;
		private const int WM_XBUTTONDOWN = 0x20B;
		private const int WM_XBUTTONUP = 0x20C;
		[StructLayout(LayoutKind.Sequential)]
		struct POINT {
			public int x;
			public int y;
		}
		[StructLayout(LayoutKind.Sequential)]
		struct MSLLHOOKSTRUCT {
			public POINT pt;
			public int mouseData;
			public int flags;
			public int time;
			public IntPtr dwExtraInfo;
		}
		[DllImport("user32.dll")]
		private static extern bool GetCursorPos(ref POINT pt);
		[DllImport("user32.dll")]
		private static extern IntPtr SetWindowsHookEx(int idHook, MouseProc lpfn, IntPtr hMod, int dwThreadId);
		[DllImport("user32.dll")]
		private static extern IntPtr CallNextHookEx(IntPtr hHook, int nCode, IntPtr wParam, ref MSLLHOOKSTRUCT lParam);
		[DllImport("user32.dll")]
		private static extern bool UnhookWindowsHookEx(IntPtr hHook); 
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate IntPtr MouseProc(int nCode, IntPtr wParam, ref MSLLHOOKSTRUCT lParam);

		private readonly double MoveDelta = 5d;
		private readonly double CommandDelta = 36d;
		private readonly int MaxCommand = 10;

		private Window target;
		private IDisposable timer;
		private bool active;
		private (int X, int Y) startPosition;
		private (int X, int Y) prevPosition;
		private (int X, int Y) activePosition;
		private readonly List<PlatformData.MouseGestureCommand> commands = new List<PlatformData.MouseGestureCommand>();

		public Action<PlatformData.MouseGestureCommands> Update { get; set; }
		public Func<PlatformData.MouseGestureCommands, bool> Fire { get; set; }
		private bool prevCancelState = false;
		private IntPtr hook;
		private readonly HwndSource hwndSource;
		private readonly HwndSourceHook hwndSourceHook;
		private readonly MouseProc hookProc;

		public MouseGesture(DependencyObject target) {
			IntPtr wndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
				//const int WM_RBUTTONDOWN = 0x0204;
				// WPFイベントが処理前のウインドウメッセージでジェスチャ処理を開始する
				if(msg == WM_RBUTTONDOWN) {
					this.StartGesture();
				}
				return IntPtr.Zero;
			}
			this.target = Window.GetWindow(target);
			this.hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(this.target).Handle);
			this.hwndSource.AddHook(this.hwndSourceHook = new HwndSourceHook(wndProc));

			this.hookProc = HookProc;
			this.timer = Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(50))
				.ObserveOn(global::Reactive.Bindings.UIDispatcherScheduler.Default)
				.Subscribe(_ => OnTimer());
		}

		public void Dispose() {
			this.hwndSource.RemoveHook(this.hwndSourceHook);
			this.ReleaseGesture();
			this.timer.Dispose();
		}

		private IntPtr HookProc(int nCode, IntPtr wParam, ref MSLLHOOKSTRUCT lParam) {
			switch(wParam.ToInt32()) {
			case WM_MOUSEMOVE:
				this.OnMouseMove(lParam.pt.x, lParam.pt.y);
				break;

			case WM_RBUTTONUP:
				this.OnMouseUp();
				this.ReleaseGesture();
				break;

			case WM_LBUTTONDOWN:
			case WM_MBUTTONDOWN:
			case WM_XBUTTONDOWN:
				this.Reset();
				this.ReleaseGesture();
				break;
			}
			return CallNextHookEx(this.hook, nCode, wParam, ref lParam);
		}

		private void StartGesture() {
			if(this.hook == IntPtr.Zero) {
				System.Diagnostics.Debug.WriteLine($"マウスジェスチャ:開始");

				var pt = new POINT();
				GetCursorPos(ref pt);
				this.active = true;
				this.startPosition = (pt.x, pt.y);
				this.prevPosition = (pt.x, pt.y);
				this.activePosition = (pt.x, pt.y);
				this.hook = SetWindowsHookEx(WH_MOUSE_LL,
					this.hookProc,
#if DEBUG
					// vshostとか嚙まされると困るので自分のインスタンスを引っ張る
					Marshal.GetHINSTANCE(typeof(MouseGesture).Module),
#else
					// バイナリ統合するとこっちじゃないと正しいインスタンスが取れない
					GetModuleHandle(null),
#endif
					0);
			}
		}
		private void ReleaseGesture() {
			if(this.hook != IntPtr.Zero) {
				UnhookWindowsHookEx(this.hook);
				this.hook = IntPtr.Zero;
			}
		}

		private void OnMouseMove(int x, int y) {
			this.activePosition = (x, y);
		}

		private void OnMouseUp() {
#if DEBUG
			System.Diagnostics.Debug.WriteLine($"マウスジェスチャ:確定{new PlatformData.MouseGestureCommands(this.commands)}");
#endif
			this.prevCancelState = this.DoFire();
			this.target.ReleaseMouseCapture();
		}

		private void OnTimer() {
			static double length(double x, double y) => Math.Sqrt(x * x + y * y);
			if(this.active) {
				// 前回位置よりdelta px以上移動した場合コマンド入力とみなす
				var moveLength = length(this.activePosition.X - this.prevPosition.X, this.activePosition.Y - this.prevPosition.Y);
				if(this.MoveDelta < moveLength) {
					var degree = Math.Atan2(this.activePosition.Y - this.prevPosition.Y, this.activePosition.X - this.prevPosition.X) * 180d / Math.PI + 180d;
					//System.Diagnostics.Debug.WriteLine($"__マウスジェスチャ:{(int)degree}");
					var command = degree switch {
						double d when(0 <= d) && (d < 45) => PlatformData.MouseGestureCommand.Left,
						double d when(d < 135) => PlatformData.MouseGestureCommand.Up,
						double d when(d < 225) => PlatformData.MouseGestureCommand.Right,
						double d when(d < 315) => PlatformData.MouseGestureCommand.Down,
						_ => PlatformData.MouseGestureCommand.Left
					};
					if(!this.commands.Any()) {
						this.commands.Add(command);
						this.Update?.Invoke(new PlatformData.MouseGestureCommands(this.commands));
#if DEBUG
						System.Diagnostics.Debug.WriteLine($"マウスジェスチャ:{new PlatformData.MouseGestureCommands(this.commands)}");
#endif
						// コマンド入力からキャプチャする
						this.target.CaptureMouse();
						this.target.MouseRightButtonUp += OnCaptureLButtonUp;
					} else {
						var commandLength = length(this.activePosition.X - this.startPosition.X, this.activePosition.Y - this.startPosition.Y);
						if((this.commands.Last() != command) && (this.CommandDelta < commandLength)) {
							if(this.commands.Count < MaxCommand) {
								this.commands.Add(command);
							}
							this.startPosition = this.activePosition;
							this.Update?.Invoke(new PlatformData.MouseGestureCommands(this.commands));
#if DEBUG
							System.Diagnostics.Debug.WriteLine($"マウスジェスチャ:{new PlatformData.MouseGestureCommands(this.commands)}");
#endif
						}
					}
					// 前回位置を更新
					this.prevPosition = this.activePosition;
				}
			}
		}

		private void OnCaptureLButtonUp(object _, MouseButtonEventArgs e) {
			e.Handled = this.prevCancelState;
			this.prevCancelState = false;
			this.target.MouseRightButtonUp -= OnCaptureLButtonUp;
		}

		private bool DoFire() {
			var b = this.Fire?.Invoke(new PlatformData.MouseGestureCommands(this.commands));
			this.Reset();
			return b ?? false;
		}

		private void Reset() {
			this.active = false;
			this.startPosition = default;
			this.prevPosition = default;
			this.activePosition = default;
			this.commands.Clear();
			this.Update?.Invoke(new PlatformData.MouseGestureCommands(this.commands));
			if(this.target.IsMouseCaptured) {
				this.target.ReleaseMouseCapture();
			}
		}
	}
}
