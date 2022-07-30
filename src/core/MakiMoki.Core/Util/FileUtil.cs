using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Util {
	public static class FileUtil {
		public static string LoadFileString(string path) {
			System.Diagnostics.Debug.Assert(path != null);
			using(var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
				return LoadFileString(fs);
			}
		}

		public static string LoadFileString(Stream s) {
			System.Diagnostics.Debug.Assert(s != null);
			using(var sr = new StreamReader(s, Encoding.UTF8)) {
				return sr.ReadToEnd();
			}
		}

		public static void SaveFileString(string path, string s) {
			System.Diagnostics.Debug.Assert(path != null);
			System.Diagnostics.Debug.Assert(s != null);
			var m = File.Exists(path) ? FileMode.Truncate : FileMode.OpenOrCreate;
			var b = Encoding.UTF8.GetBytes(s);
			Observable.Create<int>(async o => {
				try {
					using(var fs = new FileStream(path, m)) {
						fs.Write(b, 0, b.Length);
						fs.Flush();
						fs.Close();
					}
					o.OnNext(0);
					o.OnCompleted();
				}
				catch(IOException e) {
					await System.Threading.Tasks.Task.Delay(500);
					o.OnError(e);
				}
				return System.Reactive.Disposables.Disposable.Empty;
			}).Retry(5)
			.Subscribe(
				s => { },
				e => { throw e; });
		}

		public static void SaveJson(string path, object o) {
			System.Diagnostics.Debug.Assert(path != null);
			if(o == null) {
				throw new ArgumentNullException(nameof(o));
			}

			SaveFileString(path, (o is Data.JsonObject) ? o.ToString() : JsonConvert.SerializeObject(o));
		}

		public static T LoadJson<T>(string path) {
			System.Diagnostics.Debug.Assert(path != null);
			return JsonConvert.DeserializeObject<T>(LoadFileString(path));
		}

		public static void LoadConfigHelper(string configFile, Action<string> loadAction, Action<Exception, string> errorAction) {
			System.Diagnostics.Debug.Assert(configFile != null);
			System.Diagnostics.Debug.Assert(loadAction != null);
			System.Diagnostics.Debug.Assert(errorAction != null);
			try {
				loadAction(LoadFileString(configFile));
			}
			catch(JsonReaderException e) {
				errorAction(e, string.Format(
					"JSONファイル[{1}]が不正な形式です{0}{0}{2}",
					Environment.NewLine,
					configFile,
					e.Message));
			}
			catch(JsonSerializationException e) {
				errorAction(e, string.Format(
					"JSONファイル[{1}]が不正な形式です{0}{0}{2}",
					Environment.NewLine,
					configFile,
					e.Message));
			}
			catch(IOException e) {
				errorAction(e, string.Format(
					"ファイル[{1}]の読み込みに失敗しました{0}{0}{2}",
					Environment.NewLine,
					configFile,
					e.Message));
			}
		}

		public static void LoadJsonHelper(string json, Action<string> loadAction, Action<Exception, string> errorAction) {
			System.Diagnostics.Debug.Assert(json != null);
			System.Diagnostics.Debug.Assert(loadAction != null);
			System.Diagnostics.Debug.Assert(errorAction != null);
			try {
				loadAction(json);
			}
			catch(JsonReaderException e) {
				errorAction(e, string.Format(
					"JSONファイル[{1}]が不正な形式です{0}{0}{2}",
					Environment.NewLine,
					json,
					e.Message));
			}
			catch(JsonSerializationException e) {
				errorAction(e, string.Format(
					"JSONファイル[{1}]が不正な形式です{0}{0}{2}",
					Environment.NewLine,
					json,
					e.Message));
			}
		}


		public static void LoadConfigHelper(Stream configFile, Action<string> loadAction, Action<Exception, string> errorAction) {
			System.Diagnostics.Debug.Assert(configFile != null);
			System.Diagnostics.Debug.Assert(loadAction != null);
			System.Diagnostics.Debug.Assert(errorAction != null);
			try {
				loadAction(LoadFileString(configFile));
			}
			catch(JsonReaderException e) {
				errorAction(e, string.Format(
					"JSONファイル[{1}]が不正な形式です{0}{0}{2}",
					Environment.NewLine,
					configFile,
					e.Message));
			}
			catch(JsonSerializationException e) {
				errorAction(e, string.Format(
					"JSONファイル[{1}]が不正な形式です{0}{0}{2}",
					Environment.NewLine,
					configFile,
					e.Message));
			}
			catch(IOException e) {
				errorAction(e, string.Format(
					"ファイル[{1}]の読み込みに失敗しました{0}{0}{2}",
					Environment.NewLine,
					configFile,
					e.Message));
			}
		}

		public static T LoadMigrate<T>(
			string path, T defaultValue,
			Dictionary<int, Type> migrateTable = null,
			Func<string, Exception, Exception> exception = null) where T : Data.ConfigObject {

			var r = defaultValue;
			exception ??= (m, e) => new Exceptions.InitializeFailedException(m, e);
			if(File.Exists(path)) {
				LoadConfigHelper(
					path,
					(json) => r = CompatUtil.Migrate<T>(json, migrateTable),
					(e, m) => throw exception(m, e));
			}
			return r;
		}

		public static T LoadMigrate<T>(
			Stream stream, T defaultValue,
			Dictionary<int, Type> migrateTable = null,
			Func<string, Exception, Exception> exception = null) where T : Data.ConfigObject {

			var r = defaultValue;
			exception ??= (m, e) => new Exceptions.InitializeFailedException(m, e);
			LoadConfigHelper(
				stream,
				(json) => r = CompatUtil.Migrate<T>(json, migrateTable),
				(e, m) => throw exception(m, e));
			return r;
		}

		public static string ToBase64(Stream stream) {
			System.Diagnostics.Debug.Assert(stream != null);
			var list = new List<byte>();
			var b = new byte[1024];
			var r = 0;
			while(0 < (r = stream.Read(b, 0, b.Length))) {
				for(var i = 0; i < r; i++) {
					list.Add(b[i]);
				}
			}
			return Convert.ToBase64String(list.ToArray(), Base64FormattingOptions.None);
		}
	}
}