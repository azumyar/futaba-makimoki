using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Yarukizero.Net.MakiMoki.Wpf.Converters {
	class BackgroundToForegroundColorConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if(4 < values.Length) {
				throw new ArgumentException("型不正。", "values");
			}

			if((values[0] is SolidColorBrush back)
				&& (values[1] is PlatformData.StyleType type)
				&& (values[2] is Color white)
				&& (values[3] is Color black)) {

				return (back.Color.A == 0) ? black : WpfUtil.ImageUtil.GetTextColor(back.Color, white, black, type);
			}

			// values[0]が設定されていない場合があるので固定値を返す
			return values[3];
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

}
