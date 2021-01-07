using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Yarukizero.Net.MakiMoki.Wpf.Converters {

	class PostViewOpacityConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			var c = WpfConfig.WpfConfigLoader.SystemConfig;

			return c.IsEnabledOpacityPostView ? ((double)c.OpacityPostView) / 100.0d : 1.0d;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class PostViewMinWidthConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if(values.Length != 3) {
				throw new ArgumentException("型不正。", nameof(values));
			}

			var c = WpfConfig.WpfConfigLoader.SystemConfig;
			if((values[0] is double width) && (values[1] is double def)) {
				var val = (c.MinWidthPostView != 0) ? (double)c.MinWidthPostView : def;

				return Math.Min(Math.Max(def, val), width);
			}
			throw new ArgumentException("型不正。", nameof(values));
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class PostViewMaxWidthConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if(values.Length != 2) {
				throw new ArgumentException("型不正。", nameof(values));
			}

			var c = WpfConfig.WpfConfigLoader.SystemConfig;
			if(values[0] is double width) {
				var val = (c.MaxWidthPostView != 0) ? (double)c.MaxWidthPostView : width;

				return Math.Min(width, val);
			}
			throw new ArgumentException("型不正。", nameof(values));
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
