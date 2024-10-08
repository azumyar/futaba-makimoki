using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Yarukizero.Net.MakiMoki.Wpf.Controls {
	/// <summary>
	/// FutabaResBlock.xaml の相互作用ロジック
	/// </summary>
	partial class FutabaResBlock : UserControl {
		public static RoutedEvent ImageClickEvent
			= EventManager.RegisterRoutedEvent(
				nameof(ImageClick),
				RoutingStrategy.Tunnel,
				typeof(RoutedEventHandler),
				typeof(FutabaResBlock));
		public static RoutedEvent LinkClickEvent
			= EventManager.RegisterRoutedEvent(
				nameof(LinkClick),
				RoutingStrategy.Tunnel,
				typeof(PlatformData.HyperLinkEventHandler),
				typeof(FutabaResBlock));
		public static RoutedEvent QuotClickEvent
			= EventManager.RegisterRoutedEvent(
				nameof(QuotClick),
				RoutingStrategy.Tunnel,
				typeof(PlatformData.QuotClickEventHandler),
				typeof(FutabaResBlock));

		public event RoutedEventHandler ImageClick {
			add { AddHandler(ImageClickEvent, value); }
			remove { RemoveHandler(ImageClickEvent, value); }
		}

		public event PlatformData.HyperLinkEventHandler LinkClick {
			add { AddHandler(LinkClickEvent, value); }
			remove { RemoveHandler(LinkClickEvent, value); }
		}

		public event PlatformData.QuotClickEventHandler QuotClick {
			add { AddHandler(QuotClickEvent, value); }
			remove { RemoveHandler(QuotClickEvent, value); }
		}

		public FutabaResBlock() {
			InitializeComponent();

			this.ImageButton.Click += (s, e) => this.RaiseEvent(new RoutedEventArgs(ImageClickEvent, e.Source));
			this.FutabaCommentBlock.LinkClick += (s, e) => this.RaiseEvent(new PlatformData.HyperLinkEventArgs(LinkClickEvent, e.Source, e.NavigateUri));
			this.FutabaCommentBlock.QuotClick += (s, e) => this.RaiseEvent(new PlatformData.QuotClickEventArgs(QuotClickEvent, e.Source, e.TargetRes));
			this.ResCountTextBlock.PreviewMouseUp += (s, e) => {
				if((e.ChangedButton == MouseButton.Left) && (e.ClickCount == 1)) {
					if(this.DataContext is Model.BindableFutabaResItem it) {
						Windows.Popups.QuotePopup.Show(
							it.ResCitedSource,
							e.Source);
					}
				}
			};
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
						.GetEvent<PubSubEvent<ViewModels.MainWindowViewModel.WpfBugMessage>>()
						.Publish(new ViewModels.MainWindowViewModel.WpfBugMessage(o));
				}
			}
		}
	}
}
