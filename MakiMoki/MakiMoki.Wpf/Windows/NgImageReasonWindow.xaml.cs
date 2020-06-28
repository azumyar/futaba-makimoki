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
	public partial class NgImageReasonWindow : Window {
		private Helpers.AutoDisposable disposable;
		public string ReasonText { get; private set; } = "";

		public NgImageReasonWindow() {
			InitializeComponent();

			disposable = new Helpers.AutoDisposable()
				.Add(ViewModels.NgImageReasonWindowViewModel.Messenger.Instance
					.GetEvent<PubSubEvent<ViewModels.NgImageReasonWindowViewModel.DialogOkCloseMessage>>()
					.Subscribe(x => {
						ReasonText = x.ReasonText;
						DialogResult = true;
				})).Add(ViewModels.NgImageReasonWindowViewModel.Messenger.Instance
					.GetEvent<PubSubEvent<ViewModels.NgImageReasonWindowViewModel.DialogCancelCloseMessage>>()
					.Subscribe(x => {
						DialogResult = false;
					}));
		}

		protected override void OnClosed(EventArgs e) {
			base.OnClosed(e);

			disposable.Dispose();
		}
	}
}
