using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
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

namespace Yarukizero.Net.MakiMoki.Wpf.WpfHelpers {
	class FluentHelper {
		public class FluentSource {
			private HwndSource source;
			private FluentType type = FluentType.None;
			private (Color BaseColor, Color Accent) blurColor = (Color.FromArgb(0, 0, 0, 0), Color.FromArgb(0, 0, 0, 0));
			private bool enableDwmRound = false;
			private bool enableDwmRoundSmall = false;
			private bool isWindowMaxmized = false;

			public FluentSource(Visual obj) {
				this.source = (HwndSource)HwndSource.FromVisual(obj);
				if(this.source == null) {
					this.source = HwndSource.FromHwnd(new WindowInteropHelper(Window.GetWindow(obj)).Handle);
				}
				if(this.source == null) {
					throw new ArgumentException();
				}
				this.source.AddHook(new HwndSourceHook(this.WndProc));
			}

			private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
				const int WM_POWERBROADCAST = 0x218;
				const int WM_SIZE = 0x0005;
				const int SIZE_MAXIMIZED = 2;
				const int PBT_APMPOWERSTATUSCHANGE = 0xA;
				switch(msg) {
				case WM_SIZE when App.OsCompat.IsWindows11Rtm && this.enableDwmRound:
					if(wParam.ToInt32() == SIZE_MAXIMIZED) {
						this.isWindowMaxmized = true;
						this.EnableDwmRound(hwnd, false, this.enableDwmRoundSmall);
					} else {
						if(this.isWindowMaxmized) {
							this.isWindowMaxmized = false;
							this.EnableDwmRound(hwnd, true, this.enableDwmRoundSmall);
						}
					}
					break;
				case WM_POWERBROADCAST when wParam.ToInt32() == PBT_APMPOWERSTATUSCHANGE:
					if(this.type == FluentType.AcryicBlur) {
						if(this.IsPowerLine()) {
							//this.EnableAcryicBlur(hwnd, this.accentColor, true);
						} else {

						}
					}
					break;
				}
				return IntPtr.Zero;
			}

			private bool IsPowerLine() {
				bool NT_SUCCESS(uint v) {
					return v switch {
						var x when(0 <= x) && (x <= 0x3FFFFFFF) => true,
						var x when(0x40000000 <= x) && (x <= 0x7fffffff) => true,
						_ => false
					};
				}
				const int SystemBatteryState = 5;
				var state = new SYSTEM_BATTERY_STATE();
				if(NT_SUCCESS(CallNtPowerInformation(SystemBatteryState, IntPtr.Zero, 0, ref state, Marshal.SizeOf(typeof(SYSTEM_BATTERY_STATE))))) {
					return state.AcOnLine != 0;
				} else {
					return true;
				}
			}

			private void EnableDwmClient(IntPtr hwnd) {
				var mgn = new MARGINS() {
					cxLeftWidth = -1,
					cxRightWidth = -1,
					cyTopHeight = -1,
					cyBottomHeight = -1,
				};
				var s  = DwmExtendFrameIntoClientArea(hwnd, ref mgn);
				System.Diagnostics.Debug.WriteLine($"{s.ToInt32():x}");
			}

			private void EnableAeroBlur(IntPtr hwnd, Color? accent) {
				IntPtr pPolicy = IntPtr.Zero;
				try {
					this.source.CompositionTarget.BackgroundColor = accent ?? Color.FromArgb(0, 0, 0, 0);
					var policy = new ACCENT_POLICY() {
						AccentState = ACCENT_ENABLE_BLURBEHIND,
						AccentFlags = 2,
						GradientColor = 0,
						AnimationId = 0
					};
					pPolicy = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ACCENT_POLICY)));
					Marshal.StructureToPtr(policy, pPolicy, true);
					var d = new WINDOWCOMPOSITIONATTRIBDATA() {
						Attrib = WCA_ACCENT_POLICY,
						pvData = pPolicy,
						cbData = (IntPtr)Marshal.SizeOf(typeof(ACCENT_POLICY)),
					};
					SetWindowCompositionAttribute(hwnd, ref d);
				}
				finally {
					if(pPolicy != IntPtr.Zero) {
						Marshal.FreeHGlobal(pPolicy);
					}
				}
			}

			private void EnableAcryicBlur(IntPtr hwnd, (Color BaseColor, Color Accent) blur, bool powerLine) {
				IntPtr pPolicy = IntPtr.Zero;
				try {
					/*
					const int AccentFlagsDrawLeftBorder = 0x20;
					const int AccentFlagsDrawDrawTopBorder = 0x40;
					const int AccentFlagsDrawDrawRightBorder = 0x80;
					const int AccentFlagsDrawDrawBottomBorder = 0100; 
					*/
					var p = powerLine switch {
						true => (C: blur.Accent, P: new ACCENT_POLICY() {
							AccentState = ACCENT_ENABLE_ACRYLICBLURBEHIND,
							AccentFlags = 0, //AccentFlagsDrawLeftBorder | AccentFlagsDrawDrawTopBorder | AccentFlagsDrawDrawRightBorder | AccentFlagsDrawDrawBottomBorder,
							GradientColor = WpfUtil.ImageUtil.ToHsv(blur.BaseColor).V switch {
								var v when v < 50 => unchecked((int)(10 << 24 | (uint)blur.BaseColor.B << 16 | (uint)blur.BaseColor.G << 8 | (uint)blur.BaseColor.R)),
								_ => unchecked((int)(128u << 24 | (uint)blur.BaseColor.B << 16 | (uint)blur.BaseColor.G << 8 | (uint)blur.BaseColor.R)),
							},
							AnimationId = 0
						}),
						false => (C: blur.Accent, P: new ACCENT_POLICY() {
							AccentState = ACCENT_DISABLED,
							AccentFlags = 0,
							GradientColor = 0,
							AnimationId = 0
						})
					};
					this.source.CompositionTarget.BackgroundColor = p.C;
					pPolicy = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ACCENT_POLICY)));
					Marshal.StructureToPtr(p.P, pPolicy, true);
					var d = new WINDOWCOMPOSITIONATTRIBDATA() {
						Attrib = WCA_ACCENT_POLICY,
						pvData = pPolicy,
						cbData = (IntPtr)Marshal.SizeOf(typeof(ACCENT_POLICY)),
					};
					SetWindowCompositionAttribute(hwnd, ref d);
				}
				finally {
					if(pPolicy != IntPtr.Zero) {
						Marshal.FreeHGlobal(pPolicy);
					}
				}
			}

			private void DisableAeroBlur(IntPtr hwnd) {
				IntPtr pPolicy = IntPtr.Zero;
				try {
					//this.source.CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);
					var policy = new ACCENT_POLICY() {
						AccentState = ACCENT_DISABLED,
						AccentFlags = 0,
						GradientColor = 0,
						AnimationId = 0
					};
					pPolicy = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ACCENT_POLICY)));
					Marshal.StructureToPtr(policy, pPolicy, true);
					var d = new WINDOWCOMPOSITIONATTRIBDATA() {
						Attrib = WCA_ACCENT_POLICY,
						pvData = pPolicy,
						cbData = (IntPtr)Marshal.SizeOf(typeof(ACCENT_POLICY)),
					};
					SetWindowCompositionAttribute(hwnd, ref d);
				}
				finally {
					if(pPolicy != IntPtr.Zero) {
						Marshal.FreeHGlobal(pPolicy);
					}
				}
			}

			private void EnableMica(IntPtr hwnd, bool isDark) {
				// この処理はRTMでしか動かない
				this.source.CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);

				var hsv = WpfUtil.ImageUtil.ToHsv((Color)App.Current.Resources["MakimokiBackgroundColor"]);
				var @true = 1;
				var @false = 0;
				if(isDark) {
					DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref @true, sizeof(int));
				} else {
					DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref @false, sizeof(int));
				}
				DwmSetWindowAttribute(hwnd, DWMWA_MICA_EFFECT, ref @true, sizeof(int));
			}

			private void DisableMica(IntPtr hwnd) {
				// この処理はRTMでしか動かない
				var @false = 0;
				DwmSetWindowAttribute(hwnd, DWMWA_MICA_EFFECT, ref @false, sizeof(int));
			}

			private void EnableDwmRound(IntPtr hwnd, bool enable, bool small) {
				int DWM_WINDOW_CORNER_PREFERENCE = 33;
				if(enable) {
					if(small) {
						int DWMWCP_ROUNDSMALL = 3;
						DwmSetWindowAttribute(hwnd, DWM_WINDOW_CORNER_PREFERENCE, ref DWMWCP_ROUNDSMALL, sizeof(int));
					} else {
						int DWMWCP_ROUND = 2;
						DwmSetWindowAttribute(hwnd, DWM_WINDOW_CORNER_PREFERENCE, ref DWMWCP_ROUND, sizeof(int));
					}
				} else {
					int DWMWCP_DONOTROUND = 1;
					DwmSetWindowAttribute(hwnd, DWM_WINDOW_CORNER_PREFERENCE, ref DWMWCP_DONOTROUND, sizeof(int));
				}
			}


			public void EnableComposition(FluentType type, (Color BaseColor, Color Accent)? blur = null) {
				switch(this.type = type) {
				case FluentType.AeroBlur:
					if(App.OsCompat.IsWindows10Redstone5) {
						if(!blur.HasValue) {
							throw new ArgumentNullException();
						}
						this.EnableDwmClient(this.source.Handle);
						this.EnableAeroBlur(this.source.Handle, blur.Value.Accent) ;
					}
					break;
				case FluentType.AcryicBlur:
					if(App.OsCompat.IsWindows10Redstone5) {
						if(!blur.HasValue) {
							throw new ArgumentNullException();
						}
						this.blurColor = blur.Value;
						this.EnableDwmClient(this.source.Handle);
						EnableAcryicBlur(this.source.Handle, this.blurColor, this.IsPowerLine());
					}
					break;
				case FluentType.Mica:
					if(App.OsCompat.IsWindows11Rtm) {
						this.EnableDwmClient(this.source.Handle);
						this.EnableMica(
							this.source.Handle,
							WpfUtil.ImageUtil.ToHsv((Color)App.Current.Resources["MakimokiBackgroundColor"]).V < 50);
					}
					break;
				}
			}

			/*
			public void DisableComposition() {
				switch(this.type) {
				case FluentType.Blur:
				case FluentType.AcryicBlur:
					this.DisableBlur(this.source.Handle);
					break;
				case FluentType.Mica:
					if(App.OsCompat.IsWindows11Rtm) {
						this.DisableMica(this.source.Handle);
					}
					break;
				}
				this.type = FluentType.None;
			}
			*/

			public void EnableRound(bool isSmallRound = false) {
				if(App.OsCompat.IsWindows11Rtm) {
					this.enableDwmRound = true;
					this.enableDwmRoundSmall = isSmallRound;
					if(this.isWindowMaxmized = IsZoomed(this.source.Handle)) {
						this.EnableDwmRound(this.source.Handle, false, isSmallRound);
					} else {
						this.EnableDwmRound(this.source.Handle, true, isSmallRound);
					}
				}
			}
		}

		public enum FluentType {
			None,
			AeroBlur,
			AcryicBlur,
			Mica
		}


		[DllImport("Dwmapi.dll")]
		private static extern IntPtr DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS pMarInset);
		[DllImport("user32.dll")]
		private static extern bool SetWindowCompositionAttribute(IntPtr hwnd, ref WINDOWCOMPOSITIONATTRIBDATA data);
		[DllImport("Dwmapi.dll")]
		private static extern IntPtr DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);
		[DllImport("user32.dll")]
		private static extern bool SendMessage(IntPtr hwnd, int msg, IntPtr wP, IntPtr lP);
		[DllImport("user32.dll")]
		private static extern bool GetCursorPos(ref POINT p);
		[DllImport("user32.dll")]
		private static extern bool RedrawWindow(IntPtr hWnd, ref RECT lprcUpdate, IntPtr hrgnUpdate, int flags);
		[DllImport("user32.dll")]
		private static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, int flags);
		[DllImport("user32.dll")]
		private static extern bool IsZoomed(IntPtr hWnd);
		[DllImport("PowrProf.dll")]
		private static extern uint CallNtPowerInformation(int InformationLevel, IntPtr InputBuffer, int InputBufferLength, ref SYSTEM_BATTERY_STATE OutputBuffer, int OutputBufferLength);
		[StructLayout(LayoutKind.Sequential)]
		struct SYSTEM_BATTERY_STATE {
			public byte AcOnLine;
			public byte BatteryPresent;
			public byte Charging;
			public byte Discharging;
			public byte spare1;
			public byte spare2;
			public byte spare3;
			public byte spare4;
			public uint MaxCapacity;
			public uint RemainingCapacity;
			public int Rate;
			public uint EstimatedTime;
			public uint DefaultAlert1;
			public uint DefaultAlert2;
		}


		[StructLayout(LayoutKind.Sequential)]
		struct MARGINS {
			public int cxLeftWidth;
			public int cxRightWidth;
			public int cyTopHeight;
			public int cyBottomHeight;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct WINDOWCOMPOSITIONATTRIBDATA {
			public int Attrib;
			public IntPtr pvData;
			public IntPtr cbData;
		}
		[StructLayout(LayoutKind.Sequential)]
		struct ACCENT_POLICY {
			public int AccentState;
			public int AccentFlags;
			public int GradientColor;
			public int AnimationId;
		}
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
		private const int SystemPowerCapabilities = 4;

		private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
		private const int DWMWA_MICA_EFFECT = 1029;

		private const int ACCENT_DISABLED = 0;
		private const int ACCENT_ENABLE_GRADIENT = 1;
		private const int ACCENT_ENABLE_TRANSPARENTGRADIENT = 2;
		private const int ACCENT_ENABLE_BLURBEHIND = 3;
		private const int ACCENT_ENABLE_ACRYLICBLURBEHIND = 4;
		private const int ACCENT_ENABLE_HOSTBACKDROP = 5;
		private const int ACCENT_INVALID_STATE = 6;
		private const int WCA_ACCENT_POLICY = 19;

		public static FluentSource Attach(Visual o) => new FluentSource(o);

		private static void ApplyComposition(FluentSource source, bool isPopup, object type, object @base, object accent) {
			static FluentType auto() {
				if(App.OsCompat.IsWindows11Rtm) {
					//return FluentType.Mica; 22H2まで無効化
				}
				if(App.OsCompat.IsWindows10Redstone5) {
					return FluentType.AcryicBlur;
				}
				return FluentType.None;
			}
			source.EnableRound(isSmallRound: isPopup);
			source.EnableComposition(type switch {
				PlatformData.FluentType x when x == PlatformData.FluentType.None => FluentType.None,
				PlatformData.FluentType x when x == PlatformData.FluentType.Aero => FluentType.AeroBlur,
				PlatformData.FluentType x when x == PlatformData.FluentType.Acryic => FluentType.AcryicBlur,
				PlatformData.FluentType x when x == PlatformData.FluentType.Maica => FluentType.Mica,
				PlatformData.FluentType x when x == PlatformData.FluentType.Auto => auto(),
				_ => FluentType.None
			}, ((Color)@base, (Color)accent));
		}

		public static void ApplyCompositionWindow(FluentSource source)
			=> ApplyComposition(
				source,
				false,
				App.Current.Resources["FluentWindowType"],
				App.Current.Resources["MakimokiBackgroundColor"],
				App.Current.Resources["FluentWindowAccentColor"]);
		public static void ApplyCompositionPopup(FluentSource source)
			=> ApplyComposition(
				source,
				true,
				App.Current.Resources["FluentPopupType"],
				App.Current.Resources["MakimokiBackgroundColor"],
				App.Current.Resources["FluentPopupAccentColor"]);

		public static void ApplyPopupBackground(Control c) {
			if(App.Current.Resources["FluentPopupType"] switch {
				PlatformData.FluentType t when t == PlatformData.FluentType.None => false,
				PlatformData.FluentType t when t == PlatformData.FluentType.Auto && !App.OsCompat.IsWindows10Redstone5 => false,
				_ => true
			}) {
				c.Background = Brushes.Transparent;
			}
		}
		public static void ApplyPopupBackground(Border c) {
			if(App.Current.Resources["FluentPopupType"] switch {
				PlatformData.FluentType t when t == PlatformData.FluentType.None => false,
				PlatformData.FluentType t when t == PlatformData.FluentType.Auto && !App.OsCompat.IsWindows10Redstone5 => false,
				_ => true
			}) {
				c.Background = Brushes.Transparent;
			}
		}
	}
}
