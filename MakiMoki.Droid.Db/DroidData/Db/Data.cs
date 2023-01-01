using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using SQLite;

namespace Yarukizero.Net.MakiMoki.Droid.DroidData.Db {
	public class TableOject {
		[Column("time")]
		[NotNull]
		public string? TimeRaw { get; set; }

		[Ignore]
		public DateTime Time {
			get {
				return DateTime.ParseExact(this.TimeRaw, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
			}
			set {
				this.TimeRaw = value.ToString("yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
			}
		}

		public TableOject() { }
		public TableOject(DateTime time) {
			this.Time = time;
		}
	}


	[Table("Image")]
	public class ImageTable : TableOject {
		[PrimaryKey]
		[Column("url")]
		[NotNull]
		public string? Url { get; set; }
		[Column("data")]
		[NotNull]
		public byte[]? Data { get; set; }
		[Column("size")]
		[NotNull]
		public int Size { get; set; }

		public ImageTable() { }
		public ImageTable(string url, byte[] data) : base(DateTime.Now) {
			this.Url = url;
			this.Data = data;

			this.Size = data.Length;
		}
	}
}
