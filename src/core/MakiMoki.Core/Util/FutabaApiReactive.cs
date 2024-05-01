using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Yarukizero.Net.MakiMoki.Util {
	public static class FutabaApiReactive {
		public static IObservable<(
			bool Successed,
			Data.FutabaContext Data,
			string ErrorMessage,
			Data.FutabaResponse RawResponse,
			Data.Cookie2[] Cookies
		)> GetThreadRes(
			Data.BoardData board, string threadNo,
			Data.FutabaContext parent,
			Data.Cookie2[] cookies,
			bool incremental) {

			var u = new Data.UrlContext(board.Url, threadNo);
			if(!incremental || (parent == null) || (parent.ResItems.Length == 0)) {
				return GetThreadResAll(board, threadNo, cookies)
					.Select(x => (x.Successed && !x.IsDie, x.Data, x.ErrorMessage, x.RawResponse, x.Cookies));
			}

			return Observable.Create<(bool Successed, Data.FutabaContext Data, string ErrorMessage, Data.FutabaResponse RawResponse, Data.Cookie2[] Cookies)>(async o => {
				try {
					var ctx = await Task.Run<(bool Successed, Data.FutabaContext Data, string ErrorMessage, Data.FutabaResponse RawResponse, Data.Cookie2[] Cookies)>(() => {
						var successed = false;
						var result = default(Data.FutabaContext);
						var cookies = Array.Empty<Data.Cookie2>();
						var response = default(Data.FutabaResponse);
						var error = "";
						try {
							var res = parent.ResItems.Last().ResItem.No;
							var no = "";
							if(long.TryParse(res, out var v)) {
								no = (++v).ToString();
							} else {
								error = $"レスNo[{res}]は不正なフォーマットです";
								goto end;
							}

							var r = FutabaApi.GetThreadRes(
								board.Url,
								threadNo,
								no,
								cookies);
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
							successed = true;
							response = r.Result.Response;
							cookies = r.Result.cookies;
							result = Data.FutabaContext.FromThreadResResponse(parent, r.Result.Response);
						}
						catch(Exception e) {
							System.Diagnostics.Debug.WriteLine(e.ToString());
							throw;
						}
					end:
						return (successed, result, error, response, cookies);
					});
					o.OnNext(ctx);
				}
				finally {
					o.OnCompleted();
				}
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}


		public static IObservable<(
			bool Successed,
			bool IsDie,
			Data.FutabaContext Data,
			string ErrorMessage,
			Data.FutabaResponse RawResponse,
			Data.Cookie2[] Cookies
		)> GetThreadResAll(Data.BoardData board, string threadNo, Data.Cookie2[] cookies) {
			return Observable.Create<(bool Successed, bool IsDie, Data.FutabaContext Data, string ErrorMessage, Data.FutabaResponse RawResponse, Data.Cookie2[] Cookies)>(async o => {
				try {
					var ctx = await Task.Run<(bool Successed, bool IsDie, Data.FutabaContext Data, string ErrorMessage, Data.FutabaResponse RawResponse, Data.Cookie2[] Cookies)>(() => {
						var successed = false;
						var isDie = false;
						var result = default(Data.FutabaContext);
						var response = default(Data.FutabaResponse);
						var cookies = Array.Empty<Data.Cookie2>();
						var error = "";
						try {
							var u = new Data.UrlContext(board.Url, threadNo);
							var r = FutabaApi.GetThreadRes(
								board.Url,
								threadNo,
								cookies);
							var rr = FutabaApi.GetThreadResHtml(
								board.Url,
								threadNo,
								cookies);
							Task.WaitAll(r, rr);
							response = r.Result.Response;
							if(!r.Result.Successed || !rr.Result.Successed) {
								// 404の場脚の処理を行う
								if((r.Result.Successed && r.Result.Response.IsDie)
									|| (rr.Result.Is404)) {

									isDie = true;
									successed = true;
									error = "スレッドは落ちています";
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

							{
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
										var tms = $"{m.Groups[1].Value} {m.Groups[2].Value}";
										if(DateTime.TryParseExact(tms, "yy/MM/dd HH:mm:ss",
											System.Globalization.CultureInfo.InvariantCulture,
											System.Globalization.DateTimeStyles.AdjustToUniversal,
											out var dt)) {

											// JST→UTCを時差なしで行っているので再補正する
											utc = Util.TimeUtil.ToUnixTimeMilliseconds(dt) + (9 * 3600);
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
								var src = doc.QuerySelector(string.Format("div[data-res=\"{0}\"] > a", threadNo))?.GetAttribute("href") switch {
									string v when Regex.Match(v, @"^/[^/]+/src/[0-9]+\.\S+$").Success => v,
									_ => ""
								};
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
								if(!string.IsNullOrEmpty(ip) && board.Extra.AlwaysIp) {
									ip = "";
								}
								if(!string.IsNullOrEmpty(id) && board.Extra.AlwaysId) {
									time = time + " " + id;
									id = "";
								}
								var f = Data.FutabaContext.FromThreadResResponse(board, threadNo, r.Result.Response,
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

								successed = true;
								isDie = false;
								result = f;
								cookies = r.Result.cookies;
								goto end;
							}
						}
						catch(Exception e) {
							System.Diagnostics.Debug.WriteLine(e.ToString());
							throw;
						}
					end:
						return (successed, isDie, result, error, response, cookies);
					});
					o.OnNext(ctx);
				}
				finally {
					o.OnCompleted();
				}
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}


		public static IObservable<(bool Successed, string NextOrMessage, Data.Cookie2[] Cookies)> PostThread(
			Data.BoardData board,
			string name, string email, string subject,
			string comment, string filePath, string passwd,
			Data.Cookie2[] cookies, string ptua) {

			return Observable.Create<(bool Successed, string NextOrMessage, Data.Cookie2[] Cookies)>(async o => {
				try {
					var r = await FutabaApi.PostThread(board,
						cookies, ptua,
						name, email, subject, comment, filePath, passwd);
					if(r.Raw == null) {
						o.OnNext((false, "不明なエラー", Array.Empty<Data.Cookie2>()));
					} else {
						o.OnNext((r.Successed, r.NextOrMessage, r.Cookies));
					}
				}
				finally {
					o.OnCompleted();
				}
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}

		public static IObservable<(bool Successed, string ThisNo, string Message, Data.Cookie2[] Cookies)> PostRes(
			Data.BoardData board, string threadNo,
			string name, string email, string subject,
			string comment, string filePath, string passwd,
			Data.Cookie2[] cookies, string ptua) {

			return Observable.Create<(bool Successed, string ThisNo, string Message, Data.Cookie2[] Cookies)>(async o => {
				try {
					var r = await FutabaApi.PostRes(board, threadNo,
						cookies, ptua,
						name, email, subject, comment, filePath, passwd);
					if(r.Raw == null) {
						o.OnNext((false, null, "不明なエラー", Array.Empty<Data.Cookie2>()));
					} else {
						o.OnNext((r.Successed, r.ThisNo, r.Raw, r.Cookies));
					}
				}
				finally {
					o.OnCompleted();
				}
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}

		public static IObservable<(bool Successed, string Message, Data.Cookie2[] Cookies)> PostDeleteThreadRes(
			Data.BoardData board,
			string threadNo, bool imageOnlyDel, string passwd,
			Data.Cookie2[] cookies) {
			return Observable.Create<(bool Successed, string Message, Data.Cookie2[] Cookies)>(async o => {
				try {
					var r = await FutabaApi.PostDeleteThreadRes(board.Url, threadNo, cookies, imageOnlyDel, passwd);
					if(r.Raw == null) {
						o.OnNext((false, "不明なエラー", Array.Empty<Data.Cookie2>()));
					} else {
						o.OnNext((r.Successed, r.Raw, r.Cookies));
					}
				}
				finally {
					o.OnCompleted();
				}
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}

		public static IObservable<(bool Successed, string Message)> PostSoudane(Data.BoardData board, string threadNo) {
			return Observable.Create<(bool Successed, string Message)>(async o => {
				try {
					var task = await FutabaApi.PostSoudane(board.Url, threadNo);
					if(task.Raw == null) {
						o.OnNext((false, "不明なエラー"));
					} else {
						o.OnNext((task.Successed, task.Raw));
					}
				}
				finally {
					o.OnCompleted();
				}
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}

		public static IObservable<(bool Successed, string Message)> PostDel(Data.BoardData board, string threadNo, string resNo) {
			return Observable.Create<(bool Successed, string Message)>(async o => {
				try {
					var task = await FutabaApi.PostDel(board.Url, threadNo, resNo /*, Config.ConfigLoader.FutabaApi.Cookies */);
					if(task.Raw == null) {
						o.OnNext((false, "不明なエラー"));
					} else {
						o.OnNext((task.Successed, task.Raw));
					}
				}
				finally {
					o.OnCompleted();
				}
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}


		public static IObservable<(bool Successed, string FileNameOrMessage)> UploadUp2(string comment, string filePath, string passwd) {
			return Observable.Create<(bool Successed, string FileNameOrMessage)>(async o => {
				try {
					var r = await FutabaApi.UploadUp2(comment, filePath, passwd);
					o.OnNext((r.Successed, r.Message));
				}
				finally {
					o.OnCompleted();
				}
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}

		public static IObservable<(bool Successed, byte[] FileBytes, object Raw)> GetThumbImageUp(
			Data.UrlContext threadUrl,
			string fileName,
			Data.UploderData[] uploders) {

			var f = Path.GetFileNameWithoutExtension(fileName);
			return GetCompleteUrlUp(threadUrl, f, uploders)
				.Select(x => {
					if(x.Successed
						&& (x.Raw is Data.AppsweetsThumbnailCompleteResponse r)
						&& !string.IsNullOrEmpty(r.Content)) {

						return (
							true,
							Convert.FromBase64String(r.Content.Substring("data:image/png;base64,".Length)),
							x.Raw);
					} else {
						return (false, default(byte[]), x.Raw);
					}
				});
		}


		public static IObservable<(bool Successed, string UrlOrMessage, object Raw)> GetCompleteUrlUp(
			Data.UrlContext threadUrl,
			string fileNameWitfOutExtension,
			Data.UploderData[] uploders) {

			return Observable.Create<(bool Successed, string Message, object Raw)>(async o => {
				try {
					var r = await FutabaApi.GetCompleteUrlUp(GetFutabaThreadUrl(threadUrl), fileNameWitfOutExtension);
					if(r.Successed) {
						System.Diagnostics.Debug.Assert(r.CompleteResponse != null);
						if(!string.IsNullOrEmpty(r.CompleteResponse.Name)) {
							var url = uploders
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

		public static string GetFutabaThreadUrl(Data.UrlContext url) {
			// TODO: UrlContextが持つべきではないか？
			if(url.IsCatalogUrl) {
				return $"{url.BaseUrl}futaba.php?mode=cat";
			} else {
				return $"{url.BaseUrl}res/{url.ThreadNo}.htm";
			}
		}
	}
}
