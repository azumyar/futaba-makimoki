using Microsoft.Xaml.Behaviors;
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
	partial class FutabaViewer : UserControl {
		public static readonly DependencyProperty ContentsProperty =
			DependencyProperty.Register(
				nameof(Contents),
				typeof(Model.IFutabaViewerContents),
				typeof(FutabaViewer),
				new PropertyMetadata(OnContentsChanged));
		public static RoutedEvent ContentsChangedEvent
			= EventManager.RegisterRoutedEvent(
				nameof(ContentsChanged),
				RoutingStrategy.Tunnel, 
				typeof(RoutedPropertyChangedEventHandler<Model.IFutabaViewerContents>), 
				typeof(FutabaViewer));


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

		private ScrollViewer scrollViewerCatalog;
		private ScrollViewer scrollViewerThreadRes;

		public FutabaViewer() {
			InitializeComponent();
			this.CatalogListBox.Loaded += (s, e) => {
				if ((this.scrollViewerCatalog = GetListBoxScrollViewer(this.CatalogListBox)) != null) {
					this.scrollViewerCatalog.ScrollChanged += (ss, arg) => {
						if ((this.Contents != null) && string.IsNullOrWhiteSpace(this.Contents.Futaba.Value.Url.ThreadNo)) {
							this.Contents.ScrollVerticalOffset.Value = this.scrollViewerCatalog.VerticalOffset;
							this.Contents.ScrollHorizontalOffset.Value = this.scrollViewerCatalog.HorizontalOffset;
						}
					};
				}
			};
			this.ThreadResListBox.IsVisibleChanged += (s, e) => {
				if(this.scrollViewerThreadRes != null) {
					return;
				}

				if ((this.scrollViewerThreadRes = GetListBoxScrollViewer(this.ThreadResListBox)) != null) {
					this.scrollViewerThreadRes.ScrollChanged += (ss, arg) => {
						if ((this.Contents != null) && !string.IsNullOrWhiteSpace(this.Contents.Futaba.Value.Url.ThreadNo)) {
							this.Contents.ScrollVerticalOffset.Value = this.scrollViewerThreadRes.VerticalOffset;
							this.Contents.ScrollHorizontalOffset.Value = this.scrollViewerThreadRes.HorizontalOffset;
						}
					};
				}
			};
		}

		private ScrollViewer GetListBoxScrollViewer(DependencyObject o) {
			int c = VisualTreeHelper.GetChildrenCount(o);
			for(var i=0; i<c; i++) {
				var co = VisualTreeHelper.GetChild(o, i);
				if(co is ScrollViewer s) {
					return s;
				}
				var r = GetListBoxScrollViewer(co);
				if(r != null) {
					return r;
				}
			}
			return null;
		}

		private static void OnContentsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
			if(obj is UIElement el) {
				el.RaiseEvent(new RoutedPropertyChangedEventArgs<Model.IFutabaViewerContents>(
					e.OldValue as Model.IFutabaViewerContents,
					e.NewValue as Model.IFutabaViewerContents,
					ContentsChangedEvent));
				if((obj is FutabaViewer fv) && (e.NewValue is Model.IFutabaViewerContents c)) {
					var sv = string.IsNullOrWhiteSpace(c.Futaba.Value.Url.ThreadNo)
						? fv.scrollViewerCatalog : fv.scrollViewerThreadRes;
					sv?.ScrollToHorizontalOffset(c.ScrollHorizontalOffset.Value);
					sv?.ScrollToVerticalOffset(c.ScrollVerticalOffset.Value);
				}
			}
		}

		public void OnMenuItemCopyClickCommand(object swender, RoutedEventArgs e) {
			if (this.DataContext is ViewModels.FutabaViewerViewModel vm) {
				vm.MenuItemCopyClickCommand.Execute(e);
			}
		}

		public void OnMenuItemReplyClickCommand(object swender, RoutedEventArgs e) {
			if(this.DataContext is ViewModels.FutabaViewerViewModel vm) {
				vm.MenuItemReplyClickCommand.Execute(e);
			}
		}

		public void OnMenuItemSoudaneClickCommand(object swender, RoutedEventArgs e) {
			if (this.DataContext is ViewModels.FutabaViewerViewModel vm) {
				vm.MenuItemSoudaneClickCommand.Execute(e);
			}
		}


		public void OnMenuItemDelClickCommand(object swender, RoutedEventArgs e) {
			if (this.DataContext is ViewModels.FutabaViewerViewModel vm) {
				vm.MenuItemDelClickCommand.Execute(e);
			}
		}


		public void OnMenuItemDeleteClickCommand(object swender, RoutedEventArgs e) {
			if (this.DataContext is ViewModels.FutabaViewerViewModel vm) {
				vm.MenuItemDeleteClickCommand.Execute(e);
			}
		}

	}
}
