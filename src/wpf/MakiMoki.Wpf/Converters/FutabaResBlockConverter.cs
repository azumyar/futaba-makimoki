using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Yarukizero.Net.MakiMoki.Wpf.Converters {

	class FutabaResItemHeaderTextConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if(value is string s) {
				return System.Net.WebUtility.HtmlDecode(s);
			}
			throw new ArgumentException("型不正。", nameof(value));
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class FutabaResItemCommentHtmlConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if(values.Any()) {
				return values[0] switch {
					Model.BindableFutabaResItem ri => new Controls.FutabaCommentBlock.CommentItem() { Value = ri },
					_ => null
				};
			}
			var sb = new System.Text.StringBuilder()
				.AppendJoin(',', values.Select(x => x?.GetType().FullName ?? "(null)").ToArray()).AppendLine()
				.AppendJoin(Environment.NewLine, values.Select(x => x?.ToString() ?? "(null)").ToArray());
			throw new ArgumentException($"型不正。{Environment.NewLine}{sb}", nameof(values));
		}
		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
