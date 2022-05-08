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
using Prism.Navigation;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Yarukizero.Net.MakiMoki.Data;
using Yarukizero.Net.MakiMoki.Wpf.Canvas98.Controls;
using Yarukizero.Net.MakiMoki.Wpf.Model;
using Yarukizero.Net.MakiMoki.Wpf.Reactive;

namespace Yarukizero.Net.MakiMoki.Wpf.ViewModels {
	class FutabaThreadResViewerViewModel : BindableBase, IDisposable, IDestructible {
		internal class Messenger : EventAggregator {
			public static Messenger Instance { get; } = new Messenger();
		}
		internal class BaseCommandMessage {
			public Model.BindableFutaba Futaba { get; }

			public BaseCommandMessage(Model.BindableFutaba futaba) {
				this.Futaba = futaba;
			}
		}
		internal class ThreadUpdateCommandMessage : BaseCommandMessage {
			public ThreadUpdateCommandMessage(Model.BindableFutaba futaba) : base(futaba) { }
		}
		internal class ThreadSearchCommandMessage : BaseCommandMessage {
			public ThreadSearchCommandMessage(Model.BindableFutaba futaba) : base(futaba) { }
		}
		internal class ThreadOpenTegakiCommandMessage : BaseCommandMessage {
			public ThreadOpenTegakiCommandMessage(Model.BindableFutaba futaba) : base(futaba) { }
		}
		internal class ThreadOpenPostCommandMessage : BaseCommandMessage {
			public ThreadOpenPostCommandMessage(Model.BindableFutaba futaba) : base(futaba) { }
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

		public MakiMokiCommand LoadedCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand<RoutedPropertyChangedEventArgs<Model.IFutabaViewerContents>> ContentsChangedCommand { get; }
			= new MakiMokiCommand<RoutedPropertyChangedEventArgs<Model.IFutabaViewerContents>>();

		public MakiMokiCommand<RoutedEventArgs> PostClickCommand { get; } = new MakiMokiCommand<RoutedEventArgs>();

		public MakiMokiCommand<RoutedEventArgs> TegakiClickCommand { get; } = new MakiMokiCommand<RoutedEventArgs>();

		public MakiMokiCommand<RoutedEventArgs> ThreadUpdateCommand { get; } = new MakiMokiCommand<RoutedEventArgs>();

		public MakiMokiCommand<RoutedEventArgs> ImageClickCommand { get; } = new MakiMokiCommand<RoutedEventArgs>();
		public MakiMokiCommand<PlatformData.HyperLinkEventArgs> LinkClickCommand { get; } = new MakiMokiCommand<PlatformData.HyperLinkEventArgs>();
		public MakiMokiCommand<PlatformData.QuotClickEventArgs> QuotClickCommand { get; } = new MakiMokiCommand<PlatformData.QuotClickEventArgs>();

		public MakiMokiCommand<MouseButtonEventArgs> ThreadImageMouseDownCommand { get; }
			= new MakiMokiCommand<MouseButtonEventArgs>();
		public MakiMokiCommand<MouseButtonEventArgs> ThreadImageClickCommand { get; }
			= new MakiMokiCommand<MouseButtonEventArgs>();

		public ReactiveProperty<Visibility> PostViewVisibility { get; }
			= new ReactiveProperty<Visibility>(Visibility.Hidden);
		public ReactiveProperty<Visibility> TegakiViewVisibility { get; }
			= new ReactiveProperty<Visibility>(Visibility.Hidden);

		public ReactiveProperty<object> Canvas98BookmarkletToken { get; } = new ReactiveProperty<object>(DateTime.Now);


		public ReactiveProperty<string> FilterText { get; } = new ReactiveProperty<string>("");

		public MakiMokiCommand<TextChangedEventArgs> FilterTextChangedCommand { get; } = new MakiMokiCommand<TextChangedEventArgs>();

		public MakiMokiCommand<Model.IFutabaViewerContents> ThreadResHamburgerItemCopyUrlClickCommand { get; } = new MakiMokiCommand<Model.IFutabaViewerContents>();
		public MakiMokiCommand<Model.IFutabaViewerContents> ThreadResHamburgerItemOpenUrlClickCommand { get; } = new MakiMokiCommand<Model.IFutabaViewerContents>();

		public MakiMokiCommand<Model.BindableFutaba> WheelUpdateCommand { get; } = new MakiMokiCommand<BindableFutaba>();

		public MakiMokiCommand<RoutedEventArgs> PaletteButtonCopyCommand { get; } = new MakiMokiCommand<RoutedEventArgs>();
		public MakiMokiCommand<RoutedEventArgs> PaletteButtonSoudaneCommand { get; } = new MakiMokiCommand<RoutedEventArgs>();
		public MakiMokiCommand<RoutedEventArgs> PaletteButtonDelCommand { get; } = new MakiMokiCommand<RoutedEventArgs>();

		public MakiMokiCommand<BindableFutaba> MenuItemFullUpdateClickCommand { get; } = new MakiMokiCommand<BindableFutaba>();
		public MakiMokiCommand<Model.BindableFutabaResItem> MenuItemCopyClickCommand { get; } = new MakiMokiCommand<Model.BindableFutabaResItem>();
		public MakiMokiCommand<Model.BindableFutabaResItem> MenuItemReplyClickCommand { get; } = new MakiMokiCommand<Model.BindableFutabaResItem>();
		public MakiMokiCommand<Model.BindableFutabaResItem> MenuItemReplyResNoClickCommand { get; } = new MakiMokiCommand<Model.BindableFutabaResItem>();
		public MakiMokiCommand<Model.BindableFutabaResItem> MenuItemReplyImageNameoClickCommand { get; } = new MakiMokiCommand<Model.BindableFutabaResItem>();
		public MakiMokiCommand<Model.BindableFutabaResItem> MenuItemSoudaneClickCommand { get; } = new MakiMokiCommand<Model.BindableFutabaResItem>();
		public MakiMokiCommand<Model.BindableFutabaResItem> MenuItemDelClickCommand { get; } = new MakiMokiCommand<Model.BindableFutabaResItem>();
		public MakiMokiCommand<Model.BindableFutabaResItem> MenuItemDeleteClickCommand { get; } = new MakiMokiCommand<Model.BindableFutabaResItem>();
		public MakiMokiCommand<Model.BindableFutabaResItem> MenuItemDeleteImageClickCommand { get; } = new MakiMokiCommand<Model.BindableFutabaResItem>();
		public MakiMokiCommand<Model.BindableFutabaResItem> MenuItemWatchImageCommand { get; } = new MakiMokiCommand<Model.BindableFutabaResItem>();
		public MakiMokiCommand<Model.BindableFutabaResItem> MenuItemNgImageCommand { get; } = new MakiMokiCommand<Model.BindableFutabaResItem>();
		public MakiMokiCommand<Model.BindableFutabaResItem> MenuItemResHiddenCommand { get; } = new MakiMokiCommand<Model.BindableFutabaResItem>();

		public MakiMokiCommand<(BindableFutaba Futaba, TextBox TextBox)> CopyTextboxQuotCommand { get; } = new MakiMokiCommand<(BindableFutaba Futaba, TextBox TextBox)>();
		public MakiMokiCommand<(BindableFutaba Futaba, TextBox TextBox)> CopyTextboxSearchCommand { get; } = new MakiMokiCommand<(BindableFutaba Futaba, TextBox TextBox)>();
		public MakiMokiCommand<(BindableFutaba Futaba, TextBox TextBox)> CopyTextboxNgCommand { get; } = new MakiMokiCommand<(BindableFutaba Futaba, TextBox TextBox)>();
		public MakiMokiCommand<(BindableFutaba Futaba, TextBox TextBox)> CopyTextboxCopyCommand { get; } = new MakiMokiCommand<(BindableFutaba Futaba, TextBox TextBox)>();
		public MakiMokiCommand<(BindableFutaba Futaba, TextBox TextBox)> CopyTextboxGoogleCommand { get; } = new MakiMokiCommand<(BindableFutaba Futaba, TextBox TextBox)>();

		public MakiMokiCommand<Model.BindableFutaba> KeyBindingUpdateCommand { get; } = new MakiMokiCommand<BindableFutaba>();
		public MakiMokiCommand<Model.BindableFutaba> KeyBindingSearchCommand { get; } = new MakiMokiCommand<BindableFutaba>();
		public MakiMokiCommand<Model.BindableFutaba> KeyBindingTegakiCommand { get; } = new MakiMokiCommand<BindableFutaba>();
		public MakiMokiCommand<Model.BindableFutaba> KeyBindingPostCommand { get; } = new MakiMokiCommand<BindableFutaba>();

		public ReactiveProperty<bool> IsExecuteCanvas98 { get; } = new ReactiveProperty<bool>(false);
		public ReactiveProperty<PlatformData.UiPosition> Canvas98Position { get; }

		public ReactiveProperty<Visibility> MediaViewerVisibility { get; } = new ReactiveProperty<Visibility>(Visibility.Hidden);
		public ReactiveProperty<bool> IsEnbaledCanvas98FullScreen { get; }
		public ReactiveProperty<bool> IsEnbaledCanvas98Right { get; }
		public ReactiveProperty<bool> IsEnbaledCanvas98Bottom { get; }

		public ReactiveProperty<Visibility> GridSplitterVisibility { get; }
		public ReactiveProperty<int> GridSplitterRow { get; }
		public ReactiveProperty<int> GridSplitterRowSpan { get; }
		public ReactiveProperty<int> GridSplitterColumn { get; }
		public ReactiveProperty<int> GridSplitterColumnSpan { get; }
		public ReactiveProperty<double> GridSplitterWidth { get; }
		public ReactiveProperty<double> GridSplitterHeight { get; }


		public ReactiveProperty<int> ListBoxRowSpan { get; }
		public ReactiveProperty<int> ListBoxColumnSpan { get; }
		public ReactiveProperty<Visibility> Canvas98FullScreenRegionVisibility { get; }
		public ReactiveProperty<Visibility> Canvas98RightRegionVisibility { get; }
		public ReactiveProperty<Visibility> Canvas98BottomRegionVisibility { get; }
		public ReactiveProperty<double> Canvas98RightGridMinWidth { get; }
		public ReactiveProperty<double> Canvas98BottomGridMinHeight { get; }


		public MakiMokiCommand<Canvas98.Controls.FutabaCanvas98View.RoutedSucessEventArgs> Canvas98SuccessedCommand { get; }
			= new MakiMokiCommand<Canvas98.Controls.FutabaCanvas98View.RoutedSucessEventArgs>();

		private IDisposable MediaViewrSubscriber { get; }
		private IDisposable CloseToSubscriber { get; }
		private bool isThreadImageClicking = false;
		private readonly Action<PlatformData.WpfConfig> systemConfigNotifyAction;
		private readonly Action<Canvas98.Canvas98Data.Canvas98Bookmarklet> canvas98BookmarkletNotifyAction;
		public ReadOnlyReactiveProperty<IRegionManager> ThreadViewRegionManager { get; }
		public ReactiveProperty<string> MediaViewerRegion { get; }
		public ReactiveProperty<string> Canvas98FullScreenRegion { get; }
		public ReactiveProperty<string> Canvas98RightRegion { get; }
		public ReactiveProperty<string> Canvas98BottomRegion { get; }

		public FutabaThreadResViewerViewModel(IRegionManager regionManager) {
			MediaViewrSubscriber = ViewModels.FutabaMediaViewerViewModel.Messenger.Instance
				.GetEvent<PubSubEvent<ViewModels.FutabaMediaViewerViewModel.ViewerCloseMessage>>()
				.Subscribe(x => {
					if(x.RegionName == this.MediaViewerRegion.Value) {
						this.MediaViewerVisibility.Value = Visibility.Hidden;
					}
				});
			CloseToSubscriber = Canvas98.ViewModels.FutabaCanvas98ViewViewModel.Messenger.Instance
				.GetEvent<PubSubEvent<Canvas98.ViewModels.FutabaCanvas98ViewViewModel.CloseTo>>()
				.Subscribe(_ => {
					IsExecuteCanvas98.Value = false;
				});
			ThreadViewRegionManager = new ReactiveProperty<IRegionManager>(regionManager.CreateRegionManager())
				.ToReadOnlyReactiveProperty();
			var id = Guid.NewGuid().ToString();
			MediaViewerRegion = new ReactiveProperty<string>($"MediaViewerRegion-{ id }");
			Canvas98FullScreenRegion = new ReactiveProperty<string>($"Canvas98FullScreenRegion-{ id }");
			Canvas98RightRegion = new ReactiveProperty<string>($"Canvas98RightRegion-{ id }");
			Canvas98BottomRegion = new ReactiveProperty<string>($"Canvas98BottomRegion-{ id }");

			LoadedCommand.Subscribe(_ => OnLoaded());
			ContentsChangedCommand.Subscribe(x => OnContentsChanged(x));

			FilterTextChangedCommand.Subscribe(x => OnFilterTextChanged(x));

			ThreadImageMouseDownCommand.Subscribe(x => OnThreadImageMouseDown(x));
			ThreadImageClickCommand.Subscribe(x => OnThreadImageClick(x));
			ThreadUpdateCommand.Subscribe(x => OnThreadUpdateClick(x));
			PostClickCommand.Subscribe(x => OnPostClick((x.Source as FrameworkElement)?.DataContext as Model.BindableFutaba));
			TegakiClickCommand.Subscribe(x => OnTegakiClick((x.Source as FrameworkElement)?.DataContext as Model.BindableFutaba));
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
			MenuItemWatchImageCommand.Subscribe(x => OnMenuItemWatchImage(x));
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
			KeyBindingTegakiCommand.Subscribe(x => OnTegakiClick(x));
			KeyBindingPostCommand.Subscribe(x => UpdatePost(x));

			Canvas98Position = new ReactiveProperty<PlatformData.UiPosition>(WpfConfig.WpfConfigLoader.SystemConfig.Canvas98Position);
			IsEnbaledCanvas98FullScreen = new[] {
				IsExecuteCanvas98,
				Canvas98Position.Select(x => x == PlatformData.UiPosition.Default),
			}.CombineLatestValuesAreAllTrue()
				.ToReactiveProperty();
			IsEnbaledCanvas98Bottom = new[] {
				IsExecuteCanvas98,
				Canvas98Position.Select(x => x == PlatformData.UiPosition.Bottom),
			}.CombineLatestValuesAreAllTrue()
				.ToReactiveProperty();
			IsEnbaledCanvas98Right = new[] {
				IsExecuteCanvas98,
				Canvas98Position.Select(x => x == PlatformData.UiPosition.Right),
			}.CombineLatestValuesAreAllTrue()
				.ToReactiveProperty();
			Canvas98RightGridMinWidth = IsEnbaledCanvas98Right
				.Select(x => x ? 640d : 0d)
				.ToReactiveProperty();
			Canvas98BottomGridMinHeight = IsEnbaledCanvas98Bottom
				.Select(x => x ? 480d : 0d)
				.ToReactiveProperty();

			GridSplitterVisibility = new[] {
				IsEnbaledCanvas98Bottom,
				IsEnbaledCanvas98Right,
			}.CombineLatestValuesAreAllTrue()
				.Select(x => x ? Visibility.Visible : Visibility.Collapsed)
				.ToReactiveProperty();
			GridSplitterRow = IsEnbaledCanvas98Right.Select(x => x ? 0 : 1).ToReactiveProperty();
			GridSplitterRowSpan = IsEnbaledCanvas98Right.Select(x => x ? 3 : 1).ToReactiveProperty();
			GridSplitterColumn = IsEnbaledCanvas98Bottom.Select(x => x ? 0 : 1).ToReactiveProperty();
			GridSplitterColumnSpan = IsEnbaledCanvas98Bottom.Select(x => x ? 3 : 1).ToReactiveProperty();
			GridSplitterWidth = IsEnbaledCanvas98Right.Select(x => x ? 5d : double.PositiveInfinity).ToReactiveProperty();
			GridSplitterHeight = IsEnbaledCanvas98Bottom.Select(x => x ? 5d : double.PositiveInfinity).ToReactiveProperty();

			ListBoxRowSpan = IsEnbaledCanvas98Bottom.Select(x => x ? 1 : 3).ToReactiveProperty();
			ListBoxColumnSpan = IsEnbaledCanvas98Right.Select(x => x ? 1 : 3).ToReactiveProperty();
			Canvas98FullScreenRegionVisibility = IsEnbaledCanvas98FullScreen.Select(x => x ? Visibility.Visible : Visibility.Hidden).ToReactiveProperty();
			Canvas98BottomRegionVisibility = IsEnbaledCanvas98Bottom.Select(x => x ? Visibility.Visible : Visibility.Hidden).ToReactiveProperty();
			Canvas98RightRegionVisibility = IsEnbaledCanvas98Right.Select(x => x ? Visibility.Visible : Visibility.Hidden).ToReactiveProperty();

			systemConfigNotifyAction = (x) => {
				if(Canvas98Position.Value != x.Canvas98Position) {
					Canvas98Position.Value = x.Canvas98Position;
					Canvas98.ViewModels.FutabaCanvas98ViewViewModel.Messenger.Instance
						.GetEvent<PubSubEvent<Canvas98.ViewModels.FutabaCanvas98ViewViewModel.CloseTo>>()
						.Publish(new Canvas98.ViewModels.FutabaCanvas98ViewViewModel.CloseTo());
				}
			};
			canvas98BookmarkletNotifyAction = (_) => Canvas98BookmarkletToken.Value = DateTime.Now;
			WpfConfig.WpfConfigLoader.SystemConfigUpdateNotifyer.AddHandler(systemConfigNotifyAction);
			Canvas98.Canvas98Config.Canvas98ConfigLoader.BookmarkletUpdateNotifyer.AddHandler(canvas98BookmarkletNotifyAction);
			Canvas98SuccessedCommand.Subscribe(x => OnCanvas98Successed(x));
		}

		public void Dispose() {
			Helpers.AutoDisposable.GetCompositeDisposable(this).Dispose();
		}

		private void OnLoaded() {}

		private void UpdateThread(Model.BindableFutaba x) {
			if(x.IsDie.Value) {
				return;
			}

			Util.Futaba.UpdateThreadRes(x.Raw.Bord, x.Url.ThreadNo, Config.ConfigLoader.MakiMoki.FutabaThreadGetIncremental).Subscribe();
		}

		private void UpdatePost(Model.BindableFutaba x) {
			this.PostViewVisibility.Value = (this.PostViewVisibility.Value == Visibility.Hidden) ? Visibility.Visible : Visibility.Hidden;
		}

		private void OnImageClick(RoutedEventArgs e) {
			if((e.Source is FrameworkElement o) && (o.DataContext is Model.BindableFutabaResItem it)) {
				if(it.ThumbDisplay.Value ?? true) {
					this.OpenMediaViewer(PlatformData.FutabaMedia.FromFutabaUrl(
						it.Raw.Value.Url, it.Raw.Value.ResItem.Res));
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
						this.OpenMediaViewer(PlatformData.FutabaMedia.FromExternalUrl(u));
						goto end;
					}
				}
			}
			// ふたばのリンク
			foreach(var b in Config.ConfigLoader.Board.Boards) {
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
			Canvas98.ViewModels.FutabaCanvas98ViewViewModel.Messenger.Instance
				.GetEvent<PubSubEvent<Canvas98.ViewModels.FutabaCanvas98ViewViewModel.CloseTo>>()
				.Publish(new Canvas98.ViewModels.FutabaCanvas98ViewViewModel.CloseTo());
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
							this.OpenMediaViewer(
								PlatformData.FutabaMedia.FromFutabaUrl(
									it.Raw.Value.Url, it.Raw.Value.ResItem.Res));
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

		private void OnTegakiClick(Model.BindableFutaba x) {
			if(!Canvas98.Canvas98Util.Util.IsEnabledCanvas98()) {
				return;
			}

			if(x != null) {
				this.IsExecuteCanvas98.Value = true;
				var param = new NavigationParameters {
					{ typeof(Data.UrlContext).FullName, x.Url }
				};
				switch(this.Canvas98Position.Value) {
				case PlatformData.UiPosition.Default:
					this.ThreadViewRegionManager.Value.RequestNavigate(
						Canvas98FullScreenRegion.Value,
						nameof(Canvas98.Controls.FutabaCanvas98View),
						param);
					return;
				case PlatformData.UiPosition.Right:
					this.ThreadViewRegionManager.Value.RequestNavigate(
						Canvas98RightRegion.Value,
						nameof(Canvas98.Controls.FutabaCanvas98View),
						param);
					return;
				case PlatformData.UiPosition.Bottom:
					this.ThreadViewRegionManager.Value.RequestNavigate(
						Canvas98BottomRegion.Value,
						nameof(Canvas98.Controls.FutabaCanvas98View),
						param);
					return;
				}
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
			if(item.Bord.Value.Extra?.Name ?? true) {
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
							Util.Futaba.PutInformation(new Information($"そうだねx{ i }", item.Parent.Value));
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
					Util.Futaba.PutInformation(new Information(m, item.Parent.Value));
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
						Util.Futaba.PutInformation(new Information("del送信", item.Parent.Value));
					} else {
						Util.Futaba.PutInformation(new Information(x.Message, item.Parent.Value));
					}
				});
		}

		private void PostItemDelete(Model.BindableFutabaResItem item, bool imageOnlyDel) {
			Util.Futaba.PostDeleteThreadRes(item.Bord.Value, item.Raw.Value.ResItem.No, imageOnlyDel, Config.ConfigLoader.FutabaApi.SavedPassword)
				.ObserveOn(UIDispatcherScheduler.Default)
				.Subscribe(x => {
					if(x.Successed) {
						Util.Futaba.PutInformation(new Information("削除しました", item.Parent.Value));
						Util.Futaba.UpdateThreadRes(item.Bord.Value, item.Raw.Value.Url.ThreadNo).Subscribe();
					} else {
						Util.Futaba.PutInformation(new Information(x.Message, item.Parent.Value));
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

		private void OnMenuItemWatchImage(Model.BindableFutabaResItem x) {
			if(x.ThumbDisplay.Value.HasValue) {
				void f(ulong v) {
					var watch = Ng.NgConfig.NgConfigLoader.WatchImageConfig.Images
						.Where(y => y.Hash == v.ToString())
						.FirstOrDefault();
					if(watch != null) {
						// Ng.NgConfig.NgConfigLoader.RemoveNgImage(ng);
					} else {
						string r = null;
						var w = new Windows.ImageReasonWindow() {
							Owner = App.Current.MainWindow,
						};
						if(w.ShowDialog() ?? false) {
							r = w.ReasonText;
						}

						if(r != null) {
							Ng.NgConfig.NgConfigLoader.AddWatchImage(
								Ng.NgData.NgImageData.FromPerceptualHash(v, r));
						}
					}
				}

				if(x.ThumbHash.Value.HasValue) {
					f(x.ThumbHash.Value.Value);
				} else {
					x.LoadBitmapSource()
						.ObserveOn(UIDispatcherScheduler.Default)
						.Select(y => (Pixels: WpfUtil.ImageUtil.CreatePixelsBytes(y), Width: y.PixelWidth, Height: y.PixelHeight))
						.ObserveOn(System.Reactive.Concurrency.ThreadPoolScheduler.Instance)
						.Select(y => Ng.NgUtil.PerceptualHash.CalculateHash(y.Pixels, y.Width, y.Height, 32))
						.ObserveOn(UIDispatcherScheduler.Default)
						.Subscribe(y => {
							f(y);
						});
				}
			}
		}

		private void OnMenuItemNgImage(Model.BindableFutabaResItem x) {
			void hash(ulong v) {
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

			if(x.ThumbHash.Value.HasValue) {
				hash(x.ThumbHash.Value.Value);
			} else {
				x.LoadBitmapSource()
					.Select(y => (Pixels: WpfUtil.ImageUtil.CreatePixelsBytes(y), Width: y.PixelWidth, Height: y.PixelHeight))
					.ObserveOn(System.Reactive.Concurrency.ThreadPoolScheduler.Instance)
					.Select(y => Ng.NgUtil.PerceptualHash.CalculateHash(y.Pixels, y.Width, y.Height, 32))
					.ObserveOn(UIDispatcherScheduler.Default)
					.Subscribe(y => {
						hash(y);
					});
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

		private void OnCanvas98Successed(FutabaCanvas98View.RoutedSucessEventArgs e) {
			if(e.FormData != null) {
				var b = Config.ConfigLoader.Board.Boards.Where(x => x.Url == e.Url.BaseUrl).FirstOrDefault();
				if(b != null) {
					Config.ConfigLoader.UpdateFutabaInputData(
						b,
						e.FormData.Subject, e.FormData.Name,
						e.FormData.Email, e.FormData.Password);
					Util.Futaba.UpdateThreadRes(
						b,
						e.Url.ThreadNo,
						Config.ConfigLoader.MakiMoki.FutabaThreadGetIncremental).Subscribe();
				}
			}
		}

		private void OpenMediaViewer(PlatformData.FutabaMedia media) {
			this.MediaViewerVisibility.Value = Visibility.Visible;
			this.ThreadViewRegionManager.Value.RequestNavigate(
				this.MediaViewerRegion.Value,
				nameof(Controls.FutabaMediaViewer),
				new NavigationParameters {
					{
						typeof(FutabaMediaViewerViewModel.NavigationParameters).FullName,
						new FutabaMediaViewerViewModel.NavigationParameters(
							this.MediaViewerRegion.Value,
							media)
					}
				});
		}

		public void Destroy() {
			this.Dispose();
		}
	}
}