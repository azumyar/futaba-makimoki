using Android.Runtime;
using Android.Views;
using Android.OS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using AngleSharp.Dom;
using Reactive.Bindings;
using Yarukizero.Net.MakiMoki.Droid.Extensions;
using AndroidX.RecyclerView.Widget;
using Android.Widget;
using System.Diagnostics.Metrics;
using Newtonsoft.Json;
using AndroidX.SwipeRefreshLayout.Widget;
using AndroidX.Lifecycle;
using Android.Content;
using Google.Android.Material.FloatingActionButton;
using Java.Net;

namespace Yarukizero.Net.MakiMoki.Droid.Fragments {
	internal class CatalogViewerFragment : global::AndroidX.Fragment.App.Fragment {
		class ResponseObject : Data.JsonObject {
			[JsonProperty("res")]
			public Data.FutabaResponse Response { get; init; }
			[JsonProperty("html")]
			public string Html { get; init; }

		}

		public class RecyclerAdapter : App.RecyclerViewAdapter<Data.FutabaContext.Item?> {
			private class ViewHolder : RecyclerView.ViewHolder {
				public View Root { get; }
				public global::AndroidX.CardView.Widget.CardView Card { get; }
				public ImageView Image { get; }
				public TextView Title { get; }
				public TextView Counter { get; }

				public View BadgeOld { get; }
				public View BadgeId { get; }
				public View BadgeNewCounter { get; }
				public View BadgeMovie { get; }
				public View BadgeIsolate { get; }
				public TextView NewCounter { get; }

				public ViewHolder(View v) : base(v) {
					this.Root = v;
					this.Card = v.FindViewById<global::AndroidX.CardView.Widget.CardView>(Resource.Id.card);
					this.Image = v.FindViewById<ImageView>(Resource.Id.image);
					this.Title = v.FindViewById<TextView>(Resource.Id.textview_title);
					this.Counter = v.FindViewById<TextView>(Resource.Id.textview_counter);

					this.BadgeOld = v.FindViewById<View>(Resource.Id.old);
					this.BadgeId = v.FindViewById<View>(Resource.Id.id);
					this.BadgeNewCounter = v.FindViewById<View>(Resource.Id.newcount);
					this.BadgeMovie = v.FindViewById<View>(Resource.Id.movie);
					this.BadgeIsolate = v.FindViewById<View>(Resource.Id.isolate);
					this.NewCounter = v.FindViewById<TextView>(Resource.Id.textview_newcount);
				}
				protected ViewHolder(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }
			}

			private CatalogViewerFragment fragment;

			public RecyclerAdapter(CatalogViewerFragment @this) : base() {
				this.fragment = @this;
			}
			protected RecyclerAdapter(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

			public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType) {
				var v = new ViewHolder(LayoutInflater.From(parent.Context)
					.Inflate(Resource.Layout.layout_listview_catalog, parent, false));

				v.Card.Click += (s, _) => {
					if(s is View vv && vv.Tag?.ToString() is string json) {
						var p = Newtonsoft.Json.JsonConvert.DeserializeObject<Data.UrlContext>(json);
						this.fragment.Activity.SupportFragmentManager.BeginTransaction()
							.Replace(Resource.Id.container, ThreadViewerFragment.NewInstance(p))
							.AddToBackStack(null)
							.Commit();
					}
				};
				return v;
			}

			public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position) {
				static ViewStates conv(bool b) => b switch {
					true => ViewStates.Visible,
					false => ViewStates.Gone
				};
				bool isOld(Data.FutabaContext.Item item) {
					var old = false;
					var curNo = long.TryParse(item.ResItem.No, out var cno) ? cno : 0;
					var lastNo = this.Source.Select(x => long.TryParse(x?.ResItem.No, out var lno) ? lno : 0)
						.Max();
					if((0 < curNo)
						&& (0 < lastNo)
						&& ((0 < this.fragment.Properties.Board.Extra.MaxStoredRes) || (0 < this.fragment.Properties.Board.Extra.MaxStoredRes))) {

						if(0 < this.fragment.Properties.Board.Extra.MaxStoredRes) {
							old = (this.fragment.Properties.Board.Extra.MaxStoredRes * 0.9) < (lastNo - curNo);
						}

						if(0 < this.fragment.Properties.Board.Extra.MaxStoredTime) {
							if((DateTime.Now - item.ResItem.Res.NowDateTime)
								< TimeSpan.FromSeconds(this.fragment.Properties.Board.Extra.MaxStoredTime - (5 * 60))) {

								old = false;
							}
						}
					}
					return old;
				}

				if(holder is ViewHolder vh) {
					var s = this.Source[position];
					if(s == null) {
						vh.Card.Tag = null;
						vh.Card.Visibility = ViewStates.Invisible;
					} else {
						vh.Card.Tag = new Java.Lang.String(s.Url.ToString());
						vh.Card.Visibility = ViewStates.Visible;

						vh.Title.Text = Util.TextUtil.SafeSubstring(s.ResItem.Res.Com, 4);
						vh.Counter.Text = s.CounterCurrent.ToString();

						vh.BadgeOld.Visibility = conv(isOld(s));
						vh.BadgeIsolate.Visibility = conv(s.ResItem.IsolateValue);
						{
							var isId = !string.IsNullOrEmpty(s.ResItem.Res.Id);
							var thId = s.ResItem.Res.Email.ToLower() != "id表示";
							var thIp = s.ResItem.Res.Email.ToLower() != "ip表示";
							vh.BadgeId.Visibility = conv(isId && (thId || thIp));
						}
						vh.BadgeNewCounter.Visibility = conv(!s.ResItem.IsolateValue && (0 < (s.CounterCurrent - s.CounterPrev)));
						vh.BadgeMovie.Visibility = conv(Config.ConfigLoader.MimeFutaba.Types
							.Where(x => x.MimeContents == Data.MimeContents.Video)
							.Select(x => x.Ext)
							.Any(x => s.ResItem.Res.Ext.ToLower() == x));

						vh.NewCounter.Text = (s.CounterCurrent - s.CounterPrev).ToString();

						if(!string.IsNullOrEmpty(s.ResItem.Res.Thumb) && 0 < s.ResItem.Res.Fsize) {
							var uri = new Uri(s.Url.BaseUrl);
							var th = $"{uri.Scheme}://{uri.Authority}{s.ResItem.Res.Thumb}";
							vh.Image.Tag = new Java.Lang.String(th);
							vh.Image.SetImageBitmap(null);
							App.ImageResolver.Instance.Get(th, 256)
								.Subscribe(x => {
									if(vh.Image.Tag.ToString() == th) {
										vh.Image.SetImageBitmap(x);
									}
								});
						}

					}
				}
			}
		}


		private class PropertiesHolder : IDisposable {
			public ReactiveProperty<Data.FutabaContext> FutabaContext { get; } = new ReactiveProperty<Data.FutabaContext>();
			public ReactiveProperty<string> SearchText { get; } = new ReactiveProperty<string>(initialValue: "");
			public Data.BoardData Board { get; set; }
			public ReactiveProperty<ResponseObject> Response { get; } = new ReactiveProperty<ResponseObject>();

			public IDisposable ResponseSubscriber { get; set; }
			public IDisposable CatalogSubscriber { get; set; }

			public ReactiveProperty<bool> IsUpdating { get; } = new ReactiveProperty<bool>(initialValue:false);


			public void Dispose() {
				new Helpers.AutoDisposable(this).Dispose();
			}
		}

		private PropertiesHolder Properties { get; } = new PropertiesHolder();
		private RecyclerView recyclerView;
		private RecyclerAdapter adapter;
		private int scrollShrinkPx;

		public CatalogViewerFragment() : base() { }
		protected CatalogViewerFragment(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

		public override View? OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState) {
			return inflater.Inflate(Resource.Layout.fragment_catalog, container, false);
		}

		public override void OnViewCreated(View view, Bundle? savedInstanceState) {
			base.OnViewCreated(view, savedInstanceState);

			this.scrollShrinkPx = DroidUtil.Util.Dp2Px(32, view.Context);
			var isInit = this.adapter == null;
			this.recyclerView = view.FindViewById<RecyclerView>(Resource.Id.recyclerview);
			App.RecyclerViewSwipeUpdateHelper.AttachGridLayout(this.recyclerView, 4).Updating +=(_, e) => {
				this.UpdateCatalog()
					.ObserveOn(UIDispatcherScheduler.Default)
					.Subscribe(_ => {
						e.UpdateObject.EndUpdate();
					});
			};
			this.recyclerView.ScrollChange += (_, e) => {
				if(this.recyclerView.GetLayoutManager() is LinearLayoutManager lm) {
					var @new = view.FindViewById<ExtendedFloatingActionButton>(Resource.Id.button_new);
					if(@new.Extended && (0 < lm.FindFirstVisibleItemPosition())) {
						@new.Shrink();
					} else if(!@new.Extended && (lm.FindFirstVisibleItemPosition() == 0)) {
						@new.Extend();
					}
				}
			};

			if(!isInit) {
				this.recyclerView.SetAdapter(this.adapter);
				return;
			} else {
				this.recyclerView.SetAdapter(this.adapter = new RecyclerAdapter(this));

				this.Properties.Board = this.Arguments.OutJson<Data.BoardData>();
				this.Properties.FutabaContext.Value = Data.FutabaContext.FromCatalogEmpty(this.Properties.Board);
				this.Properties.ResponseSubscriber = this.SubscribeResponse();
				this.Properties.CatalogSubscriber = this.SubscribeCatalog();
				if(savedInstanceState == null) {
					this.UpdateCatalog()
						.Subscribe();
				}
			}
		}

		private IObservable<bool> UpdateCatalog() {
			return Observable.Create<bool>(o => {
				this.Properties.IsUpdating.Value = true;
				//this.adapter.Source.Clear();
				GetCatalog(this.Properties.Board)
					.ObserveOn(UIDispatcherScheduler.Default)
					.Finally(() => {
						this.Properties.IsUpdating.Value = false;
						o.OnCompleted();
					}).Subscribe(x => {
						o.OnNext(x.Successed);
						if(x.Successed) {
							this.Properties.Response.Value = new ResponseObject() {
								Response = x.RawResponse,
								Html = x.RawHtml,
							};
						} else {

						}
					});
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}


		private IDisposable SubscribeResponse() {
			return this.Properties.Response.Subscribe(x => {
				if(x == null) {
					return;
				}

				var parser = new HtmlParser();
				var doc = parser.ParseDocument(x.Html);
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
				var resList = new List<Data.NumberedResItem>(x.Response.Res);
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
				this.Properties.FutabaContext.Value = Data.FutabaContext.FromCatalogResponse(
					this.Properties.Board,
					x.Response,
					sortList.ToArray(),
					dic,
					this.Properties.FutabaContext.Value);
			});
		}

		private IDisposable SubscribeCatalog() {
			return this.Properties.FutabaContext.CombineLatest(
				this.Properties.SearchText,
				(x, y) => (Futaba: x, Search: y))
				.ObserveOn(UIDispatcherScheduler.Default)
				.Subscribe(x => {
					this.adapter.Source.BeginUpdate()
						.Clear()
						.AddRange(x.Futaba.ResItems)
						.AddRange(new Data.FutabaContext.Item[4])
						.Commit();
					this.recyclerView.GetLayoutManager().ScrollToPosition(0);               
				});
		}

		public override void OnViewStateRestored(Bundle? savedInstanceState) {
			base.OnViewStateRestored(savedInstanceState);
			if(savedInstanceState != null) {
				this.Properties.Board = savedInstanceState.OutJson<Data.BoardData>();
			}
		}

		public override void OnSaveInstanceState(Bundle outState) {
			outState.InJson(this.Properties.Board);

			base.OnSaveInstanceState(outState);
		}

		public override void OnDestroy() {
			this.Properties.Dispose();

			base.OnDestroy();
		}

		public static CatalogViewerFragment NewInstance(Data.BoardData board) {
			var b = new Bundle()
				.InJson(board);
			return new CatalogViewerFragment() {
				Arguments = b,
			};
		}

		private static IObservable<(
			bool Successed,
			Data.FutabaContext? Data,
			string ErrorMessage,
			string? RawHtml,
			Data.FutabaResponse? RawResponse,
			Data.Cookie2[]? Cookies
			)> GetCatalog(Data.BoardData board, Data.CatalogSortItem sort = null) {
			return Observable.Create<(bool, Data.FutabaContext?, string, string?, Data.FutabaResponse?, Data.Cookie2[]?)>(o => {
				_ = Task.Run(async () => {
					var successed = false;
					Data.FutabaContext result = null;
					var error = "";
					var html = default(string);
					var response = default(Data.FutabaResponse);
					var cookie = default(Data.Cookie2[]?);
					try {
						var r = await Util.FutabaApi.GetCatalog(
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
						var rr = await Util.FutabaApi.GetCatalogHtml(
							board.Url,
							Config.ConfigLoader.FutabaApi.Cookies,
							r.Response.Res.Length,
							sort);
						if(!rr.Successed) {
							error = "カタログHTMLの取得に失敗しました";
							goto end;
						}
						//Config.ConfigLoader.UpdateCookie(board.Url, rr.Cookies);
						html = rr.Raw;
						response = r.Response;
						cookie = r.Cookies;
						var parser = new HtmlParser();
						var doc = parser.ParseDocument(html);
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
						successed = true;
						result = Data.FutabaContext.FromCatalogResponse(
								board, r.Response, sortList.ToArray(), dic, null);
					end:;
					}
					catch(Exception e) { // TODO: 適切なエラーに
						System.Diagnostics.Debug.WriteLine(e.ToString());
					}
					o.OnNext((successed, result, error, html, response, cookie));
					o.OnCompleted();
				});
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}
	}
}
