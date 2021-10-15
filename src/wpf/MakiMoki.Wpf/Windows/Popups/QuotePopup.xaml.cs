using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Yarukizero.Net.MakiMoki.Wpf.Windows.Popups {
	/// <summary>
	/// QuotePopup.xaml の相互作用ロジック
	/// </summary>
	public partial class QuotePopup : Popup {
		public static readonly DependencyProperty ResSourceProperty =
			DependencyProperty.Register(
				nameof(ResSource),
				typeof(object),
				typeof(QuotePopup),
				new PropertyMetadata(null));
		public static readonly DependencyProperty CommandPaletteAlignmentProperty =
			DependencyProperty.Register(
				nameof(CommandPaletteAlignment),
				typeof(HorizontalAlignment),
				typeof(QuotePopup),
				new PropertyMetadata(HorizontalAlignment.Right));

		public object ResSource {
			get { return this.GetValue(ResSourceProperty); }
			set { this.SetValue(ResSourceProperty, value); }
		}
		public HorizontalAlignment CommandPaletteAlignment {
			get { return (HorizontalAlignment)this.GetValue(ResSourceProperty); }
			set { this.SetValue(ResSourceProperty, value); }
		}

		public static void Show(Model.BindableFutabaResItem source, object element, UIElement placementTarget = null) {
			ShowImpliment(source, element, placementTarget);
		}
		public static void Show(IEnumerable<Model.BindableFutabaResItem> source, object element, UIElement placementTarget = null) {
			ShowImpliment(source, element, placementTarget);
		}
		
		private static void ShowImpliment(object source, object element, UIElement placementTarget=null) {
			if(Application.Current.MainWindow is MainWindow w) {
				foreach(var el in w.PopupContainer.Children) {
					if(el is FrameworkElement fel) {
						if(fel.Tag == element) {
							goto end;
						}
					}
				}
				var p = new QuotePopup() {
					Placement = PlacementMode.MousePoint,
					PlacementTarget = placementTarget,
					ResSource = source,
					Tag = element,
				};
				p.Closed += (_, _) => {
					w.PopupContainer.Children.Remove(p);
				};
				w.PopupContainer.Children.Add(p);
				p.IsOpen = true;
			end:;
			}

		}

		public QuotePopup() {
			InitializeComponent();
			this.HorizontalAlignment = (WpfConfig.WpfConfigLoader.SystemConfig.CommandPalettePosition == PlatformData.UiPosition.Left)
				? HorizontalAlignment.Left : HorizontalAlignment.Right;
			this.StaysOpen = false;

			this.pinnedButton.Click += (_, _) => { 
				this.StaysOpen = true;
				this.pinnedButton.Visibility = Visibility.Hidden;
				this.closeButton.Visibility = Visibility.Visible;
			};
			this.closeButton.Click += (_, _) => { this.IsOpen = false; };
		}
	}
}
