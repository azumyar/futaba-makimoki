using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace Yarukizero.Net.MakiMoki.Config {
	public static partial class ConfigLoader {
		public static void StartAppCenter(string secrets) {
			if(string.IsNullOrEmpty(secrets)) {
				return;
			}

			if(Optout.AppCenterCrashes) {
				AppCenter.Start(secrets, typeof(Analytics));
			} else {
#if MAKIMOKI_DROID
				AppDomain.CurrentDomain.UnhandledException += (_, e) => {
					if(e.ExceptionObject is Exception ex) {
						Crashes.TrackError(ex);
					}
				};
#endif
				AppCenter.Start(secrets, typeof(Analytics), typeof(Crashes));
			}
		}
	}
}
