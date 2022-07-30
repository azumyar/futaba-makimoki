using System;
using System.Collections.Generic;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Uno.UnoUtils {
	internal static class PlatformInfo {
		private static string uaDeviceName;

		public static string UserAgentDeviceName {
			get {
				if(uaDeviceName != null) {
					return uaDeviceName;
				}
#if __ANDROID__
				uaDeviceName = $"{ Android.OS.Build.Manufacturer }-{ Android.OS.Build.Model }";
#endif
				if(string.IsNullOrEmpty(uaDeviceName)) {
					uaDeviceName = "unknown";
				}
				return uaDeviceName;
			}
		}


	}
}
