using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables.Shapes;
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using AndroidX.ConstraintLayout.Core.Widgets;
using AndroidX.Interpolator.View.Animation;
using AndroidX.RecyclerView.Widget;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yarukizero.Net.MakiMoki.Droid.App {
	internal abstract class RecyclerViewAdapter<T> : RecyclerView.Adapter {
		public class RecyclerViewAdapterCollection : ICollection<T> {
			public class BatchObject {
				private RecyclerViewAdapterCollection target;

				public BatchObject(RecyclerViewAdapterCollection @this) {
					this.target = @this;
				}

				public BatchObject Add(T item) {
					this.target.AddHook(item);
					this.target.collection.Add(item);
					return this;
				}
				public BatchObject AddRange(IEnumerable<T> items) {
					foreach(var o in items) {
						this.target.AddHook(o);
					}
					this.target.collection.AddRange(items);
					return this;
				}

				public BatchObject Clear() {
					foreach(var o in this.target) {
						this.target.RemoveHook(o);
					}
					this.target.Clear();
					return this;
				}

				public BatchObject Remove(T item) {
					if(this.target.collection.Remove(item)) {
						this.target.RemoveHook(item);
					}
					return this;
				}

				public BatchObject RemoveAt(int index) {
					var item = this.target.collection.ElementAtOrDefault(index);
					this.target.collection.RemoveAt(index);
					this.target.RemoveHook(item);
					return this;
				}

				public void Commit() {
					this.target.adapter.NotifyDataSetChanged();
				}
			}


			private RecyclerViewAdapter<T> adapter;
			private BatchObject batchObj;
			private readonly List<T> collection = new List<T>();

			public RecyclerViewAdapterCollection(RecyclerViewAdapter<T> @this) {
				this.adapter = @this;
				this.batchObj = new BatchObject(this);
			}

			public T this[int index] => this.collection.ElementAt(index);

			public int Count => collection.Count;

			public bool IsReadOnly => ((ICollection<T>)collection).IsReadOnly;

			public BatchObject BeginUpdate() => this.batchObj;

			public void Add(T item) {
				this.AddHook(item);
				this.collection.Add(item);
				this.adapter.NotifyItemChanged(this.collection.Count - 1);
			}

			public void AddRange(IEnumerable<T> items) {
				int index = this.collection.Count;
				foreach(var o in items) {
					this.AddHook(o);
				}
				this.collection.AddRange(items);
				this.adapter.NotifyItemRangeChanged(index, items.Count());
			}


			public void Clear() {
				foreach(var o in this.collection) {
					this.RemoveHook(o);
				}
				this.collection.Clear();
				this.adapter.NotifyDataSetChanged();
			}

			public bool Contains(T item) {
				return this.collection.Contains(item);
			}

			public void CopyTo(T[] array, int arrayIndex) {
				this.collection.CopyTo(array, arrayIndex);
			}

			public bool Remove(T item) {
				if(this.collection.Remove(item)) {
					this.RemoveHook(item);
					this.adapter.NotifyItemRemoved(this.collection.IndexOf(item));
					return true;
				} else {
					return false;
				}
			}

			public void RemoveAt(int index) {
				var item = this.collection.ElementAtOrDefault(index);
				this.collection.RemoveAt(index);
				this.RemoveHook(item);
				this.adapter.NotifyItemRemoved(index);
			}

			public IEnumerator<T> GetEnumerator() {
				return this.collection.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator() {
				return this.collection.GetEnumerator();
			}

			private void AddHook(T obj) {
				if(obj is INotifyPropertyChanged np) {
					np.PropertyChanged += this.OnItemPropertyChanged;
				}
			}

			private void RemoveHook(T obj) {
				if(obj is INotifyPropertyChanged np) {
					np.PropertyChanged -= this.OnItemPropertyChanged;
				}
			}

			private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e) {
				if(sender is T s) {
					var idx = this.collection.IndexOf(s);
					if(0 <= idx) {
						this.adapter.NotifyItemChanged(idx);
					}
				}
			}

		}


		public RecyclerViewAdapterCollection Source { get; }
		public override int ItemCount => this.Source.Count();


		public RecyclerViewAdapter() : base() {
			this.Source = new RecyclerViewAdapterCollection(this);
		}
		protected RecyclerViewAdapter(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { 
			this.Source = new RecyclerViewAdapterCollection(this);
		}
	}

	internal static class RecyclerViewSwipeUpdateHelper {
		public interface ISwipeUpdater {
			event EventHandler<SwipeUpdateEventArgs> Updating;
		}

		public interface ISwipeUpdateObject {
			void EndUpdate();
		}

		public class SwipeUpdateEventArgs : EventArgs {
			public ISwipeUpdateObject UpdateObject { get; }

			public SwipeUpdateEventArgs(ISwipeUpdateObject @this) {
				this.UpdateObject = @this;
			}
		}

		private class OverScrollRunner : ISwipeUpdater, ISwipeUpdateObject {
			private class ItemTouchListener : Java.Lang.Object, RecyclerView.IOnItemTouchListener {
				private readonly FastOutSlowInInterpolator interpolator = new FastOutSlowInInterpolator();
				private OverScrollRunner runner;
				private (float X, float Y) pointerPos = (0, 0);
				public ItemTouchListener(OverScrollRunner @this) {
					this.runner = @this;
				}
				protected ItemTouchListener(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) {}

				public bool OnInterceptTouchEvent(RecyclerView recyclerView, MotionEvent @event) {
					if(@event.PointerCount != 1) {
						return false;
					}

					var ac = (Android.App.Activity)recyclerView.Context;
					switch(@event.Action) {
					case MotionEventActions.Down:
						this.pointerPos = (@event.RawX, @event.RawY);
						break;
					case MotionEventActions.Move:
						if(this.runner.updateTopOrBottom.HasValue) {
							if(this.runner.updateTopOrBottom.Value) {
								if(this.pointerPos.Y < @event.RawY) {
									return true;
								}
							} else {
								if(@event.RawY < this.pointerPos.Y) {
									return true;
								}
							}
						} else {
							if(this.runner.isViewAttached) {
								this.runner.isViewAttached = false;
								ac.FindViewById<ViewGroup>(Resource.Id.container_topmost).RemoveView(this.runner.updateView);
							}
						}
						break;
					}
					return false;
				}

				public void OnRequestDisallowInterceptTouchEvent(bool disallow) {}

				public void OnTouchEvent(RecyclerView recyclerView, MotionEvent @event) {
					var ac = (Android.App.Activity)recyclerView.Context;
					switch(@event.Action) {
					case MotionEventActions.Up:
						if(!this.runner.isUpdating) {
							var x = this.pointerPos.X - @event.RawX;
							var y = this.pointerPos.Y - @event.RawY;
							var length = (int)Math.Sqrt(x * x + y * y);
							if((this.runner.overscrollPx < length) && (IsTop: this.runner.updateTopOrBottom.Value, Y: y) switch {
								var v when v.IsTop => v switch {
									var vv when vv.Y < 0 => true,
									_ => false,
								},
								var v => v switch {
									var vv when 0 < vv.Y => true,
									_ => false,
								},
							}) {
								this.runner.isUpdating = true;
								this.runner.Updating?.Invoke(recyclerView, new SwipeUpdateEventArgs(this.runner));
								if(this.runner.updateTopOrBottom.Value) {
									var lp = new RelativeLayout.LayoutParams(this.runner.sizePx, this.runner.sizePx);
									lp.AddRule(LayoutRules.CenterHorizontal);
									lp.AddRule(LayoutRules.AlignParentTop);
									lp.TopMargin = this.runner.updatePosPx;
									this.runner.updateView.LayoutParameters = lp;
								} else {
									var lp = new RelativeLayout.LayoutParams(this.runner.sizePx, this.runner.sizePx);
									lp.AddRule(LayoutRules.CenterHorizontal);
									lp.AddRule(LayoutRules.AlignParentBottom);
									lp.BottomMargin = this.runner.updatePosPx;
									this.runner.updateView.LayoutParameters = lp;
								}

								this.runner.updateView.StartAnimation(new RotateAnimation(
									0f, 360f,
									Dimension.RelativeToSelf, 0.5f,
									Dimension.RelativeToSelf, 0.5f) {
									Duration = 500,
									RepeatCount = Animation.Infinite,
									FillAfter= false,
								});
							}
							this.runner.updateTopOrBottom = null;
						}
						if(this.runner.isViewAttached && !this.runner.isUpdating) {
							this.runner.isViewAttached = false;
							ac.FindViewById<ViewGroup>(Resource.Id.container_topmost).RemoveView(this.runner.updateView);
						}
						break;
					case MotionEventActions.Move:
						if(this.runner.updateTopOrBottom.HasValue) {
							var x = this.pointerPos.X - @event.RawX;
							var y = this.pointerPos.Y - @event.RawY;
							var length = (int)Math.Sqrt(x * x + y * y);
							var viewHeight = this.runner.updateView.Height;
							var maxLength = this.runner.overscrollPx + viewHeight;
							if(this.runner.updateTopOrBottom.Value) {
								var lp = new RelativeLayout.LayoutParams(this.runner.sizePx, this.runner.sizePx);
								var @in = (float)Math.Min(maxLength, length * (y < 0) switch {
									true => 1,
									false => -1,
								});
								var val = @in switch {
									var v when 0 < v => this.interpolator.GetInterpolation(@in / maxLength) * maxLength,
									_ => 0,
								};
								var deg = val switch {
									var v when 0 < v => v / maxLength * 360,
									_ => 0f,
								};

								lp.AddRule(LayoutRules.CenterHorizontal);
								lp.AddRule(LayoutRules.AlignParentTop);
								lp.TopMargin = (int)val - viewHeight;
								this.runner.updateView.Rotation = deg;
								this.runner.updateView.LayoutParameters = lp;
							} else {
								var lp = new RelativeLayout.LayoutParams(this.runner.sizePx, this.runner.sizePx);
								var @in = (float)Math.Min(maxLength, length * (y < 0) switch {
									true => -1,
									false => 1,
								});
								var val = @in switch {
									var v when 0 < v => this.interpolator.GetInterpolation(@in / maxLength) * maxLength,
									_ => 0,
								};
								var deg = val switch {
									var v when 0 < v => v / maxLength * 360,
									_ => 0f,
								};
								
								lp.AddRule(LayoutRules.CenterHorizontal);
								lp.AddRule(LayoutRules.AlignParentBottom);
								lp.BottomMargin = (int)val - viewHeight;
								this.runner.updateView.Rotation = deg;
								this.runner.updateView.LayoutParameters = lp;
							}
							if(!this.runner.isViewAttached) {
								this.runner.isViewAttached = true;
								ac.FindViewById<ViewGroup>(Resource.Id.container_topmost).AddView(this.runner.updateView);
							}
						}
						break;
					}
				}
			}

			private readonly int updatePosPx;
			private readonly int overscrollPx;
			private volatile bool isUpdating = false;
			private volatile bool isViewAttached = false;

			private readonly RecyclerView recyclerView;
			private readonly ImageView updateView;
			private readonly int sizePx;
			private bool? updateTopOrBottom = null;

			public event EventHandler<SwipeUpdateEventArgs> Updating;

			public OverScrollRunner(RecyclerView @this) {
				this.updatePosPx = DroidUtil.Util.Dp2Px(96, @this.Context);
				this.overscrollPx = DroidUtil.Util.Dp2Px(128, @this.Context);

				this.recyclerView = @this;
				this.updateView = new ImageView(@this.Context);
				this.updateView.SetBackgroundColor(Color.Blue);
				this.sizePx = DroidUtil.Util.Dp2Px(64, @this.Context);
				this.updateView.LayoutParameters = new ViewGroup.LayoutParams(DroidUtil.Util.Dp2Px(24, @this.Context), DroidUtil.Util.Dp2Px(24, @this.Context));

				this.recyclerView.AddOnItemTouchListener(new ItemTouchListener(this));
			}

			public void EndUpdate() {
				this.isUpdating = false;
				if(this.isViewAttached) {
					this.isViewAttached = false;
					this.updateView.Animate()
						.SetDuration(200)
						.SetInterpolator(new FastOutLinearInInterpolator())
						.Alpha(0f)
						.WithEndAction(new App.Runnable(() => {
							this.updateView.ClearAnimation();
							((Android.App.Activity)this.recyclerView.Context).FindViewById<ViewGroup>(Resource.Id.container_topmost).RemoveView(this.updateView);
							this.updateView.Alpha = 1f;
						}))
						.Start();
				}
			}

			public int ScrollVerticallyBy(Func<int, RecyclerView.Recycler?, RecyclerView.State?, int> @base, int dy, RecyclerView.Recycler? recycler, RecyclerView.State? state) {
				var range = @base(dy, recycler, state);
				var overscroll = dy - range;

				this.updateTopOrBottom = ((range == 0) && (0 < Math.Abs(dy))) switch {
					true => overscroll < 0,
					false => null,
				};

				return range;
			}
		}


		private class SwipeUpdateLinearLayoutManager : LinearLayoutManager {
			internal readonly OverScrollRunner? runner = null;

			public SwipeUpdateLinearLayoutManager(RecyclerView recyclerView) : base(recyclerView.Context) {
				this.runner = new OverScrollRunner(recyclerView);
			}
			protected SwipeUpdateLinearLayoutManager(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

			public override int ScrollVerticallyBy(int dy, RecyclerView.Recycler? recycler, RecyclerView.State? state) {
				return runner?.ScrollVerticallyBy(base.ScrollVerticallyBy, dy, recycler, state) ?? base.ScrollVerticallyBy(dy, recycler, state);
			}
		}

		private class SwipeUpdateGridLayoutManager : GridLayoutManager {
			internal readonly OverScrollRunner? runner = null;

			public SwipeUpdateGridLayoutManager(RecyclerView recyclerView, int spanCount) : base(recyclerView.Context, spanCount) {
				this.runner = new OverScrollRunner(recyclerView);
			}
			protected SwipeUpdateGridLayoutManager(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

			public override int ScrollVerticallyBy(int dy, RecyclerView.Recycler? recycler, RecyclerView.State? state) {
				return runner?.ScrollVerticallyBy(base.ScrollVerticallyBy, dy, recycler, state) ?? base.ScrollVerticallyBy(dy, recycler, state);
			}
		}



		public static ISwipeUpdater AttachLinearLayout(RecyclerView view) {
			var m = new SwipeUpdateLinearLayoutManager(view);
			view.SetLayoutManager(m);
			return m.runner;
		}

		public static ISwipeUpdater AttachGridLayout(RecyclerView view, int spanCount) {
			var m = new SwipeUpdateGridLayoutManager(view, spanCount);
			view.SetLayoutManager(m);
			return m.runner;
		}
	}
}