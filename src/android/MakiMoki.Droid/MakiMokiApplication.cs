using Android.App;
using Android.Content;
using Android.Media;
using Android.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yarukizero.Net.MakiMoki.Droid {
	[global::Android.App.ApplicationAttribute(
		Label = "@string/app_name",
		LargeHeap = true,
		HardwareAccelerated = true,
		Theme = "@style/app_theme"
	)]
	internal class MakiMokiApplication : global::Android.App.Application {
		public static MakiMokiApplication Current { get; private set; }
		public App.MakiMokiContext MakiMoki { get; private set; }

		public MakiMokiApplication() : base() {}
		protected MakiMokiApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) {}


		public override void OnCreate() {
			Current = this;
			global::System.Text.Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); 
			this.MakiMoki = new App.MakiMokiContext();

			base.OnCreate();
		}

	}
}
