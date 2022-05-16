using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Yarukizero.Net.MakiMoki.Wpf.Converters {
	class WheelUpdatePositionConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if(value == null) {
				return VerticalAlignment.Bottom;
			}
			
			if(value is Behaviors.WheelUpdateBehavior.WheelUpdatePosition p) {
				return p switch {
					Behaviors.WheelUpdateBehavior.WheelUpdatePosition.Top => VerticalAlignment.Top,
					Behaviors.WheelUpdateBehavior.WheelUpdatePosition.Bottom => VerticalAlignment.Bottom,
					Behaviors.WheelUpdateBehavior.WheelUpdatePosition.Default => VerticalAlignment.Bottom,
					_ => throw new ArgumentException("値不正。", nameof(value))
				};
			}

			throw new ArgumentException("型不正。", nameof(value));
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class WheelUpdateVisibleConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if(value == null) {
				return Visibility.Collapsed;
			}

			if(value is string s) {
				return string.IsNullOrEmpty(s) switch {
					true => Visibility.Collapsed,
					false => Visibility.Visible
				};
			}

			if(value is Model.BindableFutaba bf) {
				return bf.IsDie.Value switch {
					true => Visibility.Collapsed,
					false => Visibility.Visible
				};
			}
			throw new ArgumentException("型不正。", nameof(value));
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
