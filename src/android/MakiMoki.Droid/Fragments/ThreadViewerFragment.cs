using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Yarukizero.Net.MakiMoki.Droid.Extensions;
using Android.Views;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.FloatingActionButton;
using Reactive.Bindings;
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;

namespace Yarukizero.Net.MakiMoki.Droid.Fragments {
	internal class ThreadViewerFragment : global::AndroidX.Fragment.App.Fragment {
		public class RecyclerAdapter : App.RecyclerViewAdapter<Data.FutabaContext.Item?> {
			private class ViewHolder : RecyclerView.ViewHolder {
				public View Root { get; }
				public View Container { get; }
				public ImageView Image { get; }
				public TextView Index { get; }
				public TextView Mail { get; }
				public TextView Title { get; }
				public TextView Name { get; }
				public TextView Date { get; }
				public TextView No { get; }
				public TextView Soudane { get; }
				public TextView Text { get; }

				public ViewHolder(View v) : base(v) {
					this.Root = v;
					this.Container = v.FindViewById<View>(Resource.Id.container);
					this.Image = v.FindViewById<ImageView>(Resource.Id.image);
					this.Index = v.FindViewById<TextView>(Resource.Id.textview_index);
					this.Mail = v.FindViewById<TextView>(Resource.Id.textview_mail);
					this.Title = v.FindViewById<TextView>(Resource.Id.textview_title);
					this.Name = v.FindViewById<TextView>(Resource.Id.textview_name);
					this.Date = v.FindViewById<TextView>(Resource.Id.textview_date);
					this.No = v.FindViewById<TextView>(Resource.Id.textview_no);
					this.Soudane = v.FindViewById<TextView>(Resource.Id.textview_soudane);
					this.Text = v.FindViewById<TextView>(Resource.Id.textview_text);
				}
				protected ViewHolder(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }
			}

			private ThreadViewerFragment fragment;

			public RecyclerAdapter(ThreadViewerFragment @this) : base() {
				this.fragment = @this;
			}
			protected RecyclerAdapter(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

			public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType) {
				var v = new ViewHolder(LayoutInflater.From(parent.Context)
					.Inflate(Resource.Layout.layout_listview_thread, parent, false));
				/*
				v.Click += (s, _) => {
					if(s is View vv && vv.Tag?.ToString() is string json) {
						var p = Newtonsoft.Json.JsonConvert.DeserializeObject<Data.UrlContext>(json);
						this.fragment.Activity.SupportFragmentManager.BeginTransaction()
							.Replace(Resource.Id.container, ThreadViewerFragment.NewInstance(p))
							.AddToBackStack(null)
							.Commit();
					}
				};
				*/
				return v;
			}

			public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position) {
				static ViewStates conv(bool b) => b switch {
					true => ViewStates.Visible,
					false => ViewStates.Gone
				};

				if(holder is ViewHolder vh) {
					var s = this.Source[position];
					if(s == null) {
						vh.Container.Visibility = conv(false);
					} else {
						vh.Container.Visibility = conv(true);

						if(!string.IsNullOrEmpty(s.ResItem.Res.Thumb) && 0 < s.ResItem.Res.Fsize) {
							var uri = new Uri(s.Url.BaseUrl);
							var th = $"{uri.Scheme}://{uri.Authority}{s.ResItem.Res.Thumb}";
							vh.Image.Tag = new Java.Lang.String(th);
							vh.Image.SetImageBitmap(null);
							App.ImageResolver.Instance.Get(th, 256)
								.Subscribe(x => {
									if(vh.Image.Tag.ToString() == th) {
										vh.Image.Visibility = conv(true);
										vh.Image.SetImageBitmap(x);
									}
								});
						} else {
							vh.Image.Visibility = conv(false);
						}
						vh.Mail.Visibility = conv(!string.IsNullOrEmpty(s.ResItem.Res.Email));
						vh.Name.Visibility = vh.Title.Visibility = conv(this.fragment.Properties.Board.Extra.Name);
						vh.Soudane.Visibility = conv(s.Soudane != 0);

						vh.Index.Text = s.ResItem.Res.Rsc.ToString();
						vh.Mail.Text = $"[{s.ResItem.Res.Email}]";
						vh.Title.Text = s.ResItem.Res.Sub;
						vh.Name.Text = s.ResItem.Res.Name;
						vh.Date.Text = s.ResItem.Res.Now;
						vh.No.Text = $"No.{s.ResItem.No}";
						vh.Soudane.Text = $"x{s.Soudane}";
						vh.Text.Text = Util.TextUtil.RowComment2Text(s.ResItem.Res.Com);

						if(!string.IsNullOrEmpty(s.ResItem.Res.Thumb) && 0 < s.ResItem.Res.Fsize) {
							var uri = new Uri(s.Url.BaseUrl);
							var th = $"{uri.Scheme}://{uri.Authority}{s.ResItem.Res.Thumb}";
							vh.Image.Tag = new Java.Lang.String(th);
							vh.Image.SetImageBitmap(null);
							App.ImageResolver.Instance.Get(th)
								.Subscribe(x => {
									if(vh.Image.Tag.ToString() == th) {
										vh.Image.SetImageBitmap(x);
										var w = DroidUtil.Util.Dp2Px(300, vh.Image.Context);
										var h = (int)((float)w / x.Width * x.Height);
										var p = vh.Image.LayoutParameters;
										p.Width = w;
										p.Height = h;
										vh.Image.LayoutParameters = p;
									}
								});
						}

					}
				}
			}
		}

		public class Contract : ActivityResultContract {
			private readonly ThreadViewerFragment fragment;

			public Contract(ThreadViewerFragment @this) : base() {
				this.fragment = @this;
			}
			protected Contract(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

			public override Android.Content.Intent CreateIntent(Android.Content.Context context, Java.Lang.Object? input) {
				return new Android.Content.Intent(context, typeof(Activities.PostActivity))
					.InJson(this.fragment.Properties.Board)
					.InJson(this.fragment.Properties.Url);
			}

			public override Java.Lang.Object? ParseResult(int resultCode, Android.Content.Intent? intent) {
				return resultCode switch {
					var v when v == DroidConst.ActivityResultCodePost => new Java.Lang.Boolean(
						intent.GetBooleanExtra(Activities.PostActivity.ResultCodeSucessed, false)),
					_ => throw new NotImplementedException()
				};
			}
		}

		public class ActivityResultCallback : Java.Lang.Object, IActivityResultCallback {
			private readonly ThreadViewerFragment fragment;

			public ActivityResultCallback(ThreadViewerFragment @this) : base() {
				this.fragment = @this;
			}
			protected ActivityResultCallback(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

			public void OnActivityResult(Java.Lang.Object? p) {
				if(p is Java.Lang.Boolean b && b.BooleanValue()) {
					this.fragment.UpdateThread()
						.Subscribe();
				}
			}
		}

		private class PropertiesHolder : IDisposable {
			public Data.BoardData Board { get; set; }
			public Data.UrlContext Url { get; set; }

			public ReactiveProperty<Data.FutabaContext> FutabaContext { get; } = new ReactiveProperty<Data.FutabaContext>();
			public ReactiveProperty<Data.FutabaResponse> FutabaResponse { get; } = new ReactiveProperty<Data.FutabaResponse>();
			public ReactiveProperty<string> SearchText { get; } = new ReactiveProperty<string>(initialValue: "");

			public IDisposable ThreadSubscriber { get; set; }

			public void Dispose() {
				new Helpers.AutoDisposable(this).Dispose();
			}
		}

		private PropertiesHolder Properties { get; } = new PropertiesHolder();
		private RecyclerView recyclerView;
		private RecyclerAdapter adapter;
		private ActivityResultLauncher activityLuncher;
		private static readonly int DummyItemNum = 2;

		public ThreadViewerFragment() : base() { }
		protected ThreadViewerFragment(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

		public override View? OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState) {
			return inflater.Inflate(Resource.Layout.fragment_thread, container, false);
		}

		public override void OnViewCreated(View view, Bundle? savedInstanceState) {
			base.OnViewCreated(view, savedInstanceState);

			this.activityLuncher = this.RegisterForActivityResult(new Contract(this), new ActivityResultCallback(this));
			var isInit = this.adapter == null;
			this.recyclerView = view.FindViewById<RecyclerView>(Resource.Id.recyclerview);
			App.RecyclerViewSwipeUpdateHelper.AttachLinearLayout(this.recyclerView).Updating += (_, e) => {
				this.UpdateThread()
					.ObserveOn(UIDispatcherScheduler.Default)
					.Subscribe(_ => {
						e.UpdateObject.EndUpdate();
					});
			};
			this.recyclerView.ScrollChange += (_, e) => {
				if(this.recyclerView.GetLayoutManager() is LinearLayoutManager lm) {
					var @new = view.FindViewById<ExtendedFloatingActionButton>(Resource.Id.button_new);
					if(@new.Extended && (0 < lm.FindFirstVisibleItemPosition())) {
						@new.Shrink();
					} else if(!@new.Extended && (lm.FindFirstVisibleItemPosition() == 0)) {
						@new.Extend();
					}
				}
			};
			view.FindViewById<Button>(Resource.Id.button_new).Click += (_, _) => {
				this.activityLuncher.Launch(null);
			};

			if(!isInit) {
				this.recyclerView.SetAdapter(this.adapter);
				return;
			} else {
				this.recyclerView.SetAdapter(this.adapter = new RecyclerAdapter(this));

				this.Properties.Board = this.Arguments.OutJson<Data.BoardData>();
				this.Properties.Url = this.Arguments.OutJson<Data.UrlContext>();
				this.Properties.FutabaContext.Value = Data.FutabaContext.FromThreadEmpty(this.Properties.Board, this.Properties.Url.ThreadNo);

				this.Properties.ThreadSubscriber = this.SubscribeThread();

				if(savedInstanceState == null) {
					this.UpdateThread()
						.Subscribe();
				}
			}
		}

		private IObservable<bool> UpdateThread() {
			return Observable.Create<bool>(o => {
				//this.Properties.IsUpdating.Value = true;
				Util.FutabaApiReactive.GetThreadRes(
					this.Properties.Board,
					this.Properties.Url.ThreadNo,
					this.Properties.FutabaContext.Value,
					true)
					.ObserveOn(UIDispatcherScheduler.Default)
					.Finally(() => {
						//this.Properties.IsUpdating.Value = false;
						o.OnCompleted();
					}).Subscribe(x => {
						o.OnNext(x.Successed);
						if(x.Successed) {
							this.Properties.FutabaContext.Value = x.Data;
						} else {

						}
						if(x.Cookies != null) {
							Config.ConfigLoader.UpdateCookie(this.Properties.Board.Url, x.Cookies);
						}
					});
				return System.Reactive.Disposables.Disposable.Empty;
			});
		}

		private IDisposable SubscribeThread() {
			return this.Properties.FutabaContext.CombineLatest(
				this.Properties.SearchText,
				(x, y) => (Futaba: x, Search: y))
				.ObserveOn(UIDispatcherScheduler.Default)
				.Subscribe(x => {
					var c = this.adapter.ItemCount - DummyItemNum;
					if(c < 0) {
						this.adapter.Source.BeginUpdate()
							.Clear()
							.AddRange(x.Futaba.ResItems)
							.AddRange(new Data.FutabaContext.Item[DummyItemNum])
							.Commit();
						return;
					}
					
					if(c < x.Futaba.ResItems.Length) {
						var @new = x.Futaba.ResItems.Skip(c);
						if(@new.Count() <= DummyItemNum) {
							foreach(var it in @new.Select((y, i) => (Value: y, Index: i))) {
								this.adapter.Source[c + it.Index] = @new.ElementAt(it.Index);
							}
							for(var i = 0; i < @new.Count(); i++) {
								this.adapter.Source.Add(null);
							}
						} else {
							foreach(var it in @new.Take(DummyItemNum).Select((y, i) => (Value: y, Index: i))) {
								this.adapter.Source[c + it.Index] = @new.ElementAt(it.Index);
							}
							this.adapter.Source.AddRange(@new.Skip(DummyItemNum));
							this.adapter.Source.AddRange(new Data.FutabaContext.Item[DummyItemNum]);
						}
					}
					foreach(var it in x.Futaba.ResItems.Take(c).Select((x, i) => (Val: x, Index: i))) {
						if(this.adapter.Source[it.Index].HashText != it.Val.HashText) {
							this.adapter.Source[it.Index] = it.Val;
						}
					}
				});
		}

		public static ThreadViewerFragment NewInstance(Data.BoardData board, Data.UrlContext threadUrl) {
			var b = new Bundle()
				.InJson(board)
				.InJson(threadUrl);
			return new ThreadViewerFragment() {
				Arguments = b,
			};
		}
	}
}
