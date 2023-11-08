using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Yarukizero.Net.MakiMoki.Wpf.Controls {
	[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(ContentSwitchItem))]
	internal class ContentSwitchControl : System.Windows.Controls.Primitives.Selector {
		static ContentSwitchControl() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ContentSwitchControl), new FrameworkPropertyMetadata(typeof(ContentSwitchControl)));
		}

		protected override DependencyObject GetContainerForItemOverride() {
			return new ContentSwitchItem();
		}
	}

	internal class ContentSwitchItem : Control {
		class ItemVisibilityConverter : IMultiValueConverter {
			public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
				if(values.Length == 2) {
					if(object.ReferenceEquals(values[0], values[1]) && (values[0] != null)) {
						return Visibility.Visible;
					}
					return Visibility.Collapsed;
				}
				throw new ArgumentException("型不正。", nameof(values));
			}

			public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
				throw new NotImplementedException();
			}
		}

		private static ItemVisibilityConverter ItemVisibilityConverter_ { get; } = new ItemVisibilityConverter();

		static ContentSwitchItem() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ContentSwitchItem), new FrameworkPropertyMetadata(typeof(ContentSwitchItem)));
		}

		public ContentSwitchItem() {
			var mb = new MultiBinding() {
				Converter = ItemVisibilityConverter_
			};
			mb.Bindings.Add(new Binding() {
				Path = new PropertyPath(ContentSwitchControl.SelectedItemProperty),
				RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ContentSwitchControl), 1),
				Mode = BindingMode.OneWay,
			});
			mb.Bindings.Add(new Binding() {
				Path = new PropertyPath(ContentSwitchItem.DataContextProperty),
				Source = this,
				Mode = BindingMode.OneWay,
			});
			this.SetBinding(VisibilityProperty, mb);
		}
	}
}