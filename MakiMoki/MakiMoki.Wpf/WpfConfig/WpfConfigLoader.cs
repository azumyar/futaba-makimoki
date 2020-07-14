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
		private static volatile object lockObj = new object();

		public static Helpers.UpdateNotifyer<PlatformData.WpfConfig> SystemConfigUpdateNotifyer { get; } = new Helpers.UpdateNotifyer<PlatformData.WpfConfig>();

		public class Setting {
			public string WorkDirectory { get; set; } = null;
			public string SystemDirectory { get; set; } = null;
			public string UserDirectory { get; set; } = null;
		}

		public static void Initialize(Setting setting) {
			InitializedSetting = setting;

			T getPath<T>(string path, T defaultValue, Func<string, T> convFunc = null) {
				var r = defaultValue;
				convFunc = convFunc ?? ((j) => Newtonsoft.Json.JsonConvert.DeserializeObject<T>(j));
				if(File.Exists(path)) {
					Util.FileUtil.LoadConfigHelper(path,
						(json) => r = convFunc(json),
						(e, m) => throw new Exceptions.InitializeFailedException(m, e));
				}
				return r;
			}
			T getStream<T>(Stream path, T defaultValue) {
				var r = defaultValue;
				Util.FileUtil.LoadConfigHelper(path,
					(json) => r = Newtonsoft.Json.JsonConvert.DeserializeObject < T >(json),
					(e, m) => throw new Exceptions.InitializeFailedException(m, e));
				return r;
			}
			string getResPath(string name) => $"{ typeof(WpfConfigLoader).Namespace }.{ name }";

			var asm = typeof(WpfConfigLoader).Assembly;
			SystemConfig = getStream(
				asm.GetManifestResourceStream(getResPath(SystemConfigFile)),
				PlatformData.WpfConfig.CreateDefault());
			Placement = getPath(
				Path.Combine(InitializedSetting.WorkDirectory, PlacementConfigFile),
				PlatformData.PlacementConfig.CreateDefault());
			if(Directory.Exists(InitializedSetting.UserDirectory)) {
				SystemConfig = getPath(
					Path.Combine(InitializedSetting.UserDirectory, SystemConfigFile),
					SystemConfig,
					(json) => Util.CompatUtil.Migrate<PlatformData.WpfConfig>(json, new Dictionary<int, Type>() {
						{ PlatformData.Compat.WpfConfig2020062900.CurrentVersion, typeof(PlatformData.Compat.WpfConfig2020062900) },
						{ PlatformData.Compat.WpfConfig2020070500.CurrentVersion, typeof(PlatformData.Compat.WpfConfig2020070500) },
					}));
			}
		}

		public static Setting InitializedSetting { get; private set; }
		public static PlatformData.WpfConfig SystemConfig { get; private set; }
		
		public static PlatformData.PlacementConfig Placement { get; private set; }

		public static void AddSystemConfigUpdateNotifyer(Action<PlatformData.WpfConfig> action) {
			SystemConfigUpdateNotifyer.AddHandler(action);
		}

		public static void UpdateSystemConfig(PlatformData.WpfConfig conf) {
			System.Diagnostics.Debug.Assert(conf != null);

			SystemConfig = conf;
			SystemConfigUpdateNotifyer.Notify(conf);
			if(Directory.Exists(InitializedSetting.UserDirectory)) {
				Util.FileUtil.SaveJson(
					Path.Combine(InitializedSetting.UserDirectory, SystemConfigFile),
					conf);
			}
		}

		public static void UpdatePlacementByWindowClosing(Window window) {
			var hwnd = new WindowInteropHelper(window).Handle;
			var placement = new WinApi.WINDOWPLACEMENT();
			placement.length = System.Runtime.InteropServices.Marshal.SizeOf(typeof(WinApi.WINDOWPLACEMENT));
			WinApi.Win32.GetWindowPlacement(hwnd, ref placement);
			Placement.WindowPlacement = placement;

			Util.FileUtil.SaveJson(
				Path.Combine(InitializedSetting.WorkDirectory, PlacementConfigFile),
				Placement);

		}
	}
}
