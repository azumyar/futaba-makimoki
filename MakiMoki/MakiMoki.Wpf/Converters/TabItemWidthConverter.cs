using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Yarukizero.Net.MakiMoki.Wpf.Converters {
	class TabItemWidthConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if((values == null) || (values.Length != 2)) {
				throw new ArgumentException("型不正。", "values");
			}
			if((values[0] is Model.TabItem[] ti) && (values[1] is double aw)) {
				return aw / ti.Length - 1; // 端数が出ると全部足したときに aw を超えるので切り捨て+余裕を持たせるために1引く
			}
			throw new ArgumentException("型不正。", "values");
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
