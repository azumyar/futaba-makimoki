using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Data.Compat {
	public class BoardConfig2021012000 : ConfigObject, IMigrateCompatObject {
		public static int CurrentVersion { get; } = 2021012000;

		[JsonProperty("boards", Required = Required.Always)]
		public BoardData2021012000[] Boards { get; protected set; }


		public virtual ConfigObject Migrate() {
			return BoardConfig.From(
				boards: this.Boards.Select(x => BoardData.From(
					name: x.Name,
					url: x.Url,
					defaultComment: x.DefaultComment,
					sortIndex: x.SortIndex,
					extra: BoardDataExtra.From(
						name: x.Extra.Name,
						resImage: x.Extra.ResImage,
						mailIp: x.Extra.MailIp,
						mailId: x.Extra.MailId,
						alwaysIp: x.Extra.AlwaysIp,
						alwaysId: x.Extra.AlwaysId,
						maxStoredRes: x.Extra.MaxStoredRes,
						maxStoredTime: x.Extra.MaxStoredTime,
						resTegaki: x.Extra.ResTegaki),
					makimokiExtra: MakiMokiBoardDataExtra.From(
						isEnabledPassiveReload: false),
					display: x.Display))
				.ToArray());
		}
	}

	public class BoardData2021012000 : JsonObject {
		[JsonProperty("name", Required = Required.Always)]
		public string Name { get; set; }

		[JsonProperty("url", Required = Required.Always)]
		public string Url { get; set; }

		[JsonProperty("default-comment", DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue("本文無し")]
		public string DefaultComment { get; set; }

		[JsonProperty("sort-index", DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(int.MaxValue)]
		public int SortIndex { get; set; }

		[JsonProperty("display", DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(true)]
		public bool Display { get; set; }

		[JsonProperty("extra", Required = Required.Always)]
		public BoardDataExtra2021012000 Extra { get; set; }
	}

	public class BoardDataExtra2021012000 : JsonObject {
		[JsonProperty("name", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(true)]
		public bool Name { get; private set; }

		[JsonProperty("enable-res-image", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(true)]
		public bool ResImage { get; set; }

		[JsonProperty("enable-mail-ip", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(false)]
		public bool MailIp { get; private set; }

		[JsonProperty("enable-mail-id", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(false)]
		public bool MailId { get; private set; }

		[JsonProperty("always-ip", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(false)]
		public bool AlwaysIp { get; private set; }

		[JsonProperty("always-id", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(false)]
		public bool AlwaysId { get; private set; }

		[JsonProperty("max-stored-res", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(0)]
		public int MaxStoredRes { get; private set; }

		[JsonProperty("max-stored-time", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(0)]
		public int MaxStoredTime { get; private set; }

		[JsonProperty("enable-res-tegaki", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(false)]
		public bool ResTegaki { get; private set; }
	}
}
