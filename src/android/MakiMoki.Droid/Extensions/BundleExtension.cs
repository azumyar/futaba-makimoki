using Android.Content;
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
			return @this.GetString(ToKey(typeof(T), suffix)) switch {
				string s => Newtonsoft.Json.JsonConvert.DeserializeObject<T>(s),
				_ => default,
			};
		}


		public static Intent InJson(this Intent @this, Data.JsonObject json, string? suffix = null) {
			@this.PutExtra(ToKey(json.GetType(), suffix), json.ToString());
			return @this;
		}
		public static T OutJson<T>(this Intent @this, string? suffix = null) where T : Data.JsonObject {
			return @this.GetStringExtra(ToKey(typeof(T), suffix)) switch {
				string s => Newtonsoft.Json.JsonConvert.DeserializeObject<T>(s),
				_ => default,
			};
		}

		private static string ToKey(Type t, string? suffix) => $"{t.FullName}::{suffix ?? ""}";
	}
}
