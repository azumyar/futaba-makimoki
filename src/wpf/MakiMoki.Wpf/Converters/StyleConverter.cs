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
					&& (values[1] is PlatformData.StyleType type)
					&& (values[2] is Color white)
					&& (values[3] is Color black)
					&& (values[4] is Color orig)
					&& (values[5] is Color origMouseOver)
					&& (values[6] is Color origMousePress)
					&& (values[7] is Control el)
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
					var isAnimate = false;
					try {
						isAnimate = g.CurrentState?.Storyboard?.GetCurrentState(el) == System.Windows.Media.Animation.ClockState.Active;
					}
					catch(InvalidOperationException) { }

					var b = ((back.A == 0) || isAnimate) ? GetColor() : back;
					var foreground = (b.A == 0) ? black : WpfUtil.ImageUtil.GetTextColor(b, white, black, type);
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
					&& (values[1] is PlatformData.StyleType type)
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
					var foreground = (b.A == 0) ? black : WpfUtil.ImageUtil.GetTextColor(b, white, black, type);
					//System.Diagnostics.Debug.WriteLine($"return:{ foreground }");
					return foreground;
				}
			}

			{
				if((values[0] is Color back)
					&& (values[1] is PlatformData.StyleType type)
					&& (values[2] is Color white)
					&& (values[3] is Color black)) {

					return (back.A == 0) ? black : WpfUtil.ImageUtil.GetTextColor(back, white, black, type);
				}
			}

			{
				if((values[0] is SolidColorBrush back)
					&& (values[1] is PlatformData.StyleType type)
					&& (values[2] is Color white)
					&& (values[3] is Color black)) {

					return (back.Color.A == 0) ? black : WpfUtil.ImageUtil.GetTextColor(back.Color, white, black, type);
				}
			}
			// values[0]が設定されていない場合があるので固定値を返す
			return values[3];
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
