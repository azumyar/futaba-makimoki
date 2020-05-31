using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Yarukizero.Net.MakiMoki.Data;

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

	class FutabaCatalogItemOpenedColorConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if((values.Length == 4) && (values[2] is System.Windows.Media.Color normalColor)) {
				if((values[0] is Model.BindableFutabaResItem f) 
					&& (values[1] is IEnumerable<FutabaContext> threads)
					&& (values[3] is System.Windows.Media.Color opendColor)) {
					return threads.Select(x => x.ResItems.FirstOrDefault()?.ResItem.No)
						.Contains(f.Raw.Value.ResItem.No)
							? opendColor : normalColor;
				}
				return normalColor; // スレを受信していない場合values[0]が設定されていないのでnormalColorを返す
			}

			throw new ArgumentException("型不正。", "value");
		}
		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class FutabaCatalogStyleConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if(values.Length == 3) {
				if(values[0] is bool f) {
					return f ? values[2] : values[1];
				}
				return values[1];
			}

			throw new ArgumentException("型不正。", "value");
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class FutabaCatalogItemFilterConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if((values.Length == 2) && (values[0] is IEnumerable<Model.BindableFutabaResItem> en)) {
				if(values[1] is string filter && !string.IsNullOrEmpty(filter)) {
					var f = Util.TextUtil.Filter2SearchText(filter);
					return en.Select<Model.BindableFutabaResItem, (string Text, Model.BindableFutabaResItem Raw)>(
						x => (Util.TextUtil.Comment2SearchText(x.Raw.Value.ResItem.Res.Com), x))
						.Where(x => x.Text.Contains(f))
						.Select(x => x.Raw)
						.ToArray();
				}
			}

			// まだカタログを取得していない場合values[0]が壊れているので例外は投げない
			return values[0];
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class FutabaCatalogSortCheckedConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if((values.Length == 2)
				&& (values[0] is Data.CatalogSortItem i1)
				&& (values[1] is Data.CatalogSortItem i2)) {

				return i1.ApiValue == i2.ApiValue;
			}

			throw new ArgumentException("型不正。", "value");
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class FutabaCatalogSortParamConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if((values.Length == 2)
				&& (values[0] is Data.CatalogSortItem i1)
				&& (values[1] is Model.BindableFutaba i2)) {

				return (i1, i2);
			}
	
			throw new ArgumentException("型不正。", "value");
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
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

	class FutabaResItemBackgroundConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if((values.Length == 4) && (values[2] is System.Windows.Media.Color normalColor)) {
				if((values[0] is Model.BindableFutabaResItem f)
					&& (values[1] is string search)
					&& (values[3] is System.Windows.Media.Color hitColor)) {
					return (!string.IsNullOrEmpty(search) && Util.TextUtil.Comment2SearchText(f.Raw.Value.ResItem.Res.Com).Contains(Util.TextUtil.Comment2SearchText(search)))
							? hitColor : normalColor;
				}
				return normalColor; // スレを受信していない場合values[0]が設定されていないのでnormalColorを返す
			}

			throw new ArgumentException("型不正。", "value");
		}
		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class FutabaResItemIndexConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if(values.Length == 4) {
				if((values[0] is Model.BindableFutabaResItem f)
					&& (values[1] is IEnumerable<Data.PostedResItem> res)) {

					return (res.Where(x => x.BordUrl == f.Parent.Value.Url.BaseUrl)
						.Where(x => x.Res.No == f.ThreadResNo.Value)
						.FirstOrDefault() != null)
							? values[3] : values[2];
				}
				return values[2];
			}

			throw new ArgumentException("型不正。", "value");
		}
		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
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

	class FutabaResItemFooterVisibleConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if(values.Length == 2) {
				// BindableFutabaResItemは使いまわされるので更新時検知されない親のBindableFutabaもパラメータにもってくる
				if((values[0] is Model.BindableFutabaResItem r)
					&& (values[1] is Model.BindableFutaba f)) {

					return r.Index.Value == f.ResCount.Value ? Visibility.Visible : Visibility.Collapsed;
				}
				return Visibility.Collapsed;
			}

			throw new ArgumentException("型不正。", "value");
		}
		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
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

	class FutabaResItemCopyTextBoxEventConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if((values.Length == 2) && (values[1] is System.Windows.Controls.TextBox t)) {
				if(values[0] is Model.BindableFutaba f) {
					return (f, t);
				} else {
					return (default(Model.BindableFutaba), t);
				}
			}

			throw new ArgumentException("型不正。", "value");
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
