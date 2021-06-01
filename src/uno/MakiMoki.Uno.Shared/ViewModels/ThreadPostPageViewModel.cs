using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reactive.Linq;
using Prism.Regions;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Yarukizero.Net.MakiMoki.Data;
using Windows.UI.Xaml;
using Prism.Events;
using System.IO;

#if __ANDROID__
using Android.Content;
#endif

namespace Yarukizero.Net.MakiMoki.Uno.ViewModels {
	class ThreadPostPageViewModel : PostPageViewModel {
		private volatile Navigation navigation;
		public ThreadPostPageViewModel(IRegionManager regionManager, IDialogService dialogService) : base(regionManager, dialogService) {}

		public override void OnNavigatedTo(NavigationContext navigationContext) {
			base.OnNavigatedTo(navigationContext);

			if(this.navigation != null) {
				var n = Navigation.FromContext(navigationContext);
				if(n.Board.Url == this.navigation.Board.Url) {
				} else {
					this.PostHolder.Value.Reset();
				}
			} else {
				this.navigation = Navigation.FromContext(navigationContext);
			}

			this.Title.Value = "スレッド作成";
			this.IdButtonVisibirity.Value = Visibility.Visible;
			this.IpButtonVisibirity.Value = Visibility.Collapsed;
			this.ImageButtonVisibirity.Value = Visibility.Visible;
		}

		protected override void OnPostClick() {
			if(this.IsPosting.Value) {
				return;
			}
			if(!this.PostHolder.Value.PostButtonCommand.CanExecute()) {
				return;
			}

			Util.Futaba.PostThread(this.navigation.Board,
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
					//Util.Futaba.PutInformation(new Information(y.NextOrMessage));
					UnoHelpers.Toast.Show(y.NextOrMessage);
				}
			});
		}
	}
}
