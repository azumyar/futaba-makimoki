using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Util {
	public static class FileUtil {
		public static string LoadFileString(string path) {
			using(var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
				return LoadFileString(fs);
			}
		}

		public static string LoadFileString(Stream s) {
			using(var sr = new StreamReader(s, Encoding.UTF8)) {
				return sr.ReadToEnd();
			}
		}

		public static void SaveFileString(string path, string s) {
			var m = File.Exists(path) ? FileMode.Truncate : FileMode.OpenOrCreate;
			using(var fs = new FileStream(path, m)) {
				var b = Encoding.UTF8.GetBytes(s);
				fs.Write(b, 0, b.Length);
				fs.Flush();
				fs.Close();
			}
		}

		public static void SaveJson(string path, object o) {
			if(o == null) {
				throw new ArgumentNullException(nameof(o));
			}

			SaveFileString(path, JsonConvert.SerializeObject(o));
		}

		public static T LoadJson<T>(string path) {
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

		public static string ToBase64(Stream stream) {
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