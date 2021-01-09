using System;
using System.Collections.Generic;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Util {
	public static class TimeUtil {
		public static long ToUnixTimeMilliseconds() {
			return ToUnixTimeMilliseconds(DateTime.Now);
		}

		public static long ToUnixTimeMilliseconds(DateTime ticks) {
			return new DateTimeOffset(
				ticks,
				-TimeZoneInfo.Local.GetUtcOffset(ticks)).ToUnixTimeMilliseconds();
		}
	}
}
