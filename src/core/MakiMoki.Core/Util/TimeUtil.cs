using System;
using System.Collections.Generic;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Util {
	public static class TimeUtil {
		public static long ToUnixTimeMilliseconds() {
			return ToUnixTimeMilliseconds(DateTime.Now);
		}

		public static long ToUnixTimeMilliseconds(DateTime ticks) {
			var offest = TimeZoneInfo.Local.GetUtcOffset(ticks);
			return new DateTimeOffset(ticks).ToUnixTimeMilliseconds()
					- (((offest.Hours * 3600) + (offest.Minutes * 60) + offest.Seconds) * 1000);
		}
	}
}
