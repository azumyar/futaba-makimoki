using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Yarukizero.Net.MakiMoki.Wpf.Behaviors {
	class AttachedScrollBarStyleBehavior : Behavior<Control> {
		public static readonly DependencyProperty StyleProperty =
			DependencyProperty.Register(
				nameof(Style),
				typeof(Style),
				typeof(AttachedScrollBarStyleBehavior),
				new PropertyMetadata(null));

		public Style Style {
			get => (Style)this.GetValue(StyleProperty);
			set {
				this.SetValue(StyleProperty, value);
			}
		}

		protected override void OnAttached() {
			base.OnAttached();
			this.AssociatedObject.Loaded += OnLoadedObject;
		}

		protected override void OnDetaching() {
			base.OnDetaching();
			this.AssociatedObject.Loaded -= OnLoadedObject;
		}

		private void OnLoadedObject(object sender, RoutedEventArgs e) {
			if(Style == null) {
				return;
			}

			if(e.Source is DependencyObject o) {
				var s = WpfUtil.WpfHelper.FindFirstChild<ScrollViewer>(o);
				if(s != null) {
					var grid = VisualTreeHelper.GetChild(s, 0);
					if(grid != null) {
						var len = VisualTreeHelper.GetChildrenCount(grid);
						for(var i=0; i<len; i++) {
							if(VisualTreeHelper.GetChild(grid, i) is System.Windows.Controls.Primitives.ScrollBar sb) {
								//sb.Style = Style;
							}
						}
					}
				}
			}
		}
	}
}
