using Android.Views;
using Android.OS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Runtime;
using AndroidX.RecyclerView.Widget;
using Android.Widget;
using static AndroidX.RecyclerView.Widget.RecyclerView;

namespace Yarukizero.Net.MakiMoki.Droid.Fragments {
	internal class MainFragment : global::AndroidX.Fragment.App.Fragment {
		public class RecyclerAdapter : App.RecyclerViewAdapter<Data.BoardData> {
			private class ViewHolder : RecyclerView.ViewHolder {
				public View Root { get; }
				public TextView Name { get; }

				public ViewHolder(View v) : base(v) {
					this.Root = v;
					this.Name = v.FindViewById<TextView>(Resource.Id.textview_name);
				}
				protected ViewHolder(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }
			}

			private global::AndroidX.Fragment.App.Fragment fragment;

			public RecyclerAdapter(global::AndroidX.Fragment.App.Fragment @this) : base() {
				this.fragment = @this;
			}
			protected RecyclerAdapter(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

			public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType) {
				var v = new ViewHolder(LayoutInflater.From(parent.Context)
					.Inflate(Resource.Layout.layout_listview_main, parent, false));

				v.Root.Click += (s, _) => {
					if(s is View vv && vv.Tag?.ToString() is string json) {
						var p = Newtonsoft.Json.JsonConvert.DeserializeObject<Data.BoardData>(json);
						this.fragment.Activity.SupportFragmentManager.BeginTransaction()
							.Replace(Resource.Id.container, CatalogViewerFragment.NewInstance(p))
							.AddToBackStack(null)
							.Commit();
					}
				};
				return v;
			}

			public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position) {
				if(holder is ViewHolder vh) {
					var s = this.Source[position];
					vh.Root.Tag = new Java.Lang.String(s.ToString());
					vh.Name.Text = s.Name;
				}
			}
		}

		private RecyclerAdapter adapter;

		public MainFragment() : base() { }
		protected MainFragment(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

		public override View? OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState) {
			return inflater.Inflate(Resource.Layout.fragment_main, container, false);
		}

		public override void OnViewCreated(View view, Bundle? savedInstanceState) {
			base.OnViewCreated(view, savedInstanceState);

			var rv = view.FindViewById<RecyclerView>(Resource.Id.recyclerview);
			rv.SetAdapter(this.adapter = new RecyclerAdapter(this));
			rv.SetLayoutManager(new LinearLayoutManager(this.Context));
		}

		public override void OnResume() {
			base.OnResume();

			this.adapter.Source.BeginUpdate()
				.Clear()
				.AddRange(Config.ConfigLoader.Board.Boards)
				.Commit();
		}

		public override void OnPause() {
			base.OnPause();
		}



		public static MainFragment NewInstance() {
			var b = new Bundle();
			return new MainFragment() {
				Arguments = b,
			};
		}
	}
}
