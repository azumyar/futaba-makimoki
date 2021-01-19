using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Data.Compat {
	public class BoardConfig2020062900 : ConfigObject, IMigrateCompatObject {
		public static int CurrentVersion { get; } = 2020062900;

		[JsonProperty("bords", Required = Required.Always)]
		public BoardData2020062900[] Boards { get; protected set; }

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
					display: x.Display))
				.ToArray());
		}

		/*
		internal static BoardConfig CreateDefault() {
			return new BoardConfig() {
				Version = CurrentVersion,
				Boards = Array.Empty<BoardData>(),
			};
		}
		*/
	}

	public class CoreBoardConfig2020062900 : BoardConfig2020062900, IMigrateCompatObject {
		public static new int CurrentVersion { get; } = BoardConfig2020062900.CurrentVersion;

		[JsonProperty("max-file-size", Required = Required.Always)]
		public int MaxFileSize { get; private set; }

		public override ConfigObject Migrate() {
			return CoreBoardConfig.From(
				maxFileSize: this.MaxFileSize,
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
					display: x.Display))
				.ToArray());
		}

		/*
		internal new static CoreBoardConfig CreateDefault() {
			return new CoreBoardConfig() {
				Version = CurrentVersion,
				Boards = Array.Empty<BoardData>(),
			};
		}

		internal static CoreBoardConfig CreateAppInstance(CoreBoardConfig config, BoardData[] margeData) {
			return new CoreBoardConfig() {
				Version = CurrentVersion,
				MaxFileSize = config.MaxFileSize,
				Boards = margeData,
			};
		}
		*/
	}

	public class BoardData2020062900 : JsonObject {
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
		public BoardDataExtra2020062900 Extra { get; set; }

		/*
		public static BoardData2020062900 From(
			string name,
			string url,
			string defaultComment,
			int sortIndex,
			BoardDataExtra extra,
			bool display = true) {

			System.Diagnostics.Debug.Assert(!string.IsNullOrWhiteSpace(name));
			System.Diagnostics.Debug.Assert(!string.IsNullOrWhiteSpace(url));
			System.Diagnostics.Debug.Assert(!string.IsNullOrWhiteSpace(defaultComment));
			System.Diagnostics.Debug.Assert(extra != null);

			return new BoardData() {
				Name = name,
				Url = url,
				DefaultComment = defaultComment,
				SortIndex = sortIndex,
				Extra = extra,
				Display = display,
			};
		}
		*/
	}

	public class BoardDataExtra2020062900 : JsonObject {
		[JsonProperty("name", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(true)]
		public bool Name { get; set; }

		[JsonProperty("enable-res-image", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(true)]
		public bool ResImage { get; set; }

		[JsonProperty("enable-mail-ip", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(false)]
		public bool MailIp { get; set; }

		[JsonProperty("enable-mail-id", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(false)]
		public bool MailId { get; set; }

		[JsonProperty("always-ip", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(false)]
		public bool AlwaysIp { get; set; }

		[JsonProperty("always-id", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(false)]
		public bool AlwaysId { get; set; }

		[JsonProperty("max-stored-res", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(0)]
		public int MaxStoredRes { get; set; }

		[JsonProperty("max-stored-time", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(0)]
		public int MaxStoredTime { get; set; }

		[JsonProperty("enable-res-tegaki", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(false)]
		public bool ResTegaki { get; set; }

		/*
		public static BoardDataExtra From(
			bool name,
			bool resImage,
			bool mailIp,
			bool mailId,
			bool alwaysIp,
			bool alwaysId,
			int maxStoredRes,
			int maxStoredTime,
			bool resTegaki) {

			return new BoardDataExtra() {
				Name = name,
				ResImage = resImage,
				MailIp = mailIp,
				MailId = mailId,
				AlwaysIp = alwaysIp,
				AlwaysId = alwaysId,
				MaxStoredRes = maxStoredRes,
				MaxStoredTime = maxStoredTime,
				ResTegaki = resTegaki,
			};
		}

		[JsonIgnore]
		public bool NameValue => Name;

		[JsonIgnore]
		public bool ResImageValue => ResImage;

		[JsonIgnore]
		public bool MailIpValue => MailIp;

		[JsonIgnore]
		public bool MailIdValue => MailId;

		[JsonIgnore]
		public bool AlwaysIpValue => AlwaysIp;

		[JsonIgnore]
		public bool AlwaysIdValue => AlwaysId;
		*/
	}
}
