using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Yarukizero.Net.MakiMoki.Wpf.Behaviors {
	class MakiMokiViewerBehavior : Behavior<Control> {
		private static readonly int LineUpFactor = 2;
		private static readonly int LineDownFactor = 4;
		private ScrollViewer scrollViewer;

		protected override void OnAttached() {
			base.OnAttached();
			if(this.AssociatedObject is ScrollViewer o) {
				this.scrollViewer = o;
				this.scrollViewer.PreviewMouseWheel += OnPreviewMouseWheelViewer;
			} else {
				this.AssociatedObject.Loaded += OnLoadedObject;
			}
		}

		protected override void OnDetaching() {
			base.OnDetaching();

			if(this.scrollViewer != null) {
				this.scrollViewer.PreviewMouseWheel -= OnPreviewMouseWheelViewer;
				this.scrollViewer = null;
			}
			this.AssociatedObject.Loaded -= OnLoadedObject;
		}

		private void OnLoadedObject(object sender, RoutedEventArgs e) {
			if((this.scrollViewer == null) && (e.Source is DependencyObject o)) {
				this.scrollViewer = WpfUtil.WpfHelper.FindFirstChild<ScrollViewer>(o);
				this.scrollViewer.PreviewMouseWheel += OnPreviewMouseWheelViewer;
			}
		}

		private void OnPreviewMouseWheelViewer(object _, MouseWheelEventArgs e) {
			if(this.scrollViewer != null) {
				if(0 < e.Delta) {
					for(var i = 1; i < LineUpFactor; i++) {
						this.scrollViewer.LineUp();
					}
				} else {
					for(var i = 1; i < LineDownFactor; i++) {
						this.scrollViewer.LineDown();
					}
				}
			}
		}
	}
}
