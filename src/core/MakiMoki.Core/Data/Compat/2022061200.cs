using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yarukizero.Net.MakiMoki.Data.Compat {
	public class FutabaApiConfig2020062900 : ConfigObject, IMigrateCompatObject {
		public static int CurrentVersion { get; } = 2020062900;

		[JsonProperty("ptua", Required = Required.Always)]
		public string Ptua { get; internal set; }

		[JsonProperty("cookies", Required = Required.Always)]
		public Cookie[] Cookies { get; internal set; }

		[JsonProperty("subject", Required = Required.Always)]
		public string SavedSubject { get; internal set; }

		[JsonProperty("name", Required = Required.Always)]
		public string SavedName { get; internal set; }

		[JsonProperty("mail", Required = Required.Always)]
		public string SavedMail { get; internal set; }

		[JsonProperty("password", Required = Required.Always)]
		public string SavedPassword { get; internal set; }

		public ConfigObject Migrate() {
			return new FutabaApiConfig() {
				Ptua = Ptua,
				Cookies = new Cookie2[0], // Cookieを一度削除する
				SavedSubject = SavedSubject,
				SavedName = SavedName,
				SavedMail = SavedMail,
				SavedPassword = SavedPassword
			};
		}
	}
}
