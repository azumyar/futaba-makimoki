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
				if(values[0] is Model.BindableFutabaResItem ri) {
					return new Controls.FutabaCommentBlock.CommentItem() { Value = ri };
				} else if(values[0] == null) { // まだインスタンス生成されていない場合nullが来る
					return null;
				}
			}

			throw new ArgumentException("型不正。", nameof(values));
		}
		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
