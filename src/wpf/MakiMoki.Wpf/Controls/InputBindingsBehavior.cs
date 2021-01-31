using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Yarukizero.Net.MakiMoki.Wpf.Controls {
	class InputBindingsBehavior {
		public static readonly DependencyProperty SourceProperty
			= DependencyProperty.RegisterAttached(
				"Source",
				typeof(IEnumerable),
				typeof(InputBindingsBehavior),
				new UIPropertyMetadata(null, OnSourceChanged));
		public static IEnumerable GetSource(DependencyObject obj) {
			return (IEnumerable)obj.GetValue(SourceProperty);
		}
		public static void SetSource(DependencyObject obj, IEnumerable value) {
			obj.SetValue(SourceProperty, value);
		}

		private static void OnSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
			if(obj is UIElement el) {
				el.InputBindings.Clear();
				if(e.NewValue is IEnumerable en) {
					el.InputBindings.AddRange(en.Cast<InputBinding>().ToArray());
				}
				return;
			}
			throw new ArgumentException("UIElementではない", nameof(obj));
		}
	}
}
