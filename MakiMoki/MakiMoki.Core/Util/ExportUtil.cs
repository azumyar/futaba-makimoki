using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Util {
	public static class ExportUtil {
		public static void ExportHtml(Stream exportStream, Data.ExportHolder holder) {
			StringBuilder appendResHeader(StringBuilder export, Data.ExportData it, int index) {
				export.Append(@"<div>");
				export.AppendFormat(@"<span class='res-count'>{0}</span>", index);
				if(holder.IsName) {
					export.AppendFormat(@"<span class='res-subject'>{0}</span>", it.Subject)
						.Append(@"Name ")
						.AppendFormat(@"<span class='res-name'>{0}", it.Name);
					if(!string.IsNullOrEmpty(it.Email)) {
						export.AppendFormat(@"<span class='res-email'>[{0}]</span>", it.Email);
					}
					export.Append(@"</span>");
				}
				export.AppendFormat(@"<span class='res-date'>{0}</span>", it.Date);
				if(!holder.IsName && !string.IsNullOrEmpty(it.Email)) {
					export.AppendFormat(@"<span class='res-email'>[{0}]</span>", it.Email);
				}
				if(0 < it.Soudane) {
					export.AppendFormat(@"<span class='res-soudane'>そうだねx{0}</span>", it.Soudane);
				}
				export.AppendLine(@"</div>");
				return export;
			}

			var exportHtml = new StringBuilder()
				.AppendLine(@"<!DOCTYPE html>")
				.AppendLine(@"<html>")
				.AppendLine(@"<head>")
				.AppendLine(@"<meta http-equiv='Content-Type' content='text/html; charset=utf-8' />")
				.Append(@"<title>").Append(holder.Title).Append(@"</title>").AppendLine()
				.AppendLine(@"<style>")
				.AppendLine(@"body { background-color: rgb(255, 255, 238); color: rgb(128, 0, 0); }")
				.AppendLine(@".master .res-image { float: left; margin-right: 1em; }")
				.AppendLine(@".res { display: table; background-color: #F0E0D6; margin: 0.5em 1em 0 1em; padding: 4px; min-width: 480px; }")
				.AppendLine(@".res .res-body { display: flex; }")
				.AppendLine(@".res-comment-body { margin: 1em 2em 1em 2em; word-wrap: break-word; max-width: 800px; min-width: 150px; }")
				.AppendLine(@".res-count, .res-subject, .res-name, .res-date { margin-right: 0.5em; }")
				.AppendLine(@".res-soudane { margin: 0 1em 0 1em }")
				.AppendLine(@".res-subject { color: #cc1105; font-weight: bold; }")
				.AppendLine(@".res-name { color: #117743; font-weight: bold; }")
				.AppendLine(@".res-email { color: rgb(0, 92, 230); }")
				.AppendLine(@"</style>")
				.AppendLine(@"</head>")
				.AppendLine(@"<body>")
				.Append(@"<h1>").Append(holder.Title).Append(@"</h1>").AppendLine()
				.AppendLine(@"<hr>");

			{
				var it = holder.ResItems[0];

				exportHtml.AppendLine(@"<div class='master'>");
				if(!string.IsNullOrWhiteSpace(it.OriginalImageName)) {
					exportHtml.Append(@"<div>画像ファイル名：").Append(it.OriginalImageName).Append(@"</div>");
				}
				exportHtml.AppendLine(@"<div class='res-body'>");
				if(!string.IsNullOrWhiteSpace(it.OriginalImageName)) {
					exportHtml.AppendLine(@"<div class='res-image'>")
						.AppendFormat(@"<img src='{0}'>", it.ThumbnailImageData).AppendLine()
						.AppendLine("</div>");
				}
				{
					exportHtml.AppendLine(@"<div class='res-comment'>");
					appendResHeader(exportHtml, it, 0);
					exportHtml.AppendLine(@"<div class='res-comment-body'>")
						.AppendLine(it.Comment)
						.AppendLine(@"</div>");

					exportHtml.AppendLine(@"</div>");
				}

				exportHtml.AppendLine(@"</div>")
					.AppendLine(@"</div>");

			}


			for(var i=1; i<holder.ResItems.Length; i++) {
				var it = holder.ResItems[i];

				exportHtml.AppendLine(@"<div class='res'>");
				appendResHeader(exportHtml, it, i);
				exportHtml.AppendLine(@"<div class='res-body'>");
				if(!string.IsNullOrWhiteSpace(it.OriginalImageName)) {
					exportHtml.AppendLine(@"<div class='res-image'>")
						.AppendLine(@"<div>").Append(it.OriginalImageName).Append(@"</div>")
						.AppendFormat(@"<img src='{0}'>", it.ThumbnailImageData).AppendLine()
						.AppendLine("</div>");
				}
				{
					exportHtml.AppendLine(@"<div class='res-comment'>");
					exportHtml.AppendLine(@"<div class='res-comment-body'>")
						.AppendLine(it.Comment)
						.AppendLine(@"</div>");

					exportHtml.AppendLine(@"</div>");
				}

				exportHtml.AppendLine(@"</div>")
					.AppendLine(@"</div>");

			}

			exportHtml.AppendLine("</body>")
				.AppendLine("</html>");

			var b = Encoding.UTF8.GetBytes(exportHtml.ToString());
			exportStream.Write(b, 0, b.Length);
		}

	}
}
