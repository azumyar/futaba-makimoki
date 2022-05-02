using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Nostra13.Universalimageloader.Core;
using Windows.UI.Xaml.Media;

using AndroidX.AppCompat.App;

namespace Yarukizero.Net.MakiMoki.Uno.Droid {
	[global::Android.App.ApplicationAttribute(
		Label = "@string/ApplicationName",
		LargeHeap = true,
		HardwareAccelerated = true,
		Theme = "@style/AppTheme"
	)]
	public class MakiMokiApplication : Windows.UI.Xaml.NativeApplication {
		public static MakiMokiApplication Current { get; private set; }

		public MakiMokiApplication(IntPtr javaReference, JniHandleOwnership transfer)
			: base(() => new App(), javaReference, transfer) {
			ConfigureUniversalImageLoader();
		}

		private void ConfigureUniversalImageLoader() {
			// Create global configuration and initialize ImageLoader with this config
			ImageLoaderConfiguration config = new ImageLoaderConfiguration
				.Builder(Context)
				.Build();

			ImageLoader.Instance.Init(config);

			ImageSource.DefaultImageLoader = ImageLoader.Instance.LoadImageAsync;
		}

		public override void OnCreate() {
			Current = this;

			base.OnCreate();
		}
	}
}
