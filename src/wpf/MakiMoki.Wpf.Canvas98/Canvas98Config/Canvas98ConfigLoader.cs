using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;
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

			var loader = new Util.ResourceLoader(typeof(Canvas98ConfigLoader));
			InitializedSetting = setting;
			Bookmarklet = new ReactiveProperty<Canvas98Data.Canvas98Bookmarklet>(
				Util.FileUtil.LoadMigrate(
					loader.Get(BookmarkletFile),
					default(Canvas98Data.Canvas98Bookmarklet)));
			System.Diagnostics.Debug.Assert(Bookmarklet.Value != null);
			if((setting.UserDirectory != null) && Directory.Exists(setting.UserDirectory)) {
				Bookmarklet.Value = Util.FileUtil.LoadMigrate(
					Path.Combine(setting.UserDirectory, BookmarkletFile),
					Bookmarklet.Value,
					new Dictionary<int, Type>());
			}
		}
		public static Setting InitializedSetting { get; private set; }

		//public static ReactiveProperty<Canvas98Data.Canvas98Template> Template { get; private set; }

		public static ReactiveProperty<Canvas98Data.Canvas98Bookmarklet> Bookmarklet { get; private set; }

		public static Helpers.UpdateNotifyer<Canvas98Data.Canvas98Bookmarklet> BookmarkletUpdateNotifyer { get; } = new Helpers.UpdateNotifyer<Canvas98Data.Canvas98Bookmarklet>();


		public static void UpdateBookmarklet(string bookmarklet, (string Name, string Value)[] extends) {
			if(Directory.Exists(InitializedSetting.UserDirectory)) {
				Bookmarklet.Value = Canvas98Data.Canvas98Bookmarklet.From(
					bookmarklet: bookmarklet,
					extends: extends.ToDictionary(x => x.Name, x => x.Value)
				);
				Util.FileUtil.SaveJson(
					Path.Combine(InitializedSetting.UserDirectory, BookmarkletFile),
					Bookmarklet.Value);
				BookmarkletUpdateNotifyer.Notify(Bookmarklet.Value);
			}
		}
	}
}