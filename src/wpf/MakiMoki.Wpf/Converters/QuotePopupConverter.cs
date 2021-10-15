using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Yarukizero.Net.MakiMoki.Wpf.Converters {
	class QuotePopupSourceConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if(value is Model.BindableFutabaResItem v1) {
				return new Model.BindableFutabaResItem[] { v1 };
			} else if(value is IEnumerable<Model.BindableFutabaResItem> v2) {
				return v2;
			} else {
				return Array.Empty<Model.BindableFutabaResItem>();
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
