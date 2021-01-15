using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Yarukizero.Net.MakiMoki.Wpf.Behaviors {
	class ScrollBehavior : Behavior<Control> {
		private ScrollViewer scrollViewer;

		protected override void OnAttached() {
			base.OnAttached();
			if(this.AssociatedObject is ScrollViewer o) {
				this.scrollViewer = o;
			} else {
				this.AssociatedObject.Loaded += OnLoadedObject;
			}
		}

		protected override void OnDetaching() {
			base.OnDetaching();

			if(this.scrollViewer != null) {
				this.scrollViewer = null;
			}
			this.AssociatedObject.Loaded -= OnLoadedObject;
		}

		private void OnLoadedObject(object sender, RoutedEventArgs e) {
			if(e.Source is DependencyObject o) {
				this.scrollViewer = WpfUtil.WpfHelper.FindFirstChild<ScrollViewer>(o);
			}
		}

		public void ScrollToTop() => this.scrollViewer?.ScrollToTop();
		public void ScrollToBottom() => this.scrollViewer?.ScrollToBottom();
	}
}
