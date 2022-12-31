using Android.Runtime;
using Android.Views;
using Android.OS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using AngleSharp.Dom;
using Reactive.Bindings;
using Yarukizero.Net.MakiMoki.Droid.Extensions;
using AndroidX.RecyclerView.Widget;
using Android.Widget;
using System.Diagnostics.Metrics;
using Newtonsoft.Json;
using AndroidX.SwipeRefreshLayout.Widget;
using AndroidX.Lifecycle;
using Android.Content;
using Google.Android.Material.FloatingActionButton;

namespace Yarukizero.Net.MakiMoki.Droid.Fragments {
	internal class OptionBackFragment : global::AndroidX.Fragment.App.Fragment {
		public OptionBackFragment() : base() { }
		protected OptionBackFragment(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

		public override View? OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState) {
			return inflater.Inflate(Resource.Layout.fragment_option_back, container, false);
		}

		public override void OnViewCreated(View view, Bundle? savedInstanceState) {
			base.OnViewCreated(view, savedInstanceState);
		}

		public override void OnSaveInstanceState(Bundle outState) {
			base.OnSaveInstanceState(outState);
		}

		public static OptionBackFragment NewInstance() {
			var b = new Bundle();
			return new OptionBackFragment() {
				Arguments = b,
			};
		}
	}
}
