using Android.OS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yarukizero.Net.MakiMoki.Droid.Extensions {
	internal static class BundleExtension {
		public static Bundle InJson(this Bundle @this, Data.JsonObject json, string? suffix = null) {
			@this.PutString(ToKey(json.GetType(), suffix), json.ToString());
			return @this;
		}

		public static T OutJson<T>(this Bundle @this, string? suffix = null) where T:Data.JsonObject {
			var s = @this.GetString(ToKey(typeof(T), suffix));
			return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(s);
		}


		private static string ToKey(Type t, string? suffix) => $"{t.FullName}::{suffix ?? ""}";
	}
}
