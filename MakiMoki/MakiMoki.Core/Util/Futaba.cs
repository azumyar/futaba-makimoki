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
using Yarukizero.Net.MakiMoki.Data;

namespace Yarukizero.Net.MakiMoki.Util {
	public static class Futaba {
		private static volatile object lockObj = new object();

		public static ReactiveProperty<Data.FutabaContext[]> Catalog { get; private set; }
		public static ReactiveProperty<Data.FutabaContext[]> Threads { get; private set; }
		public static ReactiveProperty<Data.PostedResItem[]> PostItems { get; private set; }
		public static ReactiveCollection<Data.Information> Informations { get; private set; }

		public static void Initialize() {
			if(Config.ConfigLoader.MakiMoki.FutabaResponseSave) {
				Catalog = new ReactiveProperty<Data.FutabaContext[]>(
					Config.ConfigLoader.SavedFutaba.Catalogs.Select(x => {
						var b = Config.ConfigLoader.Bord.Where(y => y.Url == x.Url.BaseUrl).FirstOrDefault();
						if(b != null) {
							return FutabaContext.FromCatalog_(b, x.Catalog, x.CatalogSortRes, x.CatalogResCounter);
						} else {
							return null;
						}
					}).Where(x => x != null).ToArray());
				Threads = new ReactiveProperty<Data.FutabaContext[]>(
					Config.ConfigLoader.SavedFutaba.Threads.Select(x => {
						var b = Config.ConfigLoader.Bord.Where(y => y.Url == x.Url.BaseUrl).FirstOrDefault();
						if(b != null) {
							return FutabaContext.FromThreadResResponse(
								FutabaContext.FromThreadEmpty(b, x.Url.ThreadNo),
								x.Thread);
						} else {
							return null;
						}
					}).Where(x => x != null).ToArray());
			} else {
				Catalog = new ReactiveProperty<Data.FutabaContext[]>(new Data.FutabaContext[0]);
				Threads = new ReactiveProperty<Data.FutabaContext[]>(new Data.FutabaContext[0]);
			}
			PostItems = new ReactiveProperty<Data.PostedResItem[]>(Config.ConfigLoader.PostedItem.Items.ToArray());
			Informations = new ReactiveCollection<Data.Information>(UIDispatcherScheduler.Default);

			Catalog.Subscribe(x => Config.ConfigLoader.SaveFutabaResponse(Catalog.Value.ToArray(), Threads.Value.ToArray()));
			Threads.Subscribe(x => Config.ConfigLoader.SaveFutabaResponse(Catalog.Value.ToArray(), Threads.Value.ToArray()));
			PostItems.Subscribe(x => Config.ConfigLoader.SavePostItems(PostItems.Value.ToArray()));
		}

		public static void Load(Data.FutabaContext[] catalogs, Data.FutabaContext[] threads, Data.PostedResItem[] postItems) {
			Catalog.Value = catalogs;
			Threads.Value = threads;
			PostItems.Value = postItems;
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
				if(f == null) {
					PutInformation(new Data.Information("カタログ取得エラー"));
				}
				o.OnNext(f);
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}


		public static IObservable<(Data.FutabaContext New, Data.FutabaContext Old)> UpdateThreadRes(Data.BordConfig bord, string threadNo, bool incremental=false) {
			var u = new Data.UrlContext(bord.Url, threadNo);
			Data.FutabaContext parent = null;
			lock(lockObj) {
				parent = Threads.Value.Where(x => x.Url == u).FirstOrDefault();
			}
			if(!incremental || (parent == null) || (parent.ResItems.Length == 0)) {
				return UpdateThreadResAll(bord, threadNo);
			}

			return Observable.Create<(Data.FutabaContext New, Data.FutabaContext Old)>(async o => {
				var ctx = await Task.Run<(Data.FutabaContext New, Data.FutabaContext Old)>(() => {
					Data.FutabaContext result = null;
					Data.FutabaContext prev = null;
					try {
						var no = "";
						if(long.TryParse(parent.ResItems.Last().ResItem.No, out var v)) {
							no = (++v).ToString();
						} else {
							goto end;
						}

						var r = FutabaApi.GetThreadRes(
							bord.Url,
							threadNo,
							no,
							Config.ConfigLoader.Cookies);
						Task.WaitAll(r);
						if(!r.Result.Successed) {
							// TODO: エラー処理
							goto end;
						}
						Config.ConfigLoader.UpdateCookie(r.Result.cookies);
						lock(lockObj) {
							var f = Data.FutabaContext.FromThreadResResponse(parent, r.Result.Response);

							for(var i = 0; i < Threads.Value.Length; i++) {
								if(Threads.Value[i].Url == f.Url) {
									result = f;
									prev = Threads.Value[i];
									Threads.Value[i] = f;
									Threads.Value = Threads.Value.ToArray();

									goto end;
								}
							}
							Threads.Value = Threads.Value.Concat(new Data.FutabaContext[] { f }).ToArray();

							result = f;
							goto end;
						}
					}
					catch(Exception e) {
						System.Diagnostics.Debug.WriteLine(e.ToString());
						throw;
					}
				end:
					return (result, prev);
				});
				PutThreadResResult(ctx.New, ctx.Old);
				o.OnNext(ctx);
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}


		public static IObservable<(Data.FutabaContext New, Data.FutabaContext Old)> UpdateThreadResAll(Data.BordConfig bord, string threadNo) {
			return Observable.Create<(Data.FutabaContext New, Data.FutabaContext Old)>(async o => {
				var ctx = await Task.Run<(Data.FutabaContext New, Data.FutabaContext Old)>(() => {
					Data.FutabaContext result = null;
					Data.FutabaContext prev = null;
					try {
						var u = new Data.UrlContext(bord.Url, threadNo);
						lock(lockObj) {
							if(!Threads.Value.Select(x => x.Url).Contains(u)) {
								Threads.Value = Threads.Value.Concat(new Data.FutabaContext[] {
									result = Data.FutabaContext.FromThreadEmpty(bord, threadNo),
								}).ToArray();
							}
						}

						var r = FutabaApi.GetThreadRes(
							bord.Url,
							threadNo,
							Config.ConfigLoader.Cookies);
						var rr = FutabaApi.GetThreadResHtml(
							bord.Url,
							threadNo,
							Config.ConfigLoader.Cookies);
						Task.WaitAll(r, rr);
						if(!r.Result.Successed || !rr.Result.Successed) {
							// 404の場脚の処理を行う
							if((r.Result.Successed && r.Result.Response.IsDie)
								|| (rr.Result.Is404)) {

								Data.FutabaContext fc = null;
								var index = 0;
								lock(lockObj) {
									for(index = 0; index < Threads.Value.Length; index++) {
										if(Threads.Value[index].Url == u) {
											fc = Threads.Value[index];
											break;
										}
									}

									if(fc != null) {
										var fc2 = Data.FutabaContext.FromThreadResResponse404(
											Catalog.Value.Where(x => x.Bord.Url == bord.Url).FirstOrDefault(),
											fc, r.Result.Response);
										if(fc2 != null) {
											var ary = Threads.Value.ToArray();
											ary[index] = fc2;
											Threads.Value = ary;
											result = fc2;
											prev = fc;
										} else {
											result = fc;
											prev = fc;
										}
									}
								}
							}
							goto end;
						}
						Config.ConfigLoader.UpdateCookie(r.Result.cookies);
						lock(lockObj) {
							var name = "";
							var email = "";
							var id = "";
							var ip = "";
							var soudane = 0;
							var host = "";
							var thumb = "";
							var w = 0;
							var h = 0;
							var fsize = 0;
							var time = "";
							var utc = 0l;
							var parser = new HtmlParser();
							var doc = parser.ParseDocument(rr.Result.Raw);
							var sub = doc.QuerySelector(string.Format("div[data-res=\"{0}\"] > span.csb", threadNo))?.Text() ?? "";
							var nameEl = doc.QuerySelector(string.Format("div[data-res=\"{0}\"] > span.cnm", threadNo));
							if(nameEl != null) {
								name = nameEl.Text();
								var mailEl = nameEl.QuerySelector("a");
								if(mailEl != null) {
									email = mailEl.GetAttribute("href").Substring("mailto:".Length);
								}
							}
							var timeEl = doc.QuerySelector(string.Format("div[data-res=\"{0}\"] > span.cnw", threadNo));
							if(timeEl != null) {
								time = timeEl.Text();
								var t = Regex.Split(time, @"\s+");
								if(1 < t.Length) {
									time = t[0];
									for(var i = 1; i < t.Length; i++) {
										if(t[i].StartsWith("ID:")) {
											id = t[i];
										} else if(t[i].StartsWith("IP:")) {
											ip = t[i];
										}
									}
								}
								var m = Regex.Match(time, @"^(\d\d/\d\d/\d\d)\(\S\)(\d\d:\d\d:\d\d)");
								if(m.Success) {
									var tms = m.Groups[1].Value + " " + m.Groups[2].Value;
									if(DateTime.TryParseExact(tms, "yy/MM/dd hh:mm:ss",
										System.Globalization.CultureInfo.InvariantCulture,
										System.Globalization.DateTimeStyles.None,
										out var dt)) {

										// ミリ秒以下がなくなるが仕方無し
										utc = new DateTimeOffset(dt.Ticks, new TimeSpan(9, 0, 0)).ToUnixTimeMilliseconds();
									}
								}
								if(0 < ip.Length) {
									time = time + " " + ip;
								}
								var mailEl = timeEl.QuerySelector("a");
								if(mailEl != null) {
									email = mailEl.GetAttribute("href").Substring("mailto:".Length);
								}
							}
							var sd = doc.QuerySelector(string.Format("div[data-res=\"{0}\"] > a.sod", threadNo))?.Text() ?? "";
							if(sd.StartsWith("そうだねx")) {
								var m = Regex.Match(sd, @"(\d+)$");
								if(m.Success && int.TryParse(m.Groups[1].Value, out var sdn)) {
									soudane = sdn;
								}
							}
							var src = doc.QuerySelector(string.Format("div[data-res=\"{0}\"] > a", threadNo))?.GetAttribute("href") ?? "";
							var ext = Path.GetExtension(src);
							var thumbEl = doc.QuerySelector(string.Format("div[data-res=\"{0}\"] > a > img", threadNo));
							if(thumbEl != null) {
								thumb = thumbEl.GetAttribute("src");
								if(int.TryParse(thumbEl.GetAttribute("width"), out var ww)) {
									w = ww;
								}
								if(int.TryParse(thumbEl.GetAttribute("height"), out var hh)) {
									h = hh;
								}
								var alt = thumbEl.GetAttribute("alt");
								var m = Regex.Match(alt, @"^(\d+)");
								if(m.Success && int.TryParse(m.Groups[1].Value, out var fs)) {
									fsize = fs;
								}
							}
							var com = doc.QuerySelector(string.Format("div[data-res=\"{0}\"] > blockquote", threadNo))?.InnerHtml ?? "";
							var f = Data.FutabaContext.FromThreadResResponse(bord, threadNo, r.Result.Response,
								new Data.NumberedResItem(threadNo,
									Data.ResItem.From(
										sub, name, email, com,
										id + ip, // IDとIPは同時に付与されない？
										host, "",
										src, thumb, ext, fsize, w, h,
										time, utc.ToString(),
										0)), soudane);
							if(f == null) {
								// TODO: エラー処理
								goto end;
							}
							for(var i = 0; i < Threads.Value.Length; i++) {
								if(Threads.Value[i].Url == f.Url) {
									result = f;
									prev = Threads.Value[i];
									Threads.Value[i] = f;
									Threads.Value = Threads.Value.ToArray();

									goto end;
								}
							}
							Threads.Value = Threads.Value.Concat(new Data.FutabaContext[] { f }).ToArray();

							result = f;
							goto end;
						}
					}
					catch(Exception e) {
						System.Diagnostics.Debug.WriteLine(e.ToString());
						throw;
					}
				end:
					return (result, prev);
				});
				PutThreadResResult(ctx.New, ctx.Old);
				o.OnNext(ctx);
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}

		private static void PutThreadResResult(Data.FutabaContext newFutaba, Data.FutabaContext oldFutaba) {
			if(newFutaba == null) {
				PutInformation(new Data.Information("スレ取得エラー"));
			} else {
				if(oldFutaba == null) {
					// 何もしない
				} else {
					var c = newFutaba.ResItems.Length - oldFutaba.ResItems.Length;
					if(c <= 0) {
						if(newFutaba.Raw.IsDie) {
							PutInformation(new Data.Information("スレッドは落ちています"));
						} else {
							PutInformation(new Data.Information("新着レスなし"));
						}
					} else {
						PutInformation(new Data.Information($"{c}件の新着レス"));
						if(newFutaba.Raw.IsDie) {
							PutInformation(new Data.Information("スレッドは落ちています"));
						}
					}
				}
			}
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
							url.ThreadNo)
								.Subscribe();
					}
				}
			}
		}

		public static void Remove(Data.UrlContext url) {
			System.Diagnostics.Debug.Assert(url != null);

			lock(lockObj) {
				if(url.IsCatalogUrl) {
					Catalog.Value = Catalog.Value.Where(x => x.Url != url).ToArray();
					Threads.Value = Threads.Value.Where(x => x.Url.BaseUrl != url.BaseUrl).ToArray();
				} else {
					Threads.Value = Threads.Value.Where(x => x.Url != url).ToArray();
				}
			}
		}

		public static IObservable<(bool Successed, string LocalPath, byte[] FileBytes)> GetThreadResImage(Data.UrlContext url, Data.ResItem item) {
			System.Diagnostics.Debug.Assert(url != null);
			System.Diagnostics.Debug.Assert(item != null);

			var localFile = CreateLocalFileName(url.BaseUrl, item.Src);
			var localPath = Path.Combine(Config.ConfigLoader.InitializedSetting.CacheDirectory, localFile);
			var uri = new Uri(url.BaseUrl);
			var u = string.Format("{0}://{1}{2}", uri.Scheme, uri.Authority, item.Src);
			return GetUrlImage(u, localPath);
		}

		public static IObservable<(bool Successed, string LocalPath, byte[] FileBytes)> GetThumbImage(Data.UrlContext url, Data.ResItem item) {
			System.Diagnostics.Debug.Assert(url != null);
			System.Diagnostics.Debug.Assert(item != null);

			var localFile = CreateLocalFileName(url.BaseUrl, item.Thumb);
			var localPath = Path.Combine(Config.ConfigLoader.InitializedSetting.CacheDirectory, localFile);
			var uri = new Uri(url.BaseUrl);
			var u = string.Format("{0}://{1}{2}", uri.Scheme, uri.Authority, item.Thumb);
			return GetUrlImage(u, localPath);
		}

		public static Task<string> GetThumbImageAsync(Data.UrlContext url, Data.ResItem item) {
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

		public static IObservable<(bool Successed, string NextOrMessage)> PostThread(Data.BordConfig bord,
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
					o.OnNext((r.Successed, r.NextOrMessage));
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

		public static IObservable<(bool Successed, string LocalPath, byte[] FileBytes)> GetUploaderFile(string url) {
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

		private static IObservable<(bool Successed, string LocalPath, byte[] FileBytes)> GetUrlImage(string url, string localPath) {
			return Observable.Create<(bool Successed, string LocalPath, byte[] FileBytes)>(o => {
				Task.Run(() => {
					if(File.Exists(localPath)) {
						o.OnNext((true, localPath, null));
						o.OnCompleted();
					} else {
						var c= new RestSharp.RestClient();
						var r = new RestSharp.RestRequest(url, RestSharp.Method.GET);
						var res = c.Execute(r);
						if(res.StatusCode == System.Net.HttpStatusCode.OK) {
							if(!File.Exists(localPath)) {
								var b = res.RawBytes;
								using(var fs = new FileStream(localPath, FileMode.OpenOrCreate)) {
									fs.Write(b, 0, b.Length);
									fs.Flush();
								}
							}
							o.OnNext((true, localPath, res.RawBytes));
							o.OnCompleted();
						} else {
							o.OnNext((false, null, null));
							// TODO: o.OnError();
						}
					}
				});
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}

		public static string GetThreadResImageLocalFilePath(Data.UrlContext url, Data.ResItem item) {
			System.Diagnostics.Debug.Assert(url != null);
			System.Diagnostics.Debug.Assert(item != null);

			return Path.Combine(Config.ConfigLoader.InitializedSetting.CacheDirectory, 
				CreateLocalFileName(url.BaseUrl, item.Src));
		}

		public static string GetThumbImageLocalFilePath(Data.UrlContext url, Data.ResItem item) {
			System.Diagnostics.Debug.Assert(url != null);
			System.Diagnostics.Debug.Assert(item != null);

			return Path.Combine(Config.ConfigLoader.InitializedSetting.CacheDirectory, 
				CreateLocalFileName(url.BaseUrl, item.Thumb));
		}

		private static string CreateLocalFileName(string baseUrl, string targetUrl) {
			System.Diagnostics.Debug.Assert(baseUrl != null);
			System.Diagnostics.Debug.Assert(targetUrl != null);

			var url = new Uri(baseUrl);
			var h = Regex.Replace(url.Authority, @"^([^\.]+)\..*$", "$1");
			return $"{ h }{ targetUrl.Replace('/', '_') }";
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

		public static IObservable<(bool Successed, string Message)> PostDel(Data.BordConfig bord, string threadNo, string resNo) {
			return Observable.Create<(bool Successed, string Message)>(async o => {
				var r = await FutabaApi.PostDel(bord.Url, threadNo, resNo, Config.ConfigLoader.Cookies);
				if(r.Raw == null) {
					o.OnNext((false, "不明なエラー"));
				} else {
					o.OnNext((r.Successed, r.Raw));
				}
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}

		public static IObservable<(bool Successed, string FileNameOrMessage)> UploadUp2(string filePath, string passwd) {
			return UploadUp2("", filePath, passwd);
		}

		public static IObservable<(bool Successed, string FileNameOrMessage)> UploadUp2(string comment, string filePath, string passwd) {
			return Observable.Create<(bool Successed, string Message)>(async o => {
				var r = await FutabaApi.UploadUp2(comment, filePath, passwd);
				Config.ConfigLoader.UpdateFutabaPassword(passwd);
				o.OnNext((r.Successed, r.Message));
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}

		public static void PutInformation(Data.Information information) {
			Observable.Create<Data.Information>(o => {
				Informations.AddOnScheduler(information);
				o.OnNext(information);
				return System.Reactive.Disposables.Disposable.Empty;
			}).Delay(TimeSpan.FromSeconds(3))
				.Subscribe(x => Informations.RemoveOnScheduler(x));
		}
	}
}
