using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.IO;
using AngleSharp.Html.Parser;
using AngleSharp.Dom;
using Yarukizero.Net.MakiMoki.Data;
using System.Net.Sockets;

namespace Yarukizero.Net.MakiMoki.Util {
	public static class FutabaApi {
		private static readonly string FutabaEndPoint = "/futaba.php";
		private static readonly string FutabaSoudaneEndPoint = "/sd.php";
		private static readonly string FutabaDelEndPoint = "/del.php";
		private static readonly string FutabaCachemt = "/bin/cachemt7.php";
		private static readonly string FutabaUp2Url = "https://dec.2chan.net/up2/";
		private static readonly string FutabaUp2Endpoint = "up.php";
		private static readonly string FutabaUp2Html = "https://dec.2chan.net/up2/up.htm";
		private static readonly Dictionary<string, string> FutabaUpCompletMap = new Dictionary<string, string>() {
			{ "f", "https://appsweets.net/thumbnail/up/{0}s.js" },
			{ "F", "https://appsweets.net/thumbnail/up/{0}s.js" },
			{ "fu", "https://appsweets.net/thumbnail/up2/{0}s.js" },
		};
		private static readonly Encoding FutabaEncoding = Encoding.GetEncoding("Shift_JIS");

		private static readonly int PostWebmTimeOutMiliSec = 20 * 1000;

		private static RestClient CreateRestClient(string baseUrl) {
			/* 一旦お蔵入り
			var m = Regex.Match(baseUrl, @"^(https?://)([^/]+)(/.*)$");
			if(m.Success) {
				try {
					var p = m.Groups[1].Value;
					var h = m.Groups[2].Value;
					var b = m.Groups[3].Value;
					var ip = System.Net.Dns.GetHostEntry(h)?.AddressList
						?.Where(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
						.FirstOrDefault();
					if(ip != null) {
						return new RestClient($"{ p }{ ip }{ b }") {
							UserAgent = Config.ConfigLoader.InitializedSetting.RestUserAgent,
							BaseHost = h,
						};
					}
				}
				catch(System.Net.Sockets.SocketException) { }
			}
			*/
			return new RestClient(baseUrl) {
				UserAgent = Config.ConfigLoader.InitializedSetting.RestUserAgent,
				Timeout = 5000,
			};
		}

		public static async Task<(bool Successed, Data.FutabaResponse Response, Data.Cookie2[] Cookies, string Raw)> GetCatalog(string baseUrl, Data.Cookie2[] cookies, Data.CatalogSortItem sort = null) {
			System.Diagnostics.Debug.Assert(baseUrl != null);
			return await Task.Run(() => {
				try {
					var c = CreateRestClient(baseUrl);
					var r = new RestRequest(FutabaEndPoint, Method.GET);
					r.AddParameter("mode", "cat");
					r.AddParameter("mode", "json");
					if(sort != null) {
						r.AddParameter("sort", sort.ApiValue);
					}
					foreach(var cookie in cookies) {
						if(IsCookie(baseUrl, cookie)) {
							r.AddCookie(cookie.Name, cookie.Value);
						}
					}
					var res = c.Execute(r);
					if(res.StatusCode == System.Net.HttpStatusCode.OK) {
						var rc = res.Cookies.Select(x => new Data.Cookie2(x.Name, x.Value, x.Path, x.Domain, x.Expires)).ToArray();
						if(res.Content.StartsWith("<html>")) {
							var s = FutabaEncoding.GetString(res.RawBytes);
							System.Diagnostics.Debug.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
							System.Diagnostics.Debug.WriteLine(s);
							System.Diagnostics.Debug.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
							return (false, null, rc, s);
						} else {
							var s = res.Content;
							System.Diagnostics.Debug.WriteLine(s);
							return (true, JsonConvert.DeserializeObject<Data.FutabaResponse>(s), rc, s);
						}
					} else {
						return (false, null, null, null);
					}
				}
				catch(Exception e) when(e is SocketException || e is TimeoutException) {
					return (false, null, null, null);
				}
				catch(JsonSerializationException ex) {
					throw;
				}
			});
		}

		public static async Task<(bool Successed, Data.Cookie2[] Cookies, string Raw)> GetCatalogHtml(string baseUrl, Data.Cookie2[] cookies, int maxThread, Data.CatalogSortItem sort = null) {
			System.Diagnostics.Debug.Assert(baseUrl != null);
			return await Task.Run(() => {
				try {
					var c = CreateRestClient(baseUrl);
					var r = new RestRequest(FutabaEndPoint, Method.GET);
					r.AddParameter("mode", "cat");
					if(sort != null) {
						r.AddParameter("sort", sort.ApiValue);
					}
					foreach(var cookie in cookies) {
						if(cookie.Name != "cxyl") {
							if(IsCookie(baseUrl, cookie)) {
								r.AddCookie(cookie.Name, cookie.Value);
							}
						}
					}
					{
						var w = 15;
						var h = (maxThread / w) + ((maxThread % w == 0) ? 0 : 1);
						r.AddCookie("cxyl", $"{w}x{h}x0x0x0");
					}
					var res = c.Execute(r);
					if(res.StatusCode == System.Net.HttpStatusCode.OK) {
						var s = FutabaEncoding.GetString(res.RawBytes);
						var rc = res.Cookies.Select(x => new Data.Cookie2(x.Name, x.Value, x.Path, x.Domain, x.Expires)).ToArray();
						return (true, rc, s);
					} else {
						return (false, null, null);
					}
				}
				catch(Exception e) when(e is SocketException || e is TimeoutException) {
					return (false, null, null);
				}
				finally { }
			});
		}

		public static async Task<(bool Successed, Data.FutabaResponse Response, Data.Cookie2[] cookies, string Raw)> GetThreadRes(string baseUrl, string threadNo, Data.Cookie2[] cookies) {
			System.Diagnostics.Debug.Assert(baseUrl != null);
			System.Diagnostics.Debug.Assert(threadNo != null);
			return await Task.Run(() => {
				try {
					var c = CreateRestClient(baseUrl);
					var r = new RestRequest(FutabaEndPoint, Method.GET);
					r.AddParameter("mode", "json");
					r.AddParameter("res", threadNo);
					foreach(var cookie in cookies) {
						if(IsCookie(baseUrl, cookie)) {
							r.AddCookie(cookie.Name, cookie.Value);
						}
					}
					var res = c.Execute(r);
					if(res.StatusCode == System.Net.HttpStatusCode.OK) {
						var rc = res.Cookies.Select(x => new Data.Cookie2(x.Name, x.Value, x.Path, x.Domain, x.Expires)).ToArray();
						if(res.Content.StartsWith("<html>")) {
							var s = FutabaEncoding.GetString(res.RawBytes);
							System.Diagnostics.Debug.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
							System.Diagnostics.Debug.WriteLine(s);
							System.Diagnostics.Debug.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
							return (false, null, rc, s);
						} else {
							var s = res.Content;
							return (true, JsonConvert.DeserializeObject<Data.FutabaResponse>(s), rc, s);
						}
					} else {
						return (false, null, null, null);
					}
				}
				catch(Exception e) when(e is SocketException || e is TimeoutException) {
					return (false, null, null, null);
				}
				catch(JsonSerializationException ex) {
					throw;
				}
			});
		}

		public static async Task<(bool Successed, Data.FutabaResponse Response, Data.Cookie2[] cookies, string Raw)> GetThreadRes(string baseUrl, string threadNo, string startRes, Data.Cookie2[] cookies) {
			System.Diagnostics.Debug.Assert(baseUrl != null);
			System.Diagnostics.Debug.Assert(threadNo != null);
			System.Diagnostics.Debug.Assert(startRes != null);
			return await Task.Run(() => {
				try {
					var c = CreateRestClient(baseUrl);
					var r = new RestRequest(FutabaEndPoint, Method.GET);
					r.AddParameter("mode", "json");
					r.AddParameter("res", threadNo);
					r.AddParameter("start", startRes);
					foreach(var cookie in cookies) {
						if(IsCookie(baseUrl, cookie)) {
							r.AddCookie(cookie.Name, cookie.Value);
						}
					}
					var res = c.Execute(r);
					if(res.StatusCode == System.Net.HttpStatusCode.OK) {
						var rc = res.Cookies.Select(x => new Data.Cookie2(x.Name, x.Value, x.Path, x.Domain, x.Expires)).ToArray();
						if(res.Content.StartsWith("<html>")) {
							var s = FutabaEncoding.GetString(res.RawBytes);
							System.Diagnostics.Debug.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
							System.Diagnostics.Debug.WriteLine(s);
							System.Diagnostics.Debug.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
							return (false, null, rc, s);
						} else {
							var s = res.Content;
							return (true, JsonConvert.DeserializeObject<Data.FutabaResponse>(s), rc, s);
						}
					} else {
						return (false, null, null, null);
					}
				}
				catch(Exception e) when(e is SocketException || e is TimeoutException) {
					return (false, null, null, null);
				}
				catch(JsonSerializationException ex) {
					throw;
				}
			});
		}

		public static async Task<(bool Successed, bool Is404, Data.Cookie2[] Cookies, string Raw)> GetThreadResHtml(string baseUrl, string threadNo, Data.Cookie2[] cookies) {
			System.Diagnostics.Debug.Assert(baseUrl != null);
			return await Task.Run(() => {
				try {
					var c = CreateRestClient(baseUrl);
					var r = new RestRequest(string.Format("res/{0}.htm", threadNo), Method.GET);
					foreach(var cookie in cookies) {
						if(IsCookie(baseUrl, cookie)) {
							r.AddCookie(cookie.Name, cookie.Value);
						}
					}
					var res = c.Execute(r);
					if(res.StatusCode == System.Net.HttpStatusCode.OK) {
						var s = FutabaEncoding.GetString(res.RawBytes);
						var rc = res.Cookies.Select(x => new Data.Cookie2(x.Name, x.Value, x.Path, x.Domain, x.Expires)).ToArray();
						return (true, false, rc, s);
					} else {
						return (false, res.StatusCode == System.Net.HttpStatusCode.NotFound, null, null);
					}
				}
				catch(Exception e) when(e is SocketException || e is TimeoutException) {
					return (false, false, null, null);
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

					var c = CreateRestClient(u);
					var r = new RestRequest(resItem.Src, Method.GET);
					var res = c.Execute(r);
					if(res.StatusCode == System.Net.HttpStatusCode.OK) {
						return res.RawBytes;
					} else {
						return null;
					}
				}
				catch(Exception e) when(e is SocketException || e is TimeoutException) {
					return null;
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

					var c = CreateRestClient(u);
					var r = new RestRequest(resItem.Thumb, Method.GET);
					var res = c.Execute(r);
					if(res.StatusCode == System.Net.HttpStatusCode.OK) {
						return res.RawBytes;
					} else {
						return null;
					}
				}
				catch(Exception e) when(e is SocketException || e is TimeoutException) {
					return null;
				}
				catch(JsonSerializationException) { // TODO: こない
					throw;
				}
			});
		}


		public static async Task<(bool Successed, string NextOrMessage, Data.Cookie2[] Cookies, string Raw)> PostThread(Data.BoardData board,
			Data.Cookie2[] cookies, string ptua,
			string name, string email, string subject,
			string comment, string filePath, string passwd) {
			System.Diagnostics.Debug.Assert(board != null);
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
				try {
					var c = CreateRestClient(board.Url);
					// WebMはふたば内部でエンコードが走るのでタイムアウト時間を10秒に伸ばす
					if(Path.GetExtension(filePath).ToLower() == ".webm") {
						c.Timeout = PostWebmTimeOutMiliSec;
					}
					var r = new RestRequest(FutabaEndPoint, Method.POST);
					c.Encoding = FutabaEncoding;
					r.AddHeader("Content-Type", "multipart/form-data");
					r.AddHeader("referer", string.Format("{0}futaba.htm", board.Url));
					r.AddParameter("guid", "on", ParameterType.QueryString);

					r.AlwaysMultipartFormData = true;
					SetPostParameter(r, board, "",
						ptua,
						name, email, subject, comment, filePath, passwd);
					foreach(var cookie in cookies) {
						if(IsCookie(board.Url, cookie)) {
							r.AddCookie(cookie.Name, cookie.Value);
						}
					}
					var res = c.Execute(r);
					if(res.StatusCode == System.Net.HttpStatusCode.OK) {
						var s = FutabaEncoding.GetString(res.RawBytes);
						var m = Regex.Match(s,
							"<meta\\s+http-equiv=\"refresh\"\\s+content=\"1;url=res/([0-9]+).htm\">",
							RegexOptions.IgnoreCase);
						if(m.Success) {
							return (true, m.Groups[1].Value, res.Cookies.Select(x => new Data.Cookie2(x.Name, x.Value, x.Path, x.Domain, x.Expires)).ToArray(), s);
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
							return (false, msg, res.Cookies.Select(x => new Data.Cookie2(x.Name, x.Value, x.Path, x.Domain, x.Expires)).ToArray(), s);
						}
					} else {
						return (false, "HTTPエラー", null, null);
					}
				}
				catch(SocketException) {
					return (false, "ソケットエラー", null, null);
				}
				catch(TimeoutException) {
					return (false, "HTTPタイムアウト", null, null);
				}
			});
		}

		public static async Task<(bool Successed, Data.Cookie2[] Cookies, string Raw)> PostRes(Data.BoardData board, string threadNo,
			Data.Cookie2[] cookies, string ptua,
			string name, string email, string subject,
			string comment, string filePath, string passwd) {
			System.Diagnostics.Debug.Assert(board != null);
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
				try {
					var c = CreateRestClient(board.Url);
					if(Path.GetExtension(filePath).ToLower() == ".webm") {
						c.Timeout = PostWebmTimeOutMiliSec;
					}
					//c.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko";
					//c.Encoding = Encoding.GetEncoding("Shift_JIS");
					var r = new RestRequest(FutabaEndPoint, Method.POST);
					r.AddHeader("Content-Type", "multipart/form-data");
					//r.AddHeader("origin", "https://img.2chan.net");
					r.AddHeader("referer", string.Format("{0}res/{1}.htm", board.Url, threadNo));
					r.AddParameter("guid", "on", ParameterType.QueryString);

					r.AlwaysMultipartFormData = true;
					SetPostParameter(r, board, threadNo,
						ptua,
						name, email, subject, comment, filePath, passwd);
					r.AddParameter("responsemode", "ajax", ParameterType.GetOrPost);
					foreach(var cookie in cookies) {
						if(IsCookie(board.Url, cookie)) {
							r.AddCookie(cookie.Name, cookie.Value);
						}
					}
					var res = c.Execute(r);
					if(res.StatusCode == System.Net.HttpStatusCode.OK) {
						var s = FutabaEncoding.GetString(res.RawBytes);
						return (s == "ok", res.Cookies.Select(x => new Data.Cookie2(x.Name, x.Value, x.Path, x.Domain, x.Expires)).ToArray(), s);
					} else {
						return (false, null, null);
					}
				}
				catch(Exception e) when(e is SocketException || e is TimeoutException) {
					return (false, null, null);
				}
			});
		}

		private static IRestRequest SetPostParameter(IRestRequest r, Data.BoardData bord, string threadNo,
			string ptua,
			string name, string email, string subject,
			string comment, string filePath, string passwd) {
			var pthc = GetCachemtSync(bord.Url);
			r.AddParameter("MAX_FILE_SIZE", Config.ConfigLoader.Board.MaxFileSize, ParameterType.GetOrPost);
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
			if(bord.Extra?.Name ?? true) {
				r.AddParameter("name", name, ParameterType.GetOrPost);
				r.AddParameter("sub", subject, ParameterType.GetOrPost);
			}
			if(string.IsNullOrWhiteSpace(threadNo)) {
				r.AddParameter("textonly", string.IsNullOrWhiteSpace(filePath) ? "on" : "", ParameterType.GetOrPost);
				if(!string.IsNullOrWhiteSpace(filePath)) {
					if(Config.ConfigLoader.MimeFutaba.MimeTypes.TryGetValue(Path.GetExtension(filePath).ToLower(), out var m)) {
						r.AddFile("upfile", filePath, m);
					} else {
						// TODO: なんかエラーだす
					}
				}
			} else {
				r.AddParameter("resto", threadNo, ParameterType.GetOrPost);
				if(bord.Extra?.ResImage ?? true) {
					r.AddParameter("textonly", string.IsNullOrWhiteSpace(filePath) ? "on" : "", ParameterType.GetOrPost);
					if(!string.IsNullOrWhiteSpace(filePath)) {
						if(Config.ConfigLoader.MimeFutaba.MimeTypes.TryGetValue(Path.GetExtension(filePath).ToLower(), out var m)) {
							r.AddFile("upfile", filePath, m);
						} else {
							// TODO: なんかエラーだす
						}
					}
				}
			}
			return r;
		}

		public static async Task<(bool Successed, Data.Cookie2[] Cookies, string Raw)> PostDeleteThreadRes(string baseUrl, string threadResNo, Data.Cookie2[] cookies, bool imageOnly, string passwd) {
			System.Diagnostics.Debug.Assert(baseUrl != null);
			System.Diagnostics.Debug.Assert(threadResNo != null);
			System.Diagnostics.Debug.Assert(passwd != null);
			return await Task.Run(() => {
				try {
					var c = CreateRestClient(baseUrl);
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
						if(IsCookie(baseUrl, cookie)) {
							r.AddCookie(cookie.Name, cookie.Value);
						}
					}
					var res = c.Execute(r);
					if(res.StatusCode == System.Net.HttpStatusCode.OK) {
						var s = FutabaEncoding.GetString(res.RawBytes);
						return (s == "ok", res.Cookies.Select(x => new Data.Cookie2(x.Name, x.Value, x.Path, x.Domain, x.Expires)).ToArray(), s);
					} else {
						return (false, null, null);
					}
				}
				catch(Exception e) when(e is SocketException || e is TimeoutException) {
					return (false, null, null);
				}
			});
		}

		public static async Task<(bool Successed, string Raw)> PostSoudane(string baseUrl, string threadResNo /*, Data.Cookie[] cookies = null */) {
			System.Diagnostics.Debug.Assert(baseUrl != null);
			System.Diagnostics.Debug.Assert(threadResNo != null);
			return await Task.Run(() => {
				try {
					var url = new Uri(baseUrl);
					var u = string.Format("{0}://{1}/", url.Scheme, url.Authority);
					var q = string.Format("{0}.{1}", url.AbsolutePath.Replace("/", ""), threadResNo);

					var c = CreateRestClient(u);
					var r = new RestRequest(FutabaSoudaneEndPoint, Method.GET);
					r.AddParameter(q, null);
					var res = c.Execute(r);
					if(res.StatusCode == System.Net.HttpStatusCode.OK) {
						var s = FutabaEncoding.GetString(res.RawBytes);
						return (Regex.Match(s, @"^\d+$").Success, s);
					} else {
						return (false, null);
					}
				}
				catch(Exception e) when(e is SocketException || e is TimeoutException) {
					return (false, null);
				}
			});
		}

		public static async Task<(bool Successed, string Raw)> PostDel(string baseUrl, string threadNo, string resNo /*, Data.Cookie[] cookies */) {
			System.Diagnostics.Debug.Assert(baseUrl != null);
			System.Diagnostics.Debug.Assert(threadNo != null);
			System.Diagnostics.Debug.Assert(resNo != null);
			var reason = 110;
			return await Task.Run(() => {
				try {
					var url = new Uri(baseUrl);
					var u = string.Format("{0}://{1}/", url.Scheme, url.Authority);
					var b = url.AbsolutePath.Replace("/", "");

					var c = CreateRestClient(u);
					var r = new RestRequest(FutabaDelEndPoint, Method.POST);
					r.AddHeader("referer", string.Format("{0}res/{1}.htm", baseUrl, threadNo)); // delはリファラが必要
					r.AddParameter("mode", "post", ParameterType.GetOrPost);
					r.AddParameter("responsemode", "ajax", ParameterType.GetOrPost);
					r.AddParameter("b", b, ParameterType.GetOrPost);
					r.AddParameter("d", resNo, ParameterType.GetOrPost);
					r.AddParameter("reason", reason, ParameterType.GetOrPost);
					var res = c.Execute(r);
					if(res.StatusCode == System.Net.HttpStatusCode.OK) {
						var s = FutabaEncoding.GetString(res.RawBytes);
						return (s == "ok", s);
					} else {
						return (false, null);
					}
				}
				catch(Exception e) when(e is SocketException || e is TimeoutException) {
					return (false, null);
				}
			});
		}
		public static async Task<(bool Successed, string Message)> UploadUp2(string comment, string filePath, string passwd) {
			System.Diagnostics.Debug.Assert(comment != null);
			System.Diagnostics.Debug.Assert(filePath != null);
			System.Diagnostics.Debug.Assert(passwd != null);
			return await Task.Run(() => {
				if(Config.ConfigLoader.MimeUp2.MimeTypes.TryGetValue(Path.GetExtension(filePath).ToLower(), out var m)) {
					// コメントがない場合ファイルのMD5 sumを埋め込む
					if(string.IsNullOrEmpty(comment)) {
						try {
							using(var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
							using(var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider()) {
								var bs = md5.ComputeHash(fs);
								var sb = new StringBuilder();
								foreach(byte b in bs) {
									sb.Append(b.ToString("x2"));
								}
								comment = sb.ToString();
							}
						}
						catch(FileNotFoundException) {
							return (false, "アップロードファイルが見つかりません");
						}
						catch(IOException) {
							return (false, "アップロードファイルが開けません");
						}
					}

					try {
						// アップロード
						var c = CreateRestClient(FutabaUp2Url);
						c.Encoding = FutabaEncoding;
						var r = new RestRequest(FutabaUp2Endpoint, Method.POST);
						r.AddHeader("Content-Type", "multipart/form-data");
						r.AddParameter("MAX_FILE_SIZE", "3000000", ParameterType.GetOrPost);
						r.AddParameter("mode", "reg", ParameterType.GetOrPost);
						r.AddFile("up", filePath, m);
						r.AddParameter("com", comment, ParameterType.GetOrPost);
						r.AddParameter("pass", passwd, ParameterType.GetOrPost);
						var res = c.Execute(r);
						if(res.StatusCode == System.Net.HttpStatusCode.OK) {
							// HTMLファイルから目的のファイルを見つける
							var cc = CreateRestClient(FutabaUp2Html);
							cc.Encoding = FutabaEncoding;
							var html = cc.Execute(new RestRequest(Method.GET));
							if(html.StatusCode == System.Net.HttpStatusCode.OK) {
								var parser = new HtmlParser();
								var doc = parser.ParseDocument(FutabaEncoding.GetString(html.RawBytes));
								var root = doc.QuerySelector("table.files");
								foreach(var tr in root?.QuerySelectorAll("tr")) {
									var fnm = tr.QuerySelector("td.fnm");
									var fco = tr.QuerySelector("td.fco");
									if((fnm != null) && (fco != null)) {
										if(fco.TextContent == comment) {
											return (true, fnm.TextContent);
										}
									}
								}
							}
						}
					}
					catch(SocketException) { }
					catch(TimeoutException) {
						return (false, "アップロードタイムアウト");
					}
					return (false, "アップロード失敗");
				} else {
					return (false, "アップロードできないファイル形式");
				}
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
			try {
				var url = new Uri(baseUrl);
				var u = string.Format("{0}://{1}/", url.Scheme, url.Authority);

				var c = CreateRestClient(u);
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
			catch(Exception e) when(e is SocketException || e is TimeoutException) {
				return "";
			}
		}

		public static async Task<(bool Successed, Data.AppsweetsThumbnailCompleteResponse CompleteResponse, Data.AppsweetsThumbnailErrorResponse ErrorResponse, string Raw)> GetCompleteUrlUp(string threadUrl, string fileNameWitfOutExtension) {
			System.Diagnostics.Debug.Assert(fileNameWitfOutExtension != null);
			return await Task.Run(() => {
				var r = GetCompleteUrl(threadUrl, fileNameWitfOutExtension, FutabaUpCompletMap);
				if(!string.IsNullOrEmpty(r)) {
					try {
						var response = JsonConvert.DeserializeObject<Data.AppsweetsThumbnailCompleteResponse>(r);
						return (true, response, null, r);
					}
					catch(JsonSerializationException) {
						try {
							var response = JsonConvert.DeserializeObject<Data.AppsweetsThumbnailErrorResponse>(r);
							return (false, null, response, r);
						}
						catch(JsonSerializationException) { /* 何もしない */ }
					}
					catch(JsonReaderException) { /* 何もしない */ }
				}
				return (false, default(Data.AppsweetsThumbnailCompleteResponse), default(Data.AppsweetsThumbnailErrorResponse), r);
			});
		}

		private static string GetCompleteUrl(string threadUrl, string fileNameWitfOutExtension, Dictionary<string, string> map) {
			var m = Regex.Match(fileNameWitfOutExtension, @"^([a-zA-Z]+)\d+$");
			if(m.Success && map.TryGetValue(m.Groups[1].Value, out var format)) {
				try {
					var c = CreateRestClient(string.Format(format, fileNameWitfOutExtension));
					var r = new RestRequest(Method.GET);

					// https://appsweets.net/thumbnail はリファラ設定が必要
					var u = new Uri(threadUrl);
					r.AddHeader("referer", threadUrl);
					r.AddHeader("origin", $"{u.Scheme}://{u.Authority}/");
					var res = c.Execute(r);
					if(res.StatusCode == System.Net.HttpStatusCode.OK) {
						return res.Content;
					}
				}
				catch(Exception e) when(e is SocketException || e is TimeoutException) { }
			}
			return "";
		}

		private static bool IsCookie(string baseUrl, Data.Cookie2 cookie) {
			var u = new Uri(baseUrl);
			return u.Host.EndsWith(cookie.Domain)
				&& u.AbsolutePath.StartsWith(cookie.Path);
		}
	}
}
