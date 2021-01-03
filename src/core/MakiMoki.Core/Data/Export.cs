using System;
using System.Collections.Generic;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Data {
	public class ExportHolder {
		public string Title { get; }

		public bool IsName { get; }

		public ExportData[] ResItems { get; }

		public ExportHolder(string title, bool isNmae, ExportData[] resItems) {
			this.Title = title;
			this.IsName = isNmae;
			this.ResItems = resItems;
		}
	}

	public class ExportData {
		public string Subject { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string Comment { get; set; }

		public string No { get; set; }
		public string Date { get; set; }
		public int Soudane { get; set; }

		public string Host { get; set; }


		public string OriginalImageName { get; set; }
		public string ThumbnailImageData { get; set; }
	}
}
