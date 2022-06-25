using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yarukizero.Net.MakiMoki.Wpf.WpfUtil {
	internal class GlobalMouseHook {
		[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		public struct POINT {
			public int x;
			public int y;
		}

		[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		public struct MSLLHOOKSTRUCT {
			public POINT pt;
			public int mouseData;
			public int flags;
			public int time;
			public IntPtr dwExtraInfo;
		}

		[System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.Cdecl)]
		private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, ref MSLLHOOKSTRUCT lParam);
		[System.Runtime.InteropServices.DllImport("kernel32.dll")]
		private static extern IntPtr GetModuleHandle(string lpModuleName);
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, int dwThreadId);
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern IntPtr CallNextHookEx(IntPtr hHook, int nCode, IntPtr wParam, ref MSLLHOOKSTRUCT lParam);
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern bool UnhookWindowsHookEx(IntPtr hHook);

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

		private const int WHEEL_DELTA = 120;

		private const int XBUTTON1 = 0x1;
		private const int XBUTTON2 = 0x2;


		public enum MouseButtons {
			None,
			Left,
			Right,
			Middle,
			XButton1,
			XButton2
		}

		public sealed class MouseCaptureEventArgs : EventArgs {
			private int m_nativeWParam;
			private MSLLHOOKSTRUCT m_nativeLParam;

			public MouseButtons Button { get; }
			public int X { get; }
			public int Y { get; }
			public int Delta { get; }
			public int Time { get; }

			public bool Cancel { get; private set; } = false;
			public int NativeWParam {
				set {
					this.m_nativeWParam = value;
					this.IsValueUpdate = true;
				}
				get { return this.m_nativeWParam; }
			}
			public MSLLHOOKSTRUCT NativeLParam {
				set {
					this.m_nativeLParam = value;
					this.IsValueUpdate = true;
				}
				get { return this.m_nativeLParam; }
			}
			internal bool IsValueUpdate { get; private set; } = false;
			internal MouseCaptureEventArgs(int wParam, MSLLHOOKSTRUCT lParam) {
				this.m_nativeWParam = wParam;
				this.m_nativeLParam = lParam;

				this.Button = wParam switch {
					WM_LBUTTONDOWN => MouseButtons.Left,
					WM_LBUTTONUP => MouseButtons.Left,
					WM_MBUTTONDOWN => MouseButtons.Middle,
					WM_MBUTTONUP => MouseButtons.Middle,
					WM_RBUTTONDOWN => MouseButtons.Right,
					WM_RBUTTONUP => MouseButtons.Right,
					WM_XBUTTONDOWN when(lParam.mouseData >> 16) == XBUTTON1 => MouseButtons.XButton1,
					WM_XBUTTONUP when(lParam.mouseData >> 16) == XBUTTON1 => MouseButtons.XButton1,
					WM_XBUTTONDOWN when(lParam.mouseData >> 16) == XBUTTON2 => MouseButtons.XButton2,
					WM_XBUTTONUP when(lParam.mouseData >> 16) == XBUTTON2 => MouseButtons.XButton2,
					_ => MouseButtons.None,
				};
				this.X = lParam.pt.x;
				this.Y = lParam.pt.y;
				this.Delta = wParam switch {
					WM_MOUSEWHEEL => (int)((short)(lParam.mouseData >> 16)),
					_ => 0
				};
				this.Time = lParam.time;
			}
		}

		private static IntPtr s_hook;
		private static LowLevelMouseProc s_proc; // ピン止めする
		public static event EventHandler<MouseCaptureEventArgs> MouseDown;
		public static event EventHandler<MouseCaptureEventArgs> MouseUp;
		public static event EventHandler<MouseCaptureEventArgs> MouseMove;
		public static event EventHandler<MouseCaptureEventArgs> MouseWheel;
		static GlobalMouseHook() {
			s_hook = SetWindowsHookEx(WH_MOUSE_LL,
				s_proc = HookProc,
				System.Runtime.InteropServices.Marshal.GetHINSTANCE(typeof(GlobalMouseHook).Module),
				0);
			AppDomain.CurrentDomain.DomainUnload += (_, _) => {
				if(s_hook != IntPtr.Zero) {
					UnhookWindowsHookEx(s_hook);
				}
			};
		}

		private static IntPtr HookProc(int nCode, IntPtr wParam, ref MSLLHOOKSTRUCT lParam) {
			bool cancel = false;
			if(nCode == HC_ACTION) {
				var p = new MouseCaptureEventArgs(wParam.ToInt32(), lParam);
				var e = wParam.ToInt32() switch {
					WM_LBUTTONDOWN => MouseDown,
					WM_MBUTTONDOWN => MouseDown,
					WM_RBUTTONDOWN => MouseDown,
					WM_XBUTTONDOWN => MouseDown,

					WM_LBUTTONUP => MouseUp,
					WM_MBUTTONUP => MouseUp,
					WM_RBUTTONUP => MouseUp,
					WM_XBUTTONUP => MouseUp,

					WM_MOUSEMOVE => MouseMove,
					WM_MOUSEWHEEL => MouseWheel,
					_ => null,
				};
				if(e != null) {
					e.Invoke(null, p);
					if(p.IsValueUpdate) {
						wParam = (IntPtr)p.NativeWParam;
						lParam = p.NativeLParam;
					}
					cancel = p.Cancel;
				}
			}
			return cancel ? (IntPtr)1 : CallNextHookEx(s_hook, nCode, wParam, ref lParam);
		}

		public static bool IsCapture {
			get { return s_hook != IntPtr.Zero; } 
		}
	}
}
