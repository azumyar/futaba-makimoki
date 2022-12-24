using System;

namespace Yarukizero.Net.MakiMoki.Droid.Activities {
	[global::Android.App.Activity(Label = "@string/app_name", MainLauncher = true)]
	public class MainActivity : global::AndroidX.AppCompat.App.AppCompatActivity {
		public MainActivity() : base() { }
		protected MainActivity(IntPtr javaReference, global::Android.Runtime.JniHandleOwnership transfer) : base(javaReference, transfer) {}

		protected override void OnCreate(global::Android.OS.Bundle? savedInstanceState) {
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.activity_main);
			this.SupportFragmentManager.BeginTransaction()
				.Replace(Resource.Id.container, Fragments.MainFragment.NewInstance())
				.Commit();
		}
	}
}