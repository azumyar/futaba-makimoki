using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Yarukizero.Net.MakiMoki.Wpf.Converters {
	class FutabaNewResVisibilityConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if(value == null) {
				return null;
			}

			if(value is Data.FutabaContext.Item it) {
				return (0 < (it.CounterCurrent - it.CounterPrev)) ? Visibility.Visible : Visibility.Collapsed;
			}
			throw new ArgumentException("型不正。", "value");
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
