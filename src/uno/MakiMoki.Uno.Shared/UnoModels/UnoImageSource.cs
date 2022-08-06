using System;
using System.Collections.Generic;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Uno.UnoModels {
	class UnoImageSource : Windows.UI.Xaml.Media.ImageSource {
#if __ANDROID__
		public Android.Graphics.Bitmap NativeImage { get; }

		public UnoImageSource(Android.Graphics.Bitmap bmp) : base(bmp) {
			this.NativeImage = bmp;
		}
#endif
	}
}
