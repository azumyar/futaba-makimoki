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
	partial class FutabaCatalogViewer : UserControl, Model.IFutabaContainer {
		public static readonly DependencyProperty ContentsProperty
			= DependencyProperty.Register(
				nameof(Contents),
				typeof(Model.IFutabaViewerContents),
				typeof(FutabaCatalogViewer),
				new PropertyMetadata(OnContentsChanged));
		public static readonly RoutedEvent ContentsChangedEvent
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
		private IDisposable CatalogUpdateCommandSubscriber { get; }

		public FutabaCatalogViewer() {
			InitializeComponent();

			ViewModels.FutabaCatalogViewerViewModel.Messenger.Instance
				.GetEvent<PubSubEvent<ViewModels.FutabaCatalogViewerViewModel.CatalogUpdateCommandMessage>>()
				.Subscribe(x => {
					if(x?.Futaba == null) {
						return;
					}

					if((x.Futaba.Url == this.Contents?.Futaba.Value?.Url) && (this.DataContext is ViewModels.FutabaCatalogViewerViewModel vm)) {
						vm.KeyBindingUpdateCommand.Execute(x.Futaba);
					}
				});
			ViewModels.FutabaCatalogViewerViewModel.Messenger.Instance
				.GetEvent<PubSubEvent<ViewModels.FutabaCatalogViewerViewModel.CatalogSearchCommandMessage>>()
				.Subscribe(x => {
					if(x?.Futaba == null) {
						return;
					}

					if((x.Futaba.Url == this.Contents?.Futaba.Value?.Url) && (this.DataContext is ViewModels.FutabaCatalogViewerViewModel vm)) {
						vm.KeyBindingSearchCommand.Execute(x.Futaba);
					}
				});
			ViewModels.FutabaCatalogViewerViewModel.Messenger.Instance
				.GetEvent<PubSubEvent<ViewModels.FutabaCatalogViewerViewModel.CatalogModeCommandMessage>>()
				.Subscribe(x => {
					if(x?.Futaba == null) {
						return;
					}

					if((x.Futaba.Url == this.Contents?.Futaba.Value?.Url) && (this.DataContext is ViewModels.FutabaCatalogViewerViewModel vm)) {
						vm.KeyBindingModeCommand.Execute(x.Futaba);
					}
				});
			ViewModels.FutabaCatalogViewerViewModel.Messenger.Instance
				.GetEvent<PubSubEvent<ViewModels.FutabaCatalogViewerViewModel.CatalogOpenPostCommandMessage>>()
				.Subscribe(x => {
					if(x?.Futaba == null) {
						return;
					}

					if((x.Futaba.Url == this.Contents?.Futaba.Value?.Url) && (this.DataContext is ViewModels.FutabaCatalogViewerViewModel vm)) {
						vm.KeyBindingPostCommand.Execute(x.Futaba);
					}
				});


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

		private void OnCatalogMenuItemDelClickCommand(object sender, RoutedEventArgs e) => GetViewModel()?.CatalogMenuItemDelClickCommand.Execute(e);

		private void OnImageUnloaded(object sender, RoutedEventArgs e) {
			if(sender is Image o) {
				var s = o.Source;
				//BindingOperations.ClearAllBindings(o);
				if(s != null) {
					//o.Source = null;
					if(s is BitmapImage bi) {
						//bi.StreamSource.Dispose();
					}
				}
				//o.UpdateLayout();
				/*
				var f = o.GetType().GetField(
					"_bitmapSource",
					System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
				if(f != null) {
					var a = f.GetValue(o);
					f.SetValue(o, null);
				}
				*/
			}
		}

		private void OnContentUnloaded(object sender, RoutedEventArgs e) {
			if(sender is FrameworkElement o) {
				static void del(DependencyObject o, Action<DependencyObject> act) {
					foreach(var child in LogicalTreeHelper.GetChildren(o)) {
						if(child is DependencyObject) {
							del(child as DependencyObject, act);
						}
					}
					act(o);
				}

				del(o, x => BindingOperations.ClearAllBindings(x));
				if(o.DataContext != null) {
					ViewModels.MainWindowViewModel.Messenger.Instance
						.GetEvent<PubSubEvent<System.Windows.FrameworkElement>>()
						.Publish(o);
				}
			}
		}

		public void DestroyContainer() {
			static void del(DependencyObject o, Action<DependencyObject> act) {
				foreach(var child in LogicalTreeHelper.GetChildren(o)) {
					if(child is DependencyObject) {
						del(child as DependencyObject, act);
					}
				}
				act(o);
			}

			del(this, x => BindingOperations.ClearAllBindings(x));
		}
	}
}
