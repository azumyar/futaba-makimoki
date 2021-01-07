using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Wpf.Canvas98.Canvas98Util {
	public static class Util {
		public static bool IsInstalledWebView2Runtime() {
			try {
				return !string.IsNullOrEmpty(CoreWebView2Environment.GetAvailableBrowserVersionString());
			}
			catch(EdgeNotFoundException) {
				return false;
			}
		}

		public static bool IsEnabledCanvas98() {
			return IsInstalledWebView2Runtime() && !string.IsNullOrEmpty(Canvas98Config.Canvas98ConfigLoader.Bookmarklet.Value.Bookmarklet);
		}
	}
}
