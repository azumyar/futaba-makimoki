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

		public event RoutedEventHandler ImageClick {
			add { AddHandler(ImageClickEvent, value); }
			remove { RemoveHandler(ImageClickEvent, value); }
		}

		public event PlatformData.HyperLinkEventHandler LinkClick {
			add { AddHandler(LinkClickEvent, value); }
			remove { RemoveHandler(LinkClickEvent, value); }
		}

		public FutabaResBlock() {
			InitializeComponent();

			this.ImageButton.Click += (s, e) => this.RaiseEvent(new RoutedEventArgs(ImageClickEvent, e.Source));
			this.FutabaCommentBlock.LinkClick += (s, e) => this.RaiseEvent(new PlatformData.HyperLinkEventArgs(LinkClickEvent, e.Source, e.NavigateUri));
		}
	}
}
