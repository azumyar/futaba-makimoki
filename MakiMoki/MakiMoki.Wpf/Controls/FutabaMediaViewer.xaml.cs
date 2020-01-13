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
	/// FutabaMediaViewer.xaml の相互作用ロジック
	/// </summary>
	public partial class FutabaMediaViewer : UserControl {
		public static readonly DependencyProperty ContentsProperty
			= DependencyProperty.Register(
				nameof(Contents),
				typeof(PlatformData.FutabaMedia),
				typeof(FutabaMediaViewer),
				new PropertyMetadata(OnContentsChanged));
		public static RoutedEvent ContentsChangedEvent
			= EventManager.RegisterRoutedEvent(
				nameof(ContentsChanged),
				RoutingStrategy.Tunnel,
				typeof(RoutedPropertyChangedEventHandler<PlatformData.FutabaMedia>),
				typeof(FutabaMediaViewer));

		public PlatformData.FutabaMedia Contents {
			get => (PlatformData.FutabaMedia)this.GetValue(ContentsProperty);
			set {
				this.SetValue(ContentsProperty, value);
			}
		}

		public event RoutedPropertyChangedEventHandler<PlatformData.FutabaMedia> ContentsChanged {
			add { AddHandler(ContentsChangedEvent, value); }
			remove { RemoveHandler(ContentsChangedEvent, value); }
		}

		public FutabaMediaViewer() {
			InitializeComponent();
		}

		private static void OnContentsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
			if(obj is UIElement el) {
				el.RaiseEvent(new RoutedPropertyChangedEventArgs<PlatformData.FutabaMedia>(
					e.OldValue as PlatformData.FutabaMedia,
					e.NewValue as PlatformData.FutabaMedia,
					ContentsChangedEvent));
			}
		}
	}
}