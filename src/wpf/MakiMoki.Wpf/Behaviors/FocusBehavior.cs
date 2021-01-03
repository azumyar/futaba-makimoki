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

		// Interaction.Triggersで他と組み合わせた場合にFocus()ではまだ前処理が終わってないときのために遅らせて実行する
		public async void FocusDelay() {
			await Task.Delay(1);
			this.AssociatedObject.Focus();
		}
	}
}
