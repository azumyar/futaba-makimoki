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
	class ViewModelConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			var vm = Application.Current.MainWindow?.DataContext;
			if(parameter is string p) {
				return vm?.GetType().GetProperty(p)?.GetValue(vm);
			}
			return vm;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
	class TabItemWidthConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if((values == null) || (values.Length != 2)) {
				throw new ArgumentException("型不正。", "values");
			}
			if(values[0] == null && (values[1] is double)) {
				return values[1];
			} else if((values[0] is IEnumerable<Model.TabItem> ti) && (values[1] is double aw)) {
				return aw / ti.Count() - 1; // 端数が出ると全部足したときに aw を超えるので切り捨て+余裕を持たせるために1引く
			}
			throw new ArgumentException("型不正。", "values");
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}


	class TabItemCatalogConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if(value is IEnumerable<Model.TabItem> ti) {
				return ti.Where(x => x.Futaba.Value.Url.IsCatalogUrl);
			}
			throw new ArgumentException("型不正。", "value");
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}


	class TabItemThreadConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if((values.Length == 2)
				&& values[1] is IEnumerable<Model.TabItem> ti) {
				//return ti;
				if(values[0] is Model.BindableFutaba f) {
					return ti.Where(x => x.Futaba.Value.Url.IsThreadUrl && (x.Futaba.Value.Url.BaseUrl == f.Url.BaseUrl));
				}
				return ti;
			}
			throw new ArgumentException("型不正。", "values");
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class ThreadDieOpacityConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if(values.Length == 2) {
				if((values[0] is Model.BindableFutaba bf) && (values[1] is double d)) {
					return bf.IsDie.Value ? d : 1.0;
				}
				return 1.0;
			}
			throw new ArgumentException("型不正。", "values");
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class ThreadNewResVisibleConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if(values.Length == 2) {
				if((values[0] is int i1) && (values[1] is int i2) && (0 <= i1)) { // スレ情報取得前のi1の初期値は-1
					return i1 < i2 ? Visibility.Visible : Visibility.Hidden;
					//return bf.ResCount.Value < bf.CatalogResCount.Value ? Visibility.Visible : Visibility.Hidden;
				}
				return Visibility.Hidden;
			}
			throw new ArgumentException("型不正。", "values");
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
