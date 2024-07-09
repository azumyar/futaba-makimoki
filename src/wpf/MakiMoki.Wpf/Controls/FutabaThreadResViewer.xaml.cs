using Microsoft.Xaml.Behaviors;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Yarukizero.Net.MakiMoki.Wpf.Controls {
	/// <summary>
	/// FutabaViewer.xaml の相互作用ロジック
	/// </summary>
	partial class FutabaThreadResViewer : UserControl {
		public static readonly DependencyProperty ContentsProperty
			= DependencyProperty.Register(
				nameof(Contents),
				typeof(Model.IFutabaViewerContents),
				typeof(FutabaThreadResViewer),
				new PropertyMetadata(OnContentsChanged));
		public static RoutedEvent ContentsChangedEvent
			= EventManager.RegisterRoutedEvent(
				nameof(ContentsChanged),
				RoutingStrategy.Tunnel,
				typeof(RoutedPropertyChangedEventHandler<Model.IFutabaViewerContents>),
				typeof(FutabaThreadResViewer));


		public Model.IFutabaViewerContents Contents {
			get => (Model.IFutabaViewerContents)this.GetValue(ContentsProperty);
			set {
				this.SetValue(ContentsProperty, value);
			}
		}

		public event RoutedPropertyChangedEventHandler<Model.IFutabaViewerContents> ContentsChanged {
			add { AddHandler(ContentsChangedEvent, value); }
			remove { RemoveHandler(ContentsChangedEvent, value); }
		}

		private volatile bool isDisposed = false;
		private Helpers.AutoDisposable disposable;
		private ScrollViewer scrollViewerThreadRes;
		private bool isFirstContents = false;

		public FutabaThreadResViewer() {
			InitializeComponent();

			disposable = new Helpers.AutoDisposable()
				/*
				.Add(ViewModels.FutabaThreadResViewerViewModel.Messenger.Instance
					.GetEvent<PubSubEvent<ViewModels.FutabaThreadResViewerViewModel.MediaViewerOpenMessage>>()
					.Subscribe(x => {
						if(this.Contents != null) {
							this.Contents.MediaContents.Value = x.Media;
						}
					})
				)*/
				.Add(ViewModels.FutabaThreadResViewerViewModel.Messenger.Instance
					.GetEvent<PubSubEvent<ViewModels.FutabaThreadResViewerViewModel.ThreadUpdateCommandMessage>>()
					.Subscribe(x => {
						if(x?.Futaba == null) {
							return;
						}

						if((x.Futaba.Url == this.Contents?.Futaba.Value?.Url) && (this.DataContext is ViewModels.FutabaThreadResViewerViewModel vm)) {
							vm.KeyBindingUpdateCommand.Execute(x.Futaba);
						}
					})
				).Add(ViewModels.FutabaThreadResViewerViewModel.Messenger.Instance
					.GetEvent<PubSubEvent<ViewModels.FutabaThreadResViewerViewModel.ThreadSearchCommandMessage>>()
					.Subscribe(x => {
						if(x?.Futaba == null) {
							return;
						}

						if((x.Futaba.Url == this.Contents?.Futaba.Value?.Url) && (this.DataContext is ViewModels.FutabaThreadResViewerViewModel vm)) {
							vm.KeyBindingSearchCommand.Execute(x.Futaba);
						}
					})
				).Add(ViewModels.FutabaThreadResViewerViewModel.Messenger.Instance
					.GetEvent<PubSubEvent<ViewModels.FutabaThreadResViewerViewModel.ThreadOpenPostCommandMessage>>()
					.Subscribe(x => {
						if(x?.Futaba == null) {
							return;
						}

						if((x.Futaba.Url == this.Contents?.Futaba.Value?.Url) && (this.DataContext is ViewModels.FutabaThreadResViewerViewModel vm)) {
							vm.KeyBindingPostCommand.Execute(x.Futaba);
						}
					})
				).Add(ViewModels.FutabaThreadResViewerViewModel.Messenger.Instance
					.GetEvent<PubSubEvent<ViewModels.FutabaThreadResViewerViewModel.ScrollResMessage>>()
					.Subscribe(async x => {
						if(this.Contents != null) {
							var prev = this.scrollViewerThreadRes.VerticalOffset;
							this.ThreadResListBox.ScrollIntoView(x.Res);
							// 呼び出してもメッセージループに落ちるまでスクロールしないのでawaitする
							await Task.Delay(1);
							// 移動先が画面内にあるとスクロールしないので移動しなかったら点滅させてアピールしてみる
							if(prev == this.scrollViewerThreadRes.VerticalOffset) {
								var sb = this.TryFindResource("QuotHitStoryboard") as System.Windows.Media.Animation.Storyboard;
								var p = WpfUtil.WpfHelper.FindFirstChild<VirtualizingStackPanel>(this.ThreadResListBox);
								if((sb != null) && (p != null)) {
									int c = VisualTreeHelper.GetChildrenCount(p);
									for(var i = 0; i < c; i++) {
										var co = VisualTreeHelper.GetChild(p, i);
										if((co is FrameworkElement fe) && object.ReferenceEquals(fe.DataContext, x.Res)) {
											var g1 = WpfUtil.WpfHelper.FindFirstChild<Grid>(fe);
											if(g1 != null) {
												var g2 = WpfUtil.WpfHelper.FindFirstChild<Grid>(g1);
												if(g2 != null) {
													sb.Begin(g2);
												}
											}
											break;
										}
									}
								}
							}
						}
					})
				).Add(Canvas98.ViewModels.FutabaCanvas98ViewViewModel.Messenger.Instance
					.GetEvent<PubSubEvent<Canvas98.ViewModels.FutabaCanvas98ViewViewModel.PostFrom>>()
					.Subscribe(x => {
						if(x?.Url == this.Contents.Futaba.Value?.Url) {
							var b = this.Contents.Futaba.Value.Raw.Value.Board;
							Config.ConfigLoader.UpdateFutabaInputData(
								b,
								x.Form.Subject, x.Form.Name,
								x.Form.Email, x.Form.Password);
							Util.Futaba.UpdateThreadRes(
								b,
								x.Url.ThreadNo,
								Config.ConfigLoader.MakiMoki.FutabaThreadGetIncremental)
								.Subscribe();
						}
					})
				);

			this.ThreadResListBox.Loaded += (s, e) => {
				this.scrollViewerThreadRes = WpfUtil.WpfHelper.FindFirstChild<ScrollViewer>(this.ThreadResListBox);
#if false
				if((this.scrollViewerThreadRes = WpfUtil.WpfHelper.FindFirstChild<ScrollViewer>(this.ThreadResListBox)) != null) {
					this.scrollViewerThreadRes.ScrollChanged += async (ss, arg) => {
						if((this.Contents != null) && this.Contents.Futaba.Value.Url.IsThreadUrl) {
							var p = WpfUtil.WpfHelper.FindFirstChild<VirtualizingStackPanel>(this.ThreadResListBox);
							if(p != null) {
								await Task.Delay(1); // スクロール直後はまだコンテンツが切り替わっていないので一度UIスレッドを進める
								var cp = FindLastDisplayedChild<ListBoxItem>(
									p, p,
									new Point(0, 1)); // (0, 0)だとギリギリ見えない場合でも見えると判定されるので1px下に
								if(cp != null) {
									if(this.Contents.LastVisibleItem.Value == null) {
										scrollViewerThreadRes?.ScrollToTop();
										scrollViewerThreadRes?.ScrollToLeftEnd();
									}
									this.Contents.LastVisibleItem.Value = cp.DataContext;
								}
							}
							if(this.Contents.LastVisibleItem.Value != null) {
								this.Contents.ScrollVerticalOffset.Value = this.scrollViewerThreadRes.VerticalOffset;
								this.Contents.ScrollHorizontalOffset.Value = this.scrollViewerThreadRes.HorizontalOffset;
							}
						}
					};
				}
#endif
			};

			this.Unloaded += (s, e) => {
				if(this.isDisposed) {
					disposable.Dispose();
					if(this.DataContext is IDisposable d) {
						d.Dispose();
					}
					isDisposed = true;
				}
			};
		}

		public static T FindLastDisplayedChild<T>(
			FrameworkElement el,
			FrameworkElement parent = null,
			Point? targetPt = null) where T : FrameworkElement {

			var pt = parent ?? el;
			var zeroPt = targetPt ?? new Point(0, 0);
			int c = VisualTreeHelper.GetChildrenCount(el);
			for(var i = c - 1; 0 <= i; i--) {
				var co = VisualTreeHelper.GetChild(el, i);
				if(co is FrameworkElement fe) {
					var p = fe.TranslatePoint(zeroPt, pt);
					if((0 <= p.X) && (p.X <= pt.ActualWidth)
						&& (0 <= p.Y) && (p.Y <= pt.ActualHeight)) {

						if(co is T t) {
							return t;
						}
						/* 子供はいらない
						var r = FindLastDisplayedChild<T>(fe, pt, zeroPt);
						if (r != null) {
							return r;
						}
						*/
					}
				}
			}
			return default(T);
		}

		private static async void OnContentsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
			if(obj is FutabaThreadResViewer el) {
				if(!el.isFirstContents) {
					await Task.Yield(); // 一発目のイベント発火の時まだCommandにバインドされてないので一度遅らせる
					el.isFirstContents = true;
				}
				el.RaiseEvent(new RoutedPropertyChangedEventArgs<Model.IFutabaViewerContents>(
					e.OldValue as Model.IFutabaViewerContents,
					e.NewValue as Model.IFutabaViewerContents,
					ContentsChangedEvent));
#if fasle
				if((obj is FutabaThreadResViewer fv) && (e.NewValue is Model.IFutabaViewerContents c)) {
					if(c.Futaba.Value.Url.IsThreadUrl) {
						if(c.LastVisibleItem.Value != null) {
							// コンテンツ切り替えがまだListBoxに伝搬していないので一度UIスレッドを進める
							await Task.Delay(1);
							fv.ThreadResListBox.ScrollIntoView(c.LastVisibleItem.Value);
						}
					}
				}
#endif
			}
		}

		private void OnCopyTextboxSelectionChanged(object sender, RoutedEventArgs e) {
			// この処理XAMLだけでやりたいけどよくわからん…
			if(e.Source is TextBox tb) {
				if(tb.SelectionLength == 0) {
					tb.ContextMenu = null;
				} else {
					tb.ContextMenu = tb.FindResource("ContextMenu") as ContextMenu;
					tb.ContextMenu.PlacementTarget = tb;
				}
			}
		}
	}
}
