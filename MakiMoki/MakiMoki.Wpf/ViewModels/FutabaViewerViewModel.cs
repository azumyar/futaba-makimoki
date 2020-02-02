using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Prism.Events;
using Prism.Mvvm;
using Reactive.Bindings;

namespace Yarukizero.Net.MakiMoki.Wpf.ViewModels {
	class FutabaViewerViewModel : BindableBase, IDisposable {
		internal class Messenger : EventAggregator {
			public static Messenger Instance { get; } = new Messenger();
		}

		internal class CatalogListboxUpdatedMessage { }
		internal class MediaViewerOpenMessage { 
			public PlatformData.FutabaMedia Media { get; }

			public MediaViewerOpenMessage(PlatformData.FutabaMedia media) {
				this.Media = media;
			}
		}

		private CompositeDisposable Disposable { get; } = new CompositeDisposable();


		public ReactiveCommand<RoutedPropertyChangedEventArgs<Model.IFutabaViewerContents>> ContentsChangedCommand { get; } 
			= new ReactiveCommand<RoutedPropertyChangedEventArgs<Model.IFutabaViewerContents>>();

		public ReactiveCommand<RoutedEventArgs> CatalogUpdateClickCommand { get; } = new ReactiveCommand<RoutedEventArgs>();
		public ReactiveCommand CatalogSortClickCommand { get; } = new ReactiveCommand();
		public ReactiveCommand<RoutedEventArgs> PostClickCommand { get; } = new ReactiveCommand<RoutedEventArgs>();

		public ReactiveCommand<RoutedEventArgs> ThreadUpdateCommand { get; } = new ReactiveCommand<RoutedEventArgs>();

		public ReactiveCommand<RoutedEventArgs> PostViewPostCommand { get; } = new ReactiveCommand<RoutedEventArgs>();


		public ReactiveCommand<RoutedEventArgs> LinkClickCommand { get; } = new ReactiveCommand<RoutedEventArgs>();

		public ReactiveCommand<MouseButtonEventArgs> CatalogItemMouseDownCommand { get; }
			= new ReactiveCommand<MouseButtonEventArgs>();
		public ReactiveCommand<MouseButtonEventArgs> CatalogItemClickCommand { get; }
			= new ReactiveCommand<MouseButtonEventArgs>();

		public ReactiveCommand<MouseButtonEventArgs> ThreadImageMouseDownCommand { get; }
			= new ReactiveCommand<MouseButtonEventArgs>();
		public ReactiveCommand<MouseButtonEventArgs> ThreadImageClickCommand { get; }
			= new ReactiveCommand<MouseButtonEventArgs>();

		public ReactiveProperty<Visibility> PostViewVisibility { get; }
			= new ReactiveProperty<Visibility>(Visibility.Hidden);


		public ReactiveCommand<RoutedEventArgs> MenuItemCopyClickCommand { get; } = new ReactiveCommand<RoutedEventArgs>();
		public ReactiveCommand<RoutedEventArgs> MenuItemReplyClickCommand { get; } = new ReactiveCommand<RoutedEventArgs>();
		public ReactiveCommand<RoutedEventArgs> MenuItemSoudaneClickCommand { get; } = new ReactiveCommand<RoutedEventArgs>();
		public ReactiveCommand<RoutedEventArgs> MenuItemDelClickCommand { get; } = new ReactiveCommand<RoutedEventArgs>();
		public ReactiveCommand<RoutedEventArgs> MenuItemDeleteClickCommand { get; } = new ReactiveCommand<RoutedEventArgs>();

		private bool isCatalogItemClicking = false;
		private bool isThreadImageClicking = false;

		public FutabaViewerViewModel() {
			ContentsChangedCommand.Subscribe(x => OnContentsChanged(x));

			CatalogUpdateClickCommand.Subscribe(x => OnCatalogUpdateClick(x));
			CatalogItemMouseDownCommand.Subscribe(x => OnCatalogItemMouseDown(x));
			CatalogItemClickCommand.Subscribe(x => OnCatalogClick(x));
			ThreadImageMouseDownCommand.Subscribe(x => OnThreadImageMouseDown(x));
			ThreadImageClickCommand.Subscribe(x => OnThreadImageClick(x));
			ThreadUpdateCommand.Subscribe(x => OnThreadUpdateClick(x));
			PostClickCommand.Subscribe(x => OnPostClick((x.Source as FrameworkElement)?.DataContext as Model.BindableFutaba));
			PostViewPostCommand.Subscribe(x => OnPostViewPostClick((x.Source as FrameworkElement)?.DataContext as Model.BindableFutaba));
			LinkClickCommand.Subscribe(x => OnLinkClick(x));

			MenuItemCopyClickCommand.Subscribe(x => OnMenuItemCopyClickCommand(x));
			MenuItemReplyClickCommand.Subscribe(x => OnMenuItemReplyClickCommand(x));
			MenuItemSoudaneClickCommand.Subscribe(x => OnMenuItemSoudaneClickCommand(x));
			MenuItemDelClickCommand.Subscribe(x => OnMenuItemDelClickCommand(x));
			MenuItemDeleteClickCommand.Subscribe(x => OnMenuItemDeleteClickCommand(x));
		}

		public void Dispose() {
			Disposable.Dispose();
		}

		private void OnLinkClick(RoutedEventArgs e) {
			if(e.Source is Hyperlink link) {
				// TODO: ロダのリンクこの辺定数定義設定に移動させる
				var u = link.NavigateUri.AbsoluteUri;
				if(u.StartsWith("http://www.nijibox5.com/futabafiles/tubu/src/")
					|| u.StartsWith("http://www.nijibox5.com/futabafiles/kobin/src/")
					|| u.StartsWith("https://dec.2chan.net/up2/src/")
					|| u.StartsWith("https://dec.2chan.net/up/src/")) {
					var ext = Regex.Match(u, @"\.[a-zA-Z0-9]+$");
					if(ext.Success) {
						// TODO: 内部画像ビューワを作ってそっちに移動
						if(new string[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" }.Contains(ext.Value.ToLower())) {
							Messenger.Instance.GetEvent<PubSubEvent<MediaViewerOpenMessage>>()
								.Publish(new MediaViewerOpenMessage(
									PlatformData.FutabaMedia.FromExternalUrl(u)));
							goto end;
						} else if(new string[] { ".mp4", ".webm" }.Contains(ext.Value.ToLower())) {
							this.StartBrowser(u);
							goto end;
						}
					}
				}
				// ふたばのリンク
				foreach(var b in Config.ConfigLoader.Bord) {
					var uu = new Uri(b.Url);
					if(uu.Authority == link.NavigateUri.Authority) {
						var m = Regex.Match(link.NavigateUri.LocalPath, @"^/[^/]+/res/([0-9]+)\.htm$");
						if(link.NavigateUri.LocalPath.StartsWith(uu.LocalPath) && m.Success) {
							Util.Futaba.Open(new Data.UrlContext(b.Url, m.Groups[1].Value));
							goto end;
						}
					}
				}

				this.StartBrowser(u);
			}
		end:;
			System.Diagnostics.Debug.WriteLine(e);
		}

		private void OnCatalogUpdateClick(RoutedEventArgs e) {
			if((e.Source is FrameworkElement el) && (el.DataContext is Data.FutabaContext fc)) {
				Util.Futaba.UpdateCatalog(fc.Bord)
					.ObserveOnDispatcher()
					.Subscribe(x => {
						Messenger.Instance.GetEvent<PubSubEvent<CatalogListboxUpdatedMessage>>()
							.Publish(new CatalogListboxUpdatedMessage());
					});
			}
		}

		private void OnContentsChanged(RoutedPropertyChangedEventArgs<Model.IFutabaViewerContents> e) {
			this.PostViewVisibility.Value = Visibility.Hidden;
		}

		private void OnCatalogItemMouseDown(MouseButtonEventArgs e) {
			if((e.Source is FrameworkElement o) && (o.DataContext is Model.BindableFutabaResItem it)) {
				switch(e.ChangedButton) {
				case MouseButton.Left:
				case MouseButton.Middle:
					// 2つマウスを押された場合は false
					this.isCatalogItemClicking = !this.isCatalogItemClicking;
					break;
				}
			}
		}

		private void OnCatalogClick(MouseButtonEventArgs e) {
			if((e.Source is FrameworkElement o) && (o.DataContext is Model.BindableFutabaResItem it)) {
				if((e.ClickCount == 1) && (VisualTreeHelper.HitTest(o, e.GetPosition(o)) != null)) {
					switch(e.ChangedButton) {
					case MouseButton.Left:
						if(this.isCatalogItemClicking) {
							Util.Futaba.UpdateThreadRes(it.Bord.Value, it.ThreadResNo.Value);
							this.isCatalogItemClicking = false;
						}
						e.Handled = true;
						break;
					case MouseButton.Middle:
						if(this.isCatalogItemClicking) {
							// TODO: そのうちこっちは裏で開くように返れたらいいな
							Util.Futaba.UpdateThreadRes(it.Bord.Value, it.ThreadResNo.Value);
							this.isCatalogItemClicking = false;
						}
						e.Handled = true;
						break;
					}
				}
			}
		}

		private void OnThreadImageMouseDown(MouseButtonEventArgs e) {
			if((e.Source is FrameworkElement o) && (o.DataContext is Model.BindableFutabaResItem it)) {
				switch(e.ChangedButton) {
				case MouseButton.Left:
				case MouseButton.Middle:
					// 2つマウスを押された場合は false
					this.isThreadImageClicking = !this.isThreadImageClicking;
					break;
				}
			}
		}
		private void OnThreadImageClick(MouseButtonEventArgs e) {
			if((e.Source is FrameworkElement o) && (o.DataContext is Model.BindableFutabaResItem it)) {
				if((e.ClickCount == 1) && (VisualTreeHelper.HitTest(o, e.GetPosition(o)) != null)) {
					switch(e.ChangedButton) {
					case MouseButton.Left:
					case MouseButton.Middle:
						if(this.isThreadImageClicking) {
							Messenger.Instance.GetEvent<PubSubEvent<MediaViewerOpenMessage>>()
								.Publish(new MediaViewerOpenMessage(
									PlatformData.FutabaMedia.FromFutabaUrl(
										it.Raw.Value.Url, it.Raw.Value.ResItem.Res)));
							this.isThreadImageClicking = false;
						}
						e.Handled = true;
						break;
					}
				}
			}
		}

		private void OnThreadUpdateClick(RoutedEventArgs e) {
			if(e.Source is FrameworkElement el) {
				if(el.DataContext is Data.FutabaContext x) {
					Util.Futaba.UpdateThreadRes(x.Bord, x.Url.ThreadNo);//.Subscribe();
				} else if(el.DataContext is Model.BindableFutabaResItem y) {
					Util.Futaba.UpdateThreadRes(y.Bord.Value, y.Parent.Value.Url.ThreadNo);//.Subscribe();
				}
			}
		}

		private void OnPostClick(Model.BindableFutaba x) {
			if(x != null) {
				this.PostViewVisibility.Value = (this.PostViewVisibility.Value == Visibility.Hidden) ? Visibility.Visible : Visibility.Hidden;
			}
		}

		private void OnPostViewPostClick(Model.BindableFutaba x) {
			if(x != null) {
				if(x.Url.IsCatalogUrl) {
					Util.Futaba.PostThread(x.Raw.Bord,
						x.PostData.Value.Name.Value,
						x.PostData.Value.Mail.Value,
						x.PostData.Value.Subject.Value,
						x.PostData.Value.CommentEncoded.Value,
						x.PostData.Value.ImagePath.Value,
						x.PostData.Value.Password.Value)
					.ObserveOn(UIDispatcherScheduler.Default)
					.Subscribe(y => {
						if(y.Successed) {
							PostViewVisibility.Value = Visibility.Hidden;
							x.PostData.Value = new Model.BindableFutaba.PostHolder();
							Task.Run(async () => {
								await Task.Delay(1000); // すぐにスレが作られないので1秒くらい待つ
								Util.Futaba.UpdateThreadRes(x.Raw.Bord, y.NextOrMessage);
							});
						} else {
							// TODO: あとでいい感じにする
							MessageBox.Show(y.NextOrMessage);
						}
					});
				} else {
					Util.Futaba.PostRes(x.Raw.Bord, x.Url.ThreadNo,
						x.PostData.Value.Name.Value,
						x.PostData.Value.Mail.Value,
						x.PostData.Value.Subject.Value,
						x.PostData.Value.CommentEncoded.Value,
						x.PostData.Value.ImagePath.Value,
						x.PostData.Value.Password.Value)
					.ObserveOn(UIDispatcherScheduler.Default)
					.Subscribe(y => {
						if(y.Successed) {
							PostViewVisibility.Value = Visibility.Hidden;
							x.PostData.Value = new Model.BindableFutaba.PostHolder();
							Util.Futaba.UpdateThreadRes(x.Raw.Bord, x.Url.ThreadNo);
						} else {
							// TODO: あとでいい感じにする
							MessageBox.Show(y.Message);
						}
					});
				}
			}
		}

		private void OnMenuItemCopyClickCommand(RoutedEventArgs e) {
			if((e.Source is FrameworkElement el) && (el.DataContext is Model.BindableFutabaResItem ri)) {
				var sb = new StringBuilder()
					.Append("No.")
					.Append(ri.Raw.Value.ResItem.No);
				if(ri.Bord.Value.Extra?.NameValue ?? true) {
					sb.Append(" ")
						.Append(ri.Raw.Value.ResItem.Res.Sub)
						.Append(" ")
						.Append(ri.Raw.Value.ResItem.Res.Name);
				}
				if(!string.IsNullOrWhiteSpace(ri.Raw.Value.ResItem.Res.Email)) {
					sb.Append(" [").Append(ri.Raw.Value.ResItem.Res.Email).Append("]");
				}
				sb.Append(" ").Append(ri.Raw.Value.ResItem.Res.Now);
				if(!string.IsNullOrWhiteSpace(ri.Raw.Value.ResItem.Res.Host)) {
					sb.Append(" ").Append(ri.Raw.Value.ResItem.Res.Host);
				}
				if(!string.IsNullOrWhiteSpace(ri.Raw.Value.ResItem.Res.Id)) {
					sb.Append(" ").Append(ri.Raw.Value.ResItem.Res.Id);
				}
				if(0 < ri.Raw.Value.Soudane) {
					sb.Append(" そうだね×").Append(ri.Raw.Value.Soudane);
				}
				sb.AppendLine();
				sb.Append(WpfUtil.TextUtil.RawComment2Text(ri.Raw.Value.ResItem.Res.Com));
				Clipboard.SetText(sb.ToString());
			}
		}

		private void OnMenuItemReplyClickCommand(RoutedEventArgs e) {
			if((e.Source is FrameworkElement el) && (el.DataContext is Model.BindableFutabaResItem ri)) {
				var sb = new StringBuilder();
				var c = WpfUtil.TextUtil.RawComment2Text(ri.Raw.Value.ResItem.Res.Com).Replace("\r", "").Split('\n');
				foreach(var s in c) {
					sb.Append(">").AppendLine(s);
				}
				ri.Parent.Value.PostData.Value.Comment.Value = sb.ToString();
				this.PostViewVisibility.Value = Visibility.Visible;
			}
		}

		private void OnMenuItemSoudaneClickCommand(RoutedEventArgs e) {
			if((e.Source is FrameworkElement el) && (el.DataContext is Model.BindableFutabaResItem ri)) {
				Util.Futaba.PostSoudane(ri.Bord.Value, ri.Raw.Value.ResItem.No)
					.ObserveOn(UIDispatcherScheduler.Default)
					.Subscribe(x => {
						var m = x.Message;
						if(x.Successed) {
							if(int.TryParse(x.Message, out var i)) {
								if(ri.Raw.Value.Soudane != i) {
									Util.Futaba.UpdateThreadRes(ri.Bord.Value, ri.Raw.Value.Url.ThreadNo);//.Subscribe();
									goto exit;
								}
							}
							m = "不明なエラー";
						}
						// TODO: いい感じにする
						MessageBox.Show(m);
					exit:;
					});
			}
		}

		private void OnMenuItemDelClickCommand(RoutedEventArgs e) {
			if((e.Source is FrameworkElement el) && (el.DataContext is Model.BindableFutabaResItem ri)) {
				// TODO: 確認ダイアログを出す
				var resNo = ri.Raw.Value.ResItem.No;
				var threadNo = ri.Parent.Value.Url.IsCatalogUrl ? resNo : ri.Parent.Value.ResItems.Value.First().Raw.Value.ResItem.No;
				Util.Futaba.PostDel(ri.Bord.Value, threadNo, resNo)
					.ObserveOn(UIDispatcherScheduler.Default)
					.Subscribe(x => {
						var m = x.Message;
						if(x.Successed) {
						}
						// TODO: いい感じにする
						MessageBox.Show(m);
					});
			}
		}

		private void OnMenuItemDeleteClickCommand(RoutedEventArgs e) {
			System.Diagnostics.Debug.WriteLine(e);
			if((e.Source is FrameworkElement el) && (el.DataContext is Model.BindableFutabaResItem ri)) {
				// TODO: 確認ダイアログを出す
				Util.Futaba.PostDeleteThreadRes(ri.Bord.Value, ri.Raw.Value.ResItem.No, false, Config.ConfigLoader.Password.Futaba)
					.ObserveOn(UIDispatcherScheduler.Default)
					.Subscribe(x => {
						var m = x.Message;
						if(x.Successed) {
						}
						// TODO: いい感じにする
						MessageBox.Show(m);
					exit:;
					});
			}
		}

		private void StartBrowser(string u) {
			try {
				Process.Start(new ProcessStartInfo(u));
			}
			catch(System.ComponentModel.Win32Exception) {
				// 関連付け実行に失敗
			}
		}
	}
}