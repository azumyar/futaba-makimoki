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
	class FutabaCatalogVisibleConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if(value == null) {
				return Visibility.Collapsed;
			}

			if(value is Data.FutabaContext f) {
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
			if(value == null) {
				return Visibility.Collapsed;
			}

			if(value is Data.FutabaContext f) {
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
			if(value == null) {
				return Visibility.Collapsed;
			}

			if(value is string v1) {
				return !string.IsNullOrEmpty(v1) ? Visibility.Visible : Visibility.Collapsed;
			} else if(value is int v2) {
				return (0 < v2) ? Visibility.Visible : Visibility.Collapsed;
			}
			throw new ArgumentException("型不正。", "value");
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class FutabaResItemIdTextConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if(value == null) {
				return "";
			}

			if(value is Model.BindableFutabaResItem ri) {
				var id = ri.Raw.Value.ResItem.Res.Id;
				var no = ri.Raw.Value.ResItem.No;
				if(!string.IsNullOrEmpty(id)) {
					var array = ri.Parent.Value.ResItems
						.Where(x => id == x.Raw.Value.ResItem.Res.Id)
						.Select(x => x.Raw.Value.ResItem.No)
						.ToArray();
					for(var i=0; i<array.Length; i++) {
						if(array[i] == no) {
							return $"{ id }({ i + 1 }/{ array.Length })";
						}
					}
				}
				return "";
			}

			throw new ArgumentException("型不正。", "value");
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class FutabaResItemNowConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if(value == null) {
				return Visibility.Collapsed;
			}

			if(value is string v1) {
				return Regex.Replace(v1, @"<[^>*]>", "", RegexOptions.Multiline);
			}
			throw new ArgumentException("型不正。", "value");
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class FutabaResItemFooterVisibleConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if(value == null) {
				return Visibility.Collapsed;
			}

			if(value is Model.BindableFutabaResItem v) {
				return v.Index.Value == v.Parent.Value.ResCount.Value ? Visibility.Visible : Visibility.Collapsed;
			}
			throw new ArgumentException("型不正。", "value");
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class FutabaResItemNextButtonEnabledConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if(value == null) {
				return Visibility.Collapsed;
			}

			if(value is Model.BindableFutaba v) {
				return !v.IsDie.Value; // && !v.IsMaxRes.Value; そうだねが更新できなくなるので保留
			}
			throw new ArgumentException("型不正。", "value");
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class FutabaResItemOldColorConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if((values.Length == 3) && (values[1] is System.Windows.Media.Color normalColor)) {
				if((values[0] is Model.BindableFutaba f) && (values[2] is System.Windows.Media.Color oldColor)) {
					return f.IsOld.Value ? oldColor : normalColor;
				}
				return normalColor; // スレを受信していない場合values[0]が設定されていないのでnormalColorを返す
			}

			throw new ArgumentException("型不正。", "value");
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class FutabaResItemResCountColorConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if((values.Length == 3) && (values[1] is System.Windows.Media.Color normalColor)) {
				if((values[0] is Model.BindableFutaba f) && (values[2] is System.Windows.Media.Color oldColor)) {
					return f.IsMaxRes.Value ? oldColor : normalColor;
				}
				return normalColor; // スレを受信していない場合values[0]が設定されていないのでnormalColorを返す
			}

			throw new ArgumentException("型不正。", "value");
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
