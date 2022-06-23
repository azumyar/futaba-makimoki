using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Yarukizero.Net.MakiMoki.Wpf.Behaviors {
	internal class FluentMenuBehavior : Behavior<FrameworkElement> {
		protected override void OnAttached() {
			base.OnAttached();
			WpfHelpers.FluentHelper.AttachAndApplyMenuItem(this.AssociatedObject);

			var p = this.AssociatedObject;
			var border = default(Border);
			do {
				p = VisualTreeHelper.GetParent(p) as FrameworkElement;
				if(p is Border b && (b.Name == "SubmenuBorder")) {
					border = b;
				}
				if((p is Popup || (p?.GetType().FullName == "System.Windows.Controls.Primitives.PopupRoot")) && (border != null)) {
					if(WpfHelpers.FluentHelper.IsWindowBackgroundTransparent()) {
						border.Background = Brushes.Transparent;
					}
					break;
				}
			} while(p != null);
		}
	}
}