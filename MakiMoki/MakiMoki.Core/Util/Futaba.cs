using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive;
using Reactive.Bindings;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System.Text.RegularExpressions;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using System.IO;

namespace Yarukizero.Net.MakiMoki.Util {
	public static class Futaba {
		private static volatile object lockObj = new object();

		public static ReactiveProperty<Data.FutabaContext[]> Catalog { get; private set; }
		public static ReactiveProperty<Data.FutabaContext[]> Threads { get; private set; }

		public static void Initialize() {
			Catalog = new ReactiveProperty<Data.FutabaContext[]>(new Data.FutabaContext[0]);
			Threads = new ReactiveProperty<Data.FutabaContext[]>(new Data.FutabaContext[0]);
		}

		public static IObservable<Data.FutabaContext> UpdateCatalog(Data.BordConfig bord, Data.CatalogSortItem sort = null) {
			return Observable.Create<Data.FutabaContext>(async o => {
				var f = await Task.Run(async () => {
					lock(lockObj) {
						if(!Catalog.Value.Select(x => x.Url.BaseUrl).Contains(bord.Url)) {
							Catalog.Value = Catalog.Value.Concat(new Data.FutabaContext[] {
								Data.FutabaContext.FromCatalogEmpty(bord),
							}).ToArray();
						}
					}

					Data.FutabaContext result = null;
					try {
						var r = await FutabaApi.GetCatalog(
							bord.Url,
							Config.ConfigLoader.Cookies,
							sort);
						if(r.Raw == null) {
							// TODO: エラー表示処理
							goto end;
						}
						Config.ConfigLoader.UpdateCookie(r.Cookies);
						var rr = await FutabaApi.GetCatalogHtml(
							bord.Url,
							Config.ConfigLoader.Cookies,
							r.Response.Res.Length,
							sort);
						if(rr.Raw == null) {
							// TODO: エラー表示処理
							goto end;
						}
						Config.ConfigLoader.UpdateCookie(rr.Cookies);
						var parser = new HtmlParser();
						var doc = parser.ParseDocument(rr.Raw);
						var counter = doc.QuerySelectorAll("#cattable td")
							.Select<IElement, (string No, int Count)>(x => {
								var no = Regex.Replace(
									x.QuerySelector("a")?.GetAttribute("href") ?? "",
									@"^res/([0-9]+)\.htm$",
									"$1");
								var count = x.QuerySelector("font")?.InnerHtml ?? "0";
								if(int.TryParse(count, out var c)) {
									return (no, c);
								} else {
									return (no, 0);
								}
							}).ToArray();
						var dic = new Dictionary<string, int>();
						var sortList = new List<Data.NumberedResItem>();
						foreach(var c in counter.Where(x => !string.IsNullOrWhiteSpace(x.No))) {
							var t = r.Response.Res.Where(x => x.No == c.No).FirstOrDefault();
							if(t != null) {
								sortList.Add(t);
							}
							dic.Add(c.No, c.Count);
						}
						lock(lockObj) {
							for(var i = 0; i < Catalog.Value.Length; i++) {
								if(Catalog.Value[i].Bord.Url == bord.Url) {
									result = Data.FutabaContext.FromCatalogResponse(
										bord,
										r.Response,
										sortList.ToArray(),
										dic,
										Catalog.Value[i]);
									Catalog.Value[i] = result;
									Catalog.Value = Catalog.Value.ToArray();
									goto end;
								}
							}
							result = Data.FutabaContext.FromCatalogResponse(
									bord, r.Response, sortList.ToArray(), dic, null);
							Catalog.Value = Catalog.Value.Concat(new Data.FutabaContext[] { result }).ToArray();
						}
					end:;
					}
					catch(Exception e) { // TODO: 適切なエラーに
						System.Diagnostics.Debug.WriteLine(e.ToString());
					}
					return result;
				});
				o.OnNext(f);
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}


		public static void UpdateThreadRes(Data.BordConfig bord, string threadNo) {
			TaskUtil.Push(async () => {
				try {
					lock(lockObj) {
						var u = new Data.UrlContext(bord.Url, threadNo);
						if(!Threads.Value.Select(x => x.Url).Contains(u)) {
							Threads.Value = Threads.Value.Concat(new Data.FutabaContext[] {
								Data.FutabaContext.FromThreadEmpty(bord, threadNo),
							}).ToArray();
						}
					}

					var r = await FutabaApi.GetThreadRes(
						bord.Url,
						threadNo,
						Config.ConfigLoader.Cookies);
					Config.ConfigLoader.UpdateCookie(r.cookies);
					lock(lockObj) {
						var f = Data.FutabaContext.FromThreadResResponse(bord, threadNo, r.Response);
						if(f == null) {
							return;
						}
						for(var i = 0; i < Threads.Value.Length; i++) {
							if(Threads.Value[i].Url == f.Url) {
								Threads.Value[i] = f;
								Threads.Value = Threads.Value.ToArray();
								return;
							}
						}
						Threads.Value = Threads.Value.Concat(new Data.FutabaContext[] { f }).ToArray();
					}
				}
				catch(Exception e) {
					System.Diagnostics.Debug.WriteLine(e.ToString());
					throw;
				}
			});
		}

		public static void Open(Data.UrlContext url) {
			System.Diagnostics.Debug.Assert(url != null);

			lock(lockObj) {
				if(url.IsCatalogUrl) {
					var r = Catalog.Value.Where(x => x.Url == url).FirstOrDefault();
					if(r == null) {
						UpdateCatalog(Config.ConfigLoader.Bord.Where(x => x.Url == url.BaseUrl).First())
							.Subscribe();
					}
				} else {
					var r = Threads.Value.Where(x => x.Url == url).FirstOrDefault();
					if(r == null) {
						UpdateThreadRes(
							Config.ConfigLoader.Bord.Where(x => x.Url == url.BaseUrl).First(),
							url.ThreadNo);
					}
				}
			}
		}

		public static void Remove(Data.UrlContext url) {
			System.Diagnostics.Debug.Assert(url != null);

			lock(lockObj) {
				if(url.IsCatalogUrl) {
					Catalog.Value = Catalog.Value.Where(x => x.Url != url).ToArray();
				} else {
					Threads.Value = Threads.Value.Where(x => x.Url != url).ToArray();
				}
			}
		}

		public static IObservable<(bool Successed, string LocalPath)> GetThreadResImage(Data.UrlContext url, Data.ResItem item) {
			System.Diagnostics.Debug.Assert(url != null);
			System.Diagnostics.Debug.Assert(item != null);

			var localFile = CreateLocalFileName(url.BaseUrl, item.Src);
			var localPath = Path.Combine(Config.ConfigLoader.InitializedSetting.CacheDirectory, localFile);
			var uri = new Uri(url.BaseUrl);
			var u = string.Format("{0}://{1}{2}", uri.Scheme, uri.Authority, item.Src);
			return GetUrlImage(u, localPath);
		}

		public static Task<string> GetThumbImage(Data.UrlContext url, Data.ResItem item) {
			System.Diagnostics.Debug.Assert(url != null);
			System.Diagnostics.Debug.Assert(item != null);

			return Task.Run(() => {
				byte[] b = null;
				var localFile = CreateLocalFileName(url.BaseUrl, item.Thumb);
				var localPath = Path.Combine(Config.ConfigLoader.InitializedSetting.CacheDirectory, localFile);
				if(!File.Exists(localPath)) {
					Task<byte[]> t = FutabaApi.GetThumbImage(url.BaseUrl, item);
					t.Wait();
					b = t.Result;
					if(b == null) {
						return null;
					}
					// lockしたほうがいい？
					if(!File.Exists(localPath)) {
						using(var fs = new FileStream(localPath, FileMode.OpenOrCreate)) {
							fs.Write(b, 0, b.Length);
							fs.Flush();
						}
					}
				}
				return localPath;
			});
		}
		public static string GetThumbImageSync(Data.UrlContext url, Data.ResItem item) {
			System.Diagnostics.Debug.Assert(url != null);
			System.Diagnostics.Debug.Assert(item != null);

			byte[] b = null;
			var localFile = CreateLocalFileName(url.BaseUrl, item.Thumb);
			var localPath = Path.Combine(Config.ConfigLoader.InitializedSetting.CacheDirectory, localFile);
			if(!File.Exists(localPath)) {
				Task<byte[]> t = FutabaApi.GetThumbImage(url.BaseUrl, item);
				t.Wait();
				b = t.Result;
				if(b == null) {
					return null;
				}
				// lockしたほうがいい？
				if(!File.Exists(localPath)) {
					using(var fs = new FileStream(localPath, FileMode.OpenOrCreate)) {
						fs.Write(b, 0, b.Length);
						fs.Flush();
					}
				}
			}
			return localPath;
		}

		public static IObservable<(bool Successed, string Message)> PostThread(Data.BordConfig bord,
			string name, string email, string subject,
			string comment, string filePath, string passwd) {
			return Observable.Create<(bool Successed, string Message)>(async o => {
				var r = await FutabaApi.PostThread(bord,
					Config.ConfigLoader.Cookies, Config.ConfigLoader.Ptua.Value,
					name, email, subject, comment, filePath, passwd);
				if(r.Raw == null) {
					o.OnNext((false, "不明なエラー"));
				} else {
					Config.ConfigLoader.UpdateCookie(r.Cookies);
					Config.ConfigLoader.UpdateFutabaPassword(passwd);
					o.OnNext((r.Successed, r.Raw));
				}
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}

		public static IObservable<(bool Successed, string Message)> PostRes(Data.BordConfig bord, string threadNo,
			string name, string email, string subject,
			string comment, string filePath, string passwd) {
			return Observable.Create<(bool Successed, string Message)>(async o => {
				var r = await FutabaApi.PostRes(bord, threadNo,
					Config.ConfigLoader.Cookies, Config.ConfigLoader.Ptua.Value,
					name, email, subject, comment, filePath, passwd);
				if(r.Raw == null) {
					o.OnNext((false, "不明なエラー"));
				} else {
					Config.ConfigLoader.UpdateCookie(r.Cookies);
					Config.ConfigLoader.UpdateFutabaPassword(passwd);
					o.OnNext((r.Successed, r.Raw));
				}
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}

		public static IObservable<(bool Successed, string LocalPath)> GetUploaderFile(string url) {
			System.Diagnostics.Debug.Assert(url != null);
			var localFile = CreateLocalFileNameFromUploader(url);
			if(Config.ConfigLoader.Mime.Types.Select(x => x.Ext).Contains(Path.GetExtension(localFile).ToLower())) {
				var localPath = Path.Combine(Config.ConfigLoader.InitializedSetting.CacheDirectory, localFile);
				return GetUrlImage(url, localPath);
			} else {
				// TODO: ダウンロードフォルダにDL
				throw new NotImplementedException();
			}
		}

		private static IObservable<(bool Successed, string LocalPath)> GetUrlImage(string url, string localPath) {
			return Observable.Create<(bool Successed, string LocalPath)>(o => {
				if(File.Exists(localPath)) {
					o.OnNext((true, localPath));
					o.OnCompleted();
				} else {
					var rest = new RestSharp.RestClient();
					var r = new RestSharp.RestRequest(url, RestSharp.Method.GET);
					rest.ExecuteAsync(r, (x, y) => {
						if(x.StatusCode == System.Net.HttpStatusCode.OK) {
							if(!File.Exists(localPath)) {
								var b = x.RawBytes;
								using(var fs = new FileStream(localPath, FileMode.OpenOrCreate)) {
									fs.Write(b, 0, b.Length);
									fs.Flush();
								}
							}
							o.OnNext((true, localPath));
							o.OnCompleted();
						} else {
							o.OnNext((false, null));
							// TODO: o.OnError();
						}
					});
				}
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}

		private static string CreateLocalFileName(string baseUrl, string targetUrl) {
			System.Diagnostics.Debug.Assert(baseUrl != null);
			System.Diagnostics.Debug.Assert(targetUrl != null);

			var url = new Uri(baseUrl);
			var h = Regex.Replace(url.Authority, @"^([^\.]+)\..*$", "$1");
			return string.Format("{0}{1}", h, targetUrl.Replace('/', '_'));
		}

		private static string CreateLocalFileNameFromUploader(string url) {
			System.Diagnostics.Debug.Assert(url != null);

			var m = Regex.Match(url, "/([^/]+)$");
			System.Diagnostics.Debug.Assert(m.Success);

			return m.Groups[1].Value;
		}

		public static IObservable<(bool Successed, string Message)> PostDeleteThreadRes(Data.BordConfig bord, string threadNo, bool imageOnlyDel, string passwd) {
			return Observable.Create<(bool Successed, string Message)>(async o => {
				var r = await FutabaApi.PostDeleteThreadRes(bord.Url, threadNo, Config.ConfigLoader.Cookies, imageOnlyDel, passwd);
				if(r.Raw == null) {
					o.OnNext((false, "不明なエラー"));
				} else {
					Config.ConfigLoader.UpdateCookie(r.Cookies);
					o.OnNext((r.Successed, r.Raw));
				}
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}

		public static IObservable<(bool Successed, string Message)> PostSoudane(Data.BordConfig bord, string threadNo) {
			return Observable.Create<(bool Successed, string Message)>(async o => {
				var r = await FutabaApi.PostSoudane(bord.Url, threadNo);
				if(r.Raw == null) {
					o.OnNext((false, "不明なエラー"));
				} else {
					o.OnNext((r.Successed, r.Raw));
				}
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}
	}
}
