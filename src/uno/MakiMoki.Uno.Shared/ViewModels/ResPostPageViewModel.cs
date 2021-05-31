using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reactive.Linq;
using Prism.Regions;
using Reactive.Bindings;
using Yarukizero.Net.MakiMoki.Data;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;

namespace Yarukizero.Net.MakiMoki.Uno.ViewModels {
	class ResPostPageViewModel : PostPageViewModel {
		private Navigation navigation;

		public ResPostPageViewModel(IRegionManager regionManager) : base(regionManager) {}

		public override void OnNavigatedTo(NavigationContext navigationContext) {
			base.OnNavigatedTo(navigationContext);

			if(this.navigation != null) {
				var n = Navigation.FromContext(navigationContext);
				if((n.Url != null) && (n.Url == this.navigation.Url)) {
				} else {
					this.PostHolder.Value.Reset();
				}
			} else {
				this.navigation = Navigation.FromContext(navigationContext);
			}

			this.Title.Value = "レス投稿";
			this.IdButtonVisibirity.Value = Visibility.Collapsed;
			this.IpButtonVisibirity.Value = Visibility.Collapsed;
			this.ImageButtonVisibirity.Value = Visibility.Collapsed; // img,dat専用
			if(!string.IsNullOrEmpty(this.navigation.Commnet)) {
				this.PostHolder.Value.Comment.Value = this.navigation.Commnet;
			}
		}

		protected override void OnPostClick() {
			if(this.IsPosting.Value) {
				return;
			}
			if(!this.PostHolder.Value.PostButtonCommand.CanExecute()) {
				return;
			}

			Util.Futaba.PostRes(this.navigation.Board, this.navigation.Url.ThreadNo,
				this.PostHolder.Value.NameEncoded.Value,
				this.PostHolder.Value.MailEncoded.Value,
				this.PostHolder.Value.SubjectEncoded.Value,
				this.PostHolder.Value.CommentEncoded.Value,
				this.PostHolder.Value.ImagePath.Value,
				this.PostHolder.Value.Password.Value)
			.ObserveOn(UIDispatcherScheduler.Default)
			.Finally(() => { this.IsPosting.Value = false; })
			.Subscribe(y => {
				if(y.Successed) {
					this.PostHolder.Value.Reset();
					if(this.BackCommand.CanExecute()) {
						this.BackCommand.Execute();
					} else {
#if __ANDROID__
						Droid.MainActivity.ActivityContext.Finish();
#endif
					}
				} else {
					//Util.Futaba.PutInformation(new Information(y.Message, x));
					UnoHelpers.Toast.Show(y.Message);
				}
			});

		}
	}
}
