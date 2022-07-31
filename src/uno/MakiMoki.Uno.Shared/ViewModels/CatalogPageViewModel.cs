using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Media.Imaging;
using AngleSharp.Html.Parser;
using AngleSharp.Dom;
using Reactive.Bindings;
using Yarukizero.Net.MakiMoki.Util;

namespace Yarukizero.Net.MakiMoki.Uno.ViewModels {
	class CatalogPageViewModel {
		public class CatalogItemGroup {
			public CatalogItem Item00 { get; }
			public CatalogItem Item01 { get; }
			public CatalogItem Item02 { get; }
			public CatalogItem Item03 { get; }
			public Windows.UI.Xaml.Visibility Item00Visibility => ConvVisibility(Item00);
			public Windows.UI.Xaml.Visibility Item01Visibility => ConvVisibility(Item01);
			public Windows.UI.Xaml.Visibility Item02Visibility => ConvVisibility(Item02);
			public Windows.UI.Xaml.Visibility Item03Visibility => ConvVisibility(Item03);

			public CatalogItemGroup(CatalogItem item00, CatalogItem item01, CatalogItem item02, CatalogItem item03) {
				this.Item00 = item00;
				this.Item01 = item01;
				this.Item02 = item02;
				this.Item03 = item03;
			}

			private Windows.UI.Xaml.Visibility ConvVisibility(CatalogItem? item) => item switch {
				CatalogItem _ => Windows.UI.Xaml.Visibility.Visible,
				_ => Windows.UI.Xaml.Visibility.Collapsed,
			};
		}
		public class CatalogItem : System.ComponentModel.INotifyPropertyChanged {
			public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
			public string Text { get; }
			public Data.UrlContext Url { get; }

			private BitmapSource _image = null;
			private object _imageToken = null;
			public BitmapSource Image {
				get {
					if(this._imageToken == null) {
						ImageResolver.Get(GetFutabaThumbImageUrl(Url, ResItem))
							.Subscribe(x => {
								this.Image = x;
							});
					}
					return this._image;
				}
				set {
					this._image = value;
					this._imageToken = new object();
					this.PropertyChanged?.Invoke(
						this,
						new System.ComponentModel.PropertyChangedEventArgs(nameof(Image)));
				}
			}

			private UnoModels.ImageResolver ImageResolver;
			private Data.ResItem ResItem;
			public CatalogItem(
				string Text,
				Data.UrlContext Url,
				UnoModels.ImageResolver ImageResolver,
				Data.ResItem ResItem
				) {

				this.Text = Text;
				this.Url = Url;
				this.ImageResolver = ImageResolver;
				this.ResItem = ResItem;
			}

			private string GetFutabaThumbImageUrl(Data.UrlContext url, Data.ResItem item) {
				var uri = new Uri(url.BaseUrl);
				return $"{uri.Scheme}://{uri.Authority}{item.Thumb}";
			}
		}

		private readonly UnoModels.ImageResolver imageResolver;
		private readonly Data.BoardData boardData;

		public ReactiveProperty<IEnumerable<CatalogItemGroup>> Source { get; } = new ReactiveProperty<IEnumerable<CatalogItemGroup>>();

		public CatalogPageViewModel(Data.BoardData boardData, UnoModels.ImageResolver imageResolver) {
			this.boardData = boardData;
			this.imageResolver = imageResolver;
			this.UpdateCatalog();
		}

		private void UpdateCatalog() {
			//Util.FutabaApiReactive.GetCatalog
			GetCatalog(this.boardData)
				.ObserveOn(UIDispatcherScheduler.Default)
				.Subscribe(x => {
					static IEnumerable<CatalogItemGroup> split(IEnumerable<CatalogItem> input, int split = 4) {
						var r = new List<CatalogItemGroup>();
						var @in = input;
						while(@in.Any()) {
							// TODO:配列に入れて可変にする
							r.Add(new CatalogItemGroup(
								@in.ElementAtOrDefault(0),
								@in.ElementAtOrDefault(1),
								@in.ElementAtOrDefault(2),
								@in.ElementAtOrDefault(3)));
							@in = @in.Skip(split);
						}
						return r.AsReadOnly();
					}

					var regex = new Regex("<[^>]*>");
					Source.Value = x.Successed switch {
						true => Source.Value = split(x.Data.ResItems.Select(x => new CatalogItem(
							Text: regex.Replace(x.ResItem.Res.Com, "") switch {
								string s when 4 < s.Length => s.Substring(0, 4),
								string s => s,
								_ => throw new ArgumentException()
							},
							Url: x.Url,
							ImageResolver: this.imageResolver,
							ResItem: x.ResItem.Res)).ToArray()),
						false => Array.Empty<CatalogItemGroup>()
					};
				});
		}


		private static IObservable<(
			bool Successed,
			Data.FutabaContext? Data,
			string ErrorMessage,
			string? RawHtml,
			Data.FutabaResonse? RawResponse,
			Data.Cookie2[]? Cookies
			)> GetCatalog(Data.BoardData board, Data.CatalogSortItem sort = null) {
			return Observable.Create<(bool, Data.FutabaContext?, string, string?, Data.FutabaResonse?, Data.Cookie2[]?)>(o => {
				_ = Task.Run(async () => {
					var successed = false;
					Data.FutabaContext result = null;
					var error = "";
					var html = default(string);
					var response = default(Data.FutabaResonse);
					var cookie = default(Data.Cookie2[]?);
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
						var rr = await FutabaApi.GetCatalogHtml(
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