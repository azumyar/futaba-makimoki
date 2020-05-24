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
	partial class FutabaCatalogViewer : UserControl {
		public static readonly DependencyProperty ContentsProperty
			= DependencyProperty.Register(
				nameof(Contents),
				typeof(Model.IFutabaViewerContents),
				typeof(FutabaCatalogViewer),
				new PropertyMetadata(OnContentsChanged));
		public static RoutedEvent ContentsChangedEvent
			= EventManager.RegisterRoutedEvent(
				nameof(ContentsChanged),
				RoutingStrategy.Tunnel,
				typeof(RoutedPropertyChangedEventHandler<Model.IFutabaViewerContents>),
				typeof(FutabaCatalogViewer));


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

		public FutabaCatalogViewer() {
			InitializeComponent();

			ViewModels.FutabaCatalogViewerViewModel.Messenger.Instance
				.GetEvent<PubSubEvent<ViewModels.FutabaCatalogViewerViewModel.CatalogListboxUpdatedMessage>>()
				.Subscribe(_ => {
					scrollViewerCatalog.ScrollToVerticalOffset(0);
					scrollViewerCatalog.ScrollToHorizontalOffset(0);
				});
			this.CatalogListBox.Loaded += (s, e) => {
				if((this.scrollViewerCatalog = WpfUtil.WpfHelper.FindFirstChild<ScrollViewer>(this.CatalogListBox)) != null) {
					this.scrollViewerCatalog.ScrollChanged += (ss, arg) => {
						if((this.Contents != null) && this.Contents.Futaba.Value.Url.IsCatalogUrl) {
							this.Contents.ScrollVerticalOffset.Value = this.scrollViewerCatalog.VerticalOffset;
							this.Contents.ScrollHorizontalOffset.Value = this.scrollViewerCatalog.HorizontalOffset;
						}
					};
				}
			};
		}

		private static void OnContentsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
			if(obj is UIElement el) {
				el.RaiseEvent(new RoutedPropertyChangedEventArgs<Model.IFutabaViewerContents>(
					e.OldValue as Model.IFutabaViewerContents,
					e.NewValue as Model.IFutabaViewerContents,
					ContentsChangedEvent));
				if((obj is FutabaCatalogViewer fv) && (e.NewValue is Model.IFutabaViewerContents c)) {
					if(c.Futaba.Value.Url.IsCatalogUrl) {
						fv.scrollViewerCatalog?.ScrollToHorizontalOffset(c.ScrollHorizontalOffset.Value);
						fv.scrollViewerCatalog?.ScrollToVerticalOffset(c.ScrollVerticalOffset.Value);
					}
				}
			}
		}

		private ViewModels.FutabaCatalogViewerViewModel GetViewModel() => this.DataContext as ViewModels.FutabaCatalogViewerViewModel;

		private void OnCatalogSortItemCatalogClickCommand(object sender, RoutedEventArgs e) => GetViewModel()?.CatalogSortItemCatalogClickCommand.Execute(e);
		private void OnCatalogSortItemNewClickCommand(object sender, RoutedEventArgs e) => GetViewModel()?.CatalogSortItemNewClickCommand.Execute(e);
		private void OnCatalogSortItemOldClickCommand(object sender, RoutedEventArgs e) => GetViewModel()?.CatalogSortItemOldClickCommand.Execute(e);
		private void OnCatalogSortItemManyClickCommand(object sender, RoutedEventArgs e) => GetViewModel()?.CatalogSortItemManyClickCommand.Execute(e);
		private void OnCatalogSortItemMomentumClickCommand(object sender, RoutedEventArgs e) => GetViewModel()?.CatalogSortItemMomentumClickCommand.Execute(e);
		private void OnCatalogSortItemFewClickCommand(object sender, RoutedEventArgs e) => GetViewModel()?.CatalogSortItemFewClickCommand.Execute(e);
		private void OnCatalogSortItemSoudaneClickCommand(object swender, RoutedEventArgs e) => GetViewModel()?.CatalogSortItemSoudaneClickCommand.Execute(e);

		private void OnCatalogMenuItemDelClickCommand(object sender, RoutedEventArgs e) => GetViewModel()?.CatalogMenuItemDelClickCommand.Execute(e);
	}
}
