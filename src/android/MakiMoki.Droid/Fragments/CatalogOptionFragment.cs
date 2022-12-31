using Android.OS;
using Android.Runtime;
using Android.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yarukizero.Net.MakiMoki.Droid.Fragments {
	internal class CatalogOptionFragment : global::AndroidX.Fragment.App.Fragment {
		public CatalogOptionFragment() : base() { }
		protected CatalogOptionFragment(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

		public override View? OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState) {
			return inflater.Inflate(Resource.Layout.fragment_catalog_option, container, false);
		}

		public override void OnViewCreated(View view, Bundle? savedInstanceState) {
			base.OnViewCreated(view, savedInstanceState);
		}

		public static CatalogOptionFragment NewInstance() {
			var b = new Bundle();
			return new CatalogOptionFragment() {
				Arguments = b,
			};
		}
	}
}

