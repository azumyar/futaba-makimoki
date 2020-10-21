using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Ng.NgData.Compat {
	public class NgConfig2020062900 : Data.ConfigObject, Data.IMigrateCompatObject {
		public static int CurrentVersion { get; } = 2020062900;

		[JsonProperty("ng-id", Required = Required.DisallowNull)]
		public bool EnableIdNg { get; internal set; }

		[JsonProperty("ng-catalog-words", Required = Required.DisallowNull)]
		public string[] CatalogWords { get; internal set; }

		[JsonProperty("ng-catalog-regex", Required = Required.DisallowNull)]
		public string[] CatalogRegex { get; internal set; }

		[JsonProperty("ng-thread-words", Required = Required.DisallowNull)]
		public string[] ThreadWords { get; internal set; }

		[JsonProperty("ng-thread-regex", Required = Required.DisallowNull)]
		public string[] ThreadRegex { get; internal set; }

		public Data.ConfigObject Migrate() {
			return NgConfig.Create(
				enableThreadIdNg: this.EnableIdNg,
				catalogWords: this.CatalogWords,
				catalogRegex: this.CatalogRegex,
				threadWords: this.CatalogWords,
				threadRegex: this.ThreadRegex,

				// 2020102900
				enableCatalogIdNg: false
			);
		}

		/*
		internal static NgConfig CreateDefault() {
			return new NgConfig() {
				Version = CurrentVersion,
				EnableIdNg = false,
				CatalogWords = new string[0],
				CatalogRegex = new string[0],
				ThreadWords = new string[0],
				ThreadRegex = new string[0],
			};
		}
		*/
	}

}
