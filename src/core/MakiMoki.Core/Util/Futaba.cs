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
		public static IReadOnlyReactiveProperty<IEnumerable<Data.Information>> Informations { get; private set; }
		private static ReactiveProperty<IEnumerable<Data.Information>> InformationsProperty { get; set; }

		private static Helpers.ConnectionQueue<object> PassiveReloadQueue { get; set; }
		private static Helpers.ConnectionQueue<(bool Successed, string Message)> SoudaneQueue { get; set; }
		private static Helpers.ConnectionQueue<(bool Successed, string Message)> DelQueue { get; set; }

		private static readonly Action BoardConfigUpdateAction = () => {
			var dic = Config.ConfigLoader.Board.Boards.ToDictionary(x => x.Url);
			if(Catalog != null) {
				Catalog.Value = Catalog.Value
					.Where(x => dic.Keys.Any(y => y == x.Url.BaseUrl))
					.ToArray();
			}
			if(Threads != null) {
				Threads.Value = Threads.Value
					.Where(x => dic.Keys.Any(y => y == x.Url.BaseUrl))
					.ToArray();
				var d2 = dic
					.Where(x => x.Value.MakiMokiExtra.IsEnabledPassiveReload)
					.ToDictionary(x => x.Key, x => x.Value);
				var d3 = Threads.Value
					.Where(x => !x.Raw.IsDie && !x.Raw.IsMaxRes && d2.Keys.Any(y => y == x.Url.BaseUrl))
					.ToDictionary(x => CreatePassiveReloadQueueTag(x.Url.BaseUrl, x.Url.ThreadNo));
				foreach(var it in PassiveReloadQueue.ExceptTag(d3.Keys)) {
					if(d3.TryGetValue(it, out var v)) {
						// TODO: 直後発火で本当に良いのか？
						UpdateThreadRes(v.Board, v.Url.ThreadNo, true, true)
							.Subscribe();
					}
				}
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
			InformationsProperty = new ReactiveProperty<IEnumerable<Data.Information>>(Array.Empty<Data.Information>());
			Informations = InformationsProperty.ToReadOnlyReactiveProperty();

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
				name: "そうだねAPIキュー",
				forceWait: true
			);
			DelQueue = new Helpers.ConnectionQueue<(bool Successed, string Message)>(
				name: "delAPIキュー",
				maxConcurrency: 1,
				forceWait: true,
				waitTime: 5000
			);
			Config.ConfigLoader.BoardConfigUpdateNotifyer.AddHandler(BoardConfigUpdateAction);
			foreach(var it in Threads.Value) {
				if(!it.Raw.IsDie) {
					UpdateThreadRes(
						it.Board,
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

		public static IObservable<(bool Successed, Data.FutabaContext Catalog, string ErrorMessage)> UpdateCatalog(Data.BoardData board, Data.CatalogSortItem sort = null) {
			return Observable.Create<(bool Successed, Data.FutabaContext Catalog, string ErrorMessage)>(async o => {
				try {
					var f = await Task.Run<(bool Successed, Data.FutabaContext Catalog, string ErrorMessage)>(async () => {
						lock(lockObj) {
							if(!Catalog.Value.Select(x => x.Url.BaseUrl).Contains(board.Url)) {
								Catalog.Value = Catalog.Value.Concat(new Data.FutabaContext[] {
								Data.FutabaContext.FromCatalogEmpty(board),
							}).ToArray();
							}
						}

						var successed = false;
						Data.FutabaContext result = null;
						var error = "";
						try {
							var r = await FutabaApi.GetCatalog(
								board.Url,
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
							Config.ConfigLoader.UpdateCookie(board.Url, r.Cookies);
							var rr = await FutabaApi.GetCatalogHtml(
								board.Url,
								Config.ConfigLoader.FutabaApi.Cookies,
								r.Response.Res.Length,
								sort);
							if(!rr.Successed) {
								error = "カタログHTMLの取得に失敗しました";
								goto end;
							}
							Config.ConfigLoader.UpdateCookie(board.Url, rr.Cookies);
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
									if(Catalog.Value[i].Board.Url == board.Url) {
										successed = true;
										result = Data.FutabaContext.FromCatalogResponse(
											board,
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
										board, r.Response, sortList.ToArray(), dic, null);
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
			var url = board.Url; // 設定が変わる場合に備えてURLを保持
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
						/* スレ落ち時間と板の流れがあっていないと連続更新されるのでいったん削除
						if(x.New.Raw.DieDateTime.HasValue && (x.New.Raw.DieDateTime.Value.AddSeconds(60) < fireDate)) {
							fireDate = x.New.Raw.DieDateTime.Value;
						}
						*/
					}
				} else {
					// 取得失敗リトライ
					fireDate = DateTime.Now.AddSeconds(ErrorWaitSec);
				}
				return fireDate;
			}
			void fire(IObserver<(bool Successed, Data.FutabaContext New, Data.FutabaContext Old, string ErrorMessage)> o, bool incremental, bool passive, int priority, DateTime fireTime, object tag) {
				var d = default(IDisposable);
				d = PassiveReloadQueue.Push(Helpers.ConnectionQueueItem<object>.From(
					action: (_) => {
						// 設定が変わる場合に備えてURLから検索
						var b = Config.ConfigLoader.Board.Boards
							.Where(x => x.Url == url)
							.FirstOrDefault();
						if(b == null) {
							o.OnCompleted();
						} else {
							var dd = default(IDisposable);
							dd = UpdateThreadResInternal(b, threadNo, incremental, passive)
								.Subscribe(x => {
									dd?.Dispose();
									if(x.Successed && (x.New.Raw.IsDie || x.New.Raw.IsMaxRes)) {
										o.OnNext(x);
										o.OnCompleted();
									} else {
										if(b.MakiMokiExtra.IsEnabledPassiveReload) {
											fire(o, true, true, PassivePriority, get(x), tag);
										}
										o.OnNext(x);
									}
								});
						}
					},
					priority: priority,
					fireTime: fireTime,
					tag: tag))
					.Subscribe(_ => d?.Dispose());
			}

			return Observable.Create<(bool Successed, Data.FutabaContext New, Data.FutabaContext Old, string ErrorMessage)>(o => {
				fire(
					o, incremental, passive,
					passive ? PassivePriority : 0,
					passive ? DateTime.Now.AddSeconds(MinWaitSec) :  DateTime.MinValue,
					CreatePassiveReloadQueueTag(url, threadNo));

				return System.Reactive.Disposables.Disposable.Empty;
			});
		}

		private static IObservable<(bool Successed, Data.FutabaContext New, Data.FutabaContext Old, string ErrorMessage)> UpdateThreadResInternal(Data.BoardData board, string threadNo, bool incremental, bool autoReload) {
			var u = new Data.UrlContext(board.Url, threadNo);
			Data.FutabaContext parent = null;
			lock(lockObj) {
				parent = Threads.Value.Where(x => x.Url == u).FirstOrDefault();
			}
			if(!incremental || (parent == null) || (parent.ResItems.Length == 0)) {
				lock(lockObj) {
					if(!Threads.Value.Select(x => x.Url).Contains(u)) {
						Threads.Value = Threads.Value.Concat(new Data.FutabaContext[] {
							Data.FutabaContext.FromThreadEmpty(board, threadNo),
						}).ToArray();
					}
				}
				return FutabaApiReactive.GetThreadResAll(board, threadNo)
					.Select(x => {
						var @new = default(Data.FutabaContext);
						var prev = default(Data.FutabaContext);
						if(x.Successed) {
							if(x.IsDie) {
								// スレ落ち処理
								var fc = default(Data.FutabaContext);
								var index = 0;
								lock(lockObj) {
									for(index = 0; index < Threads.Value.Length; index++) {
										if(Threads.Value[index].Url == u) {
											fc = Threads.Value[index];
											break;
										}
									}

									if(fc != null) {
										var fc2 = default(Data.FutabaContext);
										if(x.RawResponse != null) {
											fc2 = Data.FutabaContext.FromThreadResResponse404(
												Catalog.Value.Where(x => x.Board.Url == board.Url).FirstOrDefault(),
												fc, x.RawResponse);
										}
										if(fc2 != null) {
											var ary = Threads.Value.ToArray();
											ary[index] = fc2;
											Threads.Value = ary;
											@new = fc2;
											prev = fc;
										} else {
											@new = fc;
											prev = fc;
										}
									}
								}
								return (true, @new, prev, x.ErrorMessage);
							} else {
								Config.ConfigLoader.UpdateCookie(board.Url, x.Cookies);
								lock(lockObj) {
									var f = x.Data;
									for(var i = 0; i < Threads.Value.Length; i++) {
										if(Threads.Value[i].Url == f.Url) {
											@new = f;
											prev = Threads.Value[i];
											Threads.Value[i] = f;
											Threads.Value = Threads.Value.ToArray();

											goto end;
										}
									}
									Threads.Value = Threads.Value.Concat(new Data.FutabaContext[] { f }).ToArray();
								}
							}
						} else {
							// 失敗時の処理はない
						}
					end:;
						PutThreadResResult(@new, prev, x.ErrorMessage, false);
						return (x.Successed, @new, prev, x.ErrorMessage);
					});
			} else {
				return FutabaApiReactive.GetThreadRes(board, threadNo, parent, incremental)
					.Select(x => {
						var r = (
							Successed: x.Successed,
							New: default(Data.FutabaContext),
							Old: default(Data.FutabaContext),
							ErrorMessage: default(string)
						);
						if(x.Successed) {
							Config.ConfigLoader.UpdateCookie(board.Url, x.Cookies);
							lock(lockObj) {
								var f = x.Data;
								for(var i = 0; i < Threads.Value.Length; i++) {
									if(Threads.Value[i].Url == f.Url) {
										var p = Threads.Value[i];
										Threads.Value[i] = f;
										Threads.Value = Threads.Value.ToArray();
										r = (true, f, p, x.ErrorMessage);
										goto end;
									}
								}
								Threads.Value = Threads.Value.Concat(new Data.FutabaContext[] { f }).ToArray();
								r = (true, x.Data, null, x.ErrorMessage);
								goto end;
							}
						} else {
							r = (false, null, null, x.ErrorMessage);
						}
					end:
						PutThreadResResult(r.New, r.Old, r.ErrorMessage, autoReload);
						return r;
					});
			}
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
					PassiveReloadQueue.RemoveFromTags(
						Threads.Value
							.Where(x => x.Url.BaseUrl == url.BaseUrl)
							.Select(x => CreatePassiveReloadQueueTag(url.BaseUrl, url.ThreadNo))
							.ToArray());
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

		public static IObservable<(bool Successed, string NextOrMessage)> PostThread(
			Data.BoardData board,
			string name, string email, string subject,
			string comment, string filePath, string passwd) {

			return FutabaApiReactive.PostThread(board, name, email, subject, comment, filePath, passwd)
				.Select(x => {
					if(x.Cookies != null) {
						Config.ConfigLoader.UpdateCookie(board.Url, x.Cookies);
					}
					if(x.Successed) {
						Config.ConfigLoader.UpdateFutabaInputData(board, subject, name, email, passwd);
					}
					return (x.Successed, x.NextOrMessage);
				});
		}

		public static IObservable<(bool Successed, string Message)> PostRes(Data.BoardData board, string threadNo,
			string name, string email, string subject,
			string comment, string filePath, string passwd) {

			return FutabaApiReactive.PostRes(board, threadNo, name, email, subject, comment, filePath, passwd)
				.Select(x => {
					if(x.Cookies != null) {
						Config.ConfigLoader.UpdateCookie(board.Url, x.Cookies);
					}
					if(x.Successed) {
						Config.ConfigLoader.UpdateFutabaInputData(board, subject, name, email, passwd);
					}
					return (x.Successed, x.Message);
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
						try {
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
						catch(TimeoutException) {
							o.OnNext((false, null, null));
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
						try {
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
						catch(TimeoutException) {
							o.OnNext((false, null, null));
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
							using(var t1 = await c.GetAsync(u)) {
								var t2 = t1.IsSuccessStatusCode ? await t1.Content.ReadAsByteArrayAsync() : default;
								return (true, t1.StatusCode, t2);
							}
						} else {
							using(var t1 = c.GetAsync(u)) {
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
					}
					catch(HttpRequestException) {
						return (false, default, default);
					}
					catch(TimeoutException) {
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

		public static IObservable<(bool Successed, string Message)> PostDeleteThreadRes(Data.BoardData board, string threadNo, bool imageOnlyDel, string passwd) {
			return FutabaApiReactive.PostDeleteThreadRes(board, threadNo, imageOnlyDel, passwd)
				.Select(x => {
					if(x.Successed) {
						Config.ConfigLoader.UpdateCookie(board.Url, x.Cookies);
					}
					return (x.Successed, x.Message);
				});
		}

		public static IObservable<(bool Successed, string Message)> PostSoudane(Data.BoardData board, string threadNo) {
			return SoudaneQueue.Push(Helpers.ConnectionQueueItem<(bool Successed, string Message)>.From(
				(o) => {
					FutabaApiReactive.PostSoudane(board, threadNo)
						.Subscribe(x => {
							try {
								o.OnNext(x);
							}
							finally {
								o.OnCompleted();
							}
						});

				}));
		}

		public static IObservable<(bool Successed, string Message)> PostDel(Data.BoardData board, string threadNo, string resNo) {
			return DelQueue.Push(Helpers.ConnectionQueueItem<(bool Successed, string Message)>.From(
				(o) => {
					FutabaApiReactive.PostDel(board, threadNo, resNo)
						.Subscribe(x => {
							try {
								o.OnNext(x);
							}
							finally {
								o.OnCompleted();
							}
						});
				}));
		}

		public static IObservable<(bool Successed, string FileNameOrMessage)> UploadUp2(string filePath, string passwd) {
			return UploadUp2("", filePath, passwd);
		}

		public static IObservable<(bool Successed, string FileNameOrMessage)> UploadUp2(string comment, string filePath, string passwd) {
			return FutabaApiReactive.UploadUp2(comment, filePath, passwd)
				.Select(x => {
					Config.ConfigLoader.UpdateFutabaPassword(passwd);
					return x;
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
				return FutabaApiReactive.GetThumbImageUp(
					threadUrl,
					fileName,
					Config.ConfigLoader.Uploder.Uploders)
					.Select(x => {
						if(File.Exists(cache)) {
							return (true, cache, default(byte[]));
						} else {
							if(x.Successed) {
								return (true, cache, x.FileBytes);
							} else {
								return (false, default(string), default(byte[]));
							}
						}
					});
			}
		}

		public static IObservable<(bool Successed, string UrlOrMessage, object Raw)> GetCompleteUrlUp(Data.UrlContext threadUrl, string fileNameWitfOutExtension) {
			return FutabaApiReactive.GetCompleteUrlUp(
				threadUrl,
				fileNameWitfOutExtension,
				Config.ConfigLoader.Uploder.Uploders);
		}
			
		public static void PutInformation(Data.Information information) {
			Observable.Return(information)
				.ObserveOn(DefaultScheduler.Instance)
				.Select(x => {
					InformationsProperty.Value = InformationsProperty.Value.Append(x);
					return x;
				}).Delay(TimeSpan.FromSeconds(3))
				.ObserveOn(DefaultScheduler.Instance)
				.Subscribe(x => {
					InformationsProperty.Value = InformationsProperty.Value.Where(y => !object.ReferenceEquals(x, y));
				});
		}


		public static string GetFutabaThreadUrl(Data.UrlContext url) {
			return FutabaApiReactive.GetFutabaThreadUrl(url);
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
			return $"https://www.google.com/searchbyimage?sbisrc=app&image_url={ System.Web.HttpUtility.UrlEncode(url) }";
		}

		public static string GetGoogleLensUrl(string url) {
			return $"https://lens.google.com/uploadbyurl?url={System.Web.HttpUtility.UrlEncode(url)}";
		}

		public static string GetAscii2dImageSearchUrl(string url) {
			return $"https://ascii2d.net/search/url/{ System.Web.HttpUtility.UrlEncode(url) }";
		}
	}
}