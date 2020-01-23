using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Yarukizero.Net.MakiMoki.Wpf.Behaviors {
	class FocusBehavior : Behavior<UIElement> {
		public void Focus() {
			this.AssociatedObject.Focus();
		}
	}
}
