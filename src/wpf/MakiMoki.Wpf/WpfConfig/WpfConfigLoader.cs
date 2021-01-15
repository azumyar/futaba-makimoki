using AngleSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Yarukizero.Net.MakiMoki.Util;
using Yarukizero.Net.MakiMoki.Wpf.PlatformData;

namespace Yarukizero.Net.MakiMoki.Wpf.WpfConfig {
	static class WpfConfigLoader {
		internal static readonly string SystemConfigFile = "windows.makimoki.json";
		internal static readonly string PlacementConfigFile = "windows.placement.json";
		internal static readonly string StyleLightConfigFile = "windows.style.light.json";
		internal static readonly string StyleDarkConfigFile = "windows.style.dark.json";
		internal static readonly string StyleUserConfigFile = "windows.style.user.json";
#pragma warning disable IDE0044, IDE0052
		private static volatile object lockObj = new object();
#pragma warning restore IDE0052, IDE0044

		public static Helpers.UpdateNotifyer<PlatformData.WpfConfig> SystemConfigUpdateNotifyer { get; } = new Helpers.UpdateNotifyer<PlatformData.WpfConfig>();

		public class Setting {
			public string WorkDirectory { get; set; } = null;
			public string SystemDirectory { get; set; } = null;
			public string UserDirectory { get; set; } = null;
		}

		public static void Initialize(Setting setting) {
			InitializedSetting = setting;

			var loader = new Util.ResourceLoader(typeof(WpfConfigLoader));
			SystemConfig = Util.FileUtil.LoadMigrate(
				loader.Get(SystemConfigFile),
				PlatformData.WpfConfig.CreateDefault());
			Placement = Util.FileUtil.LoadMigrate(
				Path.Combine(InitializedSetting.WorkDirectory, PlacementConfigFile),
				PlatformData.PlacementConfig.CreateDefault());
			if(Directory.Exists(InitializedSetting.UserDirectory)) {
				SystemConfig = Util.FileUtil.LoadMigrate(
					Path.Combine(InitializedSetting.UserDirectory, SystemConfigFile),
					SystemConfig,
					new Dictionary<int, Type>() {
						{ PlatformData.Compat.WpfConfig2020062900.CurrentVersion, typeof(PlatformData.Compat.WpfConfig2020062900) },
						{ PlatformData.Compat.WpfConfig2020070500.CurrentVersion, typeof(PlatformData.Compat.WpfConfig2020070500) },
						{ PlatformData.Compat.WpfConfig2020071900.CurrentVersion, typeof(PlatformData.Compat.WpfConfig2020071900) },
					});
				if(File.Exists(Path.Combine(InitializedSetting.UserDirectory, StyleUserConfigFile))) {
					Style = Util.FileUtil.LoadMigrate(
						Path.Combine(InitializedSetting.UserDirectory, StyleUserConfigFile),
						Style);
					var r = Style.Validate();
					if(!r.Successed) {
						throw new Exceptions.InitializeFailedException(r.ErrorText);
					}
				}
			}
			UpdateStyle();
		}

		public static Setting InitializedSetting { get; private set; }
		public static PlatformData.WpfConfig SystemConfig { get; private set; }
		
		public static PlatformData.PlacementConfig Placement { get; private set; }

		public static PlatformData.StyleConfig Style { get; private set; }

		public static void AddSystemConfigUpdateNotifyer(Action<PlatformData.WpfConfig> action) {
			SystemConfigUpdateNotifyer.AddHandler(action);
		}

		public static void UpdateSystemConfig(PlatformData.WpfConfig conf) {
			System.Diagnostics.Debug.Assert(conf != null);

			SystemConfig = conf;
			//UpdateStyle();
			SystemConfigUpdateNotifyer.Notify(conf);
			if(Directory.Exists(InitializedSetting.UserDirectory)) {
				Util.FileUtil.SaveJson(
					Path.Combine(InitializedSetting.UserDirectory, SystemConfigFile),
					conf);
			}
		}

		public static void UpdatePlacementByWindowClosing(Window window) {
			var hwnd = new WindowInteropHelper(window).Handle;
			var placement = new WinApi.WINDOWPLACEMENT() {
				length = System.Runtime.InteropServices.Marshal.SizeOf(typeof(WinApi.WINDOWPLACEMENT))
			};
			WinApi.Win32.GetWindowPlacement(hwnd, ref placement);
			Placement.WindowPlacement = placement;

			Util.FileUtil.SaveJson(
				Path.Combine(InitializedSetting.WorkDirectory, PlacementConfigFile),
				Placement);
		}

		private static void UpdateStyle() {
			if(!File.Exists(Path.Combine(InitializedSetting.UserDirectory, StyleUserConfigFile))) {
				var styleFile = (SystemConfig.WindowTheme == WindowTheme.Light) ? StyleLightConfigFile : StyleDarkConfigFile;
				Style = Util.FileUtil.LoadMigrate(
					new Util.ResourceLoader(typeof(WpfConfigLoader)).Get(styleFile),
					PlatformData.StyleConfig.CreateDefault());
			}
		}
	}
}
