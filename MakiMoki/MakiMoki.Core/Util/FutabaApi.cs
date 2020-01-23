using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.IO;

namespace Yarukizero.Net.MakiMoki.Util {
	public static class FutabaApi {
		private static readonly string FutabaEndPoint = "/futaba.php";
		private static readonly string FutabaSoudaneEndPoint = "/sd.php";
		private static readonly string FutabaDelEndPoint = "/del.php";
		private static readonly string FutabaCachemt = "/bin/cachemt7.php";
		private static readonly Encoding FutabaEncoding = Encoding.GetEncoding("Shift_JIS");

		public static async Task<(Data.FutabaResonse Response, Data.Cookie[] Cookies, string Raw)> GetCatalog(string baseUrl, Data.Cookie[] cookies, Data.CatalogSortItem sort = null) {
			System.Diagnostics.Debug.Assert(baseUrl != null);
			return await Task.Run(() => {
				try {
					var c = new RestClient(baseUrl);
					var r = new RestRequest(FutabaEndPoint, Method.GET);
					r.AddParameter("mode", "cat");
					r.AddParameter("mode", "json");
					if(sort != null) {
						r.AddParameter("sort", sort.ApiValue);
					}
					foreach(var cookie in cookies) {
						r.AddCookie(cookie.Name, cookie.Value);
					}
					var res = c.Execute(r);
					if(res.StatusCode == System.Net.HttpStatusCode.OK) {
						var s = res.Content;
						Console.WriteLine(s);
						var rc = res.Cookies.Select(x => new Data.Cookie(x.Name, x.Value)).ToArray();
						return (JsonConvert.DeserializeObject<Data.FutabaResonse>(s), rc, s);
					} else {
						return (null, null, null);
					}
				}
				catch(JsonSerializationException ex) {
					throw;
				}
			});
		}

		public static async Task<(Data.Cookie[] Cookies, string Raw)> GetCatalogHtml(string baseUrl, Data.Cookie[] cookies, int maxThread, Data.CatalogSortItem sort = null) {
			System.Diagnostics.Debug.Assert(baseUrl != null);
			return await Task.Run(() => {
				try {
					var c = new RestClient(baseUrl);
					var r = new RestRequest(FutabaEndPoint, Method.GET);
					r.AddParameter("mode", "cat");
					if(sort != null) {
						r.AddParameter("sort", sort.ApiValue);
					}
					foreach(var cookie in cookies) {
						if(cookie.Name != "cxyl") {
							r.AddCookie(cookie.Name, cookie.Value);
						}
					}
					var w = 15;
					var h = (maxThread / w) + ((maxThread % w == 0) ? 0 : 1);
					r.AddCookie("cxyl", string.Format("{0}x{1}x0x0x0", w, h));
					var res = c.Execute(r);
					if(res.StatusCode == System.Net.HttpStatusCode.OK) {
						var s = FutabaEncoding.GetString(res.RawBytes);
						var rc = res.Cookies.Select(x => new Data.Cookie(x.Name, x.Value)).ToArray();
						return (rc, s);
					} else {
						return (null, null);
					}
				}
				finally { }
			});
		}

		public static async Task<(Data.FutabaResonse Response, Data.Cookie[] cookies, string Row)> GetThreadRes(string baseUrl, string threadNo, Data.Cookie[] cookies) {
			System.Diagnostics.Debug.Assert(baseUrl != null);
			System.Diagnostics.Debug.Assert(threadNo != null);
			return await Task.Run(() => {
				try {
					var c = new RestClient(baseUrl);
					var r = new RestRequest(FutabaEndPoint, Method.GET);
					r.AddParameter("mode", "json");
					r.AddParameter("res", threadNo);
					foreach(var cookie in cookies) {
						r.AddCookie(cookie.Name, cookie.Value);
					}
					var res = c.Execute(r);
					if(res.StatusCode == System.Net.HttpStatusCode.OK) {
						var rc = res.Cookies.Select(x => new Data.Cookie(x.Name, x.Value)).ToArray();
						if(res.Content.StartsWith("<html>")) {
							var s = Encoding.GetEncoding("shift_jis").GetString(res.RawBytes);
							System.Diagnostics.Debug.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
							System.Diagnostics.Debug.WriteLine(s);
							System.Diagnostics.Debug.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
							return (null, rc, s);
						} else {
							var s = res.Content;
							return (JsonConvert.DeserializeObject<Data.FutabaResonse>(s), rc, s);
						}
					} else {
						return (null, null, null);
					}
				}
				catch(JsonSerializationException ex) {
					throw;
				}
			});
		}

		public static async Task<(Data.Cookie[] Cookies, string Raw)> GetThreadResHtml(string baseUrl, string threadNo, Data.Cookie[] cookies) {
			System.Diagnostics.Debug.Assert(baseUrl != null);
			return await Task.Run(() => {
				try {
					var c = new RestClient(baseUrl);
					var r = new RestRequest(string.Format("res/{0}.htm", threadNo), Method.GET);
					foreach(var cookie in cookies) {
						r.AddCookie(cookie.Name, cookie.Value);
					}
					var res = c.Execute(r);
					if(res.StatusCode == System.Net.HttpStatusCode.OK) {
						var s = FutabaEncoding.GetString(res.RawBytes);
						var rc = res.Cookies.Select(x => new Data.Cookie(x.Name, x.Value)).ToArray();
						return (rc, s);
					} else {
						return (null, null);
					}
				}
				finally { }
			});
		}
		public static async Task<byte[]> GetThreadResImage(string baseUrl, Data.ResItem resItem) {
			System.Diagnostics.Debug.Assert(baseUrl != null);
			return await Task.Run(() => {
				try {
					var url = new Uri(baseUrl);
					var u = string.Format("{0}://{1}/", url.Scheme, url.Authority);

					var c = new RestClient(u);
					var r = new RestRequest(resItem.Src, Method.GET);
					var res = c.Execute(r);
					if(res.StatusCode == System.Net.HttpStatusCode.OK) {
						return res.RawBytes;
					} else {
						return null;
					}
				}
				catch(JsonSerializationException) { // TODO: こない
					throw;
				}
			});
		}

		public static async Task<byte[]> GetThumbImage(string baseUrl, Data.ResItem resItem) {
			System.Diagnostics.Debug.Assert(baseUrl != null);
			return await Task.Run(() => {
				try {
					var url = new Uri(baseUrl);
					var u = string.Format("{0}://{1}/", url.Scheme, url.Authority);

					var c = new RestClient(u);
					var r = new RestRequest(resItem.Thumb, Method.GET);
					var res = c.Execute(r);
					if(res.StatusCode == System.Net.HttpStatusCode.OK) {
						return res.RawBytes;
					} else {
						return null;
					}
				}
				catch(JsonSerializationException) { // TODO: こない
					throw;
				}
			});
		}


		public static async Task<(bool Successed, string NextOrMessage, Data.Cookie[] Cookies, string Raw)> PostThread(Data.BordConfig bord,
			Data.Cookie[] cookies, string ptua,
			string name, string email, string subject,
			string comment, string filePath, string passwd) {
			System.Diagnostics.Debug.Assert(bord != null);
			System.Diagnostics.Debug.Assert(cookies != null);
			System.Diagnostics.Debug.Assert(ptua != null);
			System.Diagnostics.Debug.Assert(name != null);
			System.Diagnostics.Debug.Assert(email != null);
			System.Diagnostics.Debug.Assert(subject != null);
			System.Diagnostics.Debug.Assert(comment != null);
			System.Diagnostics.Debug.Assert(filePath != null);
			System.Diagnostics.Debug.Assert(passwd != null);
			return await Task.Run(() => {
				// nc -vv -k -l 127.0.0.1 8080;
				//var c = new RestClient("http://127.0.0.1:8080/");
				var c = new RestClient(bord.Url);
				var r = new RestRequest(FutabaEndPoint, Method.POST);
				c.Encoding = FutabaEncoding;
				r.AddHeader("Content-Type", "multipart/form-data");
				r.AddParameter("guid", "on", ParameterType.QueryString);

				r.AlwaysMultipartFormData = true;
				SetPostParameter(r, bord, "",
					ptua,
					name, email, subject, comment, filePath, passwd);
				foreach(var cookie in cookies) {
					r.AddCookie(cookie.Name, cookie.Value);
				}
				var res = c.Execute(r);
				if(res.StatusCode == System.Net.HttpStatusCode.OK) {
					var s = FutabaEncoding.GetString(res.RawBytes);
					var m = Regex.Match(s,
						"<meta\\s+http-equiv=\"refresh\"\\s+content=\"1;url=res/([0-9]+).htm\">",
						RegexOptions.IgnoreCase);
					if(m.Success) {
						return (true, m.Groups[1].Value, res.Cookies.Select(x => new Data.Cookie(x.Name, x.Value)).ToArray(), s);
					} else {
						// エラー解析めどい…めどくない？
						var msg = "不明なエラー";
						var ln = s.Replace("\r", "").Split('\n');
						if(1 < ln.Length) {
							msg = Regex.Replace(ln[ln.Length - 1], "<[^>]+>", "");
							if(msg.EndsWith("リロード")) {
								msg = msg.Substring(0, msg.Length - "リロード".Length);
							}
						} else {
							var mm = Regex.Match(s,
								"<body>(.+)</body>",
								RegexOptions.IgnoreCase);
							if(mm.Success && !mm.Groups[1].Value.Contains("<")) {
								msg = mm.Groups[1].Value;
							}
						}
						return (false, msg, res.Cookies.Select(x => new Data.Cookie(x.Name, x.Value)).ToArray(), s);
					}
				} else {
					return (false, "HTTPエラー", null, null);
				}
			});
		}

		public static async Task<(bool Successed, Data.Cookie[] Cookies, string Raw)> PostRes(Data.BordConfig bord, string threadNo,
			Data.Cookie[] cookies, string ptua,
			string name, string email, string subject,
			string comment, string filePath, string passwd) {
			System.Diagnostics.Debug.Assert(bord != null);
			System.Diagnostics.Debug.Assert(threadNo != null);
			System.Diagnostics.Debug.Assert(cookies != null);
			System.Diagnostics.Debug.Assert(ptua != null);
			System.Diagnostics.Debug.Assert(name != null);
			System.Diagnostics.Debug.Assert(email != null);
			System.Diagnostics.Debug.Assert(subject != null);
			System.Diagnostics.Debug.Assert(comment != null);
			System.Diagnostics.Debug.Assert(filePath != null);
			System.Diagnostics.Debug.Assert(passwd != null);
			return await Task.Run(() => {
				// nc -vv -k -l 127.0.0.1 8080;
				//var c = new RestClient("http://127.0.0.1:8080/");
				var c = new RestClient(bord.Url);
				//c.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko";
				//c.Encoding = Encoding.GetEncoding("Shift_JIS");
				var r = new RestRequest(FutabaEndPoint, Method.POST);
				r.AddHeader("Content-Type", "multipart/form-data");
				//r.AddHeader("origin", "https://img.2chan.net");
				//r.AddHeader("referer", string.Format("{0}res/{1}.htm", baseUrl, threadNo));
				r.AddParameter("guid", "on", ParameterType.QueryString);

				r.AlwaysMultipartFormData = true;
				SetPostParameter(r, bord, threadNo,
					ptua,
					name, email, subject, comment, filePath, passwd);
				r.AddParameter("responsemode", "ajax", ParameterType.GetOrPost);
				foreach(var cookie in cookies) {
					r.AddCookie(cookie.Name, cookie.Value);
				}
				var res = c.Execute(r);
				if(res.StatusCode == System.Net.HttpStatusCode.OK) {
					var s = FutabaEncoding.GetString(res.RawBytes);
					return (s == "ok", res.Cookies.Select(x => new Data.Cookie(x.Name, x.Value)).ToArray(), s);
				} else {
					return (false, null, null);
				}
			});
		}

		private static IRestRequest SetPostParameter(IRestRequest r, Data.BordConfig bord, string threadNo,
			string ptua,
			string name, string email, string subject,
			string comment, string filePath, string passwd) {
			var pthc = GetCachemtSync(bord.Url);
			r.AddParameter("MAX_FILE_SIZE", bord.MaxFileSzieValue, ParameterType.GetOrPost);
			r.AddParameter("com", comment, ParameterType.GetOrPost);
			r.AddParameter("email", email, ParameterType.GetOrPost);
			r.AddParameter("pwd", passwd, ParameterType.GetOrPost);
			r.AddParameter("mode", "regist", ParameterType.GetOrPost);
			r.AddParameter("ptua", ptua, ParameterType.GetOrPost);
			r.AddParameter("pthb", "", ParameterType.GetOrPost);
			r.AddParameter("pthc", pthc, ParameterType.GetOrPost);
			r.AddParameter("pthd", "", ParameterType.GetOrPost);
			r.AddParameter("baseform", "", ParameterType.GetOrPost);
			r.AddParameter("js", "on", ParameterType.GetOrPost);
			r.AddParameter("scsz", "1024x768x24", ParameterType.GetOrPost);
			r.AddParameter("chrenc", "文字", ParameterType.GetOrPost);
			if(bord.Extra?.NameValue ?? true) {
				r.AddParameter("name", name, ParameterType.GetOrPost);
				r.AddParameter("sub", subject, ParameterType.GetOrPost);
			}
			if(string.IsNullOrWhiteSpace(threadNo)) {
				r.AddParameter("textonly", string.IsNullOrWhiteSpace(filePath) ? "on" : "", ParameterType.GetOrPost);
				if(!string.IsNullOrWhiteSpace(filePath)) {
					if(Config.ConfigLoader.Mime.MimeTypes.TryGetValue(Path.GetExtension(filePath).ToLower(), out var m)) {
						r.AddFile("upfile", filePath, m);
					} else {
						// TODO: なんかエラーだす
					}
				}
			} else {
				r.AddParameter("resto", threadNo, ParameterType.GetOrPost);
				if(bord.Extra?.ResImageValue ?? true) {
					r.AddParameter("textonly", string.IsNullOrWhiteSpace(filePath) ? "on" : "", ParameterType.GetOrPost);
					if(!string.IsNullOrWhiteSpace(filePath)) {
						if(Config.ConfigLoader.Mime.MimeTypes.TryGetValue(Path.GetExtension(filePath).ToLower(), out var m)) {
							r.AddFile("upfile", filePath, m);
						} else {
							// TODO: なんかエラーだす
						}
					}
				}
			}
			return r;
		}

		public static async Task<(bool Successed, Data.Cookie[] Cookies, string Raw)> PostDeleteThreadRes(string baseUrl, string threadResNo, Data.Cookie[] cookies, bool imageOnly, string passwd) {
			System.Diagnostics.Debug.Assert(baseUrl != null);
			System.Diagnostics.Debug.Assert(threadResNo != null);
			System.Diagnostics.Debug.Assert(passwd != null);
			return await Task.Run(() => {
				var c = new RestClient(baseUrl);
				var r = new RestRequest(FutabaEndPoint, Method.POST);
				r.AddParameter("guid", "on", ParameterType.QueryString);
				r.AddParameter("responsemode", "ajax", ParameterType.GetOrPost);
				r.AddParameter(threadResNo, "delete", ParameterType.GetOrPost);
				r.AddParameter("pwd", passwd, ParameterType.GetOrPost);
				r.AddParameter("mode", "usrdel", ParameterType.GetOrPost);
				if(imageOnly) {
					r.AddParameter("onlyimgdel", "on", ParameterType.GetOrPost);
				}

				foreach(var cookie in cookies) {
					r.AddCookie(cookie.Name, cookie.Value);
				}
				var res = c.Execute(r);
				if(res.StatusCode == System.Net.HttpStatusCode.OK) {
					var s = FutabaEncoding.GetString(res.RawBytes);
					return (s == "ok", res.Cookies.Select(x => new Data.Cookie(x.Name, x.Value)).ToArray(), s);
				} else {
					return (false, null, null);
				}
			});
		}

		public static async Task<(bool Successed, string Raw)> PostSoudane(string baseUrl, string threadResNo, Data.Cookie[] cookies = null) {
			System.Diagnostics.Debug.Assert(baseUrl != null);
			System.Diagnostics.Debug.Assert(threadResNo != null);
			return await Task.Run(() => {
				var url = new Uri(baseUrl);
				var u = string.Format("{0}://{1}/", url.Scheme, url.Authority);
				var q = string.Format("{0}.{1}", url.AbsolutePath.Replace("/", ""), threadResNo);

				var c = new RestClient(u);
				var r = new RestRequest(FutabaSoudaneEndPoint, Method.GET);
				r.AddParameter(q, null);
				var res = c.Execute(r);
				if(res.StatusCode == System.Net.HttpStatusCode.OK) {
					var s = FutabaEncoding.GetString(res.RawBytes);
					return (Regex.Match(s, @"^\d+$").Success, s);
				} else {
					return (false, null);
				}
			});
		}

		public static async Task<bool> PostDel(string baseUrl, string threadResNo, Data.Cookie[] cookies, Data.DelReasonItem reason) {
			System.Diagnostics.Debug.Assert(baseUrl != null);
			System.Diagnostics.Debug.Assert(threadResNo != null);
			System.Diagnostics.Debug.Assert(reason != null);
			return await Task.Run(() => {
				var url = new Uri(baseUrl);
				var u = string.Format("{0}://{1}/", url.Scheme, url.Authority);
				var b = url.AbsolutePath.Replace("/", "");

				var c = new RestClient(u);
				var r = new RestRequest(FutabaDelEndPoint, Method.POST);
				r.AddParameter("mode", "post", ParameterType.GetOrPost);
				r.AddParameter("responsemode", "ajax", ParameterType.GetOrPost);
				r.AddParameter("b", b, ParameterType.GetOrPost);
				r.AddParameter("d", threadResNo, ParameterType.GetOrPost);
				r.AddParameter("reason", reason.ApiValue, ParameterType.GetOrPost);
				var res = c.Execute(r);
				if(res.StatusCode == System.Net.HttpStatusCode.OK) {
					var s = res.Content;
					return s == "ok";
				} else {
					return false;
				}
			});
		}

		public static async Task<string> UploadUp2(string comment, string filePath, string passwd) {
			System.Diagnostics.Debug.Assert(comment != null);
			System.Diagnostics.Debug.Assert(filePath != null);
			System.Diagnostics.Debug.Assert(passwd != null);
			return await Task.Run(() => {
				return "";
			});
		}

		public static async Task<string> GetCachemt(string baseUrl) {
			System.Diagnostics.Debug.Assert(baseUrl != null);
			return await Task.Run(() => {
				return GetCachemtSync(baseUrl);
			});
		}

		private static string GetCachemtSync(string baseUrl) {
			System.Diagnostics.Debug.Assert(baseUrl != null);
			var url = new Uri(baseUrl);
			var u = string.Format("{0}://{1}/", url.Scheme, url.Authority);
			var b = url.AbsolutePath.Replace("/", "");

			var c = new RestClient(u);
			var r = new RestRequest(FutabaCachemt, Method.GET);
			var res = c.Execute(r);
			if(res.StatusCode == System.Net.HttpStatusCode.OK) {
				var s = res.Content;
				var m = Regex.Match(s, @"\d+");
				if(m.Success) {
					return m.Value;
				} else {
					return "";
				}
			} else {
				return "";
			}
		}
	}
}
