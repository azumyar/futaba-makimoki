using Prism.Events;
using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Yarukizero.Net.MakiMoki.Wpf.ViewModels {
	class NgImageReasonWindowViewModel : BindableBase, IDisposable {
		internal class Messenger : EventAggregator {
			public static Messenger Instance { get; } = new Messenger();
		}

		internal class DialogOkCloseMessage {
			public string ReasonText { get; }

			public DialogOkCloseMessage(string reasonText) {
				this.ReasonText = reasonText;
			}
		}

		internal class DialogCancelCloseMessage {
			public DialogCancelCloseMessage() { }
		}

		public ReactiveProperty<string> ReasonText { get; } = new ReactiveProperty<string>("");

		public ReactiveCommand<RoutedEventArgs> OkButtonClickCommand { get; } = new ReactiveCommand<RoutedEventArgs>();
		public ReactiveCommand<RoutedEventArgs> CancelButtonClickCommand { get; } = new ReactiveCommand<RoutedEventArgs>();

		public NgImageReasonWindowViewModel() {
			OkButtonClickCommand.Subscribe(x => OnOkButtonClick(x));
			CancelButtonClickCommand.Subscribe(x => OnCancelButtonClick(x));
		}

		public void Dispose() {
			Helpers.AutoDisposable.GetCompositeDisposable(this).Dispose();
		}

		private void OnOkButtonClick(RoutedEventArgs e) {
			Messenger.Instance.GetEvent<PubSubEvent<DialogOkCloseMessage>>()
				.Publish(new DialogOkCloseMessage(this.ReasonText.Value));
		}

		private void OnCancelButtonClick(RoutedEventArgs e) {
			Messenger.Instance.GetEvent<PubSubEvent<DialogCancelCloseMessage>>()
				.Publish(new DialogCancelCloseMessage());
		}
	}
}