﻿using AngleSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yarukizero.Net.MakiMoki.Wpf.PlatformData;

namespace Yarukizero.Net.MakiMoki.Wpf.WpfConfig {
	static class WpfConfigLoader {
		private static readonly string SystemConfigFile = "windows.makimoki.json";
		private static readonly string StateConfigFile = "windows.state.json";
		private static volatile object lockObj = new object();

		public static Helpers.UpdateNotifyer<PlatformData.WpfConfig> SystemConfigUpdateNotifyer { get; } = new Helpers.UpdateNotifyer<PlatformData.WpfConfig>();

		public class Setting {
			public string WorkDirectory { get; set; } = null;
			public string SystemDirectory { get; set; } = null;
			public string UserDirectory { get; set; } = null;
		}

		public static void Initialize(Setting setting) {
			InitializedSetting = setting;

			T getPath<T>(string path, T defaultValue) {
				var r = defaultValue;
				if(File.Exists(path)) {
					Util.FileUtil.LoadConfigHelper(path,
						(json) => r = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json),
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
			State = getPath(
				Path.Combine(InitializedSetting.WorkDirectory, StateConfigFile),
				PlatformData.StateConfig.CreateDefault());
			if(Directory.Exists(InitializedSetting.UserDirectory)) {
				SystemConfig = getPath(
					Path.Combine(InitializedSetting.UserDirectory, SystemConfigFile),
					SystemConfig);
			}
		}

		public static Setting InitializedSetting { get; private set; }
		public static PlatformData.WpfConfig SystemConfig { get; set; }
		
		private static PlatformData.StateConfig State { get; set; }

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
	}
}