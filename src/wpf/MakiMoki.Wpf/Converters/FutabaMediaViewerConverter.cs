using Reactive.Bindings.Interactivity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Yarukizero.Net.MakiMoki.Wpf.Converters {
	class MediaEnumQuickSaveItemConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if(values.Length != 2) {
				throw new ArgumentException("型不正。", "values");
			}

			if((values[0] is PlatformData.FutabaMedia fm) && (WpfConfig.WpfConfigLoader.SystemConfig.MediaExportPath.Length != 0)) {
				return WpfConfig.WpfConfigLoader.SystemConfig.MediaExportPath
					.Select(x => new PlatformData.MediaQuickSaveItem(fm, x))
					.ToArray();
			} else {
				return new[] { new PlatformData.MediaQuickSaveItem(), };
			}
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
