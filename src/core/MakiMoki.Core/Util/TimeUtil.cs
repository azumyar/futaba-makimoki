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
					- (((offest.Hours * 3600) + (offest.Minutes * 60) + offest.Seconds));
		}

		public static long ToUnixTimeSeconds() {
			return ToUnixTimeSeconds(DateTime.Now);
		}

		public static long ToUnixTimeSeconds(DateTime ticks) {
			var offest = TimeZoneInfo.Local.GetUtcOffset(ticks);
			return new DateTimeOffset(ticks).ToUnixTimeSeconds()
					- (((offest.Hours * 3600) + (offest.Minutes * 60) + offest.Seconds));
		}

		public static DateTime FromUnixTime(long unixTime) {
			// 今日より1年後の時間より大きい場合ミリ秒まで含まれているとみなす
			if(unixTime < ToUnixTimeMilliseconds(DateTime.Now.AddYears(1))) {
				return FromUnixTimeSeconds(unixTime);
			} else {
				return FromUnixTimeMilliseconds(unixTime);
			}
		}

		public static DateTime FromUnixTimeSeconds(long unixTime) {
			return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
				.AddSeconds(unixTime)
				.ToLocalTime();
		}

		public static DateTime FromUnixTimeMilliseconds(long unixTime) {
			return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
				.AddMilliseconds(unixTime)
				.ToLocalTime();
		}
	}
}
