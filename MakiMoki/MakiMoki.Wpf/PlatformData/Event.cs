using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Yarukizero.Net.MakiMoki.Wpf.PlatformData {
	delegate void HyperLinkEventHandler(object sender, HyperLinkEventArgs e);
	class HyperLinkEventArgs : RoutedEventArgs {
		public Uri NavigateUri { get; }

		public HyperLinkEventArgs(Uri navigateUri) : base() => this.NavigateUri = navigateUri;
		public HyperLinkEventArgs(RoutedEvent routedEvent, Uri navigateUri) : base(routedEvent) => this.NavigateUri = navigateUri;
		public HyperLinkEventArgs(RoutedEvent routedEvent, object source, Uri navigateUri) : base(routedEvent, source) => this.NavigateUri = navigateUri;
	}
}
