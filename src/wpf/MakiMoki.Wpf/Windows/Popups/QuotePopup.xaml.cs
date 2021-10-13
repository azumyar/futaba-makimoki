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
				typeof(IEnumerable<Model.BindableFutabaResItem>),
				typeof(QuotePopup),
				new PropertyMetadata(null));
		public IEnumerable<Model.BindableFutabaResItem> ResSource {
			get => (IEnumerable<Model.BindableFutabaResItem>)this.GetValue(ResSourceProperty);
			set {
				this.SetValue(ResSourceProperty, value);
			}
		}


		public QuotePopup() {
			InitializeComponent();

			this.Loaded += (s, e) => {
				if(HwndSource.FromVisual(this) is HwndSource hs) {
					hs.AddHook(new HwndSourceHook(WndProc));
				}
			};
			this.closeButton.Click += (s, e) => {
				this.IsOpen = false;
			};
		}

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
			const int WM_ACTIVATE = 0x0006;
			const int WM_LBUTTONDOWN = 0x0201;
			const int WM_RBUTTONDOWN = 0x0204;
			switch(msg) {
			case WM_ACTIVATE:
				if((wParam.ToInt32() & 0xffff) != 0) {
					if(IsMouseCaptured) {
						this.CaptureMouse();
					}
				}
				break;
			case WM_LBUTTONDOWN:
			case WM_RBUTTONDOWN: { 
					var x = lParam.ToInt32() & 0xFFFF;
					var y = (lParam.ToInt32() >> 16) & 0xFFFF;
					if((x < 0) || (this.ActualWidth < x) || (y < 0) || (this.ActualHeight < y)) {
						if(!(this.pinnedButton.IsChecked ?? false)) {
							this.IsOpen = false;
						}
					}
				}
				break;
			}
			return IntPtr.Zero;
		}

		protected override async void OnOpened(EventArgs e) {
			base.OnOpened(e);

			await Task.Delay(1);
			this.CaptureMouse();
		}

		protected override void OnClosed(EventArgs e) {
			base.OnClosed(e);

			this.ReleaseMouseCapture();
		}
	}
}
