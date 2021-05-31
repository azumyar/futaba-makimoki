using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive;
using Prism.Regions;
using Prism.Commands;
using Prism.Navigation;
using Reactive.Bindings;

namespace Yarukizero.Net.MakiMoki.Uno.ViewModels {
	public class ViewModelBase : Prism.Mvvm.BindableBase, IDisposable, INavigationAware, IJournalAware, IDestructible {
		private class NavigationCommand {
			private IRegionNavigationService RegionNavigationService { get; }
			public ReactiveProperty<bool> CanGoBack { get; } = new ReactiveProperty<bool>(initialValue: false);
			public ReactiveProperty<bool> CanGoForward { get; } = new ReactiveProperty<bool>(initialValue: false);

			public ReactiveCommand BackCommand { get; }
			public ReactiveCommand ForwardCommand { get; }

			public NavigationCommand(IRegionNavigationService navigationContext) {
				this.RegionNavigationService = navigationContext;
				this.BackCommand = this.CanGoBack.ToReactiveCommand();
				this.BackCommand.Subscribe(_ => this.RegionNavigationService.Journal.GoBack());
				this.ForwardCommand = this.CanGoForward.ToReactiveCommand();
				this.ForwardCommand.Subscribe(_ => this.RegionNavigationService.Journal.GoForward());

				navigationContext.Navigated += (_, __) => {
					this.UpdateNavigation();
				};
				this.UpdateNavigation();
			}

			private void UpdateNavigation() {
				this.CanGoBack.Value = this.RegionNavigationService?.Journal?.CanGoBack ?? false;
				this.CanGoForward.Value = this.RegionNavigationService?.Journal?.CanGoForward ?? false;
			}
		}

		private static ReactiveProperty<NavigationCommand> Navigation { get; }
			= new ReactiveProperty<NavigationCommand>(initialValue: null);

		public ReactiveCommand BackCommand { get; }
		public ReactiveCommand ForwardCommand { get; }

		protected ReactiveProperty<bool> CanBack { get; } = new ReactiveProperty<bool>(false);
		protected ReactiveProperty<bool> CanForward { get; } = new ReactiveProperty<bool>(false);

		public ViewModelBase() {
			Navigation.Subscribe(x => {
				if(x != null) {
					Navigation.Value.CanGoBack.Subscribe(y => this.CanBack.Value = y);
					Navigation.Value.CanGoForward.Subscribe(y => this.CanForward.Value = y);
				}
			});
			this.BackCommand = this.CanBack.ToReactiveCommand();
			this.ForwardCommand = this.CanForward.ToReactiveCommand();
			this.BackCommand.Subscribe((_) => Navigation.Value?.BackCommand.Execute());
			this.ForwardCommand.Subscribe((_) => Navigation.Value?.ForwardCommand.Execute());
		}

		~ViewModelBase() {
			this.Dispose(false);
		}

		protected virtual void Dispose(bool disposing) {
			if(disposing) {
				Yarukizero.Net.MakiMoki.Helpers.AutoDisposable.GetCompositeDisposable(this).Dispose();
			}
		}

		public virtual void Destroy() => this.Dispose();
		public void Dispose() => this.Dispose(true);

		public virtual bool IsNavigationTarget(NavigationContext navigationContext) => true;
		public virtual void OnNavigatedFrom(NavigationContext navigationContext) {}
		public virtual void OnNavigatedTo(NavigationContext navigationContext) {
			if((Navigation.Value == null)
				&& (navigationContext.NavigationService.Region.Name == UnoConst.ContentRegionName)) {

				Navigation.Value = new NavigationCommand(navigationContext.NavigationService);
			}
		}

		public virtual bool PersistInHistory() => true;
	}
}
