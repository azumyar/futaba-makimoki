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
			throw new ArgumentException("型不正。", nameof(value));
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class FutabaCatalogToolTipResCountConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if(value == null) {
				return Visibility.Collapsed;
			}

			if(value is Model.BindableFutabaResItem f) {
				return (f.Raw.Value.ResItem.Isolate ?? false)
					? "隔離" : $"{ f.Raw.Value.CounterCurrent }レス"; 
			}
			throw new ArgumentException("型不正。", nameof(value));
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
			throw new ArgumentException("型不正。", nameof(value));
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class FutabaCatalogItemBackgroundConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if((values.Length == 7) && (values[3] is System.Windows.Media.Color normalColor)) {
				if((values[0] is Model.BindableFutabaResItem f)
					&& (values[1] is string search)
					&& (values[2] is IEnumerable<FutabaContext> threads)
					&& (values[4] is System.Windows.Media.Color hitColor)
					&& (values[5] is System.Windows.Media.Color watchColor)
					&& (values[6] is System.Windows.Media.Color opendColor)) {
					if(threads.Select(x => x.ResItems.FirstOrDefault()?.ResItem.No)
						.Contains(f.Raw.Value.ResItem.No)) {

						return opendColor;
					}

					if(!string.IsNullOrEmpty(search) 
						&& Util.TextUtil.Comment2SearchText(f.Raw.Value.ResItem.Res.Com)
							.Contains(Util.TextUtil.Comment2SearchText(search))) {

						return hitColor;
					} else if(f.IsWatch.Value) {
						return watchColor;
					}
				}
				return normalColor; // スレを受信していない場合values[0]が設定されていないのでnormalColorを返す
			}

			throw new ArgumentException("型不正。", nameof(values));
		}
		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class FutabaOldResVisibilityConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if(value == null) {
				return Visibility.Collapsed;
			}

			if(value is Model.BindableFutabaResItem it) {
				var old = false;
				var curNo = long.TryParse(it.ThreadResNo.Value, out var cno) ? cno : 0;
				var lastNo = it.Parent.Value.ResItems
					.Select(x => long.TryParse(x.ThreadResNo.Value, out var lno) ? lno : 0)
					.Max();
				if((0 < curNo)
					&& (0 < lastNo) 
					&& ((0 < it.Bord.Value.Extra.MaxStoredRes) || (0 < it.Bord.Value.Extra.MaxStoredTime))) { 
				
					if(0 < it.Bord.Value.Extra.MaxStoredRes) {
						old = (it.Bord.Value.Extra.MaxStoredRes * 0.9) < (lastNo - curNo);
					}

					if(0 < it.Bord.Value.Extra.MaxStoredTime) {
						if((DateTime.Now - it.Raw.Value.ResItem.Res.NowDateTime)
							< TimeSpan.FromSeconds(it.Bord.Value.Extra.MaxStoredTime - (5 * 60))) {

							old = false;
						}
					}
				}
				return old ? Visibility.Visible : Visibility.Collapsed;
			}
			throw new ArgumentException("型不正。", nameof(value));
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
	class FutabaNewResVisibilityConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if(value == null) {
				return Visibility.Collapsed;
			}

			if(value is Data.FutabaContext.Item it) {
				return (!it.ResItem.IsolateValue && (0 < (it.CounterCurrent - it.CounterPrev))) ? Visibility.Visible : Visibility.Collapsed;
			}
			throw new ArgumentException("型不正。", nameof(value));
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	[Obsolete]
	class FutabaIdResVisibilityConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if(values.Length != 2) {
				throw new ArgumentException("型不正。", nameof(values));
			}

			if(values[1] is bool flag) {
				if(flag && (values[0] is Data.FutabaContext.Item it)) {
					var b1 = !string.IsNullOrEmpty(it.ResItem.Res.Id);
					var b2 = (it.ResItem.Res.Email != "id表示") || (it.ResItem.Res.Email != "ip表示");
					return (b1 && b2) ? Visibility.Visible : Visibility.Collapsed;
				}
			}
			return Visibility.Collapsed;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class FutabaMovieResVisibilityConverter : IMultiValueConverter {
		static string[] ext = null;
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if((values.Length == 2) && (values[0] is Data.FutabaContext.Item it)) {
				if(WpfConfig.WpfConfigLoader.SystemConfig.IsEnabledMovieMarker) {
					if(ext == null) {
						ext = Config.ConfigLoader.MimeFutaba.Types
							.Where(x => x.MimeContents == MimeContents.Video)
							.Select(x => x.Ext)
							.ToArray();
					}
					return ext.Contains(it.ResItem.Res.Ext.ToLower()) ? Visibility.Visible : Visibility.Collapsed;
				/*
				} else {
					return Visibility.Collapsed;
				*/
				}
			}
			return Visibility.Collapsed;
			//throw new ArgumentException("型不正。", nameof(values));
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class FutabaIsolateResVisibilityConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if((values.Length == 2) && (values[0] is Data.FutabaContext.Item it)) {
				if(WpfConfig.WpfConfigLoader.SystemConfig.IsVisibleCatalogIsolateThread) {
					return it.ResItem.IsolateValue ? Visibility.Visible : Visibility.Collapsed;
				/*
				} else {
					return Visibility.Collapsed;
				*/
				}
			}
			return Visibility.Collapsed;
			//throw new ArgumentException("型不正。", nameof(values));
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class FutabaIsolateResCountVisibilityConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if(value == null) {
				return Visibility.Hidden;
			}

			if(value is Data.FutabaContext.Item it) {
				return it.ResItem.IsolateValue ? Visibility.Hidden : Visibility.Visible;
			}
			throw new ArgumentException("型不正。", nameof(value));
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
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

			throw new ArgumentException("型不正。", nameof(values));
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class FutabaCatalogItemFilterConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if((values.Length == 3) && (values[0] is IEnumerable<Model.BindableFutabaResItem> en)) {
				var en2 = en.Where(x => !x.IsNg.Value && !x.IsHidden.Value && !x.IsNgImageHidden.Value);
				var en3 = en2.Where(x => x.IsWatch.Value).Concat(en2.Where(x => !x.IsWatch.Value)).ToArray();
				if(values[1] is string filter && !string.IsNullOrEmpty(filter)) {
					var f = Util.TextUtil.Filter2SearchText(filter);
					var sr = en3.Select<Model.BindableFutabaResItem, (string Text, Model.BindableFutabaResItem Raw)>(
						x => (Util.TextUtil.Comment2SearchText(x.Raw.Value.ResItem.Res.Com), x))
						.Where(x => x.Text.Contains(f))
						.Select(x => x.Raw)
						.ToArray();
					switch(WpfConfig.WpfConfigLoader.SystemConfig.CatalogSearchResult) {
					case PlatformData.CatalogSearchResult.Default:
						return sr;
					case PlatformData.CatalogSearchResult.Nijiran: {
							var t = sr.Select(y => y.ThreadResNo.Value).ToArray();
							return sr.Concat(en3.Where(x => !t.Contains(x.ThreadResNo.Value))).ToArray();
						}
					}
					throw new InvalidOperationException();
				} else {
					return en3;
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

			throw new ArgumentException("型不正。", nameof(values));
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
	
			throw new ArgumentException("型不正。", nameof(values));
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
			throw new ArgumentException("型不正。", nameof(value));
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class FutabaResItemBackgroundConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if(values.Length != 4) {
				throw new ArgumentException("型不正。", nameof(values));
			}

			if((values[0] is Model.BindableFutabaResItem f)
				&& (values[1] is string search)
				&& (values[2] is System.Windows.Media.Color normalColor)
				&& (values[3] is System.Windows.Media.Color hitColor)) {

				return (!string.IsNullOrEmpty(search) && Util.TextUtil.Comment2SearchText(f.Raw.Value.ResItem.Res.Com).Contains(Util.TextUtil.Comment2SearchText(search)))
					? hitColor : normalColor;
			}
			return values[2]; // スレを受信していない場合values[0]が設定されていないのでnormalColorを返す
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

					return (res.Where(x => x.BoardUrl == f.Parent.Value.Url.BaseUrl)
						.Where(x => x.Res.No == f.ThreadResNo.Value)
						.FirstOrDefault() != null)
							? values[3] : values[2];
				}
				return values[2];
			}

			throw new ArgumentException("型不正。", nameof(values));
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

			throw new ArgumentException("型不正。", nameof(value));
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
			throw new ArgumentException("型不正。", nameof(value));
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

					return (r.Raw.Value.ResItem.Res.Rsc == f.ResCount.Value) ? Visibility.Visible : Visibility.Collapsed;
				}
				return Visibility.Collapsed;
			}

			throw new ArgumentException("型不正。", nameof(values));
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
			throw new ArgumentException("型不正。", nameof(value));
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class FutabaResItemOldColorConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if((values.Length == 3) && (values[1] is System.Windows.Media.Color normalColor)) {
				if((values[0] is bool isOld) && (values[2] is System.Windows.Media.Color oldColor)) {
					return isOld switch {
						true => oldColor,
						false => normalColor,
					};
				}
				return normalColor; // スレを受信していない場合values[0]が設定されていないのでnormalColorを返す
			}

			throw new ArgumentException("型不正。", nameof(values));
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

			throw new ArgumentException("型不正。", nameof(values));
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

			throw new ArgumentException("型不正。", nameof(values));
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class FutabaEnabledTegakiResConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if(values.Length == 2) {
				if(Canvas98.Canvas98Util.Util.IsEnabledCanvas98()
					&& (values[0] is Model.BindableFutaba f)
					&& (f.Raw.Value.Board.Extra.ResTegaki)) {

					return System.Windows.Visibility.Visible;
				} else {
					return System.Windows.Visibility.Collapsed;
				}
			}

			throw new ArgumentException("型不正。", nameof(values));
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
