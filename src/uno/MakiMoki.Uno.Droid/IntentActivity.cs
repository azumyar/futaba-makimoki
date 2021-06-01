using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using Android.Views;
using Android.Content;
using AndroidX.AppCompat.App;
using Android.Runtime;

namespace Yarukizero.Net.MakiMoki.Uno.Droid {
	/*
	 * Intent.Dataに何か入っているとUnoが起動しないのでProxyActivityを挟む
	 *
	 */
	[Activity(
		ConfigurationChanges = global::Uno.UI.ActivityHelper.AllConfigChanges,
		WindowSoftInputMode = SoftInput.AdjustPan | SoftInput.StateHidden)]
	[IntentFilter(
		new[] { Intent.ActionView },
		Categories = new [] {
			Intent.CategoryDefault,
			Intent.CategoryBrowsable,
		},
		DataSchemes = new[] { "http", "https" },
		DataHost = "img.2chan.net",
		DataPathPattern ="/b/res/.*")]
	public class IntentActivity : AppCompatActivity {
		public static readonly string IntentExtraFilterUri = "IntentExtraFileterUri";

		protected override void OnCreate(Bundle bundle) {
			base.OnCreate(bundle);

			if((this.Intent.Action ==  global::Android.Content.Intent.ActionView)
				&& (this.Intent.Data != null)) {
				this.StartActivityForResult(
					new Intent(this, typeof(MainActivity))
						.PutExtra(IntentExtraFilterUri, this.Intent.Data.ToString()),
					0);
			} else {
				if(BuildVersionCodes.Lollipop <= Build.VERSION.SdkInt) {
					this.FinishAndRemoveTask();
				} else {
					this.Finish();
				}
				global::System.Environment.Exit(0);
			}
		}

		protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data) {
			base.OnActivityResult(requestCode, resultCode, data);

			if(BuildVersionCodes.Lollipop <= Build.VERSION.SdkInt) {
				this.FinishAndRemoveTask();
			} else {
				this.Finish();
			}
			global::System.Environment.Exit(0);
		}
	}
}

