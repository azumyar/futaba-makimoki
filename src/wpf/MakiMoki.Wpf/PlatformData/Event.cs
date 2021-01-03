using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Yarukizero.Net.MakiMoki.Wpf.PlatformData {
	delegate void HyperLinkEventHandler(object sender, HyperLinkEventArgs e);
	delegate void QuotClickEventHandler(object sender, QuotClickEventArgs e);
	class HyperLinkEventArgs : RoutedEventArgs {
		public Uri NavigateUri { get; }

		public HyperLinkEventArgs(Uri navigateUri) : base() => this.NavigateUri = navigateUri;
		public HyperLinkEventArgs(RoutedEvent routedEvent, Uri navigateUri) : base(routedEvent) => this.NavigateUri = navigateUri;
		public HyperLinkEventArgs(RoutedEvent routedEvent, object source, Uri navigateUri) : base(routedEvent, source) => this.NavigateUri = navigateUri;
	}

	class QuotClickEventArgs : RoutedEventArgs {
		public Model.BindableFutabaResItem TargetRes { get; }

		public QuotClickEventArgs(Model.BindableFutabaResItem targetRes) : base() => this.TargetRes = targetRes;
		public QuotClickEventArgs(RoutedEvent routedEvent, Model.BindableFutabaResItem targetRes) : base(routedEvent) => this.TargetRes = targetRes;
		public QuotClickEventArgs(RoutedEvent routedEvent, object source, Model.BindableFutabaResItem targetRes) : base(routedEvent, source) => this.TargetRes = targetRes;
	}
}
