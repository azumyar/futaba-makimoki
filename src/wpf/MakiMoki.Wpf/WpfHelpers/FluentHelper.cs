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
					this.Source = HwndSource.FromHwnd(new WindowInteropHelper(Window.GetWindow(obj)).Handle);
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
				var v = enable ? -1 : 0;
				var mgn = new MARGINS() {
					cxLeftWidth = v,
					cxRightWidth = v,
					cyTopHeight = v,
					cyBottomHeight = v,
				};
				DwmExtendFrameIntoClientArea(hwnd, ref mgn);
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
				this.SetComposition(hwnd, p.P);
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
				this.Source.CompositionTarget.BackgroundColor = p.C;
				this.EnableDwmClient(hwnd, powerLine);
				this.SetComposition(hwnd, p.P);
			}

			private void DisableBlur(IntPtr hwnd) {
				var policy = new ACCENT_POLICY() {
					AccentState = ACCENT_DISABLED,
					AccentFlags = 0,
					GradientColor = 0,
					AnimationId = 0
				};
				this.EnableDwmClient(hwnd, false);
				this.SetComposition(hwnd, policy);
			}

			private void EnableMica(IntPtr hwnd, bool isDark) {
				// この処理はRTMでしか動かない
				this.Source.CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);

				var hsv = WpfUtil.ImageUtil.ToHsv((Color)App.Current.Resources["MakimokiBackgroundColor"]);
				var @true = 1;
				var @false = 0;
				if(isDark) {
					DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref @true, sizeof(int));
				} else {
					DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref @false, sizeof(int));
				}
				this.EnableDwmClient(hwnd, true);
				DwmSetWindowAttribute(hwnd, DWMWA_MICA_EFFECT, ref @true, sizeof(int));
			}

			private void DisableMica(IntPtr hwnd) {
				// この処理はRTMでしか動かない
				var @false = 0;
				this.EnableDwmClient(hwnd, false);
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

		class MenuFluentSource : FluentSource {
			private static Dictionary<IntPtr, FluentSource> s_menu = new Dictionary<IntPtr, FluentSource>();

			public static bool IsApplied(MenuItem item, out FluentSource source) {
				var hwnd = ((HwndSource)HwndSource.FromVisual(item))?.Handle ?? IntPtr.Zero;
				if(hwnd == IntPtr.Zero) {
					hwnd = new WindowInteropHelper(Window.GetWindow(item)).Handle;
				}
				if(hwnd == IntPtr.Zero) {
					throw new ArgumentException();
				}
				return s_menu.TryGetValue(hwnd, out source);
			}

			public MenuFluentSource(FrameworkElement obj) : base(obj) {
				s_menu.Add(this.Source.Handle, this);
			}

			public MenuFluentSource(Popup obj) : base(obj.Child) {
				s_menu.Add(this.Source.Handle, this);
			}

			protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
				const int WM_DESTROY = 0x0002;
				switch(msg) {
				case WM_DESTROY:
					s_menu.Remove(hwnd);
					break;
				}
				return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
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


		public static void AttachAndApplyMenuItem(FrameworkElement item) {
			static void apply(FrameworkElement el) {
				static T? parent<T>(FrameworkElement child, string name = null) where T : class {
					var p = child;
					do {
						p = VisualTreeHelper.GetParent(p) as FrameworkElement;
						if(p is T cm) {
							if(name == null) {
								break;
							}
							if(p.GetType().FullName == name) {
								break;
							}
						}
					} while(p != null);
					return p switch {
						T t => t,
						_ => null
					};
				}

				var mi = parent<MenuItem>(el);
				if(mi == null) {
					return;
				}
				if(MenuFluentSource.IsApplied(mi, out var s)) {
					return;
				}
				if(parent<ContextMenu>(mi) is ContextMenu cm) {
					ApplyCompositionPopup(new MenuFluentSource(cm));
					if(IsWindowBackgroundTransparent()) {
						cm.Background = Brushes.Transparent;
					}
				} else if(parent<Popup>(mi) is Popup pp) {
					ApplyCompositionPopup(new MenuFluentSource(pp));
				} else if(parent<FrameworkElement>(mi, "System.Windows.Controls.Primitives.PopupRoot") is FrameworkElement el2) {
					ApplyCompositionPopup(new MenuFluentSource(el2));
				}
			}

			static void on(object _, RoutedEventArgs e) {
				if(e.Source is FrameworkElement it) {
					apply(it);
				}
			}
			if(item.IsLoaded) {
				apply(item);
			} else {
				item.Loaded += on;
			}
		}

		public static bool IsWindowBackgroundTransparent()
			=> App.Current.Resources["FluentPopupType"] switch {
				PlatformData.FluentType t when t == PlatformData.FluentType.None => false,
				PlatformData.FluentType t when t == PlatformData.FluentType.Auto && !App.OsCompat.IsWindows10Redstone5 => false,
				_ => true
			};
	}
}
