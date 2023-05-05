using Android.Widget;
using Google.Android.Material.TextField;
using System;
using System.Reactive.Linq;
using Reactive.Bindings;
using Yarukizero.Net.MakiMoki.Droid.Extensions;
using Android.Content;
using Google.Android.Material.Button;
using Android.Graphics;
using Java.IO;
using static Android.Media.Audiofx.AudioEffect;
using Yarukizero.Net.MakiMoki.Droid.App;
using Android.Provider;

namespace Yarukizero.Net.MakiMoki.Droid.Activities {
	[global::Android.App.Activity(Label = "@string/app_name")]
	public class PostActivity : global::AndroidX.AppCompat.App.AppCompatActivity {
		public static readonly string ResultCodeSucessed = "sucessed";
		public static readonly string ResultThreadNo = "thread_no";
		private DroidUtil.Util.IActivityResultLauncher activityLuncherImage;

		private string postImagePath = "";

		public PostActivity() : base() { }
		protected PostActivity(IntPtr javaReference, global::Android.Runtime.JniHandleOwnership transfer) : base(javaReference, transfer) {}

		protected override void OnCreate(global::Android.OS.Bundle? savedInstanceState) {
			base.OnCreate(savedInstanceState);

			this.activityLuncherImage = DroidUtil.Util.RegisterForActivityResult<Intent?>(this,
				(context, _) => new Intent(Intent.ActionOpenDocument)
					.AddCategory(Intent.CategoryOpenable)
					.SetType("*/*")
					.PutExtra(Intent.ExtraMimeTypes, new[] {
						"image/gif",
						"image/jpeg",
						"image/png",
						"image/webp",
						"video/mp4",
						"video/webm",
					}),
				(resultCode, intent) => resultCode switch {
					-1 => intent, // RESULT_OK
					_ => default,
				}, (intent) => {
					if(intent is not null) {
						var afd = this.ContentResolver.OpenAssetFileDescriptor(intent.Data, "r");
						try {
							if(afd?.FileDescriptor != null) {
								var image = this.FindViewById<ImageView>(Resource.Id.image);
								image.Visibility = Android.Views.ViewStates.Visible;
								image.SetImageBitmap(BitmapFactory.DecodeFileDescriptor(afd.FileDescriptor));

								if(DroidUtil.Util.Uri2Path(this, intent.Data) is string fn) {
									this.postImagePath = System.IO.Path.Combine(MakiMokiApplication.Current.MakiMoki.AppCacheDirectory, $"post{System.IO.Path.GetExtension(fn)}");
									using var ws = new System.IO.FileStream(this.postImagePath, System.IO.FileMode.Create);
									using var rs = afd.CreateInputStream();
									{
										var b = new byte[1024 * 10];
										var i = 0;
										while(0 < (i = rs.Read(b, 0, b.Length))) {
											ws.Write(b, 0, i);
										}
										ws.Flush();
									}
								}
							}
						}
						finally {
							afd?.Close();
						}
					}
				});

			SetContentView(Resource.Layout.activity_post);
			var board = this.Intent.OutJson<Data.BoardData>();
			var url = this.Intent.OutJson<Data.UrlContext?>();
			this.SetResult((Android.App.Result)DroidConst.ActivityResultCodePost, new Intent()
				.PutExtra(ResultCodeSucessed, false)
				.InJson(board));

			{
				var name = this.FindViewById<TextInputLayout>(Resource.Id.name);
				var title = this.FindViewById<TextInputLayout>(Resource.Id.title);
				if(board.Extra.Name) {
					name.Visibility
						= title.Visibility
						= Android.Views.ViewStates.Visible;
				} else {
					name.Visibility
						= title.Visibility
						= Android.Views.ViewStates.Gone;
				}
			}
			{
				var image = this.FindViewById<ImageView>(Resource.Id.image);
				var imageButton = this.FindViewById<MaterialButton>(Resource.Id.button_image);
				image.Visibility = Android.Views.ViewStates.Gone;
				if(url == null) {
					imageButton.Visibility = Android.Views.ViewStates.Visible;
				} else {
					imageButton.Visibility = board.Extra.ResImage switch {
						true => Android.Views.ViewStates.Visible,
						false => Android.Views.ViewStates.Gone,
					};
				}
				imageButton.Click += (_, _) => {
					this.activityLuncherImage.Launch();
				};
			}
			{
				var sage = this.FindViewById<MaterialButton>(Resource.Id.button_sage);
				var id = this.FindViewById<MaterialButton>(Resource.Id.button_id);
				var ip = this.FindViewById<MaterialButton>(Resource.Id.button_ip);
				if(url == null) {
					sage.Visibility = Android.Views.ViewStates.Visible;
					ip.Visibility = board.Extra.AlwaysIp switch {
						true => Android.Views.ViewStates.Visible,
						false => Android.Views.ViewStates.Gone,
					};
					id.Visibility = board.Extra.AlwaysId switch {
						true => Android.Views.ViewStates.Visible,
						false => Android.Views.ViewStates.Gone,
					};
				} else {
					sage.Visibility = ip.Visibility = id.Visibility = Android.Views.ViewStates.Gone;
				}
				sage.Click += (_, _) => this.FindViewById<TextInputLayout>(Resource.Id.mail).EditText.Text = "sage";
				ip.Click += (_, _) => this.FindViewById<TextInputLayout>(Resource.Id.mail).EditText.Text = "ip表示";
				id.Click += (_, _) => this.FindViewById<TextInputLayout>(Resource.Id.mail).EditText.Text = "id表示";
			}

			this.FindViewById<TextInputLayout>(Resource.Id.password).EditText.Text = Config.ConfigLoader.FutabaApi.SavedPassword;
			this.FindViewById<Button>(Resource.Id.button_send).Click += (_, _) => {
				var time = Util.TimeUtil.ToUnixTimeSeconds(DateTime.Now) - MakiMokiApplication.Current.MakiMoki.InitUnixTime;
				if(time < 3600L) {
					Toast.MakeText(this, $"あと{(3600L - time)}秒投稿できません", ToastLength.Long).Show();
					return;
				}

				var name = this.FindViewById<TextInputLayout>(Resource.Id.name).EditText.Text;
				var mail = this.FindViewById<TextInputLayout>(Resource.Id.mail).EditText.Text;
				var sub = this.FindViewById<TextInputLayout>(Resource.Id.title).EditText.Text;
				var com = this.FindViewById<TextInputLayout>(Resource.Id.comment).EditText.Text;
				var file = this.postImagePath;
				var pass = this.FindViewById<TextInputLayout>(Resource.Id.password).EditText.Text;
				if(url == null) {
					Util.FutabaApiReactive.PostThread(
						board,
						name, mail, sub,
						com, file, pass)
						.ObserveOn(UIDispatcherScheduler.Default)
						.Subscribe(x => {
							if(x.Successed) {
								Config.ConfigLoader.UpdateFutabaInputData(board, "", "", "", pass);
								this.SetResult((Android.App.Result)DroidConst.ActivityResultCodePost, new Intent()
									.PutExtra(ResultCodeSucessed, true)
									.PutExtra(ResultThreadNo, x.NextOrMessage)
									.InJson(board));
								this.Finish();
							} else {
								Toast.MakeText(this, x.NextOrMessage, ToastLength.Long).Show();
							}
							Config.ConfigLoader.UpdateCookie(board.Url, x.Cookies);
						});
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