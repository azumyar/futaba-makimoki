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
using System.Windows.Controls.Primitives;

namespace Yarukizero.Net.MakiMoki.Wpf.WpfHelpers {
	class FluentHelper {
		public class FluentSource {
			protected HwndSource Source { get; }
			private FluentType type = FluentType.None;
			private (Color BaseColor, Color Accent) blurColor = (Color.FromArgb(0, 0, 0, 0), Color.FromArgb(0, 0, 0, 0));
			private bool enableDwmRound = false;
			private bool enableDwmRoundSmall = false;
			private bool isWindowMaxmized = false;

			public FluentSource(Visual obj) {
				this.Source = (HwndSource)HwndSource.FromVisual(obj);
				if(this.Source == null) {
					var h = new WindowInteropHelper(Window.GetWindow(obj));
					var hwnd = h.Handle;
					if(hwnd == IntPtr.Zero) {
						hwnd = h.EnsureHandle();
					}
					this.Source = HwndSource.FromHwnd(hwnd);
				}
				if(this.Source == null) {
					throw new ArgumentException();
				}
				this.Source.AddHook(new HwndSourceHook(this.WndProc));
			}

			protected virtual IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
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
				case WM_POWERBROADCAST when App.OsCompat.IsWindows10Redstone5 && (wParam.ToInt32() == PBT_APMPOWERSTATUSCHANGE): {
						Action<IntPtr, (Color, Color), bool> f = this.type switch {
							FluentType.AeroBlur => (x, y, z) => this.EnableAeroBlur(x, y, z),
							FluentType.AcryicBlur => (x, y, z) => this.EnableAcryicBlur(x, y, z),
							_ => null
						};
						f?.Invoke(hwnd, this.blurColor, this.IsPowerLine());
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

			private void EnableDwmClient(IntPtr hwnd, bool enable) {
				const int WS_EX_NOREDIRECTIONBITMAP = 0x00200000;
				const int GWL_EXSTYLE = -20;

				var v = enable switch {
					true => -1,
					false => 0,
				};
				var mgn = new MARGINS() {
					cxLeftWidth = v,
					cxRightWidth = v,
					cyTopHeight = v,
					cyBottomHeight = v,
				};
				DwmExtendFrameIntoClientArea(hwnd, ref mgn);
				/*
				var ex = GetWindowLongValue(hwnd, GWL_EXSTYLE).ToInt32();
				System.Diagnostics.Debug.WriteLine($"EX_STYLE  = {ex & WS_EX_NOREDIRECTIONBITMAP}");
				/*
				if(!enable && ((ex & WS_EX_NOREDIRECTIONBITMAP) == WS_EX_NOREDIRECTIONBITMAP)) {
					return;
				}
				*/
				/*
				SetWindowLongValue(hwnd, GWL_EXSTYLE, enable switch {
					true => (IntPtr)(ex | WS_EX_NOREDIRECTIONBITMAP),
					false => (IntPtr)(ex & ~WS_EX_NOREDIRECTIONBITMAP),
				});
				*/
			}

			private void SetComposition(IntPtr hwnd, ACCENT_POLICY policy) {
				IntPtr pPolicy = IntPtr.Zero;
				try {
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

			private void EnableAeroBlur(IntPtr hwnd, (Color BaseColor, Color Accent) blur, bool powerLine) {
				var p = powerLine switch {
					true => (C: blur.Accent, P: new ACCENT_POLICY() {
						AccentState = ACCENT_ENABLE_BLURBEHIND,
						AccentFlags = 2,
						GradientColor = 0,
						AnimationId = 0
					}),
					false => (C: Color.FromRgb(blur.Accent.R, blur.Accent.G, blur.Accent.B), P: new ACCENT_POLICY() {
						AccentState = ACCENT_DISABLED,
						AccentFlags = 0,
						GradientColor = 0,
						AnimationId = 0
					})
				};
				this.Source.CompositionTarget.BackgroundColor = p.C;
				this.EnableDwmClient(hwnd, powerLine);
				if(App.OsCompat.IsWindows11Ver22H2 && false) {
					// Win11 22H2では公式APIを使用するのでエアロはサポートしない
					var type = DWMSBT_TRANSIENTWINDOW;
					DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref type, sizeof(int));
				} else {
					this.SetComposition(hwnd, p.P);
				}
			}

			private void EnableAcryicBlur(IntPtr hwnd, (Color BaseColor, Color Accent) blur, bool powerLine) {
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
					false => (C: Color.FromRgb(blur.Accent.R, blur.Accent.G, blur.Accent.B), P: new ACCENT_POLICY() {
						AccentState = ACCENT_DISABLED,
						AccentFlags = 0,
						GradientColor = 0,
						AnimationId = 0
					})
				};
				if(App.OsCompat.IsWindows11Ver22H2 && false) {
					if(powerLine) {
						this.Source.CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);
						this.Source.CompositionTarget.BackgroundColor = Color.FromArgb(128, blur.Accent.R, blur.Accent.G, blur.Accent.B);
						this.ApplyDarkMode(hwnd, true);
					} else {
						this.Source.CompositionTarget.BackgroundColor = Color.FromRgb(blur.Accent.R, blur.Accent.G, blur.Accent.B);
					}
					this.EnableDwmClient(hwnd, powerLine);
					var type = powerLine switch {
						true => DWMSBT_TRANSIENTWINDOW,
						false => DWMSBT_NONE
					};
					DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref type, sizeof(int));
				} else {
					this.Source.CompositionTarget.BackgroundColor = p.C;
					this.EnableDwmClient(hwnd, powerLine);
					this.SetComposition(hwnd, p.P);
				}
			}

			private void DisableBlur(IntPtr hwnd) {
				this.EnableDwmClient(hwnd, false);
				if(App.OsCompat.IsWindows11Ver22H2 && false) {
					var type = DWMSBT_NONE;
					DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref type, sizeof(int));
				} else {
					var policy = new ACCENT_POLICY() {
						AccentState = ACCENT_DISABLED,
						AccentFlags = 0,
						GradientColor = 0,
						AnimationId = 0
					};
					this.SetComposition(hwnd, policy);
				}
			}

			private void ApplyDarkMode(IntPtr hwnd, bool isDark) {
				var @true = 1;
				var @false = 0;
				if(isDark) {
					DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref @true, sizeof(int));
				} else {
					DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref @false, sizeof(int));
				}
			}

			private void EnableMica(IntPtr hwnd, bool isDark) {
				this.Source.CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);
				this.ApplyDarkMode(hwnd, isDark);
				this.EnableDwmClient(hwnd, true);
				if(App.OsCompat.IsWindows11Ver22H2 && false) {
					var type = DWMSBT_MAINWINDOW;
					DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref type, sizeof(int));
				} else {
					// この処理はRTMでしか動かない
					var @true = 1;
					DwmSetWindowAttribute(hwnd, DWMWA_MICA_EFFECT, ref @true, sizeof(int));
				}
			}

			private void DisableMica(IntPtr hwnd) {
				this.EnableDwmClient(hwnd, false); 
				if(App.OsCompat.IsWindows11Ver22H2 && false) {
					var type = DWMSBT_NONE;
					DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref type, sizeof(int));
				} else {
					// この処理はRTMでしか動かない
					var @false = 0;
					DwmSetWindowAttribute(hwnd, DWMWA_MICA_EFFECT, ref @false, sizeof(int));
				}
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
						this.blurColor = blur.Value;
						this.EnableAeroBlur(this.Source.Handle, this.blurColor, this.IsPowerLine());
					}
					break;
				case FluentType.AcryicBlur:
					if(App.OsCompat.IsWindows10Redstone5) {
						if(!blur.HasValue) {
							throw new ArgumentNullException();
						}
						this.blurColor = blur.Value;
						EnableAcryicBlur(this.Source.Handle, this.blurColor, this.IsPowerLine());
					}
					break;
				case FluentType.Mica:
					if(App.OsCompat.IsWindows11Rtm) {
						this.EnableMica(
							this.Source.Handle,
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
					if(this.isWindowMaxmized = IsZoomed(this.Source.Handle)) {
						this.EnableDwmRound(this.Source.Handle, false, isSmallRound);
					} else {
						this.EnableDwmRound(this.Source.Handle, true, isSmallRound);
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
		[DllImport("user32.dll")]
		private static extern long SetWindowLongPtr(IntPtr hWnd, int flag, long newVal);
		[DllImport("user32.dll")]
		private static extern int SetWindowLong(IntPtr hWnd, int flag, int newVal);

		private static IntPtr SetWindowLongValue(IntPtr hWnd, int flag, IntPtr newVal) {
			return IntPtr.Size switch {
				4 => (IntPtr)SetWindowLong(hWnd, flag, newVal.ToInt32()),
				_ => (IntPtr)SetWindowLongPtr(hWnd, flag, newVal.ToInt64()),
			};
		}

		[DllImport("user32.dll")]
		private static extern long GetWindowLongPtr(IntPtr hWnd, int flag);
		[DllImport("user32.dll")]
		private static extern int GetWindowLong(IntPtr hWnd, int flag);

		private static IntPtr GetWindowLongValue(IntPtr hWnd, int flag) {
			return IntPtr.Size switch {
				4 => (IntPtr)GetWindowLong(hWnd, flag),
				_ => (IntPtr)GetWindowLongPtr(hWnd, flag),
			};
		}

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

		private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
		private const int DWMSBT_AUTO = 0;
		private const int DWMSBT_NONE = 1;
		private const int DWMSBT_MAINWINDOW = 2;
		private const int DWMSBT_TRANSIENTWINDOW = 3;
		private const int DWMSBT_TABBEDWINDOW = 4;

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
			if(IsWindowBackgroundTransparent(App.Current.Resources["FluentPopupType"])) {
				c.Background = Brushes.Transparent;
			}
		}
		public static void ApplyPopupBackground(Border c) {
			if(IsWindowBackgroundTransparent(App.Current.Resources["FluentPopupType"])) {
				c.Background = Brushes.Transparent;
			}
		}
		public static void ApplyPopupBackground(object o) {
			if(IsWindowBackgroundTransparent(App.Current.Resources["FluentPopupType"])) {
				switch(o) {
				case Control c:
					c.Background = Brushes.Transparent;
					break;
				case Border b:
					b.Background = Brushes.Transparent;
					break;
				case Panel p:
					p.Background = Brushes.Transparent;
					break;
				case object obj when obj != null: {
						obj.GetType().GetProperty(
							"Background",
							System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public,
							null,
							typeof(Brush),
							Array.Empty<Type>(),
							null)?.SetValue(obj, Brushes.Transparent);
					}
					break;
				}
			}
		}

		public static void AttachAndApplyContextMenu(ContextMenu contextMenu) {
			ApplyCompositionPopup(new FluentSource(contextMenu));
			if(IsWindowBackgroundTransparent()) {
				contextMenu.Background = Brushes.Transparent;
			}
		}

		public static void AttachAndApplySubMenu(Popup popup) {
			ApplyCompositionPopup(new FluentSource(popup.Child));
		}

		public static bool IsWindowBackgroundTransparent() => IsWindowBackgroundTransparent(App.Current.Resources["FluentPopupType"]);
		private static bool IsWindowBackgroundTransparent(object type)
			=> type switch {
				PlatformData.FluentType t when t == PlatformData.FluentType.None => false,
				PlatformData.FluentType t when t == PlatformData.FluentType.Auto && !App.OsCompat.IsWindows10Redstone5 => false,
				_ => true
			};
	}
}
