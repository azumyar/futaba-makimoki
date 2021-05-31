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
	class ThreadPageViewModel : ViewModelBase {
		public class Navigation {
			public BoardData Board { get; }
			public UrlContext Url { get; }

			private Navigation(Models.BindableFutabaResItem item) 
				:this(item.Bord.Value,
					 new UrlContext(item.Bord.Value.Url, item.ThreadResNo.Value)) {}

			private Navigation(BoardData board, UrlContext url) {
				this.Board = board;
				this.Url = url;
			}

			public static NavigationParameters FromParamater(Models.BindableFutabaResItem item) {
				return new NavigationParameters() {
					{
						typeof(Navigation).FullName,
						new Navigation(item)
					}
				};
			}

			public static NavigationParameters FromParamater(BoardData board, UrlContext url) {
				return new NavigationParameters() {
					{
						typeof(Navigation).FullName,
						new Navigation(board, url)
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
		private ReactiveProperty<Models.BindableFutaba> Threads { get; } = new ReactiveProperty<Models.BindableFutaba>();
		public ReactiveProperty<Models.BindableFutabaResItem[]> ThreadItems { get; }
		public ReactiveCommand<ItemClickEventArgs> ResClickCommand { get; } = new ReactiveCommand<ItemClickEventArgs>();
		public ReactiveCommand WriteClickCommand { get; } = new ReactiveCommand();
		public ReactiveCommand UpdateClickCommand { get; } = new ReactiveCommand();


		private Navigation navigation;

		public ThreadPageViewModel(IRegionManager regionManager) {
			this.regionManager = regionManager;
			this.ThreadItems = this.Threads
				.Select(x => x?.ResItems.ToArray() ?? Array.Empty<Models.BindableFutabaResItem>())
				.ToReactiveProperty();

			this.ResClickCommand.Subscribe(x => this.OnResClick(x));
			this.WriteClickCommand.Subscribe(() => this.OnWriteClick());
			this.UpdateClickCommand.Subscribe(() => this.OnUpdateClick());
		}

		public override void OnNavigatedTo(NavigationContext navigationContext) {
			base.OnNavigatedTo(navigationContext);

			if(this.navigation != null) {
				var n = Navigation.FromContext(navigationContext);
				if(n.Url == this.navigation.Url) {
					goto end;
				} else {
					// タブはサポートしていないのでスレを閉じる
					Util.Futaba.Remove(this.navigation.Url);
					this.Threads.Value = null;
					this.navigation = n;
				}
			} else {
				this.navigation = Navigation.FromContext(navigationContext);
			}
			Util.Futaba.UpdateThreadResAll(this.navigation.Board, this.navigation.Url.ThreadNo)
				.ObserveOn(UIDispatcherScheduler.Default)
				.Subscribe(x => {
					if(x.Successed) {
						this.Threads.Value = new Models.BindableFutaba(x.New, this.Threads.Value);
					} else {
						UnoHelpers.Toast.Show(x.ErrorMessage);
					}
				});
		end:;
		}

		private void OnResClick(ItemClickEventArgs e) {
			if(e.ClickedItem is Models.BindableFutabaResItem it) {
				/* うまく動かないので今回はなし
				var sb = new StringBuilder();
				var c = Util.TextUtil.RowComment2Text(it.Raw.Value.ResItem.Res.Com).Replace("\r", "").Split('\n');
				foreach(var s in c) {
					sb.Append(">").AppendLine(s);
				}
				this.regionManager.RequestNavigate(
					UnoConst.ContentRegionName,
					nameof(Views.ResPostPage),
					PostPageViewModel.Navigation.FromParamater(
						this.navigation.Board,
						this.navigation.Url,
						sb.ToString()));
				*/
			}
		}
		private void OnWriteClick() {
			this.regionManager.RequestNavigate(
				UnoConst.ContentRegionName,
				nameof(Views.ResPostPage),
				PostPageViewModel.Navigation.FromParamater(
					this.navigation.Board,
					this.navigation.Url));
		}

		public void OnUpdateClick() {
			Util.Futaba.UpdateThreadRes(this.navigation.Board, this.navigation.Url.ThreadNo)
				.ObserveOn(UIDispatcherScheduler.Default)
				.Subscribe(x => {
					if(x.Successed) {
						this.Threads.Value = new Models.BindableFutaba(x.New, this.Threads.Value);
					} else {
						UnoHelpers.Toast.Show(x.ErrorMessage);
					}
				});
		}

	}
}
