using System;
using System.Collections.Generic;
using System.Text;
using Prism.Regions;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services.Dialogs;

namespace MakiMoki.Uno.ViewModels {
	internal class ViewModelBase : ModelBase, INavigationAware, IJournalAware, IDestructible {
		public void Destroy() => this.Dispose();

		public virtual bool IsNavigationTarget(NavigationContext navigationContext) {
			return true;
		}

		public void OnNavigatedFrom(NavigationContext navigationContext) {}

		public void OnNavigatedTo(NavigationContext navigationContext) {}

		public virtual bool PersistInHistory() {
			return true;
		}
	}
}
