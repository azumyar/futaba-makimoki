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
using Yarukizero.Net.MakiMoki.Data;
using Yarukizero.Net.MakiMoki.Wpf.Controls;
using Yarukizero.Net.MakiMoki.Wpf.Model;
using Yarukizero.Net.MakiMoki.Wpf.Reactive;

namespace Yarukizero.Net.MakiMoki.Wpf.ViewModels {
	class FutabaCatalogViewerViewModel : BindableBase, IDisposable {
		internal class Messenger : EventAggregator {
			public static Messenger Instance { get; } = new Messenger();
		}
		internal class BaseCommandMessage {
			public Model.BindableFutaba Futaba { get; }

			public BaseCommandMessage(Model.BindableFutaba futaba) {
				this.Futaba = futaba;
			}
		}
		internal class CatalogUpdateCommandMessage : BaseCommandMessage {
			public CatalogUpdateCommandMessage(Model.BindableFutaba futaba) : base(futaba) { }
		}
		internal class CatalogSearchCommandMessage : BaseCommandMessage {
			public CatalogSearchCommandMessage(Model.BindableFutaba futaba) : base(futaba) { }
		}
		internal class CatalogModeCommandMessage : BaseCommandMessage {
			public CatalogModeCommandMessage(Model.BindableFutaba futaba) : base(futaba) { }
		}
		internal class CatalogOpenPostCommandMessage : BaseCommandMessage {
			public CatalogOpenPostCommandMessage(Model.BindableFutaba futaba) : base(futaba) { }
		}

		internal class CatalogSortContextMenuMessage { }

		internal class CatalogListboxUpdatedMessage { }


		public MakiMokiCommand<RoutedPropertyChangedEventArgs<Model.IFutabaViewerContents>> ContentsChangedCommand { get; }
			= new MakiMokiCommand<RoutedPropertyChangedEventArgs<Model.IFutabaViewerContents>>();

		public MakiMokiCommand<RoutedEventArgs> CatalogUpdateClickCommand { get; } = new MakiMokiCommand<RoutedEventArgs>();
		public MakiMokiCommand CatalogSortClickCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand<RoutedEventArgs> CatalogListWrapClickCommand { get; } = new MakiMokiCommand<RoutedEventArgs>();
		public MakiMokiCommand<RoutedEventArgs> PostClickCommand { get; } = new MakiMokiCommand<RoutedEventArgs>();


		public MakiMokiCommand<MouseButtonEventArgs> CatalogItemMouseDownCommand { get; }
			= new MakiMokiCommand<MouseButtonEventArgs>();
		public MakiMokiCommand<MouseButtonEventArgs> CatalogItemClickCommand { get; }
			= new MakiMokiCommand<MouseButtonEventArgs>();

		public ReactiveProperty<Visibility> PostViewVisibility { get; }
			= new ReactiveProperty<Visibility>(Visibility.Hidden);

		public MakiMokiCommand<TextChangedEventArgs> FilterTextChangedCommand { get; } = new MakiMokiCommand<TextChangedEventArgs>();

		public ReactiveProperty<Data.CatalogSortItem[]> CatalogSortItem { get; } = new ReactiveProperty<CatalogSortItem[]>(Data.CatalogSort.Items);
		public MakiMokiCommand<(Data.CatalogSortItem Item, Model.BindableFutaba Futaba)> CatalogSortItemClickCommand { get; } = new MakiMokiCommand<(Data.CatalogSortItem Item, Model.BindableFutaba Futaba)>();

		public MakiMokiCommand<Model.BindableFutaba> WheelUpdateCommand { get; } = new MakiMokiCommand<BindableFutaba>();

		public MakiMokiCommand<RoutedEventArgs> CatalogMenuItemDelClickCommand { get; } = new MakiMokiCommand<RoutedEventArgs>();
		public MakiMokiCommand<Model.BindableFutabaResItem> CatalogMenuItemThreadHiddenCommand { get; } = new MakiMokiCommand<Model.BindableFutabaResItem>();
		public MakiMokiCommand<Model.BindableFutabaResItem> CatalogMenuItemWatchImageCommand { get; } = new MakiMokiCommand<Model.BindableFutabaResItem>();
		public MakiMokiCommand<Model.BindableFutabaResItem> CatalogMenuItemNgImageCommand { get; } = new MakiMokiCommand<Model.BindableFutabaResItem>();


		public MakiMokiCommand<Model.BindableFutaba> KeyBindingUpdateCommand { get; } = new MakiMokiCommand<BindableFutaba>();
		public MakiMokiCommand<Model.BindableFutaba> KeyBindingSearchCommand { get; } = new MakiMokiCommand<BindableFutaba>();
		public MakiMokiCommand<Model.BindableFutaba> KeyBindingSortCommand { get; } = new MakiMokiCommand<BindableFutaba>();
		public MakiMokiCommand<Model.BindableFutaba> KeyBindingModeCommand { get; } = new MakiMokiCommand<BindableFutaba>();
		public MakiMokiCommand<Model.BindableFutaba> KeyBindingPostCommand { get; } = new MakiMokiCommand<BindableFutaba>();

		private bool isCatalogItemClicking = false;
		public FutabaCatalogViewerViewModel() {
			ContentsChangedCommand.Subscribe(x => OnContentsChanged(x));

			FilterTextChangedCommand.Subscribe(x => OnFilterTextChanged(x));
			WheelUpdateCommand.Subscribe(x => OnWheelUpdate(x));

			CatalogUpdateClickCommand.Subscribe(x => OnCatalogUpdateClick(x));
			CatalogListWrapClickCommand.Subscribe(x => OnCatalogListWrapClick(x));
			CatalogSortItemClickCommand.Subscribe(x => OnCatalogSortItemClick(x));
			CatalogItemMouseDownCommand.Subscribe(x => OnCatalogItemMouseDown(x));
			CatalogItemClickCommand.Subscribe(x => OnCatalogClick(x));
			PostClickCommand.Subscribe(x => OnPostClick((x.Source as FrameworkElement)?.DataContext as Model.BindableFutaba));

			CatalogMenuItemDelClickCommand.Subscribe(x => OnCatalogMenuItemDelClickCommand(x));
			CatalogMenuItemThreadHiddenCommand.Subscribe(x => OnCatalogMenuItemThreadHidden(x));
			CatalogMenuItemWatchImageCommand.Subscribe(x => OnCatalogMenuItemWatchImage(x));
			CatalogMenuItemNgImageCommand.Subscribe(x => OnCatalogMenuItemNgImage(x));

			KeyBindingUpdateCommand.Subscribe(x => UpdateCatalog(x));
			//KeyBindingSearchCommand.Subscribe(x => x);
			//KeyBindingSortCommand.Subscribe(x => x);
			KeyBindingModeCommand.Subscribe(x => UpdateCatalogListWrap(x));
			KeyBindingPostCommand.Subscribe(x => OnPostClick(x));
		}

		public void Dispose() {
			Helpers.AutoDisposable.GetCompositeDisposable(this).Dispose();
		}

		private void UpdateCatalog(BindableFutaba bf) {
			Util.Futaba.UpdateCatalog(bf.Raw.Bord, bf.CatalogSortItem.Value)
				.ObserveOn(UIDispatcherScheduler.Default)
				.Subscribe(async x => {
					await Task.Delay(1); // この時点ではCatalogListBoxのConverterが動いていいないので一度待つ

					Messenger.Instance.GetEvent<PubSubEvent<CatalogListboxUpdatedMessage>>()
						.Publish(new CatalogListboxUpdatedMessage());
				});
		}

		private void UpdateCatalogListWrap(BindableFutaba bf) {
			bf.CatalogListMode.Value = !bf.CatalogListMode.Value;
		}

		private void OnCatalogUpdateClick(RoutedEventArgs e) {
			if((e.Source is FrameworkElement el) && (el.DataContext is BindableFutaba bf)) {
				this.UpdateCatalog(bf);
			}
		}

		private void OnCatalogListWrapClick(RoutedEventArgs e) {
			if(e.Source is FrameworkElement el && el.DataContext is BindableFutaba bf) {
				this.UpdateCatalogListWrap(bf);
			}
		}

		private void OnCatalogSortItemClick((Data.CatalogSortItem Item, Model.BindableFutaba Futaba) e) {
			e.Futaba.CatalogSortItem.Value = e.Item;
			this.UpdateCatalog(e.Futaba);
		}

		private void OnContentsChanged(RoutedPropertyChangedEventArgs<Model.IFutabaViewerContents> e) {
			this.PostViewVisibility.Value = Visibility.Hidden;
		}

		private async void OnFilterTextChanged(TextChangedEventArgs e) {
			if((e.Source is TextBox tb) && (tb.Tag is BindableFutaba bf)) {
				var s = tb.Text.Clone().ToString();
				await Task.Delay(500);
				if(tb.Text == s) {
					bf.FilterText.Value = s;
				}
			}
		}

		private void OnWheelUpdate(Model.BindableFutaba f) {
			this.UpdateCatalog(f);
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
							Util.Futaba.UpdateThreadRes(it.Bord.Value, it.ThreadResNo.Value, Config.ConfigLoader.MakiMoki.FutabaThreadGetIncremental).Subscribe(
								x => {
									if(x.New != null) {
										MainWindowViewModel.Messenger.Instance.GetEvent<PubSubEvent<MainWindowViewModel.CurrentThreadChanged>>()
											.Publish(new MainWindowViewModel.CurrentThreadChanged(x.New));
									}
								});
							this.isCatalogItemClicking = false;
						}
						e.Handled = true;
						break;
					case MouseButton.Middle:
						if(this.isCatalogItemClicking) {
							// TODO: そのうちこっちは裏で開くように返れたらいいな
							Util.Futaba.UpdateThreadRes(it.Bord.Value, it.ThreadResNo.Value, Config.ConfigLoader.MakiMoki.FutabaThreadGetIncremental).Subscribe(
								x => {
									/*
									if(x != null) {
										MainWindowViewModel.Messenger.Instance.GetEvent<PubSubEvent<MainWindowViewModel.CurrentTabChanged>>()
											.Publish(new MainWindowViewModel.CurrentTabChanged(x));
									}
									*/
								});
							this.isCatalogItemClicking = false;
						}
						e.Handled = true;
						break;
					}
				}
			}
		}

		private void OnPostClick(Model.BindableFutaba x) {
			if(x != null) {
				this.PostViewVisibility.Value = (this.PostViewVisibility.Value == Visibility.Hidden) ? Visibility.Visible : Visibility.Hidden;
			}
		}

		private void OnCatalogMenuItemDelClickCommand(RoutedEventArgs e) {
			if((e.Source is FrameworkElement el) && (el.DataContext is Model.BindableFutabaResItem ri)) {
				// TODO: 確認ダイアログを出す
				var resNo = ri.Raw.Value.ResItem.No;
				var threadNo = ri.Parent.Value.Url.IsCatalogUrl ? resNo : ri.Parent.Value.ResItems.First().Raw.Value.ResItem.No;
				Util.Futaba.PostDel(ri.Bord.Value, threadNo, resNo)
					.ObserveOn(UIDispatcherScheduler.Default)
					.Subscribe(x => {
						if(x.Successed) {
							Util.Futaba.PutInformation(new Information("del送信", ri.ThumbSource.Value));
						} else {
							Util.Futaba.PutInformation(new Information(x.Message, ri.ThumbSource.Value));
						}
					});
			}
		}

		private void OnCatalogMenuItemThreadHidden(Model.BindableFutabaResItem x) {
			Ng.NgConfig.NgConfigLoader.AddHiddenRes(Ng.NgData.HiddenData.FromResItem(
				x.Raw.Value.Url.BaseUrl, x.Raw.Value.ResItem));
		}
		
		private void OnCatalogMenuItemWatchImage(Model.BindableFutabaResItem x) {
			if(x.OriginSource.Value != null) {
				var v = x.ThumbHash.Value ?? WpfUtil.ImageUtil.CalculatePerceptualHash(x.OriginSource.Value);
				var ng = Ng.NgConfig.NgConfigLoader.WatchImageConfig.Images
					.Where(y => y.Hash == v.ToString())
					.FirstOrDefault();
				if(ng != null) {
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
		}

		private void OnCatalogMenuItemNgImage(Model.BindableFutabaResItem x) {
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
						r = $"カタログから登録({ Util.TextUtil.GetStringYyyymmddHhmmss() })";
					}

					if(r != null) {
						Ng.NgConfig.NgConfigLoader.AddNgImage(
							Ng.NgData.NgImageData.FromPerceptualHash(v, r));
					}
				}
			}
		}
	}
}