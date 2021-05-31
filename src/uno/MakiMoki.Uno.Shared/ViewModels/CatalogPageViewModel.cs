using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reactive.Linq;
using Prism.Regions;
using Reactive.Bindings;
using Yarukizero.Net.MakiMoki.Data;
using Windows.UI.Xaml.Controls;

namespace Yarukizero.Net.MakiMoki.Uno.ViewModels {
	class CatalogPageViewModel : ViewModelBase {
		public class Navigation {
			public BoardData Board { get; }

			private Navigation(BoardData board) {
				this.Board = board;
			}

			public static NavigationParameters FromParamater(BoardData board) {
				return new NavigationParameters() {
					{
						typeof(Navigation).FullName,
						new Navigation(board)
					}
				};
			}

			public static Navigation FromContext(NavigationContext navigationContext) {
				if(navigationContext.Parameters.TryGetValue<object>(typeof(Navigation).FullName, out var p)
					&& (p is Navigation n)) {

					return n;
				}
				throw new InvalidOperationException();
			}
		}

		private readonly IRegionManager regionManager;
		private ReactiveProperty<Models.BindableFutaba> Catalog { get; } = new ReactiveProperty<Models.BindableFutaba>();
		public ReactiveProperty<Models.BindableFutabaResItem[]> CatalogItems { get; }

		public ReactiveCommand<AutoSuggestBoxQuerySubmittedEventArgs> QuerySubmittedCommand { get; } = new ReactiveCommand<AutoSuggestBoxQuerySubmittedEventArgs>();
		public ReactiveCommand<ItemClickEventArgs> CatalogClickCommand { get; } = new ReactiveCommand<ItemClickEventArgs>();
		public ReactiveCommand WriteClickCommand { get; } = new ReactiveCommand();
		public ReactiveCommand UpdateClickCommand { get; } = new ReactiveCommand();

		private Data.BoardData board;

		public CatalogPageViewModel(IRegionManager regionManager) {
			this.regionManager = regionManager;
			this.CatalogItems = this.Catalog
				.Select(x => x?.ResItems.ToArray() ?? Array.Empty<Models.BindableFutabaResItem>())
				.ToReactiveProperty();

			this.CatalogClickCommand.Subscribe(x => this.OnCatalogClick(x));
			this.WriteClickCommand.Subscribe(() => this.OnWriteClick());
			this.UpdateClickCommand.Subscribe(() => this.OnUpdateClick());
		}

		public override void OnNavigatedTo(NavigationContext navigationContext) {
			base.OnNavigatedTo(navigationContext);
			board = Navigation.FromContext(navigationContext).Board;

			this.OnUpdateClick();
		}

		private void OnCatalogClick(ItemClickEventArgs e) {
			if(e.ClickedItem is Models.BindableFutabaResItem it) {
				this.regionManager.RequestNavigate(
					UnoConst.ContentRegionName,
					nameof(Views.ThreadPage),
					ThreadPageViewModel.Navigation.FromParamater(it));
			}
		}
		private void OnWriteClick() {
			this.regionManager.RequestNavigate(
				UnoConst.ContentRegionName,
				nameof(Views.ThreadPostPage),
				PostPageViewModel.Navigation.FromParamater(board));
		}

		public void OnUpdateClick() {
			Util.Futaba.UpdateCatalog(this.board)
				.ObserveOn(UIDispatcherScheduler.Default)
				.Subscribe(x => {
					if(x.Successed) {
						this.Catalog.Value = new Models.BindableFutaba(x.Catalog, this.Catalog.Value);
					} else {
						UnoHelpers.Toast.Show(x.ErrorMessage);
					}
				});
		}
	}
}
