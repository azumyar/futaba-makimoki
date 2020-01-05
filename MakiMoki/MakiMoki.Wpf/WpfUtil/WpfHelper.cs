using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Yarukizero.Net.MakiMoki.Wpf.WpfUtil {
	static class WpfHelper {
		public static T FindFirstChild<T>(DependencyObject o) {
			int c = VisualTreeHelper.GetChildrenCount(o);
			for (var i = 0; i < c; i++) {
				var co = VisualTreeHelper.GetChild(o, i);
				if (co is T t) {
					return t;
				}
				var r = FindFirstChild<T>(co);
				if (r != null) {
					return r;
				}
			}
			return default(T);
		}

		public static T FindLastChild<T>(DependencyObject o) {
			int c = VisualTreeHelper.GetChildrenCount(o);
			for (var i = c - 1; 0 <= i; i--) {
				var co = VisualTreeHelper.GetChild(o, i);
				if (co is T t) {
					return t;
				}
				var r = FindLastChild<T>(co);
				if (r != null) {
					return r;
				}
			}
			return default(T);
		}
	}
}
