using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Yarukizero.Net.MakiMoki.Wpf.WinApi {
	static class Win32 {
		public static readonly int WPF_ASYNCWINDOWPLACEMENT = 0x0004;
		public static readonly int WPF_RESTORETOMAXIMIZED = 0x0002;
		public static readonly int WPF_SETMINPOSITION = 0x0001;

		public static readonly int SW_HIDE = 0;
		public static readonly int SW_MAXIMIZE = 3;
		public static readonly int SW_MINIMIZE = 6;
		public static readonly int SW_RESTORE = 9;
		public static readonly int SW_SHOW = 5;
		public static readonly int SW_SHOWMAXIMIZED = 3;
		public static readonly int SW_SHOWMINIMIZED = 2;
		public static readonly int SW_SHOWMINNOACTIVE = 7;
		public static readonly int SW_SHOWNA = 8;
		public static readonly int SW_SHOWNOACTIVATE = 4;
		public static readonly int SW_SHOWNORMAL = 1;

		[DllImport("user32.dll")]
		public static extern bool SetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

		[DllImport("user32.dll")]
		public static extern bool GetWindowPlacement(IntPtr hwnd, ref WINDOWPLACEMENT lpwndpl);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		public static extern bool SetDllDirectory([MarshalAs(UnmanagedType.LPWStr)]string lpPathName);

		[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
		public static extern IntPtr ShellExecute(
			IntPtr hwnd,
			[MarshalAs(UnmanagedType.LPWStr)] string lpVerb,
			[MarshalAs(UnmanagedType.LPWStr)] string lpFile,
			[MarshalAs(UnmanagedType.LPWStr)] string lpParameters,
			[MarshalAs(UnmanagedType.LPWStr)] string lpDirectory,
			int nShowCmd);
	}

	[StructLayout(LayoutKind.Sequential)]
	struct WINDOWPLACEMENT {
		[JsonProperty]
		public int length;
		[JsonProperty]
		public int flags;
		[JsonProperty]
		public int showCmd;
		[JsonProperty]
		public POINT ptMinPosition;
		[JsonProperty]
		public POINT ptMaxPosition;
		[JsonProperty]
		public RECT rcNormalPosition;
		// #ifdef _MAC
		//[JsonProperty]
		//public RECT rcDevice;
		// #endif
	}

	[StructLayout(LayoutKind.Sequential)]
	struct POINT {
		[JsonProperty]
		public int x;
		[JsonProperty]
		public int y;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct RECT {
		[JsonProperty]
		public int left;
		[JsonProperty]
		public int top;
		[JsonProperty]
		public int right;
		[JsonProperty]
		public int bottom;
	}
}
