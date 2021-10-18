using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive;
using System.Net.Http;
using Reactive.Bindings;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System.Text.RegularExpressions;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using System.IO;

namespace Yarukizero.Net.MakiMoki.Util {
	public static class Futaba {
#pragma warning disable IDE0044
		private static volatile object lockObj = new object();
#pragma warning restore IDE0044
		private static HttpClient HttpClient { get; set; }

		public static ReactiveProperty<Data.FutabaContext[]> Catalog { get; private set; }
		public static ReactiveProperty<Data.FutabaContext[]> Threads { get; private set; }
		public static ReactiveProperty<Data.PostedResItem[]> PostItems { get; private set; }
		public static ReactiveCollection<Data.Information> Informations { get; private set; }

		private static Helpers.ConnectionQueue<object> PassiveReloadQueue { get; set; }
		private static Helpers.ConnectionQueue<(bool Successed, string Message)> SoudaneQueue { get; set; }
		private static Helpers.ConnectionQueue<(bool Successed, string Message)> DelQueue { get; set; }

		private static readonly Action BoardConfigUpdateAction = () => {
			if(Catalog != null) {
				Catalog.Value = Catalog.Value
					.Where(x => Config.ConfigLoader.Board.Boards.Any(y => y.Url == x.Url.BaseUrl))
					.ToArray();
			}
		};

		public static void Initialize(HttpClient client = null) {
			HttpClient = client ?? new HttpClient();
			if(Config.ConfigLoader.MakiMoki.FutabaResponseSave) {
				Catalog = new ReactiveProperty<Data.FutabaContext[]>(
					Config.ConfigLoader.SavedFutaba.Catalogs.Select(x => {
						var b = Config.ConfigLoader.Board.Boards.Where(y => y.Url == x.Url.BaseUrl).FirstOrDefault();
						if(b != null) {
							return Data.FutabaContext.FromCatalog_(b, x.Catalog, x.CatalogSortRes, x.CatalogResCounter);
						} else {
							return null;
						}
					}).Where(x => x != null).ToArray());
				Threads = new ReactiveProperty<Data.FutabaContext[]>(
					Config.ConfigLoader.SavedFutaba.Threads.Select(x => {
						var b = Config.ConfigLoader.Board.Boards.Where(y => y.Url == x.Url.BaseUrl).FirstOrDefault();
						if(b != null) {
							return Data.FutabaContext.FromThread_(b, x.Url, x.Thread);
						} else {
							return null;
						}
					}).Where(x => x != null).ToArray());
			} else {
				Catalog = new ReactiveProperty<Data.FutabaContext[]>(Array.Empty<Data.FutabaContext>());
				Threads = new ReactiveProperty<Data.FutabaContext[]>(Array.Empty<Data.FutabaContext>());
			}
			PostItems = new ReactiveProperty<Data.PostedResItem[]>(Config.ConfigLoader.PostedItem.Items.ToArray());
			Informations = new ReactiveCollection<Data.Information>(UIDispatcherScheduler.Default);

			Catalog.Subscribe(x => Config.ConfigLoader.SaveFutabaResponse(Catalog.Value.ToArray(), Threads.Value.ToArray()));
			Threads.Subscribe(x => Config.ConfigLoader.SaveFutabaResponse(Catalog.Value.ToArray(), Threads.Value.ToArray()));
			PostItems.Subscribe(x => Config.ConfigLoader.SavePostItems(PostItems.Value.ToArray()));

			PassiveReloadQueue = new Helpers.ConnectionQueue<object>(
				name: "パッシブリロードキュー",
				maxConcurrency: 4,
				waitTime: 5000,
				sleepTime: 60 * 1000
			);
			SoudaneQueue = new Helpers.ConnectionQueue<(bool Successed, string Message)>(
				name: "そうだねAPIキュー"
			);
			DelQueue = new Helpers.ConnectionQueue<(bool Successed, string Message)>(
				name: "delAPIキュー",
				maxConcurrency: 1,
				waitTime: 5000
			);
			Config.ConfigLoader.BoardConfigUpdateNotifyer.AddHandler(BoardConfigUpdateAction);
			foreach(var it in Threads.Value) {
				if(!it.Raw.IsDie) {
					UpdateThreadRes(
						it.Bord,
						it.Url.ThreadNo,
						true,
						true)
						.Subscribe();
				}
			}
		}

		private static object CreatePassiveReloadQueueTag(string boardUrl, string threadNo) {
			return $"{ boardUrl }+{ threadNo }";
		}

		public static IObservable<(bool Successed, Data.FutabaContext Catalog, string ErrorMessage)> UpdateCatalog(Data.BoardData bord, Data.CatalogSortItem sort = null) {
			return Observable.Create<(bool Successed, Data.FutabaContext Catalog, string ErrorMessage)>(async o => {
				try {
					var f = await Task.Run<(bool Successed, Data.FutabaContext Catalog, string ErrorMessage)>(async () => {
						lock(lockObj) {
							if(!Catalog.Value.Select(x => x.Url.BaseUrl).Contains(bord.Url)) {
								Catalog.Value = Catalog.Value.Concat(new Data.FutabaContext[] {
								Data.FutabaContext.FromCatalogEmpty(bord),
							}).ToArray();
							}
						}

						var successed = false;
						Data.FutabaContext result = null;
						var error = "";
						try {
							var r = await FutabaApi.GetCatalog(
								bord.Url,
								Config.ConfigLoader.FutabaApi.Cookies,
								sort);
							if(!r.Successed) {
								if(!string.IsNullOrEmpty(r.Raw)) {
									var parser_ = new HtmlParser();
									var doc_ = parser_.ParseDocument(r.Raw);
									error = doc_.QuerySelector("body").TextContent; ;
								} else {
									error = "カタログの取得に失敗しました";
								}
								goto end;
							}
							Config.ConfigLoader.UpdateCookie(r.Cookies);
							var rr = await FutabaApi.GetCatalogHtml(
								bord.Url,
								Config.ConfigLoader.FutabaApi.Cookies,
								r.Response.Res.Length,
								sort);
							if(!rr.Successed) {
								error = "カタログHTMLの取得に失敗しました";
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
							var resList = new List<Data.NumberedResItem>(r.Response.Res);
							var sortList = new List<Data.NumberedResItem>();
							foreach(var c in counter.Where(x => !string.IsNullOrWhiteSpace(x.No))) {
								var t = resList.Where(x => x.No == c.No).FirstOrDefault();
								if(t != null) {
									sortList.Add(t);
									resList.Remove(t);
								}
								dic.Add(c.No, c.Count);
							}
							sortList.AddRange(
								resList.Reverse<Data.NumberedResItem>()
									.Select(x => new Data.NumberedResItem(x.No, x.Res, true)));
							lock(lockObj) {
								for(var i = 0; i < Catalog.Value.Length; i++) {
									if(Catalog.Value[i].Bord.Url == bord.Url) {
										successed = true;
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
								successed = true;
								result = Data.FutabaContext.FromCatalogResponse(
										bord, r.Response, sortList.ToArray(), dic, null);
								Catalog.Value = Catalog.Value.Concat(new Data.FutabaContext[] { result }).ToArray();
							}
						end:;
						}
						catch(Exception e) { // TODO: 適切なエラーに
							System.Diagnostics.Debug.WriteLine(e.ToString());
						}
						return (successed, result, error);
					});
					if(!f.Successed) {
						PutInformation(new Data.Information(f.ErrorMessage));
					}
					o.OnNext(f);
				}
				finally {
					o.OnCompleted();
				}
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}

		public static IObservable<(bool Successed, Data.FutabaContext New, Data.FutabaContext Old, string ErrorMessage)> UpdateThreadRes(Data.BoardData board, string threadNo, bool incremental = false, bool passive = false) {
			const int MinWaitSec = 30;
			const int ErrorWaitSec = 60;
			const int PassivePriority = 100;
			DateTime get((bool Successed, Data.FutabaContext New, Data.FutabaContext Old, string ErrorMessage) x) {
				var fireDate = DateTime.MinValue;
				if(x.Successed) {
					if(x.New.Raw.IsDie || x.New.Raw.IsMaxRes) {
						// スレ落ち or 最大レス
					} else {
						if(x.New.ResItems.Any()) {
							// レスあり
							if(x.Old.ResItems.Any() && (x.Old.ResItems.Length != x.New.ResItems.Length)) {
								var span = x.New.ResItems.Last().ResItem.Res.NowDateTime - x.Old.ResItems.Last().ResItem.Res.NowDateTime;
								var sec = Math.Max(span.TotalSeconds / 2, MinWaitSec);
								fireDate = DateTime.Now.AddSeconds(sec);
							} else {
								// 古いレスなし
								var time = x.New.Raw.NowDateTime ?? DateTime.Now;
								var span = time - x.New.ResItems.Last().ResItem.Res.NowDateTime;
								var sec = Math.Max(span.TotalSeconds / 2, MinWaitSec);
								fireDate = DateTime.Now.AddSeconds(sec);
							}
						} else {
							// レスなし
							var time = x.New.Raw.NowDateTime ?? DateTime.Now;
							var span = time - x.New.ResItems.Last().ResItem.Res.NowDateTime;
							var sec = Math.Max(span.TotalSeconds / 2, MinWaitSec);
							fireDate = DateTime.Now.AddSeconds(sec);
						}
						// スレ落ち予測時間に補正する
						if(x.New.Raw.DieDateTime.HasValue && (x.New.Raw.DieDateTime.Value.AddSeconds(60) < fireDate)) {
							fireDate = x.New.Raw.DieDateTime.Value;
						}
					}
				} else {
					// 取得失敗リトライ
					fireDate = DateTime.Now.AddSeconds(ErrorWaitSec);
				}
				return fireDate;
			}
			void fire(IObserver<(bool Successed, Data.FutabaContext New, Data.FutabaContext Old, string ErrorMessage)> o, bool incremental, int priority, DateTime fireTime, object tag) {
				PassiveReloadQueue.Push(Helpers.ConnectionQueueItem<object>.From(
					action: (_) => {
						UpdateThreadResInternal(board, threadNo, incremental, passive)
							.Subscribe(x => {
								if(x.Successed && (x.New.Raw.IsDie || x.New.Raw.IsMaxRes)) {
									o.OnNext(x);
									o.OnCompleted();
								} else {
									fire(o, true, PassivePriority, get(x), tag);
									o.OnNext(x);
								}
							});
					},
					priority: priority,
					fireTime: fireTime,
					tag: tag))
					.Subscribe();
			}

			return Observable.Create<(bool Successed, Data.FutabaContext New, Data.FutabaContext Old, string ErrorMessage)>(o => {
				fire(
					o, incremental,
					passive ? PassivePriority : 0,
					passive ? DateTime.Now.AddSeconds(MinWaitSec) :  DateTime.MinValue,
					CreatePassiveReloadQueueTag(board.Url, threadNo));

				return System.Reactive.Disposables.Disposable.Empty;
			});
		}
		private static IObservable<(bool Successed, Data.FutabaContext New, Data.FutabaContext Old, string ErrorMessage)> UpdateThreadResInternal(Data.BoardData bord, string threadNo, bool incremental, bool autoReload) {
			var u = new Data.UrlContext(bord.Url, threadNo);
			Data.FutabaContext parent = null;
			lock(lockObj) {
				parent = Threads.Value.Where(x => x.Url == u).FirstOrDefault();
			}
			if(!incremental || (parent == null) || (parent.ResItems.Length == 0)) {
				return UpdateThreadResAll(bord, threadNo);
			}

			return Observable.Create<(bool Successed, Data.FutabaContext New, Data.FutabaContext Old, string ErrorMessage)>(async o => {
				try {
					var ctx = await Task.Run<(bool Successed, Data.FutabaContext New, Data.FutabaContext Old, string ErrorMessage)>(() => {
						var successed = false;
						Data.FutabaContext result = null;
						Data.FutabaContext prev = null;
						var error = "";
						try {
							var res = parent.ResItems.Last().ResItem.No;
							var no = "";
							if(long.TryParse(res, out var v)) {
								no = (++v).ToString();
							} else {
								error = $"レスNo[{ res }]は不正なフォーマットです";
								goto end;
							}

							var r = FutabaApi.GetThreadRes(
								bord.Url,
								threadNo,
								no,
								Config.ConfigLoader.FutabaApi.Cookies);
							Task.WaitAll(r);
							if(!r.Result.Successed) {
								if(!string.IsNullOrEmpty(r.Result.Raw)) {
									var parser = new HtmlParser();
									var doc = parser.ParseDocument(r.Result.Raw);
									error = doc.QuerySelector("body").TextContent;
								} else {
									error = "スレッドの取得に失敗しました";
								}
								goto end;
							}
							Config.ConfigLoader.UpdateCookie(r.Result.cookies);
							lock(lockObj) {
								var f = Data.FutabaContext.FromThreadResResponse(parent, r.Result.Response);

								for(var i = 0; i < Threads.Value.Length; i++) {
									if(Threads.Value[i].Url == f.Url) {
										successed = true;
										result = f;
										prev = Threads.Value[i];
										Threads.Value[i] = f;
										Threads.Value = Threads.Value.ToArray();

										goto end;
									}
								}
								Threads.Value = Threads.Value.Concat(new Data.FutabaContext[] { f }).ToArray();

								successed = true;
								result = f;
								goto end;
							}
						}
						catch(Exception e) {
							System.Diagnostics.Debug.WriteLine(e.ToString());
							throw;
						}
					end:
						return (successed, result, prev, error);
					});
					PutThreadResResult(ctx.New, ctx.Old, ctx.ErrorMessage, autoReload);
					o.OnNext(ctx);
				}
				finally {
					o.OnCompleted();
				}
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}


		private static IObservable<(bool Successed, Data.FutabaContext New, Data.FutabaContext Old, string ErrorMessage)> UpdateThreadResAll(Data.BoardData bord, string threadNo) {
			return Observable.Create<(bool Successed, Data.FutabaContext New, Data.FutabaContext Old, string ErrorMessage)>(async o => {
				try {
					var ctx = await Task.Run<(bool Successed, Data.FutabaContext New, Data.FutabaContext Old, string ErrorMessage)>(() => {
						var successed = false;
						Data.FutabaContext result = null;
						Data.FutabaContext prev = null;
						var error = "";
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
								Config.ConfigLoader.FutabaApi.Cookies);
							var rr = FutabaApi.GetThreadResHtml(
								bord.Url,
								threadNo,
								Config.ConfigLoader.FutabaApi.Cookies);
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
									successed = true;
								} else {
									if(!r.Result.Successed) {
										if(!string.IsNullOrEmpty(r.Result.Raw)) {
											var parser = new HtmlParser();
											var doc = parser.ParseDocument(r.Result.Raw);
											error = doc.QuerySelector("body").TextContent;
										} else {
											error = "スレッドの取得に失敗しました";
										}
									} else {
										error = "スレッドHTMLの取得に失敗しました";
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
								var utc = 0L;
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
											utc = Util.TimeUtil.ToUnixTimeMilliseconds(dt);
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
								// 常時IP/ID表示の板はJSONのidが常に空なので破棄する
								if(!string.IsNullOrEmpty(ip) && bord.Extra.AlwaysIp) {
									ip = "";
								}
								if(!string.IsNullOrEmpty(id) && bord.Extra.AlwaysId) {
									time = time + " " + id;
									id = "";
								}
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
									error = "Contextの作成に失敗しました";
									goto end;
								}
								for(var i = 0; i < Threads.Value.Length; i++) {
									if(Threads.Value[i].Url == f.Url) {
										successed = true;
										result = f;
										prev = Threads.Value[i];
										Threads.Value[i] = f;
										Threads.Value = Threads.Value.ToArray();

										goto end;
									}
								}
								Threads.Value = Threads.Value.Concat(new Data.FutabaContext[] { f }).ToArray();

								successed = true;
								result = f;
								goto end;
							}
						}
						catch(Exception e) {
							System.Diagnostics.Debug.WriteLine(e.ToString());
							throw;
						}
					end:
						return (successed, result, prev, error);
					});
					PutThreadResResult(ctx.New, ctx.Old, ctx.ErrorMessage, false);
					o.OnNext(ctx);
				}
				finally {
					o.OnCompleted();
				}
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}

		private static void PutThreadResResult(Data.FutabaContext newFutaba, Data.FutabaContext oldFutaba, string error, bool autoReload) {
			if(newFutaba == null) {
				if(!autoReload
#if DEBUG
					|| true
#endif
					) {
					PutInformation(new Data.Information(string.IsNullOrEmpty(error) ? "スレ取得エラー" : error, newFutaba));
				}
			} else {
				if(oldFutaba == null) {
					// 何もしない
				} else {
					var c = newFutaba.ResItems.Length - oldFutaba.ResItems.Length;
					if(c <= 0) {
						if(newFutaba.Raw.IsDie) {
							PutInformation(new Data.Information("スレッドは落ちています", newFutaba));
						} else {
							if(!autoReload
#if DEBUG
								|| true
#endif
								) {
								PutInformation(new Data.Information("新着レスなし", newFutaba));
							}
						}
					} else {
						if(oldFutaba.ResItems.Any()) {
							PutInformation(new Data.Information($"{c}件の新着レス", newFutaba));
						} else {
							c = c - 1;
							if(c == 0) {
								if(!autoReload
#if DEBUG
									|| true
#endif
									) {
									PutInformation(new Data.Information("新着レスなし", newFutaba));
								}
							} else {
								PutInformation(new Data.Information($"{c}件の新着レス", newFutaba));
							}
						}
						if(newFutaba.Raw.IsDie) {
							PutInformation(new Data.Information("スレッドは落ちています", newFutaba));
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
						UpdateCatalog(Config.ConfigLoader.Board.Boards.Where(x => x.Url == url.BaseUrl).First())
							.Subscribe();
					}
				} else {
					var r = Threads.Value.Where(x => x.Url == url).FirstOrDefault();
					if(r == null) {
						UpdateThreadRes(
							Config.ConfigLoader.Board.Boards.Where(x => x.Url == url.BaseUrl).First(),
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
					PassiveReloadQueue.RemoveFromTag(CreatePassiveReloadQueueTag(url.BaseUrl, url.ThreadNo));
				}
			}
		}

		public static IObservable<(bool Successed, string LocalPath, byte[] FileBytes)> GetThreadResImage(Data.UrlContext url, Data.ResItem item, bool isAsync = true) {
			System.Diagnostics.Debug.Assert(url != null);
			System.Diagnostics.Debug.Assert(item != null);

			var localFile = CreateLocalFileName(url.BaseUrl, item.Src);
			var localPath = Path.Combine(Config.ConfigLoader.InitializedSetting.CacheDirectory, localFile);
			var uri = new Uri(url.BaseUrl);
			var u = string.Format("{0}://{1}{2}", uri.Scheme, uri.Authority, item.Src);
			return GetUrlImage(u, localPath, isAsync);
		}

		public static IObservable<(bool Successed, string LocalPath, byte[] FileBytes)> GetThumbImage(Data.UrlContext url, Data.ResItem item, bool isAsync = true) {
			System.Diagnostics.Debug.Assert(url != null);
			System.Diagnostics.Debug.Assert(item != null);
			System.Diagnostics.Debug.Assert(0 < item.Fsize);

			var localFile = CreateLocalFileName(url.BaseUrl, item.Thumb);
			var localPath = Path.Combine(Config.ConfigLoader.InitializedSetting.CacheDirectory, localFile);
			var uri = new Uri(url.BaseUrl);
			var u = string.Format("{0}://{1}{2}", uri.Scheme, uri.Authority, item.Thumb);
			return GetUrlImage(u, localPath, isAsync);
		}

		public static IObservable<(bool Successed, string LocalPath, byte[] FileBytes)> GetThumbImages(Data.UrlContext url, Data.ResItem[] items, bool isAsync = true) {
			System.Diagnostics.Debug.Assert(url != null);
			System.Diagnostics.Debug.Assert(items != null);

			return Observable.Create<(bool Successed, string LocalPath, byte[] FileBytes)>(o => {
				var itemCount = items.Length;
				foreach(var item in items) {
					var localFile = CreateLocalFileName(url.BaseUrl, item.Thumb);
					var localPath = Path.Combine(Config.ConfigLoader.InitializedSetting.CacheDirectory, localFile);
					var uri = new Uri(url.BaseUrl);
					var url2 = string.Format("{0}://{1}{2}", uri.Scheme, uri.Authority, item.Thumb);
					GetUrlImage(
						HttpClient,
						async (c, u) => {
							try {
								var t1 = await c.GetAsync(u);
								var t2 = t1.IsSuccessStatusCode ? await t1.Content.ReadAsByteArrayAsync() : default;
								return (true, t1.StatusCode, t2);
							}
							catch(HttpRequestException) {
								return (false, default, default);
							}
						},
						url2, localPath, isAsync)
						.Finally(() => {
							var c = System.Threading.Interlocked.Decrement(ref itemCount);
							if(c <= 0) {
								o.OnCompleted();
							}
						})
						.Subscribe(x => o.OnNext(x));
				}
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}

		public static IObservable<(bool Successed, string NextOrMessage)> PostThread(Data.BoardData bord,
			string name, string email, string subject,
			string comment, string filePath, string passwd) {
			return Observable.Create<(bool Successed, string Message)>(async o => {
				try {
					var r = await FutabaApi.PostThread(bord,
						Config.ConfigLoader.FutabaApi.Cookies, Config.ConfigLoader.FutabaApi.Ptua,
						name, email, subject, comment, filePath, passwd);
					if(r.Raw == null) {
						o.OnNext((false, "不明なエラー"));
					} else {
						Config.ConfigLoader.UpdateCookie(r.Cookies);
						Config.ConfigLoader.UpdateFutabaInputData(bord, subject, name, email, passwd);
						o.OnNext((r.Successed, r.NextOrMessage));
					}
				}
				finally {
					o.OnCompleted();
				}
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}

		public static IObservable<(bool Successed, string Message)> PostRes(Data.BoardData bord, string threadNo,
			string name, string email, string subject,
			string comment, string filePath, string passwd) {
			return Observable.Create<(bool Successed, string Message)>(async o => {
				try {
					var r = await FutabaApi.PostRes(bord, threadNo,
						Config.ConfigLoader.FutabaApi.Cookies, Config.ConfigLoader.FutabaApi.Ptua,
						name, email, subject, comment, filePath, passwd);
					if(r.Raw == null) {
						o.OnNext((false, "不明なエラー"));
					} else {
						Config.ConfigLoader.UpdateCookie(r.Cookies);
						Config.ConfigLoader.UpdateFutabaInputData(bord, subject, name, email, passwd);
						o.OnNext((r.Successed, r.Raw));
					}
				}
				finally {
					o.OnCompleted();
				}
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}

		public static IObservable<(bool Successed, string LocalPath, byte[] FileBytes)> GetUploaderFile(string url) {
			System.Diagnostics.Debug.Assert(url != null);
			var localFile = CreateLocalFileNameFromUploader(url);
			if(Config.ConfigLoader.MimeFutaba.Types.Select(x => x.Ext).Contains(Path.GetExtension(localFile).ToLower())) {
				var localPath = Path.Combine(Config.ConfigLoader.InitializedSetting.CacheDirectory, localFile);
				return GetUrlImage(url, localPath);
			} else {
				// TODO: ダウンロードフォルダにDL
				throw new NotImplementedException();
			}
		}

		private static IObservable<(bool Successed, string LocalPath, byte[] FileBytes)> GetUrlImage<T>(
			T client, Func<T, string, (bool IsSucceeded, System.Net.HttpStatusCode StatusCode, byte[] Content)> request,
			string url, string localPath,
			bool isAsync = true) {

			void run(IObserver<(bool Successed, string LocalPath, byte[] FileBytes)> o) {
				try {
					if(File.Exists(localPath)) {
						o.OnNext((true, localPath, null));
					} else {
						var res = request(client, url);
						if(res.IsSucceeded && res.StatusCode == System.Net.HttpStatusCode.OK) {
							Observable.Create<byte[]>(async (oo) => {
								try {
									var b = res.Content;
									try {
										if(!File.Exists(localPath)) {
											using(var fs = new FileStream(localPath, FileMode.OpenOrCreate)) {
												fs.Write(b, 0, b.Length);
												fs.Flush();
											}
										}
										oo.OnNext(b);
									}
									catch(IOException e) {
										await Task.Delay(500);
										oo.OnError(e);
									}
								}
								finally {
									oo.OnCompleted();
								}
								return System.Reactive.Disposables.Disposable.Empty;
							}).Retry(5)
							.Subscribe(
								s => {
									o.OnNext((true, localPath, s));
								},
								ex => {
									o.OnNext((false, null, null));
								});
						} else {
							o.OnNext((false, null, null));
							// TODO: o.OnError();
						}
					}
				}
				finally {
					o.OnCompleted();
				}
			};

			return Observable.Create<(bool Successed, string LocalPath, byte[] FileBytes)>(o => {
				if(isAsync) {
					Task.Run(() => run(o));
				} else {
					run(o);
				}
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}

		private static IObservable<(bool Successed, string LocalPath, byte[] FileBytes)> GetUrlImage<T>(
			T client, Func<T, string, Task<(bool IsSucceeded, System.Net.HttpStatusCode StatusCode, byte[] Content)>> request,
			string url, string localPath,
			bool isAsync = true) {

			async void run(IObserver<(bool Successed, string LocalPath, byte[] FileBytes)> o) {
				try {
					if(File.Exists(localPath)) {
						o.OnNext((true, localPath, null));
					} else {
						var res = await request(client, url);
						if(res.IsSucceeded && res.StatusCode == System.Net.HttpStatusCode.OK) {
							Observable.Create<byte[]>(async (oo) => {
								var b = res.Content;
								try {
									if(!File.Exists(localPath)) {
										using(var fs = new FileStream(localPath, FileMode.OpenOrCreate)) {
											fs.Write(b, 0, b.Length);
											fs.Flush();
										}
									}
									oo.OnNext(b);
								}
								catch(IOException e) {
									await Task.Delay(500);
									oo.OnError(e);
								}
								finally {
									oo.OnCompleted();
								}
								return System.Reactive.Disposables.Disposable.Empty;
							}).Retry(5)
							.Subscribe(
								s => {
									o.OnNext((true, localPath, s));
								},
								ex => {
									o.OnNext((false, null, null));
								});
						} else {
							o.OnNext((false, null, null));
							// TODO: o.OnError();
						}
					}
				}
				finally {
					o.OnCompleted();
				}
			};

			return Observable.Create<(bool Successed, string LocalPath, byte[] FileBytes)>(async o => {
				if(isAsync) {
					Task.Run(() => run(o));
				} else {
					run(o);
				}
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}

		private static IObservable<(bool Successed, string LocalPath, byte[] FileBytes)> GetUrlImage(
			string url, string localPath,
			bool isAsync = true) {

			return GetUrlImage(
				HttpClient,
				async (c, u) => {
					try {
						if(isAsync) {
							var t1 = await c.GetAsync(u);
							var t2 = t1.IsSuccessStatusCode ? await t1.Content.ReadAsByteArrayAsync() : default;
							return (true, t1.StatusCode, t2);
						} else {
							var t1 = c.GetAsync(u);
							t1.Wait();
							if(t1.IsCompleted) {
								var t2 = t1.Result.Content.ReadAsByteArrayAsync();
								t2.Wait();
								return (true, t1.Result.StatusCode, t2.Result);
							} else {
								return (true, t1.Result.StatusCode, default);
							}
						}
					}
					catch(HttpRequestException) {
						return (false, default, default);
					}
				}, url, localPath,
				isAsync);
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

		public static string GetUploderLocalFilePath(string url) {
			System.Diagnostics.Debug.Assert(url != null);

			return Path.Combine(Config.ConfigLoader.InitializedSetting.CacheDirectory,
				CreateLocalFileNameFromUploader(url));
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

		public static IObservable<(bool Successed, string Message)> PostDeleteThreadRes(Data.BoardData bord, string threadNo, bool imageOnlyDel, string passwd) {
			return Observable.Create<(bool Successed, string Message)>(async o => {
				try {
					var r = await FutabaApi.PostDeleteThreadRes(bord.Url, threadNo, Config.ConfigLoader.FutabaApi.Cookies, imageOnlyDel, passwd);
					if(r.Raw == null) {
						o.OnNext((false, "不明なエラー"));
					} else {
						Config.ConfigLoader.UpdateCookie(r.Cookies);
						o.OnNext((r.Successed, r.Raw));
					}
				}
				finally {
					o.OnCompleted();
				}
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}

		public static IObservable<(bool Successed, string Message)> PostSoudane(Data.BoardData bord, string threadNo) {
			return SoudaneQueue.Push(Helpers.ConnectionQueueItem<(bool Successed, string Message)>.From(
				(o) => {
					try {
						var task = FutabaApi.PostSoudane(bord.Url, threadNo);
						task.Wait();
						if(task.Result.Raw == null) {
							o.OnNext((false, "不明なエラー"));
						} else {
							o.OnNext((task.Result.Successed, task.Result.Raw));
						}
					}
					finally {
						o.OnCompleted();
					}
				}));
		}

		public static IObservable<(bool Successed, string Message)> PostDel(Data.BoardData bord, string threadNo, string resNo) {
			return DelQueue.Push(Helpers.ConnectionQueueItem<(bool Successed, string Message)>.From(
				(o) => {
					try {
						var task = FutabaApi.PostDel(bord.Url, threadNo, resNo /*, Config.ConfigLoader.FutabaApi.Cookies */);
						task.Wait();
						if(task.Result.Raw == null) {
							o.OnNext((false, "不明なエラー"));
						} else {
							o.OnNext((task.Result.Successed, task.Result.Raw));
						}
					}
					finally {
						o.OnCompleted();
					}
				}));
		}

		public static IObservable<(bool Successed, string FileNameOrMessage)> UploadUp2(string filePath, string passwd) {
			return UploadUp2("", filePath, passwd);
		}

		public static IObservable<(bool Successed, string FileNameOrMessage)> UploadUp2(string comment, string filePath, string passwd) {
			return Observable.Create<(bool Successed, string Message)>(async o => {
				try {
					var r = await FutabaApi.UploadUp2(comment, filePath, passwd);
					Config.ConfigLoader.UpdateFutabaPassword(passwd);
					o.OnNext((r.Successed, r.Message));
				}
				finally {
					o.OnCompleted();
				}
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}

		public static IObservable<(bool Successed, string LocalPath, byte[] FileBytes)> GetThumbImageUp(Data.UrlContext threadUrl, string fileName) {
			var f = Path.GetFileNameWithoutExtension(fileName);
			var cache = Path.Combine(Config.ConfigLoader.InitializedSetting.CacheDirectory, $"{ f }.thumb.png");
			if(File.Exists(cache)) {
				return Observable.Create<(bool Successed, string LocalPath, byte[] FileBytes)>(async o => {
					try {
						await Task.Delay(1);
						o.OnNext((true, cache, null));
					}
					finally {
						o.OnCompleted();
					}
					return System.Reactive.Disposables.Disposable.Empty;
				});
			} else {
				return GetCompleteUrlUp(threadUrl, f)
					.Select(x => {
						if(File.Exists(cache)) {
							return (true, cache, default(byte[]));
						} else {
							if(x.Successed
								&& (x.Raw is Data.AppsweetsThumbnailCompleteResponse r)
								&& !string.IsNullOrEmpty(r.Content)) {

								return (true, cache, Convert.FromBase64String(
									r.Content.Substring("data:image/png;base64,".Length)));
							}
						}
						return (false, default(string), default(byte[]));
					});
			}
		}

		public static IObservable<(bool Successed, string UrlOrMessage, object Raw)> GetCompleteUrlUp(Data.UrlContext threadUrl, string fileNameWitfOutExtension) {
			return Observable.Create<(bool Successed, string Message, object Raw)>(async o => {
				try {
					var r = await FutabaApi.GetCompleteUrlUp(GetFutabaThreadUrl(threadUrl), fileNameWitfOutExtension);
					if(r.Successed) {
						System.Diagnostics.Debug.Assert(r.CompleteResponse != null);
						if(!string.IsNullOrEmpty(r.CompleteResponse.Name)) {
							var url = Config.ConfigLoader.Uploder.Uploders
								.Where(x => Regex.IsMatch(r.CompleteResponse.Name, x.File))
								.Select(x => x.Root + r.CompleteResponse.Name)
								.FirstOrDefault();
							if(url != null) {
								o.OnNext((true, url, r.CompleteResponse));
								goto end;
							}
						}
					} else if(r.ErrorResponse != null) {
						o.OnNext((false, r.ErrorResponse.Error, r.ErrorResponse));
						goto end;
					}
					o.OnNext((false, "不明なエラー", null));
				}
				finally {
					o.OnCompleted();
				}
			end:
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}
			
		public static void PutInformation(Data.Information information) {
			Observable.Create<Data.Information>(o => {
				try {
					Informations.AddOnScheduler(information);
					o.OnNext(information);
				}
				finally {
					o.OnCompleted();
				}
				return System.Reactive.Disposables.Disposable.Empty;
			}).Delay(TimeSpan.FromSeconds(3))
				.Subscribe(x => Informations.RemoveOnScheduler(x));
		}


		public static string GetFutabaThreadUrl(Data.UrlContext url) {
			if(url.IsCatalogUrl) {
				return $"{ url.BaseUrl }futaba.php?mode=cat";
			} else {
				return $"{ url.BaseUrl }res/{ url.ThreadNo }.htm";
			}
		}

		public static string GetFutabaThreadImageUrl(Data.UrlContext url, Data.ResItem item) {
			var uri = new Uri(url.BaseUrl);
			return $"{  uri.Scheme }://{ uri.Authority }{ item.Src }";
		}

		public static string GetFutabaThumbImageUrl(Data.UrlContext url, Data.ResItem item) {
			var uri = new Uri(url.BaseUrl);
			return $"{  uri.Scheme }://{ uri.Authority }{ item.Thumb }";
		}

		public static string GetGoogleImageSearchdUrl(string url) {
			return $"https://www.google.com/searchbyimage?image_url={ System.Web.HttpUtility.UrlEncode(url) }";
		}

		public static string GetAscii2dImageSearchUrl(string url) {
			return $"https://ascii2d.net/search/url/{ System.Web.HttpUtility.UrlEncode(url) }";
		}
	}
}