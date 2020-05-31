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

		private ScrollViewer scrollViewerThreadRes;

		public FutabaThreadResViewer() {
			InitializeComponent();

			ViewModels.FutabaThreadResViewerViewModel.Messenger.Instance
				.GetEvent<PubSubEvent<ViewModels.FutabaThreadResViewerViewModel.MediaViewerOpenMessage>>()
				.Subscribe(x => {
					if(this.Contents != null) {
						this.Contents.MediaContents.Value = x.Media;
					}
				});

			this.ThreadResListBox.Loaded += (s, e) => {
				if((this.scrollViewerThreadRes = WpfUtil.WpfHelper.FindFirstChild<ScrollViewer>(this.ThreadResListBox)) != null) {
					this.scrollViewerThreadRes.ScrollChanged += async (ss, arg) => {
						if((this.Contents != null) && this.Contents.Futaba.Value.Url.IsThreadUrl) {
							this.Contents.ScrollVerticalOffset.Value = this.scrollViewerThreadRes.VerticalOffset;
							this.Contents.ScrollHorizontalOffset.Value = this.scrollViewerThreadRes.HorizontalOffset;
							var p = WpfUtil.WpfHelper.FindFirstChild<VirtualizingStackPanel>(this.ThreadResListBox);
							if(p != null) {
								await Task.Delay(1); // スクロール直後はまだコンテンツが切り替わっていないので一度UIスレッドを進める
								var cp = FindLastDisplayedChild<ListBoxItem>(
									p, p,
									new Point(0, 1)); // (0, 0)だとギリギリ見えない場合でも見えると判定されるので1px下に
								if(cp != null) {
									this.Contents.LastVisibleItem.Value = cp.DataContext;
								}
							}
						}
					};
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
			if(obj is UIElement el) {
				el.RaiseEvent(new RoutedPropertyChangedEventArgs<Model.IFutabaViewerContents>(
					e.OldValue as Model.IFutabaViewerContents,
					e.NewValue as Model.IFutabaViewerContents,
					ContentsChangedEvent));
				if((obj is FutabaThreadResViewer fv) && (e.NewValue is Model.IFutabaViewerContents c)) {
					if(c.Futaba.Value.Url.IsThreadUrl) {
						if(c.LastVisibleItem.Value != null) {
							// コンテンツ切り替えがまだListBoxに伝搬していないので一度UIスレッドを進める
							await Task.Delay(1);
							fv.ThreadResListBox.ScrollIntoView(c.LastVisibleItem.Value);
						} else {
							// nullの場合初期位置に
							fv.scrollViewerThreadRes?.ScrollToHorizontalOffset(0);
							fv.scrollViewerThreadRes?.ScrollToVerticalOffset(0);
						}
					}
				}
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

		private ViewModels.FutabaThreadResViewerViewModel GetViewModel() => this.DataContext as ViewModels.FutabaThreadResViewerViewModel;

		private void OnThreadResHamburgerItemUrlClickCommand(object sender, RoutedEventArgs e) => GetViewModel()?.ThreadResHamburgerItemUrlClickCommand.Execute(e);

		private void OnMenuItemCopyClickCommand(object sender, RoutedEventArgs e) => GetViewModel()?.MenuItemCopyClickCommand.Execute(e);
		private void OnMenuItemReplyClickCommand(object sender, RoutedEventArgs e) => GetViewModel()?.MenuItemReplyClickCommand.Execute(e);
		private void OnMenuItemSoudaneClickCommand(object sender, RoutedEventArgs e) => GetViewModel()?.MenuItemSoudaneClickCommand.Execute(e);
		private void OnMenuItemDelClickCommand(object sender, RoutedEventArgs e) => GetViewModel()?.MenuItemDelClickCommand.Execute(e);
		private void OnMenuItemDeleteClickCommand(object swender, RoutedEventArgs e) => GetViewModel()?.MenuItemDeleteClickCommand.Execute(e);
	}
}
