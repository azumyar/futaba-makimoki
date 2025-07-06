using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Yarukizero.Net.MakiMoki.Data {
	public class MakiMokiConfig : ConfigObject {
		public static int CurrentVersion { get; } = 2020070500;

		[JsonProperty("futaba-thread-get-incremental", Required = Required.Always)]
		public bool FutabaThreadGetIncremental { get; private set; }

		[JsonProperty("futaba-response-data-save", Required = Required.Always)]
		public bool FutabaResponseSave { get; private set; }

		[JsonProperty("futaba-post-data-expire-day", Required = Required.Always)]
		public int FutabaPostDataExpireDay { get; private set; }

		[JsonProperty("futaba-post-save-subject", Required = Required.Always)]
		public bool FutabaPostSavedSubject { get; private set; }
		[JsonProperty("futaba-post-save-name", Required = Required.Always)]
		public bool FutabaPostSavedName { get; private set; }
		[JsonProperty("futaba-post-save-mail", Required = Required.Always)]
		public bool FutabaPostSavedMail { get; private set; }

		public static MakiMokiConfig From(
			bool threadGetIncremental, bool responseSave, int postDataExpireDay,
			bool isSavedPostSubject, bool isSavedPostName, bool isSavedPostMail) {

			return new MakiMokiConfig() {
				Version = CurrentVersion,
				FutabaThreadGetIncremental = threadGetIncremental,
				FutabaResponseSave = responseSave,
				FutabaPostDataExpireDay = postDataExpireDay,
				FutabaPostSavedSubject = isSavedPostSubject,
				FutabaPostSavedName = isSavedPostName,
				FutabaPostSavedMail = isSavedPostMail,
			};
		}
	}

	// 別のオプトアウト案件が発生する可能性があるのでここはとりあえず残しておく
	public class MakiMokiOptout : ConfigObject {
		public static int CurrentVersion { get; } = 2021011600;

		// Appセンターはサービス終了してる
		[Obsolete]
		[JsonProperty("optout-appcenter-crashes", Required = Required.Always)]
		public bool AppCenterCrashes { get; private set; }

		public static MakiMokiOptout From() {
			return new MakiMokiOptout() {
				Version = CurrentVersion,
			};
		}
	}
}
