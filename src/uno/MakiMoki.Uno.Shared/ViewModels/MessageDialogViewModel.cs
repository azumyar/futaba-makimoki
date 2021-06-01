using System;
using System.Collections.Generic;
using System.Text;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Yarukizero.Net.MakiMoki.Shared.Bindings;

namespace Yarukizero.Net.MakiMoki.Uno.ViewModels {
	class MessageDialogViewModel : DialogViewModelBase {
		public static void ShowDialog(
			Prism.Services.Dialogs.IDialogService dialogService,
			Action<Prism.Services.Dialogs.IDialogResult> resultAction = null) {
			dialogService.ShowDialog(
				nameof(Views.MessageDialog),
				new DialogParameters(),
				 (r) => {
					 resultAction?.Invoke(r);
				 });
		}

		public MakiMokiCommand PositiveClickCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand NegativeClickCommand { get; } = new MakiMokiCommand();

		public MessageDialogViewModel() {
			this.PositiveClickCommand.Subscribe(() => this.OnPositiveClick());
			this.NegativeClickCommand.Subscribe(() => this.OnNegativeClick());
		}

		private void OnPositiveClick() {
			this.FireRequestClose(new DialogResult(ButtonResult.OK));
		}

		private void OnNegativeClick() {
			this.FireRequestClose(new DialogResult(ButtonResult.Cancel));
		}

	}
}
