using Android.Widget;
using Google.Android.Material.TextField;
using System;
using System.Reactive.Linq;
using Reactive.Bindings;
using Yarukizero.Net.MakiMoki.Droid.Extensions;
using Android.Content;

namespace Yarukizero.Net.MakiMoki.Droid.Activities {
	[global::Android.App.Activity(Label = "@string/app_name")]
	public class PostActivity : global::AndroidX.AppCompat.App.AppCompatActivity {
		public static readonly string ResultCodeSucessed = "sucessed";

		public PostActivity() : base() { }
		protected PostActivity(IntPtr javaReference, global::Android.Runtime.JniHandleOwnership transfer) : base(javaReference, transfer) {}

		protected override void OnCreate(global::Android.OS.Bundle? savedInstanceState) {
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.activity_post);
			var board = this.Intent.OutJson<Data.BoardData>();
			var url = this.Intent.OutJson<Data.UrlContext>();
			this.SetResult((Android.App.Result)DroidConst.ActivityResultCodePost, new Intent()
				.PutExtra(ResultCodeSucessed, false)
				.InJson(board));
			this.FindViewById<TextInputLayout>(Resource.Id.password).EditText.Text = Config.ConfigLoader.FutabaApi.SavedPassword;
			this.FindViewById<Button>(Resource.Id.button_send).Click += (_, _) => {
				var time = Util.TimeUtil.ToUnixTimeSeconds(DateTime.Now) - MakiMokiApplication.Current.MakiMoki.InitUnixTime;
				if(time < 3600L) {
					Toast.MakeText(this, $"あと{(3600L - time)}秒投稿できません", ToastLength.Long).Show();
					return;
				}

				var name = ""; // name
				var mail = this.FindViewById<TextInputLayout>(Resource.Id.mail).EditText.Text;
				var sub = ""; // sub
				var com = this.FindViewById<TextInputLayout>(Resource.Id.comment).EditText.Text;
				var file = "";
				var pass = this.FindViewById<TextInputLayout>(Resource.Id.password).EditText.Text;
				if(url == null) {

				} else {
					Util.FutabaApiReactive.PostRes(
						board,
						url.ThreadNo,
						name, mail, sub,
						com, file, pass)
						.ObserveOn(UIDispatcherScheduler.Default)
						.Subscribe(x => {
							if(x.Successed) {
								Config.ConfigLoader.UpdateFutabaInputData(board, "", "", "", pass);
								this.SetResult((Android.App.Result)DroidConst.ActivityResultCodePost, new Intent()
									.PutExtra(ResultCodeSucessed, true)
									.InJson(board)
									.InJson(url));
								this.Finish();
							} else {
								Toast.MakeText(this, x.Message, ToastLength.Long).Show();
							}
							Config.ConfigLoader.UpdateCookie(board.Url, x.Cookies);
						});
				}
			};
		}
	}
}