using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Yarukizero.Net.MakiMoki.Wpf.Converters {
	class CatalogTextConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if (value is Data.ResItem r) {
				var s1 = Util.TextUtil.RowComment2Text(r.Com);
				var s2 = Util.TextUtil.RemoveCrLf(s1);

				return Util.TextUtil.SafeSubstring(s2, 4);
			}
			throw new ArgumentException("型不正。", "value");
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
