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

namespace Yarukizero.Net.MakiMoki.Wpf.ViewModels {
	class FutabaCatalogViewerViewModel : BindableBase, IDisposable {
		internal class Messenger : EventAggregator {
			public static Messenger Instance { get; } = new Messenger();
		}
		internal class CatalogSortContextMenuMessage { }

		internal class CatalogListboxUpdatedMessage { }
		internal class AppendUploadFileMessage {
			public string FileName { get; }

			public AppendUploadFileMessage(string fileName) {
				this.FileName = fileName;
			}
		}


		private CompositeDisposable Disposable { get; } = new CompositeDisposable();


		public ReactiveCommand<RoutedPropertyChangedEventArgs<Model.IFutabaViewerContents>> ContentsChangedCommand { get; } 
			= new ReactiveCommand<RoutedPropertyChangedEventArgs<Model.IFutabaViewerContents>>();

		public ReactiveCommand<RoutedEventArgs> CatalogUpdateClickCommand { get; } = new ReactiveCommand<RoutedEventArgs>();
		public ReactiveCommand CatalogSortClickCommand { get; } = new ReactiveCommand();
		public ReactiveCommand<RoutedEventArgs> CatalogListWrapClickCommand { get; } = new ReactiveCommand<RoutedEventArgs>();
		public ReactiveCommand<RoutedEventArgs> PostClickCommand { get; } = new ReactiveCommand<RoutedEventArgs>();

		public ReactiveCommand<DragEventArgs> ImageDragOverCommand { get; } = new ReactiveCommand<DragEventArgs>();
		public ReactiveCommand<DragEventArgs> ImageDropCommand { get; } = new ReactiveCommand<DragEventArgs>();
		public ReactiveCommand<MouseButtonEventArgs> OpenUploadCommand { get; } = new ReactiveCommand<MouseButtonEventArgs>();
		public ReactiveCommand<DragEventArgs> UploadDragOverpCommand { get; } = new ReactiveCommand<DragEventArgs>();
		public ReactiveCommand<DragEventArgs> UploadDroppCommand { get; } = new ReactiveCommand<DragEventArgs>();

		public ReactiveCommand<RoutedEventArgs> PostViewPostCommand { get; } = new ReactiveCommand<RoutedEventArgs>();


		public ReactiveCommand<MouseButtonEventArgs> CatalogItemMouseDownCommand { get; }
			= new ReactiveCommand<MouseButtonEventArgs>();
		public ReactiveCommand<MouseButtonEventArgs> CatalogItemClickCommand { get; }
			= new ReactiveCommand<MouseButtonEventArgs>();

		public ReactiveProperty<Visibility> PostViewVisibility { get; }
			= new ReactiveProperty<Visibility>(Visibility.Hidden);

		public ReactiveProperty<string> FilterText { get; } = new ReactiveProperty<string>("");

		public ReactiveCommand<TextChangedEventArgs> FilterTextChangedCommand { get; } = new ReactiveCommand<TextChangedEventArgs>();

		public ReactiveCommand<RoutedEventArgs> CatalogSortItemCatalogClickCommand { get; } = new ReactiveCommand<RoutedEventArgs>();
		public ReactiveCommand<RoutedEventArgs> CatalogSortItemNewClickCommand { get; } = new ReactiveCommand<RoutedEventArgs>();
		public ReactiveCommand<RoutedEventArgs> CatalogSortItemOldClickCommand { get; } = new ReactiveCommand<RoutedEventArgs>();
		public ReactiveCommand<RoutedEventArgs> CatalogSortItemManyClickCommand { get; } = new ReactiveCommand<RoutedEventArgs>();
		public ReactiveCommand<RoutedEventArgs> CatalogSortItemMomentumClickCommand { get; } = new ReactiveCommand<RoutedEventArgs>();
		public ReactiveCommand<RoutedEventArgs> CatalogSortItemFewClickCommand { get; } = new ReactiveCommand<RoutedEventArgs>();
		public ReactiveCommand<RoutedEventArgs> CatalogSortItemSoudaneClickCommand { get; } = new ReactiveCommand<RoutedEventArgs>();

		public ReactiveCommand<RoutedEventArgs> CatalogMenuItemDelClickCommand { get; } = new ReactiveCommand<RoutedEventArgs>();

		private bool isCatalogItemClicking = false;
		private bool isThreadImageClicking = false;

		public FutabaCatalogViewerViewModel() {
			ContentsChangedCommand.Subscribe(x => OnContentsChanged(x));

			FilterTextChangedCommand.Subscribe(x => OnFilterTextChanged(x));

			CatalogUpdateClickCommand.Subscribe(x => OnCatalogUpdateClick(x));
			CatalogSortItemCatalogClickCommand.Subscribe(x => OnCatalogSortItemCatalogClick(x));
			CatalogListWrapClickCommand.Subscribe(x => OnCatalogListWrapClick(x));
			CatalogSortItemNewClickCommand.Subscribe(x => OnCatalogSortItemNewClick(x));
			CatalogSortItemOldClickCommand.Subscribe(x => OnCatalogSortItemOldClick(x));
			CatalogSortItemManyClickCommand.Subscribe(x => OnCatalogSortItemManyClick(x));
			CatalogSortItemMomentumClickCommand.Subscribe(x => OnCatalogSortItemMomentumClick(x));
			CatalogSortItemFewClickCommand.Subscribe(x => OnCatalogSortItemFewClick(x));
			CatalogSortItemSoudaneClickCommand.Subscribe(x => OnCatalogSortItemSoudaneClick(x));
			CatalogItemMouseDownCommand.Subscribe(x => OnCatalogItemMouseDown(x));
			CatalogItemClickCommand.Subscribe(x => OnCatalogClick(x));
			PostClickCommand.Subscribe(x => OnPostClick((x.Source as FrameworkElement)?.DataContext as Model.BindableFutaba));
			OpenUploadCommand.Subscribe(x => OnOpenUpload(x));
			ImageDragOverCommand.Subscribe(x => OnImageDragOver(x));
			ImageDropCommand.Subscribe(x => OnImageDrop(x));
			UploadDragOverpCommand.Subscribe(x => OnUploadDragOver(x));
			UploadDroppCommand.Subscribe(x => OnUploadDrop(x));
			PostViewPostCommand.Subscribe(x => OnPostViewPostClick((x.Source as FrameworkElement)?.DataContext as Model.BindableFutaba));

			CatalogMenuItemDelClickCommand.Subscribe(x => OnCatalogMenuItemDelClickCommand(x));
		}

		public void Dispose() {
			Disposable.Dispose();
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

		private void OnCatalogUpdateClick(RoutedEventArgs e) {
			if((e.Source is FrameworkElement el) && (el.DataContext is BindableFutaba bf)) {
				this.UpdateCatalog(bf);
			}
		}

		private void OnCatalogSortItemCatalogClick(RoutedEventArgs e) {
			if(e.Source is FrameworkElement el && el.DataContext is BindableFutaba bf) {
				bf.CatalogSortItem.Value = Data.CatalogSort.Catalog;
				this.UpdateCatalog(bf);
			}
		}

		private void OnCatalogListWrapClick(RoutedEventArgs e) {
			if(e.Source is FrameworkElement el && el.DataContext is BindableFutaba bf) {
				bf.CatalogListMode.Value = !bf.CatalogListMode.Value;
			}
		}
		private void OnCatalogSortItemNewClick(RoutedEventArgs e) {
			if(e.Source is FrameworkElement el && el.DataContext is BindableFutaba bf) {
				bf.CatalogSortItem.Value = Data.CatalogSort.New;
				this.UpdateCatalog(bf);
			}
		}
		private void OnCatalogSortItemOldClick(RoutedEventArgs e) {
			if(e.Source is FrameworkElement el && el.DataContext is BindableFutaba bf) {
				bf.CatalogSortItem.Value = Data.CatalogSort.Old;
				this.UpdateCatalog(bf);
			}
		}
		private void OnCatalogSortItemManyClick(RoutedEventArgs e) {
			if(e.Source is FrameworkElement el && el.DataContext is BindableFutaba bf) {
				bf.CatalogSortItem.Value = Data.CatalogSort.Many;
				this.UpdateCatalog(bf);
			}
		}
		private void OnCatalogSortItemMomentumClick(RoutedEventArgs e) {
			if(e.Source is FrameworkElement el && el.DataContext is BindableFutaba bf) {
				bf.CatalogSortItem.Value = Data.CatalogSort.Momentum;
				this.UpdateCatalog(bf);
			}
		}
		private void OnCatalogSortItemFewClick(RoutedEventArgs e) {
			if(e.Source is FrameworkElement el && el.DataContext is BindableFutaba bf) {
				bf.CatalogSortItem.Value = Data.CatalogSort.Few;
				this.UpdateCatalog(bf);
			}
		}
		private void OnCatalogSortItemSoudaneClick(RoutedEventArgs e) {
			if(e.Source is FrameworkElement el && el.DataContext is BindableFutaba bf) {
				bf.CatalogSortItem.Value = Data.CatalogSort.Soudane;
				this.UpdateCatalog(bf);
			}
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
							Util.Futaba.UpdateThreadRes(it.Bord.Value, it.ThreadResNo.Value).Subscribe(
								x => {
									if(x != null) {
										MainWindowViewModel.Messenger.Instance.GetEvent<PubSubEvent<MainWindowViewModel.CurrentTabChanged>>()
											.Publish(new MainWindowViewModel.CurrentTabChanged(x));
									}
								});
							this.isCatalogItemClicking = false;
						}
						e.Handled = true;
						break;
					case MouseButton.Middle:
						if(this.isCatalogItemClicking) {
							// TODO: そのうちこっちは裏で開くように返れたらいいな
							Util.Futaba.UpdateThreadRes(it.Bord.Value, it.ThreadResNo.Value).Subscribe(
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

		private void OnImageDragOver(DragEventArgs e) {
			if(IsValidDragFile(e)) {
				e.Effects = DragDropEffects.Copy;
			} else {
				e.Effects = DragDropEffects.None; // 機能していないけど今度調べる
			}

			e.Handled = true;
		}

		private void OnImageDrop(DragEventArgs e) {
			if((e.Source as FrameworkElement)?.DataContext is Model.BindableFutaba f) {
				if(IsValidDragFile(e) && e.Data.GetData(DataFormats.FileDrop) is string[] files) {
					f.PostData.Value.ImagePath.Value = files[0];
				} else {
					MessageBox.Show("未対応のファイル"); // TODO: メッセージをいい感じにする
				}
			}
		}

		private async void OnOpenUpload(MouseButtonEventArgs e) {
			if(e.Source is FrameworkElement o && o.DataContext is Model.BindableFutaba f) {
				if((e.ClickCount == 1) && (VisualTreeHelper.HitTest(o, e.GetPosition(o)) != null)) {
					switch(e.ChangedButton) {
					case MouseButton.Left:
						try {
							Application.Current.MainWindow.IsEnabled = false;
							var ext = Config.ConfigLoader.Mime.Types.Select(x => x.Ext);
							var ofd = new Microsoft.Win32.OpenFileDialog() {
								Filter = "ふたば画像ファイル|"
									+ string.Join(";", ext.Select(x => "*" + x).ToArray())
									+ "|すべてのファイル|*.*"
							};
							e.Handled = true;
							if(ofd.ShowDialog() ?? false) {
								Util.Futaba.UploadUp2(ofd.FileName, f.PostData.Value.Password.Value)
									.Subscribe(x => {
										if(x.Successed) {
											Messenger.Instance.GetEvent<PubSubEvent<AppendUploadFileMessage>>()
												.Publish(new AppendUploadFileMessage(x.FileNameOrMessage));
										} else {
											MessageBox.Show(x.FileNameOrMessage);
										}
									});
								// ダイアログをダブルクリックで選択するとウィンドウに当たり判定がいくので
								// 一度待つ
								await Task.Delay(1);
							}
						}
						finally {
							Application.Current.MainWindow.IsEnabled = true;
						}
						break;
					}
				}
			}
		}

		private void OnUploadDragOver(DragEventArgs e) {
			if(IsValidDragFile(e)) {
				e.Effects = DragDropEffects.Copy;
			} else {
				e.Effects = DragDropEffects.None; // 機能していないけど今度調べる
			}

			e.Handled = true;
		}

		private void OnUploadDrop(DragEventArgs e) {
			if((e.Source as FrameworkElement)?.DataContext is Model.BindableFutaba f) {
				if(IsValidDragFile(e) && e.Data.GetData(DataFormats.FileDrop) is string[] files) {
					Util.Futaba.UploadUp2(files[0], f.PostData.Value.Password.Value)
						.Subscribe(x => {
							if(x.Successed) {
								Messenger.Instance.GetEvent<PubSubEvent<AppendUploadFileMessage>>()
									.Publish(new AppendUploadFileMessage(x.FileNameOrMessage));
							} else {
								MessageBox.Show(x.FileNameOrMessage);
							}
						});
				} else {
					MessageBox.Show("未対応のファイル"); // TODO: メッセージをいい感じにする
				}
			}
		}

		private bool IsValidDragFile(DragEventArgs e) {
			var ext = Config.ConfigLoader.Mime.Types.Select(x => x.Ext);
			return (e.Data.GetDataPresent(DataFormats.FileDrop, false)
				&& e.Data.GetData(System.Windows.DataFormats.FileDrop) is string[] files
				&& (files.Length == 1)
				&& ext.Contains(Path.GetExtension(files[0]).ToLower()));
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
								Util.Futaba.UpdateThreadRes(x.Raw.Bord, y.NextOrMessage).Subscribe();
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
							Util.Futaba.UpdateThreadRes(x.Raw.Bord, x.Url.ThreadNo).Subscribe();
						} else {
							// TODO: あとでいい感じにする
							MessageBox.Show(y.Message);
						}
					});
				}
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
						var m = x.Message;
						if(x.Successed) {
						}
						// TODO: いい感じにする
						MessageBox.Show(m);
					});
			}
		}
	}
}