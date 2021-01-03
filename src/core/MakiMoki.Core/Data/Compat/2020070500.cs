using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Data.Compat {
	public class MakiMokiConfig2020062900 : ConfigObject, IMigrateCompatObject {
		public static int CurrentVersion { get; } = 2020062900;

		[JsonProperty("futaba-thread-get-incremental", Required = Required.Always)]
		public bool FutabaThreadGetIncremental { get; private set; }

		[JsonProperty("futaba-response-data-save", Required = Required.Always)]
		public bool FutabaResponseSave { get; private set; }

		[JsonProperty("futaba-post-data-expire-day", Required = Required.Always)]
		public int FutabaPostDataExpireDay { get; private set; }

		public ConfigObject Migrate() {
			var t = typeof(Config.ConfigLoader);
			var conf = JsonConvert.DeserializeObject<MakiMokiConfig>(
				Util.FileUtil.LoadFileString(t.Assembly.GetManifestResourceStream(
					$"{ t.Namespace }.{ Config.ConfigLoader.MakiMokiConfigFile }")));

			return MakiMokiConfig.From(
				threadGetIncremental: FutabaThreadGetIncremental,
				responseSave: FutabaResponseSave,
				postDataExpireDay: FutabaPostDataExpireDay,

				// 2020070500
				isSavedPostSubject: conf.FutabaPostSavedSubject,
				isSavedPostName: conf.FutabaPostSavedName,
				isSavedPostMail: conf.FutabaPostSavedMail
			);
		}

		/*
		public static MakiMokiConfig From(bool threadGetIncremental, bool responseSave, int postDataExpireDay) {
			return new MakiMokiConfig() {
				Version = CurrentVersion,
				FutabaThreadGetIncremental = threadGetIncremental,
				FutabaResponseSave = responseSave,
				FutabaPostDataExpireDay = postDataExpireDay,
			};
		}
		*/
	}
}