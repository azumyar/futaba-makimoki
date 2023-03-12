using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Yarukizero.Net.MakiMoki.Util;
using Reactive.Bindings;

namespace Yarukizero.Net.MakiMoki.Wpf.Controls {
	class ThumbToolTip : ToolTip {
		private bool loaded = false;

		public ThumbToolTip(Func<IObservable<(bool Successed, string LocalPath, byte[] FileBytes)>> loader) {
			System.Diagnostics.Debug.Assert(loader != null);
			// 見せたくないのではじめは完全透過
			Opacity = 0;
			this.Loaded += (s, e) => {
				if(!this.loaded) {
					this.loaded = true;
					loader().Select(x => {
						if(x.Successed) {
							return WpfUtil.ImageUtil.CreateImage(x.LocalPath,  x.FileBytes);
						} else {
							return null;
						}
					}).ObserveOn(UIDispatcherScheduler.Default)
						.Subscribe(x => {
							if(x != null) {
								Content = new Image() {
									Source = x.Image,
								};
								Opacity = 1;
							}
						});
				}
			};
		}
	}
}
