using Android.Views;
using Android.OS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yarukizero.Net.MakiMoki.Droid.Extensions;
using Android.Runtime;

namespace Yarukizero.Net.MakiMoki.Droid.Fragments {
	internal class ThreadViewerFragment : global::AndroidX.Fragment.App.Fragment {
		public ThreadViewerFragment() : base() { }
		protected ThreadViewerFragment(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

		public override View? OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState) {
			return inflater.Inflate(Resource.Layout.fragment_main, container, false);
		}

		public override void OnViewCreated(View view, Bundle? savedInstanceState) {
			base.OnViewCreated(view, savedInstanceState);
		}

		public static ThreadViewerFragment NewInstance(Data.UrlContext threadUrl) {
			var b = new Bundle()
				.InJson(threadUrl);
			return new ThreadViewerFragment() {
				Arguments = b,
			};
		}
	}
}
