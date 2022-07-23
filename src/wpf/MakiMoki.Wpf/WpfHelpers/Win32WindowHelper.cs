using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
namespace Yarukizero.Net.MakiMoki.Wpf.WpfHelpers {
	internal class Win32WindowHelper {
		public class WindowLongSource {
			[DllImport("user32.dll", CharSet = CharSet.Unicode)]
			private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

			[DllImport("user32.dll", CharSet = CharSet.Unicode)]
			private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

			[DllImport("user32.dll", CharSet = CharSet.Unicode)]
			private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int newLong);

			[DllImport("user32.dll", CharSet = CharSet.Unicode)]
			private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr newLong);

			const int GWL_WNDPROC = -4;
			const int GWL_HINSTANCE = -6;
			const int GWL_HWNDPARENT = -8;
			const int GWL_ID = -12;
			const int GWL_STYLE = -16;
			const int GWL_EXSTYLE = -20;
			const int GWL_USERDATA = -21;

			public IntPtr Handle { get; }
			internal WindowLongSource(IntPtr hwnd) {
				this.Handle = hwnd;
			}

			public int Id {
				get => GetLong(GWL_ID).ToInt32();
				set => SetLong(GWL_ID, value);
			}
			public int Style {
				get => GetLong(GWL_STYLE).ToInt32();
				set => SetLong(GWL_STYLE, value);
			}
			public int ExStyle {
				get => GetLong(GWL_EXSTYLE).ToInt32();
				set => SetLong(GWL_EXSTYLE, value);
			}
			public IntPtr Owner {
				get => GetLong(GWL_HWNDPARENT);
				set => SetLong(GWL_HWNDPARENT, value);
			}
			public IntPtr UserData {
				get => GetLong(GWL_USERDATA);
				set => SetLong(GWL_USERDATA, value);
			}

			private IntPtr GetLong(int nIndex) {
				return IntPtr.Size switch {
					4 => (IntPtr)GetWindowLong(this.Handle, nIndex),
					8 => GetWindowLongPtr(this.Handle, nIndex),
					_ => throw new InvalidOperationException(),
				};
			}
			private IntPtr SetLong(int nIndex, int newLong) => SetLong(nIndex, (IntPtr)newLong);
			private IntPtr SetLong(int nIndex, IntPtr newLong) {
				var p = IntPtr.Size switch {
					4 => (IntPtr)SetWindowLong(this.Handle, nIndex, newLong.ToInt32()),
					8 => SetWindowLongPtr(this.Handle, nIndex, newLong),
					_ => throw new InvalidOperationException(),
				};
				return p;
			}
		}

		public IntPtr Handle { get; }
		public WindowLongSource WindowLong { get; }

		public Win32WindowHelper(IntPtr hwnd) {
			this.Handle = hwnd;
			this.WindowLong = new WindowLongSource(hwnd);
		}
	}
}
