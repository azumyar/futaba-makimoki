using Android.Runtime;
using Android.Views;
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
}