using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Windows.UI.Xaml;
using System.Text.RegularExpressions;
using System.Reactive.Disposables;

namespace Yarukizero.Net.MakiMoki.Uno.Models {
	class BindableFutaba : IDisposable {
		public string Name { get; }

		public Data.UrlContext Url { get; }

		public Data.FutabaContext Raw { get; }

		public ReactiveCollection<BindableFutabaResItem> ResItems { get; }
		public ReactiveProperty<int> ResCount { get; }
		public ReactiveProperty<bool> IsOld { get; }
		public ReactiveProperty<bool> IsDie { get; }
		public ReactiveProperty<bool> IsMaxRes { get; }

		public ReactiveProperty<string> FilterText { get; } = new ReactiveProperty<string>("");

		public ReactiveProperty<Data.CatalogSortItem> CatalogSortItem { get; } = new ReactiveProperty<Data.CatalogSortItem>(Data.CatalogSort.Catalog);
		public ReactiveProperty<bool> CatalogListMode { get; } = new ReactiveProperty<bool>(false);
		public ReactiveProperty<int> CatalogResCount { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<object> UpdateToken { get; } = new ReactiveProperty<object>(DateTime.Now);

		public BindableFutaba(Data.FutabaContext futaba, BindableFutaba old = null) {
			if(old != null) {
				FilterText.Value = old.FilterText.Value;
				CatalogSortItem.Value = old.CatalogSortItem.Value;
				CatalogListMode.Value = old.CatalogListMode.Value;
				CatalogResCount.Value = old.CatalogResCount.Value;
			}

			this.Raw = futaba;
			this.Name = futaba.Name;
			this.Url = futaba.Url;
			var updateItems = new List<(int Index, BindableFutabaResItem Item)>();
			var disps = new CompositeDisposable();
			if((old == null) || old.Raw.Url.IsCatalogUrl) { // カタログはそうとっかえする
				this.ResItems = new ReactiveCollection<BindableFutabaResItem>();
				int c = 0;
				foreach(var it in futaba.ResItems
						.Select(x => new BindableFutabaResItem(c++, x, futaba.Url.BaseUrl, this))
						.ToArray()) {

					it.IsWatch.Subscribe(x => {
						if(x) {
							this.UpdateToken.Value = DateTime.Now;
						}
					});
					this.ResItems.Add(it);
				}

				if(old?.ResItems != null) {
					disps.Add(old.ResItems);
				}
			} else {
				this.ResItems = old.ResItems;
				var i = 0;
				var prevId = this.ResItems.Select(x => x.Raw.Value.ResItem.No).ToArray();
				var newId = futaba.ResItems.Select(x => x.ResItem.No).ToArray();
				var ep = prevId.Except(newId).ToArray();
				foreach(var it in ep) {
					for(var ii = 0; ii < this.ResItems.Count; ii++) {
						if(it == this.ResItems[ii].Raw.Value.ResItem.No) {
							disps.Add(this.ResItems[ii]);
							this.ResItems.RemoveAt(ii);
							break;
						}
					}
				}

				while((i < this.ResItems.Count) && (i < futaba.ResItems.Length)) {
					var a = this.ResItems[i];
					var b = futaba.ResItems[i];
					if(a.Raw.Value.ResItem.No != b.ResItem.No) {
						// 普通は来ない
						System.Diagnostics.Debug.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%% this.ResItems.RemoveAt(i) %%%%%%%%%%%%%%%%%%%%%%%");
						disps.Add(this.ResItems[i]);
						this.ResItems.RemoveAt(i);
						continue;
					}

					if(a.Raw.Value.HashText != b.HashText) {
						// 画面から参照されているので this の初期化が終わっていないこのタイミングで書き換えてはいけない
						//this.ResItems[i] = new BindableFutabaResItem(i, b, futaba.Url.BaseUrl, this);
						var bf = new BindableFutabaResItem(i, b, futaba.Url.BaseUrl, this);
						/* 暫定コメントアウト
						if(b.ResItem.Res.IsHavedImage) {
							BindableFutabaResItem.CopyImage(a, bf);
						}
						*/
						updateItems.Add((i, bf));
						disps.Add(a);
					}
					i++;
				}

				if(i < futaba.ResItems.Length) {
					foreach(var it in futaba.ResItems
							.Skip(i)
							.Select(x => new BindableFutabaResItem(i++, x, futaba.Url.BaseUrl, this))
							.ToArray()) {

						this.ResItems.Add(it);
					}
				}
			}

			this.ResCount = new ReactiveProperty<int>(futaba.Url.IsCatalogUrl ? this.ResItems.Count : (this.ResItems.Count - 1));
			this.IsDie = new ReactiveProperty<bool>(futaba.Raw?.IsDie ?? false);
			this.IsOld = new ReactiveProperty<bool>(futaba.Raw?.IsOld ?? false || this.IsDie.Value);
			this.IsMaxRes = new ReactiveProperty<bool>(futaba.Raw?.IsMaxRes ?? false);

			// 初期化がすべて終わったタイミングで書き換える
			foreach(var it in this.ResItems) {
				it.Parent.Value = this;
			}
			foreach(var it in updateItems) {
				this.ResItems[it.Index] = it.Item;
			}

			// 古いインスタンスを削除
			disps.Dispose();
		}

		public void Dispose() { }
	}

	class BindableFutabaResItem : IDisposable {
		private class RefValue<T> where T : struct {
			public T Value { get; }

			public RefValue(T val) {
				this.Value = val;
			}
		}
		/* NGは現在無効
		private static Helpers.WeakCache<string, RefValue<ulong>> HashCache { get; }
			= new Helpers.WeakCache<string, RefValue<ulong>>();
		private static Helpers.ConnectionQueue<ulong?> HashQueue = new Helpers.ConnectionQueue<ulong?>(
			name: "NG/Watchハッシュ計算キュー",
			maxConcurrency: 4,
			delayTime: 100,
			waitTime: 100);
		*/

		public ReactiveProperty<int> Index { get; }
		public ReactiveProperty<string> ImageName { get; }
		public ReactiveProperty<Data.BoardData> Bord { get; }
		public ReactiveProperty<string> ThreadResNo { get; }
		public ReactiveProperty<Data.FutabaContext.Item> Raw { get; }
		public ReactiveProperty<BindableFutaba> Parent { get; }
		public ReactiveProperty<int> ResCount { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<string> ResCountText { get; }
		public ReactiveProperty<string> HeadLineHtml { get; }
		public ReactiveProperty<string> DisplayHtml { get; }
		public ReactiveProperty<string> CommentHtml { get; }
		public ReactiveProperty<string> OriginHtml { get; }
		public ReactiveProperty<bool> IsNg { get; }
		public ReactiveProperty<bool> IsWatch { get; }
		public ReactiveProperty<bool> IsWatchWord { get; }
		public ReactiveProperty<bool> IsWatchImage { get; }
		public ReactiveProperty<bool> IsHidden { get; }
		public ReactiveProperty<bool> IsDel { get; }
		public ReactiveProperty<bool> IsVisibleOriginComment { get; }
		public ReactiveProperty<bool> IsNgImageHidden { get; }
		public ReactiveProperty<string> CommentCopy { get; }
		public ReactiveProperty<bool> IsVisibleCatalogIdMarker { get; }
		public ReactiveProperty<ulong?> ThumbHash { get; }

		private RefValue<ulong> hashValue;

		// Uno Only
		public ReactiveProperty<string> DisplayText { get; }
		public ReactiveProperty<Visibility> NameVisibility { get; }
		public ReactiveProperty<Visibility> ResImageVisibility { get; }
		public ReactiveProperty<string> CatalogResCountText { get; }


		public BindableFutabaResItem(int index, Data.FutabaContext.Item item, string baseUrl, BindableFutaba parent) {
			System.Diagnostics.Debug.Assert(item != null);
			System.Diagnostics.Debug.Assert(baseUrl != null);
			System.Diagnostics.Debug.Assert(parent != null);
			var bord = Config.ConfigLoader.Board.Boards.Where(x => x.Url == baseUrl).FirstOrDefault();
			System.Diagnostics.Debug.Assert(bord != null); 
			this.Index = new ReactiveProperty<int>(index);
			this.Bord = new ReactiveProperty<Data.BoardData>(bord);
			this.Parent = new ReactiveProperty<BindableFutaba>(parent);
			this.ThreadResNo = new ReactiveProperty<string>(item.ResItem.No);
			this.Raw = new ReactiveProperty<Data.FutabaContext.Item>(item);
			this.ResCountText = this.ResCount.Select(x => (0 < x) ? $"{ x }レス" : "").ToReactiveProperty();
			this.ThumbHash = new ReactiveProperty<ulong?>();

			{
				// 暫定的にテキストにする
				CatalogResCountText = new ReactiveProperty<string>(
					(item.ResItem.Isolate ?? false)
						? "隔離" : $"{ item.CounterCurrent }レス");
			}

			// delとhostの処理
			{
				var headLine = new StringBuilder();
				if(Raw.Value.ResItem.Res.IsDel) {
					headLine.Append("<font color=\"#ff0000\">スレッドを立てた人によって削除されました</font><br>");
				} else if(Raw.Value.ResItem.Res.IsDel2) {
					headLine.Append("<font color=\"#ff0000\">削除依頼によって隔離されました</font><br>");
				}
				if(!string.IsNullOrEmpty(Raw.Value.ResItem.Res.Host)) {
					headLine.AppendFormat("[<font color=\"#ff0000\">{0}</font>]<br>", Raw.Value.ResItem.Res.Host);
				}
				this.IsNg = new ReactiveProperty<bool>(
					parent.Url.IsCatalogUrl ? Ng.NgUtil.NgHelper.CheckCatalogNg(parent.Raw, item)
						: Ng.NgUtil.NgHelper.CheckThreadNg(parent.Raw, item));
				this.IsWatchWord = new ReactiveProperty<bool>(Ng.NgUtil.NgHelper.CheckCatalogWatch(parent.Raw, item));
				this.IsWatchImage = this.ThumbHash
					.Select(x => x.HasValue ? Ng.NgUtil.NgHelper.CheckImageWatch(x.Value) : false)
					.ToReactiveProperty();
				this.IsWatch = new[] {
					this.IsWatchWord,
					this.IsWatchImage,
				}.CombineLatest(x => x.Any(y => y))
					.ToReactiveProperty();
				this.IsHidden = new ReactiveProperty<bool>(Ng.NgUtil.NgHelper.CheckHidden(parent.Raw, item));
				//this.IsDel = new ReactiveProperty<bool>(
				//	(item.ResItem.Res.IsDel || item.ResItem.Res.IsDel2)
				//		&& (WpfConfig.WpfConfigLoader.SystemConfig.ThreadDelResVisibility == PlatformData.ThreadDelResVisibility.Hidden));
				this.IsDel = new ReactiveProperty<bool>(item.ResItem.Res.IsDel || item.ResItem.Res.IsDel2);

				this.IsNgImageHidden = new ReactiveProperty<bool>(false);
				this.HeadLineHtml = new ReactiveProperty<string>(headLine.ToString());
				this.CommentHtml = new ReactiveProperty<string>("");
				this.OriginHtml = new ReactiveProperty<string>(Raw.Value.ResItem.Res.Com);
				this.SetCommentHtml();
				this.IsVisibleOriginComment = this.IsHidden
					.CombineLatest(
						this.IsNg, this.IsDel,
						(x, y, z) => !(x || y || z))
					.ToReactiveProperty();
				this.DisplayHtml = IsVisibleOriginComment
					.Select(x => x ? this.OriginHtml.Value : this.CommentHtml.Value)
					.ToReactiveProperty();
			}

			// コピー用コメント生成
			{
				var sb = new StringBuilder()
					.Append(Index.Value);
				if(Bord.Value.Extra?.Name ?? true) {
					sb.Append($" {Raw.Value.ResItem.Res.Sub} {Raw.Value.ResItem.Res.Name}");
				}
				if(!string.IsNullOrWhiteSpace(Raw.Value.ResItem.Res.Email)) {
					sb.Append($" [{Raw.Value.ResItem.Res.Email}]");
				}
				sb.Append($" {Raw.Value.ResItem.Res.Now}");
				if(!string.IsNullOrWhiteSpace(Raw.Value.ResItem.Res.Host)) {
					sb.Append($" {Raw.Value.ResItem.Res.Host}");
				}
				if(!string.IsNullOrWhiteSpace(Raw.Value.ResItem.Res.Id)) {
					sb.Append($" {Raw.Value.ResItem.Res.Id}");
				}
				if(0 < Raw.Value.Soudane) {
					sb.Append($" そうだね×{Raw.Value.Soudane}");
				}
				sb.Append($" No.{Raw.Value.ResItem.No}")
					.AppendLine()
					.Append(Util.TextUtil.RowComment2Text(Raw.Value.ResItem.Res.Com));
				this.CommentCopy = new ReactiveProperty<string>(sb.ToString());
			}

			if(item.ResItem.Res.Fsize != 0) {
				// 削除された場合srcの拡張子が消える
				var m = Regex.Match(item.ResItem.Res.Src, @"^.+/([^\.]+\..+)$");
				if(m.Success) {
					this.ImageName = new ReactiveProperty<string>(m.Groups[1].Value);
				} else {
					this.ImageName = new ReactiveProperty<string>("");
				}
			} else {
				this.ImageName = new ReactiveProperty<string>("");
			}

			this.DisplayText = this.CommentHtml
				.Select(x => Util.TextUtil.RowComment2Text(x))
				.ToReactiveProperty();
		}

		public void Dispose() {
			Helpers.AutoDisposable.GetCompositeDisposable(this).Dispose();
		}

		private void SetCommentHtml() {
			var del = false; // WpfConfig.WpfConfigLoader.SystemConfig.ThreadDelResVisibility == PlatformData.ThreadDelResVisibility.Hidden;
			if(this.IsNg.Value) {
				this.CommentHtml.Value = "<font color=\"#ff0000\">NG設定に抵触しています</font>";
			} else if(this.IsHidden.Value) {
				this.CommentHtml.Value = "<font color=\"#ff0000\">非表示に設定されています</font>";
			} else if(this.Raw.Value.ResItem.Res.IsDel && del) {
				this.CommentHtml.Value = "<font color=\"#ff0000\">スレッドを立てた人によって削除されました</font>";
			} else if(this.Raw.Value.ResItem.Res.IsDel2 && del) {
				this.CommentHtml.Value = "<font color=\"#ff0000\">削除依頼によって隔離されました</font>";
			} else {
				this.CommentHtml.Value = this.OriginHtml.Value;
			}
		}
	}
}