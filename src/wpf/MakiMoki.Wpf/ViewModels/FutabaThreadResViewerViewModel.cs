using System;
using System.Collections.Generic;
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
using Yarukizero.Net.MakiMoki.Data;
using Yarukizero.Net.MakiMoki.Wpf.Model;

namespace Yarukizero.Net.MakiMoki.Wpf.ViewModels {
	class FutabaThreadResViewerViewModel : BindableBase, IDisposable {
		internal class Messenger : EventAggregator {
			public static Messenger Instance { get; } = new Messenger();
		}
		internal class MediaViewerOpenMessage { 
			public PlatformData.FutabaMedia Media { get; }

			public MediaViewerOpenMessage(PlatformData.FutabaMedia media) {
				this.Media = media;
			}
		}
		internal class ScrollResMessage {
			public Model.BindableFutabaResItem Res { get; }

			public ScrollResMessage(Model.BindableFutabaResItem res) {
				this.Res = res;
			}
		}

		public ReactiveCommand<RoutedPropertyChangedEventArgs<Model.IFutabaViewerContents>> ContentsChangedCommand { get; } 
			= new ReactiveCommand<RoutedPropertyChangedEventArgs<Model.IFutabaViewerContents>>();

		public ReactiveCommand<RoutedEventArgs> PostClickCommand { get; } = new ReactiveCommand<RoutedEventArgs>();

		public ReactiveCommand<RoutedEventArgs> ThreadUpdateCommand { get; } = new ReactiveCommand<RoutedEventArgs>();

		public ReactiveCommand<RoutedEventArgs> ImageClickCommand { get; } = new ReactiveCommand<RoutedEventArgs>();
		public ReactiveCommand<PlatformData.HyperLinkEventArgs> LinkClickCommand { get; } = new ReactiveCommand<PlatformData.HyperLinkEventArgs>();
		public ReactiveCommand<PlatformData.QuotClickEventArgs> QuotClickCommand { get; } = new ReactiveCommand<PlatformData.QuotClickEventArgs>();

		public ReactiveCommand<MouseButtonEventArgs> ThreadImageMouseDownCommand { get; }
			= new ReactiveCommand<MouseButtonEventArgs>();
		public ReactiveCommand<MouseButtonEventArgs> ThreadImageClickCommand { get; }
			= new ReactiveCommand<MouseButtonEventArgs>();

		public ReactiveProperty<Visibility> PostViewVisibility { get; }
			= new ReactiveProperty<Visibility>(Visibility.Hidden);

		public ReactiveProperty<string> FilterText { get; } = new ReactiveProperty<string>("");

		public ReactiveCommand<TextChangedEventArgs> FilterTextChangedCommand { get; } = new ReactiveCommand<TextChangedEventArgs>();

		public ReactiveCommand<Model.IFutabaViewerContents> ThreadResHamburgerItemCopyUrlClickCommand { get; } = new ReactiveCommand<Model.IFutabaViewerContents>();
		public ReactiveCommand<Model.IFutabaViewerContents> ThreadResHamburgerItemOpenUrlClickCommand { get; } = new ReactiveCommand<Model.IFutabaViewerContents>();

		public ReactiveCommand<Model.BindableFutaba> WheelUpdateCommand { get; } = new ReactiveCommand<BindableFutaba>();

		public ReactiveCommand<RoutedEventArgs> PaletteButtonCopyCommand { get; } = new ReactiveCommand<RoutedEventArgs>();
		public ReactiveCommand<RoutedEventArgs> PaletteButtonSoudaneCommand { get; } = new ReactiveCommand<RoutedEventArgs>();
		public ReactiveCommand<RoutedEventArgs> PaletteButtonDelCommand { get; } = new ReactiveCommand<RoutedEventArgs>();

		public ReactiveCommand<BindableFutaba> MenuItemFullUpdateClickCommand { get; } = new ReactiveCommand<BindableFutaba>();
		public ReactiveCommand<Model.BindableFutabaResItem> MenuItemCopyClickCommand { get; } = new ReactiveCommand<Model.BindableFutabaResItem>();
		public ReactiveCommand<Model.BindableFutabaResItem> MenuItemReplyClickCommand { get; } = new ReactiveCommand<Model.BindableFutabaResItem>();
		public ReactiveCommand<Model.BindableFutabaResItem> MenuItemReplyResNoClickCommand { get; } = new ReactiveCommand<Model.BindableFutabaResItem>();
		public ReactiveCommand<Model.BindableFutabaResItem> MenuItemReplyImageNameoClickCommand { get; } = new ReactiveCommand<Model.BindableFutabaResItem>();
		public ReactiveCommand<Model.BindableFutabaResItem> MenuItemSoudaneClickCommand { get; } = new ReactiveCommand<Model.BindableFutabaResItem>();
		public ReactiveCommand<Model.BindableFutabaResItem> MenuItemDelClickCommand { get; } = new ReactiveCommand<Model.BindableFutabaResItem>();
		public ReactiveCommand<Model.BindableFutabaResItem> MenuItemDeleteClickCommand { get; } = new ReactiveCommand<Model.BindableFutabaResItem>();
		public ReactiveCommand<Model.BindableFutabaResItem> MenuItemDeleteImageClickCommand { get; } = new ReactiveCommand<Model.BindableFutabaResItem>();
		public ReactiveCommand<Model.BindableFutabaResItem> MenuItemNgImageCommand { get; } = new ReactiveCommand<Model.BindableFutabaResItem>();
		public ReactiveCommand<Model.BindableFutabaResItem> MenuItemResHiddenCommand { get; } = new ReactiveCommand<Model.BindableFutabaResItem>();

		public ReactiveCommand<(BindableFutaba Futaba, TextBox TextBox)> CopyTextboxQuotCommand { get; } = new ReactiveCommand<(BindableFutaba Futaba, TextBox TextBox)>();
		public ReactiveCommand<(BindableFutaba Futaba, TextBox TextBox)> CopyTextboxSearchCommand { get; } = new ReactiveCommand<(BindableFutaba Futaba, TextBox TextBox)>();
		public ReactiveCommand<(BindableFutaba Futaba, TextBox TextBox)> CopyTextboxNgCommand { get; } = new ReactiveCommand<(BindableFutaba Futaba, TextBox TextBox)>();
		public ReactiveCommand<(BindableFutaba Futaba, TextBox TextBox)> CopyTextboxCopyCommand { get; } = new ReactiveCommand<(BindableFutaba Futaba, TextBox TextBox)>();
		public ReactiveCommand<(BindableFutaba Futaba, TextBox TextBox)> CopyTextboxGoogleCommand { get; } = new ReactiveCommand<(BindableFutaba Futaba, TextBox TextBox)>();

		public ReactiveCommand<Model.BindableFutaba> KeyBindingUpdateCommand { get; } = new ReactiveCommand<BindableFutaba>();
		public ReactiveCommand<Model.BindableFutaba> KeyBindingSearchCommand { get; } = new ReactiveCommand<BindableFutaba>();
		public ReactiveCommand<Model.BindableFutaba> KeyBindingPostCommand { get; } = new ReactiveCommand<BindableFutaba>();
		public ReactiveCommand<Model.BindableFutaba> KeyBindingCloseCommand { get; } = new ReactiveCommand<BindableFutaba>();



		private bool isThreadImageClicking = false;

		public FutabaThreadResViewerViewModel() {
			ContentsChangedCommand.Subscribe(x => OnContentsChanged(x));

			FilterTextChangedCommand.Subscribe(x => OnFilterTextChanged(x));

			ThreadImageMouseDownCommand.Subscribe(x => OnThreadImageMouseDown(x));
			ThreadImageClickCommand.Subscribe(x => OnThreadImageClick(x));
			ThreadUpdateCommand.Subscribe(x => OnThreadUpdateClick(x));
			PostClickCommand.Subscribe(x => OnPostClick((x.Source as FrameworkElement)?.DataContext as Model.BindableFutaba));
			ImageClickCommand.Subscribe(x => OnImageClick(x));
			LinkClickCommand.Subscribe(x => OnLinkClick(x));
			QuotClickCommand.Subscribe(x => OnQuotClick(x));
			WheelUpdateCommand.Subscribe(x => OnWheelUpdate(x));

			ThreadResHamburgerItemCopyUrlClickCommand.Subscribe(x => OnThreadResHamburgerItemCopyUrlClick(x));
			ThreadResHamburgerItemOpenUrlClickCommand.Subscribe(x => OnThreadResHamburgerItemOpenUrlClick(x));
			MenuItemFullUpdateClickCommand.Subscribe(x => OnMenuItemFullUpdateClick(x));
			MenuItemCopyClickCommand.Subscribe(x => OnMenuItemCopyClick(x));
			MenuItemReplyClickCommand.Subscribe(x => OnMenuItemReplyClick(x));
			MenuItemReplyResNoClickCommand.Subscribe(x => OnMenuItemReplyResNoClick(x));
			MenuItemReplyImageNameoClickCommand.Subscribe(x => OnMenuItemReplyImageNameoClick(x));
			MenuItemSoudaneClickCommand.Subscribe(x => OnMenuItemSoudaneClick(x));
			MenuItemDelClickCommand.Subscribe(x => OnMenuItemDelClick(x));
			MenuItemDeleteClickCommand.Subscribe(x => OnMenuItemDeleteClick(x));
			MenuItemDeleteImageClickCommand.Subscribe(x => OnMenuItemDeleteImageClick(x));
			MenuItemResHiddenCommand.Subscribe(x => OnMenuItemResHidden(x));
			MenuItemNgImageCommand.Subscribe(x => OnMenuItemNgImage(x));

			PaletteButtonCopyCommand.Subscribe(x => OnPaletteButtonCopy(x));
			PaletteButtonSoudaneCommand.Subscribe(x => OnPaletteButtonSoudane(x));
			PaletteButtonDelCommand.Subscribe(x => OnPaletteButtonDel(x));
		
			CopyTextboxQuotCommand.Subscribe(x => OnCopyTextboxQuot(x));
			CopyTextboxSearchCommand.Subscribe(x => OnCopyTextboxSearch(x));
			CopyTextboxNgCommand.Subscribe(x => OnCopyTextboxNg(x));
			CopyTextboxCopyCommand.Subscribe(x => OnCopyTextboxCopy(x));
			CopyTextboxGoogleCommand.Subscribe(x => OnCopyTextboxGoogle(x));

			KeyBindingUpdateCommand.Subscribe(x => UpdateThread(x));
			//KeyBindingSearchCommand.Subscribe(x => x);
			KeyBindingPostCommand.Subscribe(x => UpdatePost(x));
			//KeyBindingCloseCommand.Subscribe(x => x);
		}

		public void Dispose() {
			Helpers.AutoDisposable.GetCompositeDisposable(this).Dispose();
		}

		private void UpdateThread(Model.BindableFutaba x) {
			Util.Futaba.UpdateThreadRes(x.Raw.Bord, x.Url.ThreadNo, Config.ConfigLoader.MakiMoki.FutabaThreadGetIncremental).Subscribe();
		}

		private void UpdatePost(Model.BindableFutaba x) {
			this.PostViewVisibility.Value = (this.PostViewVisibility.Value == Visibility.Hidden) ? Visibility.Visible : Visibility.Hidden;
		}

		private void OnImageClick(RoutedEventArgs e) {
			if((e.Source is FrameworkElement o) && (o.DataContext is Model.BindableFutabaResItem it)) {
				if(object.ReferenceEquals(it.ThumbSource.Value, it.OriginSource.Value)) {
					Messenger.Instance.GetEvent<PubSubEvent<MediaViewerOpenMessage>>()
						.Publish(new MediaViewerOpenMessage(
							PlatformData.FutabaMedia.FromFutabaUrl(
								it.Raw.Value.Url, it.Raw.Value.ResItem.Res)));
				}
				e.Handled = true;
			}
		}

		private void OnLinkClick(PlatformData.HyperLinkEventArgs e) {
			var u = e.NavigateUri.AbsoluteUri;
			if(Config.ConfigLoader.Uploder.Uploders
				.Where(x => u.StartsWith(x.Root))
				.FirstOrDefault() != null) {

				var ext = Regex.Match(u, @"\.[a-zA-Z0-9]+$");
				if(ext.Success) {
					if(Config.ConfigLoader.MimeFutaba.Types.Select(x => x.Ext).Contains(ext.Value.ToLower())) {
						Messenger.Instance.GetEvent<PubSubEvent<MediaViewerOpenMessage>>()
							.Publish(new MediaViewerOpenMessage(
								PlatformData.FutabaMedia.FromExternalUrl(u)));
						goto end;
					}
				}
			}
			// ふたばのリンク
			foreach(var b in Config.ConfigLoader.Bord.Bords) {
				var uu = new Uri(b.Url);
				if(uu.Authority == e.NavigateUri.Authority) {
					var m = Regex.Match(e.NavigateUri.LocalPath, @"^/[^/]+/res/([0-9]+)\.htm$");
					if(e.NavigateUri.LocalPath.StartsWith(uu.LocalPath) && m.Success) {
						Util.Futaba.Open(new Data.UrlContext(b.Url, m.Groups[1].Value));
						goto end;
					}
				}
			}

			WpfUtil.PlatformUtil.StartBrowser(e.NavigateUri);
		end:;
			System.Diagnostics.Debug.WriteLine(e);
		}

		private void OnQuotClick(PlatformData.QuotClickEventArgs e) {
			Messenger.Instance.GetEvent<PubSubEvent<ScrollResMessage>>()
				.Publish(new ScrollResMessage(e.TargetRes));
		}

		private void OnWheelUpdate(Model.BindableFutaba f) {
			this.UpdateThread(f);
		}

		private void OnContentsChanged(RoutedPropertyChangedEventArgs<Model.IFutabaViewerContents> e) {
			this.PostViewVisibility.Value = Visibility.Hidden;
		}

		private async void OnFilterTextChanged(TextChangedEventArgs e) {
			if(e.Source is TextBox tb) {
				var s = tb.Text.Clone().ToString();
				await Task.Delay(500);
				if(tb.Text == s) {
					FilterText.Value = s;
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
				if(el.DataContext is BindableFutaba x) {
					UpdateThread(x);
				} else if(el.DataContext is Model.BindableFutabaResItem y) {
					Util.Futaba.UpdateThreadRes(y.Bord.Value, y.Parent.Value.Url.ThreadNo, Config.ConfigLoader.MakiMoki.FutabaThreadGetIncremental).Subscribe();
				}
			}
		}

		private void OnPostClick(Model.BindableFutaba x) {
			if(x != null) {
				UpdatePost(x);
			}
		}

		private void OnPaletteButtonCopy(RoutedEventArgs e) {
			if((e.Source is FrameworkElement el) && (el.DataContext is Model.BindableFutabaResItem ri)) {
				OnMenuItemCopyClick(ri);
			}
		}

		private void OnPaletteButtonSoudane(RoutedEventArgs e) {
			if((e.Source is FrameworkElement el) && (el.DataContext is Model.BindableFutabaResItem ri)) {
				OnMenuItemSoudaneClick(ri);
			}
		}

		private void OnPaletteButtonDel(RoutedEventArgs e) {
			if((e.Source is FrameworkElement el) && (el.DataContext is Model.BindableFutabaResItem ri)) {
				OnMenuItemDelClick(ri);
			}
		}

		private void OnThreadResHamburgerItemCopyUrlClick(Model.IFutabaViewerContents c) {
			var u = c.Futaba.Value.Raw.Url;
			if(u.IsThreadUrl) {
				Clipboard.SetText(Util.Futaba.GetFutabaThreadUrl(u));
			}
		}

		private void OnThreadResHamburgerItemOpenUrlClick(Model.IFutabaViewerContents c) {
			WpfUtil.PlatformUtil.StartBrowser(new Uri(Util.Futaba.GetFutabaThreadUrl(c.Futaba.Value.Raw.Url)));
		}

		private void OnMenuItemFullUpdateClick(BindableFutaba futaba) {
			Util.Futaba.UpdateThreadRes(futaba.Raw.Bord, futaba.Url.ThreadNo)
				.Subscribe();
		}

		private void OnMenuItemCopyClick(Model.BindableFutabaResItem item) {
			var sb = new StringBuilder()
				.Append("No.")
				.Append(item.Raw.Value.ResItem.No);
			if(item.Bord.Value.Extra?.NameValue ?? true) {
				sb.Append(" ")
					.Append(item.Raw.Value.ResItem.Res.Sub)
					.Append(" ")
					.Append(item.Raw.Value.ResItem.Res.Name);
			}
			if(!string.IsNullOrWhiteSpace(item.Raw.Value.ResItem.Res.Email)) {
				sb.Append(" [").Append(item.Raw.Value.ResItem.Res.Email).Append("]");
			}
			sb.Append(" ").Append(item.Raw.Value.ResItem.Res.Now);
			if(!string.IsNullOrWhiteSpace(item.Raw.Value.ResItem.Res.Host)) {
				sb.Append(" ").Append(item.Raw.Value.ResItem.Res.Host);
			}
			if(!string.IsNullOrWhiteSpace(item.Raw.Value.ResItem.Res.Id)) {
				sb.Append(" ").Append(item.Raw.Value.ResItem.Res.Id);
			}
			if(0 < item.Raw.Value.Soudane) {
				sb.Append(" そうだね×").Append(item.Raw.Value.Soudane);
			}
			sb.AppendLine();
			sb.Append(WpfUtil.TextUtil.RawComment2Text(item.Raw.Value.ResItem.Res.Com));
			Clipboard.SetText(sb.ToString());
		}

		private void OnMenuItemReplyClick(Model.BindableFutabaResItem item) {
			var sb = new StringBuilder();
			var c = WpfUtil.TextUtil.RawComment2Text(item.Raw.Value.ResItem.Res.Com).Replace("\r", "").Split('\n');
			foreach(var s in c) {
				sb.Append(">").AppendLine(s);
			}
			FutabaPostViewViewModel.Messenger.Instance.GetEvent<PubSubEvent<FutabaPostViewViewModel.ReplaceTextMessage>>()
				.Publish(new FutabaPostViewViewModel.ReplaceTextMessage(item.Parent.Value.Url, sb.ToString()));
			this.PostViewVisibility.Value = Visibility.Visible;
		}

		private void OnMenuItemReplyResNoClick(Model.BindableFutabaResItem item) {
			FutabaPostViewViewModel.Messenger.Instance.GetEvent<PubSubEvent<FutabaPostViewViewModel.AppendTextMessage>>()
				.Publish(new FutabaPostViewViewModel.AppendTextMessage(item.Parent.Value.Url, $">No.{ item.Raw.Value.ResItem.No }"));
			this.PostViewVisibility.Value = Visibility.Visible;
		}

		private void OnMenuItemReplyImageNameoClick(Model.BindableFutabaResItem item) {
			FutabaPostViewViewModel.Messenger.Instance.GetEvent<PubSubEvent<FutabaPostViewViewModel.AppendTextMessage>>()
				.Publish(new FutabaPostViewViewModel.AppendTextMessage(item.Parent.Value.Url, $">{ item.ImageName.Value }"));
			this.PostViewVisibility.Value = Visibility.Visible;
		}

		private void OnMenuItemSoudaneClick(Model.BindableFutabaResItem item) {
			Util.Futaba.PostSoudane(item.Bord.Value, item.Raw.Value.ResItem.No)
				.ObserveOn(UIDispatcherScheduler.Default)
				.Subscribe(x => {
					var m = x.Message;
					if(x.Successed) {
						if(int.TryParse(x.Message, out var i)) {
							Util.Futaba.PutInformation(new Information($"そうだねx{ i }"));
							if(item.Raw.Value.Soudane != i) {
								Util.Futaba.UpdateThreadRes(
									item.Bord.Value,
									item.Raw.Value.Url.ThreadNo,
									Config.ConfigLoader.MakiMoki.FutabaThreadGetIncremental)
									.Subscribe();
							}
							goto exit;
						}
						m = "不明なエラー";
					}
					Util.Futaba.PutInformation(new Information(m));
				exit:;
				});
		}

		private void OnMenuItemDelClick(Model.BindableFutabaResItem item) {
			var resNo = item.Raw.Value.ResItem.No;
			var threadNo = item.Parent.Value.Url.IsCatalogUrl ? resNo : item.Parent.Value.ResItems.First().Raw.Value.ResItem.No;
			Util.Futaba.PostDel(item.Bord.Value, threadNo, resNo)
				.ObserveOn(UIDispatcherScheduler.Default)
				.Subscribe(x => {
					if(x.Successed) {
						Util.Futaba.PutInformation(new Information("del送信"));
					} else {
						Util.Futaba.PutInformation(new Information(x.Message));
					}
				});
		}

		private void PostItemDelete(Model.BindableFutabaResItem item, bool imageOnlyDel) {
			Util.Futaba.PostDeleteThreadRes(item.Bord.Value, item.Raw.Value.ResItem.No, imageOnlyDel, Config.ConfigLoader.FutabaApi.SavedPassword)
				.ObserveOn(UIDispatcherScheduler.Default)
				.Subscribe(x => {
					if(x.Successed) {
						Util.Futaba.PutInformation(new Information("削除しました"));
						Util.Futaba.UpdateThreadRes(item.Bord.Value, item.Raw.Value.Url.ThreadNo).Subscribe();
					} else {
						Util.Futaba.PutInformation(new Information(x.Message));
					}
				});
		}

		private void OnMenuItemDeleteClick(Model.BindableFutabaResItem item) {
			PostItemDelete(item, false);
		}

		private void OnMenuItemDeleteImageClick(Model.BindableFutabaResItem item) {
			PostItemDelete(item, true);
		}

		private void OnMenuItemResHidden(Model.BindableFutabaResItem x) {
			var ng = Ng.NgData.HiddenData.FromResItem(x.Raw.Value.Url.BaseUrl, x.Raw.Value.ResItem);
			if(Ng.NgUtil.NgHelper.CheckHidden(x.Parent.Value.Raw, x.Raw.Value)) {
				Ng.NgConfig.NgConfigLoader.RemoveHiddenRes(ng);
			} else {
				Ng.NgConfig.NgConfigLoader.AddHiddenRes(ng);
			}
		}

		private void OnMenuItemNgImage(Model.BindableFutabaResItem x) {
			if(x.OriginSource.Value != null) {
				var v = x.ThumbHash.Value ?? WpfUtil.ImageUtil.CalculatePerceptualHash(x.OriginSource.Value);
				var ng = Ng.NgConfig.NgConfigLoader.NgImageConfig.Images
					.Where(y => y.Hash == v.ToString())
					.FirstOrDefault();
				if(ng != null) {
					// Ng.NgConfig.NgConfigLoader.RemoveNgImage(ng);
				} else {
					string r = null;
					if(WpfConfig.WpfConfigLoader.SystemConfig.IsEnabledNgReasonInput) {
						var w = new Windows.ImageReasonWindow() {
							Owner = App.Current.MainWindow,
						};
						if(w.ShowDialog() ?? false) {
							r = w.ReasonText;
						}
					} else {
						r = $"スレッドから登録({ Util.TextUtil.GetStringYyyymmddHhmmss() })";
					}

					if(r != null) {
						Ng.NgConfig.NgConfigLoader.AddNgImage(
							Ng.NgData.NgImageData.FromPerceptualHash(v, r));
					}
				}
			}
		}
		private void OnCopyTextboxQuot((BindableFutaba Futaba, TextBox TextBox) e) {
			var sb = new StringBuilder();
			foreach(var s in e.TextBox.SelectedText.Replace("\r", "").Split('\n')) {
				sb.Append(">").AppendLine(s);
			}
			FutabaPostViewViewModel.Messenger.Instance.GetEvent<PubSubEvent<FutabaPostViewViewModel.AppendTextMessage>>()
				.Publish(new FutabaPostViewViewModel.AppendTextMessage(e.Futaba.Url, sb.ToString()));
			this.PostViewVisibility.Value = Visibility.Visible;
		}

		private void OnCopyTextboxSearch((BindableFutaba Futaba, TextBox TextBox) e) {
			var s = e.TextBox.SelectedText
				.Replace("\r", "")
				.Split('\n')
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.FirstOrDefault();
			if(s != null) {
				e.Futaba.FilterText.Value = s;

				// TODO: 検索ボックス展開処理
			}
		}

		private void OnCopyTextboxNg((BindableFutaba Futaba, TextBox TextBox) e) {
			if(!string.IsNullOrEmpty(e.TextBox.SelectedText)) {
				Ng.NgConfig.NgConfigLoader.AddThreadNgWord(e.TextBox.SelectedText);
			}
		}

		private void OnCopyTextboxCopy((BindableFutaba Futaba, TextBox TextBox) e) {
			e.TextBox.Copy();
		}

		private void OnCopyTextboxGoogle((BindableFutaba Futaba, TextBox TextBox) e) {
			var s = e.TextBox.SelectedText
				.Replace("\r", "")
				.Split('\n')
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.FirstOrDefault();
			if(s != null) {
				WpfUtil.PlatformUtil.StartBrowser(new Uri(
					$"https://www.google.com/search?q={ System.Web.HttpUtility.UrlEncode(s) }"));
			}
		}
	}
}