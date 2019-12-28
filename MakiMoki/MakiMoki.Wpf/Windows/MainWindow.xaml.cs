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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Yarukizero.Net.MakiMoki.Wpf.Windows {
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();
		}

		protected async override void OnActivated(EventArgs e) {
			base.OnActivated(e);

			/*
			var r = await Util.FutabaApi.GetCatalog("https://img.2chan.net/b/");
			foreach(var res in r.Res) {
				System.Diagnostics.Debug.WriteLine(r);
			}
			*/
		}
	}
}