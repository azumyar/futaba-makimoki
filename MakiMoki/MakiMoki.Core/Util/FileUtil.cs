using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Util {
	public static class FileUtil {
		public static string LoadFileString(string path) {
			using(var fs = new FileStream(path, FileMode.Open)) {
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
	}
}