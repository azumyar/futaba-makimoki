using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Yarukizero.Net.MakiMoki.Wpf.Converters {
	class BackgroundToForegroundColorConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if(9 < values.Length) {
				throw new ArgumentException("型不正。", nameof(values));
			}

			//System.Diagnostics.Debug.WriteLine($"{ values.Length }");
			if(9 == values.Length) {
				if((values[0] is Color back)
					&& (values[2] is Color white)
					&& (values[3] is Color black)
					&& (values[4] is Color orig)
					&& (values[5] is Color origMouseOver)
					&& (values[6] is Color origMousePress)
					&& (values[7] is FrameworkElement el)
					&& (values[8] is VisualStateGroup g)) {

					Color GetColor() {
						if(el.IsMouseCaptured && (System.Windows.Input.Mouse.LeftButton == System.Windows.Input.MouseButtonState.Pressed)) {
							return origMousePress;
						} else if(el.IsMouseOver) {
							return origMouseOver;
						} else {
							return orig;
						}
					}
					double GetThreshold() {
						if(el.IsMouseCaptured && (System.Windows.Input.Mouse.LeftButton == System.Windows.Input.MouseButtonState.Pressed)) {
							return 0.4;
						} else if(el.IsMouseOver) {
							return 0.4;
						} else {
							return 0.5;
						}
					}
					var isAnimate = false;
					try {
						isAnimate = g.CurrentState?.Storyboard?.GetCurrentState(el) == System.Windows.Media.Animation.ClockState.Active;
					}
					catch(InvalidOperationException) { }

					var t = GetThreshold();
					var b = ((back.A == 0) || isAnimate) ? GetColor() : back;
					var foreground = (b.A == 0) ? black : WpfUtil.ImageUtil.GetTextColor(b, white, black, t);
					//System.Diagnostics.Debug.WriteLine($"return:{ foreground }");
					return foreground;
				}
			}
			if(7 == values.Length) {
				/*
				if(7 == values.Length) {
					System.Diagnostics.Debug.WriteLine($"{ values[0] }");
					System.Diagnostics.Debug.WriteLine($"{ values[1] }");
					System.Diagnostics.Debug.WriteLine($"{ values[2] }");
					System.Diagnostics.Debug.WriteLine($"{ values[3] }");
					System.Diagnostics.Debug.WriteLine($"{ values[4] }");
					System.Diagnostics.Debug.WriteLine($"{ values[5] }");
					System.Diagnostics.Debug.WriteLine($"{ values[6] }");
				}
				*/
				if((values[0] is Color back)
					&& (values[2] is Color white)
					&& (values[3] is Color black)
					&& (values[4] is Color orig)
					&& (values[5] is FrameworkElement el)
					&& (values[6] is VisualStateGroup g)) {

					var isAnimate = false;
					try {
						isAnimate = g.CurrentState?.Storyboard?.GetCurrentState(el) == System.Windows.Media.Animation.ClockState.Active;
					}
					catch(InvalidOperationException) { }

					var b = ((back.A == 0) || isAnimate) ? orig : back;
					var foreground = (b.A == 0) ? black : WpfUtil.ImageUtil.GetTextColor(b, white, black);
					//System.Diagnostics.Debug.WriteLine($"return:{ foreground }");
					return foreground;
				}
			}

			{
				if((values[0] is Color back)
					&& (values[2] is Color white)
					&& (values[3] is Color black)) {

					return (back.A == 0) ? black : WpfUtil.ImageUtil.GetTextColor(back, white, black);
				}
			}

			{
				if((values[0] is SolidColorBrush back)
					&& (values[2] is Color white)
					&& (values[3] is Color black)) {

					return (back.Color.A == 0) ? black : WpfUtil.ImageUtil.GetTextColor(back.Color, white, black);
				}
			}
			// values[0]が設定されていない場合があるので固定値を返す
			return values[3];
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	class MaterialForegroundColorConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if(values.Length != 4) {
				throw new ArgumentException("型不正。", nameof(values));
			}

			if(values[0] is SolidColorBrush @default && (@default.Color.A != 0)) {
				return values[0];
			}

			if((values[1] is Brush accent)
				&& (values[2] is Color white)
				&& (values[3] is Color black)) {

				if(accent is SolidColorBrush sb) {
					return WpfUtil.ImageUtil.ToHsv(sb.Color) switch {
						var hsv when hsv.V < 50 => new SolidColorBrush(white),
						_ => new SolidColorBrush(black)
					};
				} else {
					return new SolidColorBrush(white);
				}
			}
			return values[0];
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}

	}

	class DisableBrushConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if(value == null) {
				return Colors.Transparent;
			}

			if(value is SolidColorBrush sb) {
				var hsv = WpfUtil.ImageUtil.ToHsv(sb.Color);
				return new SolidColorBrush(WpfUtil.ImageUtil.HsvToRgb(hsv.H, 0, hsv.V * 0.4));
			}
			throw new ArgumentException("型不正。", nameof(value));
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

}
