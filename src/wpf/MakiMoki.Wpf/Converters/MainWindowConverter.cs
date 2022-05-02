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

	class TabItemWidthConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if(values == null) {
				throw new ArgumentException("型不正。", nameof(values));
			}
			if(values.Length == 3) {
				if(values[0] == null && (values[1] is double)) {
					return values[1];
				} else if((values[0] is IEnumerable<Model.TabItem> ti) && (values[1] is double aw)) {
					return aw / ti.Count() - 1; // 端数が出ると全部足したときに aw を超えるので切り捨て+余裕を持たせるために1引く
				}
			}
			throw new ArgumentException("型不正。", nameof(values));
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
			throw new ArgumentException("型不正。", nameof(value));
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
				if((values[0] is Model.BindableFutaba f) && (ti.Count() != 0)) {
					return ti.Where(x => x.Futaba.Value.Url.IsThreadUrl && (x.Futaba.Value.Url.BaseUrl == f.Url.BaseUrl));
				}
				return ti;
			}
			throw new ArgumentException("型不正。", nameof(values));
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class TabLastMenuItemConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if(values.Length == 2) {
				if((values[0] is IEnumerable<Data.FutabaContext> fc)
					&& (values[1] is Model.BindableFutaba bf)) {

					if(bf.Url.IsCatalogUrl) {
						return fc.LastOrDefault()?.Url != bf.Url;
					} else {
						return fc.Where(x => x.Url.BaseUrl == bf.Url.BaseUrl).LastOrDefault()?.Url != bf.Url;
					}
				}
				return true;
			}
			throw new ArgumentException("型不正。", nameof(values));
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
			throw new ArgumentException("型不正。", nameof(values));
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class ThreadNewResVisibleConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if(values.Length == 3) {
				if((values[0] is int i1) && (values[1] is int i2) && (values[2] is int i3) && (0 <= i1)) { // スレ情報取得前のi1の初期値は-1
					if(i1 == i3) {
						return i1 < i2 ? Visibility.Visible : Visibility.Hidden;
					} else {
						return Visibility.Visible;
					}
					//return bf.ResCount.Value < bf.CatalogResCount.Value ? Visibility.Visible : Visibility.Hidden;
				}
				return Visibility.Hidden;
			}
			throw new ArgumentException("型不正。", nameof(values));
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	/*
	class InformationItemConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if(value is IEnumerable<Data.Information> ti) {
				// Count() == 0の時ReverseするとAddOnSchedulerが機能しなくなる？
				return (ti.Count() != 0) ? ti.Reverse() : ti;
			}
			throw new ArgumentException("型不正。", nameof(value));
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class InformationObjectConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if(value is Model.BindableFutaba bf) {
				return new Model.InformationBindableExObject(bf);
			} else if((value is Data.FutabaContext fc) && fc.Url.IsThreadUrl) {
				var it = fc.ResItems.FirstOrDefault();
				if(it != null) {
					return new Model.InformationBindableExObject(
						WpfUtil.ImageUtil.CreateImage(WpfUtil.ImageUtil.GetImageCache(
							Util.Futaba.GetThumbImageLocalFilePath(fc.Url, it.ResItem.Res))));
				}
			} else if(value is Data.UrlContext c) {
				if(c.IsThreadUrl) {
					var f = Util.Futaba.Threads.Value
						.Where(x => x.Url == c)
						.FirstOrDefault();
					if((f != null) && f.ResItems.Any()) {
						return new Model.InformationBindableExObject(
							WpfUtil.ImageUtil.CreateImage(WpfUtil.ImageUtil.GetImageCache(
								Util.Futaba.GetThumbImageLocalFilePath(c, f.ResItems.First().ResItem.Res))));
					}
				}
			}

			return new Model.InformationBindableExObject();
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
	*/
}