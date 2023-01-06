using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yarukizero.Net.MakiMoki.Droid {
	internal static class DroidConst {
		public static string PreferencesName { get; } = "makimoki";
		public static string PreferencesKeyInitTime { get; } = "__init_time";

		public static int ActivityResultCodePost { get; } = 1;

		public static string DbName { get; } = "__test_db_v20230101.db";
	}
}
