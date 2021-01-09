using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reactive;
using Reactive.Bindings;

namespace Yarukizero.Net.MakiMoki.Wpf.Canvas98.Canvas98Config {
	public static class Canvas98ConfigLoader {
		//internal static readonly string TemplateFile = "windows.makimoki.canvas98.template.json";
		internal static readonly string BookmarkletFile = "windows.makimoki.canvas98.bookmarklet.json";

		public class Setting {
			public string SystemDirectory { get; set; } = null;

			public string UserDirectory { get; set; } = null;
		}

		public static void Initialize(Setting setting) {
			System.Diagnostics.Debug.Assert(setting != null);
			System.Diagnostics.Debug.Assert(setting.SystemDirectory != null);
			System.Diagnostics.Debug.Assert(Directory.Exists(setting.SystemDirectory));

			InitializedSetting = setting;
			/*
			{
				var teamp = default(Canvas98Data.Canvas98Template);
				Util.FileUtil.LoadConfigHelper(Path.Combine(setting.SystemDirectory, TemplateFile),
					(json) => teamp = Newtonsoft.Json.JsonConvert.DeserializeObject<Canvas98Data.Canvas98Template>(json),
					(e, m) => throw new Exceptions.InitializeFailedException(m, e));
				System.Diagnostics.Debug.Assert(teamp != null);
				Template = new ReactiveProperty<Canvas98Data.Canvas98Template>(teamp);
			}
			*/
			if((setting.UserDirectory != null) && Directory.Exists(setting.UserDirectory)) {
				var file = Path.Combine(setting.UserDirectory, BookmarkletFile);
				if(File.Exists(file)) {
					var teamp = default(Canvas98Data.Canvas98Bookmarklet);
					Util.FileUtil.LoadConfigHelper(Path.Combine(setting.UserDirectory, BookmarkletFile),
						(json) => teamp = Newtonsoft.Json.JsonConvert.DeserializeObject<Canvas98Data.Canvas98Bookmarklet>(json),
						(e, m) => throw new Exceptions.InitializeFailedException(m, e));
					System.Diagnostics.Debug.Assert(teamp != null);
					Bookmarklet = new ReactiveProperty<Canvas98Data.Canvas98Bookmarklet>(teamp);
				}
			}

			if(Bookmarklet == null) {
				Bookmarklet = new ReactiveProperty<Canvas98Data.Canvas98Bookmarklet>(
					Canvas98Data.Canvas98Bookmarklet.From(
						bookmarklet: ""
					));
			}
		}
		public static Setting InitializedSetting { get; private set; }

		//public static ReactiveProperty<Canvas98Data.Canvas98Template> Template { get; private set; }

		public static ReactiveProperty<Canvas98Data.Canvas98Bookmarklet> Bookmarklet { get; private set; }

		public static Helpers.UpdateNotifyer<Canvas98Data.Canvas98Bookmarklet> BookmarkletUpdateNotifyer { get; } = new Helpers.UpdateNotifyer<Canvas98Data.Canvas98Bookmarklet>();


		public static void UpdateBookmarklet(string bookmarklet) {
			if(Directory.Exists(InitializedSetting.UserDirectory)) {
				Bookmarklet.Value = Canvas98Data.Canvas98Bookmarklet.From(
					bookmarklet: bookmarklet,
					extends: Bookmarklet.Value.ExtendBookmarklet
				);
				Util.FileUtil.SaveJson(
					Path.Combine(InitializedSetting.UserDirectory, BookmarkletFile),
					Bookmarklet.Value);
				BookmarkletUpdateNotifyer.Notify(Bookmarklet.Value);
			}
		}
	}
}