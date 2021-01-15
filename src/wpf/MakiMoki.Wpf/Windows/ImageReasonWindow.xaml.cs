using Prism.Events;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Yarukizero.Net.MakiMoki.Wpf.Windows {
	/// <summary>
	/// NgReasonWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class ImageReasonWindow : Window {
		public static readonly DependencyProperty ReasonTextProperty
			= DependencyProperty.Register(
				nameof(ReasonText),
				typeof(string),
				typeof(ImageReasonWindow),
				new PropertyMetadata(""));
		public string ReasonText {
			get => (string)this.GetValue(ReasonTextProperty);
			set {
				this.SetValue(ReasonTextProperty, value);
			}
		}

		public ImageReasonWindow() {
			InitializeComponent();
		}
	}
}
