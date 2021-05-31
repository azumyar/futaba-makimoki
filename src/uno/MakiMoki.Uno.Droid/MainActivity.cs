using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using Android.Views;
using Android.Content;

namespace Yarukizero.Net.MakiMoki.Uno.Droid {
	[Activity(
		MainLauncher = true,
		ConfigurationChanges = global::Uno.UI.ActivityHelper.AllConfigChanges,
		WindowSoftInputMode = SoftInput.AdjustPan | SoftInput.StateHidden)]
	/*
	[IntentFilter(
		new[] { Intent.ActionView },
		Categories = new [] {
			Intent.CategoryDefault,
			Intent.CategoryBrowsable,
		},
		DataSchemes = new[] { "http", "https" },
		DataHost = "img.2chan.net",
		DataPathPattern ="/b/res/.*")]
	*/
	public class MainActivity : Windows.UI.Xaml.ApplicationActivity {
		public static MainActivity ActivityContext { get; private set; }

		public static readonly int RequestCodeChoosePostImage = 1;
		public static readonly int RequestCodeChooseUploadItem = 2;


		protected override void OnCreate(Bundle bundle) {
			base.OnCreate(bundle);

			ActivityContext = this;
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data) {
			base.OnActivityResult(requestCode, resultCode, data);

			if((requestCode == RequestCodeChoosePostImage) || (requestCode == RequestCodeChooseUploadItem)) {
				ViewModels.PostPageViewModel.Messenger.Instance.GetEvent<
					Prism.Events.PubSubEvent<ViewModels.PostPageViewModel.DroidPostChoseResultMessage>>()
					.Publish(new ViewModels.PostPageViewModel.DroidPostChoseResultMessage(
						requestCode,
						resultCode,
						data));
			}
		}

	}
}

