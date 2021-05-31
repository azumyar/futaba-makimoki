using System;
using System.Collections.Generic;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Uno.UnoHelpers {
	static class Toast {

		public static void Show(string text) {
			System.Diagnostics.Debug.WriteLine(text);
			Android.Widget.Toast.MakeText(
				Droid.MainActivity.ActivityContext,
				text, Android.Widget.ToastLength.Long).Show();
		}
	}
}
