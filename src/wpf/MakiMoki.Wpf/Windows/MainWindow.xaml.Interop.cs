using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Yarukizero.Net.MakiMoki.Wpf.Windows {
	public partial class MainWindow {
		[DllImport("user32.dll")]
		private static extern bool SendMessage(IntPtr hwnd, int msg, IntPtr wP, IntPtr lP);
		[DllImport("user32.dll")]
		private static extern bool GetCursorPos(ref POINT p);
		[DllImport("user32.dll")]
		private static extern bool RedrawWindow(IntPtr hWnd, ref RECT lprcUpdate, IntPtr hrgnUpdate, int flags);
		[DllImport("user32.dll")]
		private static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, int flags);

		[StructLayout(LayoutKind.Sequential)]
		struct POINT {
			public int x;
			public int y;
		}
		[StructLayout(LayoutKind.Sequential)]
		struct RECT {
			public int left;
			public int top;
			public int right;
			public int bottom;
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
	}
}
