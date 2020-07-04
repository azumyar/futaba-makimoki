using System;
using System.Collections.Generic;
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


}
