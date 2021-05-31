using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Reactive.Bindings;
using Prism.Regions;
using System.Text.RegularExpressions;

namespace Yarukizero.Net.MakiMoki.Uno.ViewModels {
	class ShellViewModel : ViewModelBase {
		private readonly IRegionManager regionManager;
		public ReactiveCommand LoadedCommand { get; } = new ReactiveCommand();

		public ShellViewModel(IRegionManager regionManager) {
			this.regionManager = regionManager;

			this.LoadedCommand.Subscribe(_ => OnLoaded());
			this.regionManager.RegisterViewWithRegion(
				UnoConst.ContentRegionName,
				() => new Windows.UI.Xaml.Controls.Page());

		}

		private async void OnLoaded() {
			// 一旦先に進めないと実行されない
			await Task.Yield();

#if __ANDROID__
			var url = Droid.MainActivity.ActivityContext.Intent?.GetStringExtra(
				Droid.IntentActivity.IntentExtraFilterUri);
			if(!string.IsNullOrEmpty(url)) {
				var m = Regex.Match(url, @"^https?://([^\.]+\.2chan.net/[^/]+/)res/([0-9]+)\.htm");
				if(m.Success) {
					var board = $"https://{ m.Groups[1].Value }";
					var thread = m.Groups[2].Value;
					this.regionManager.RequestNavigate(
						UnoConst.ContentRegionName,
						nameof(Views.ResPostPage),
						PostPageViewModel.Navigation.FromParamater(
							App.TmpImgBoard,
							new Data.UrlContext(board, thread)));
					goto end;
				}
			}
#endif
			this.regionManager.RequestNavigate(
				UnoConst.ContentRegionName,
				nameof(Views.CatalogPage),
				CatalogPageViewModel.Navigation.FromParamater(App.TmpImgBoard));
#if __ANDROID__
		end:;
#endif
		}
	}
}
