using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yarukizero.Net.MakiMoki.Wpf.Controls {
	public class MakiMokiControl : System.Windows.Controls.UserControl {
		public MakiMokiControl() {
			this.Unloaded += (s, e) => UnloadHelper(this);
		}


		public static void UnloadHelper(object o) {
			if((o is System.Windows.FrameworkElement el)
				&& (el.DataContext is IDisposable d)) {

				d.Dispose();
			}

		}
	}
}
