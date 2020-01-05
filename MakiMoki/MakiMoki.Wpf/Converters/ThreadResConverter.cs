using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Yarukizero.Net.MakiMoki.Wpf.Converters {
	class FutabaCatalogVisibleConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if(value == null) {
				return Visibility.Collapsed;
			}

			if (value is Data.FutabaContext f) {
				return string.IsNullOrWhiteSpace(f.Url.ThreadNo) ? Visibility.Visible : Visibility.Hidden;
			}
			throw new ArgumentException("型不正。", "value");
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class FutabaThreadResVisibleConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if (value == null) {
				return Visibility.Collapsed;
			}

			if (value is Data.FutabaContext f) {
				return !string.IsNullOrWhiteSpace(f.Url.ThreadNo) ? Visibility.Visible : Visibility.Hidden;
			}
			throw new ArgumentException("型不正。", "value");
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class FutabaResItemVisibleConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if (value == null) {
				return Visibility.Collapsed;
			}

			if (value is string v1) {
				return !string.IsNullOrEmpty(v1) ? Visibility.Visible : Visibility.Collapsed;
			} else if (value is int v2) {
				return (0 < v2) ? Visibility.Visible : Visibility.Collapsed;
			}
			throw new ArgumentException("型不正。", "value");
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class FutabaResItemNowConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if (value == null) {
				return Visibility.Collapsed;
			}

			if (value is string v1) {
				return Regex.Replace(v1, @"<[^>*]>", "", RegexOptions.Multiline);
			}
			throw new ArgumentException("型不正。", "value");
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
