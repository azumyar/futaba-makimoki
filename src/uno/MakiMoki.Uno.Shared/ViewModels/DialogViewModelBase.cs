using Prism.Navigation;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Uno.ViewModels {
	class DialogViewModelBase : Prism.Mvvm.BindableBase, IDialogAware, IDestructible {
		private string title = "";

		public string Title {
			get { return this.title; }
			set { this.SetProperty(ref title, value); }
		}

		public event Action<IDialogResult> RequestClose;

		public virtual bool CanCloseDialog() => true;

		public void Destroy() {
			System.Diagnostics.Debug.WriteLine("!!Destroy!!");
		}

		public virtual void OnDialogClosed() { }

		public virtual void OnDialogOpened(IDialogParameters parameters) { }

		protected void FireRequestClose(IDialogResult result) => this.RequestClose?.Invoke(result);
	}
}
