using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Linq;

namespace Yarukizero.Net.MakiMoki.Helpers {
	public class ConnectionQueue<T> : IDisposable {
#pragma warning disable IDE0044
		private volatile object lockObj = new object();
#pragma warning restore IDE0044
		private volatile bool isRun = false;
		private ConcurrentBag<(ConnectionQueueItem<T> Item, IObserver<T> Observer)> queue
			= new ConcurrentBag<(ConnectionQueueItem<T> Item, IObserver<T> Observer)>();
		private readonly AutoResetEvent condition = new AutoResetEvent(false);

		public ConnectionQueue(
			string name = null,
			int maxConcurrency = 2,
			int delayTime = 100,
			int waitTime = 2000,
			int? sleepTime = null) {

			System.Diagnostics.Debug.Assert(0 < maxConcurrency);
			System.Diagnostics.Debug.Assert(0 < delayTime);
			System.Diagnostics.Debug.Assert(0 < waitTime);

			// 無限ループだし名前が付けられるのでTaskじゃなくてThreadを採用
			new Thread(() => {
				this.isRun = true;
				while(this.isRun) {
					System.Diagnostics.Debug.WriteLine($"{ nameof(ConnectionQueue<T>) }[{ name }]::Any()");
					var q = default((ConnectionQueueItem<T> Item, IObserver<T> Observer)[]);
					lock(this.lockObj) {
						if(!this.queue.Any()) {
							goto sleep;
						}

						System.Diagnostics.Debug.WriteLine($"{ nameof(ConnectionQueue<T>) }[{ name }]::Get()");
						q = this.queue
							.Where(x => x.Item.FireTime < DateTime.Now)
							.OrderBy(x => x.Item.Priority)
							.OrderBy(x => x.Item.FireTime)
							.Take(maxConcurrency)
							.ToArray();
						if(!q.Any()) {
							goto sleep;
						}
						this.queue = new ConcurrentBag<(ConnectionQueueItem<T> Item, IObserver<T> Observer)>(this.queue.Except(q));
					}

					System.Diagnostics.Debug.WriteLine($"{ nameof(ConnectionQueue<T>) }[{ name }]::Action()");
					foreach(var it in q.Take(maxConcurrency)) {
						Task.Run(() => it.Item.Action(it.Observer));
					}

					var time = DateTime.Now;
					do {
						Task.Delay(delayTime).Wait();
					} while((DateTime.Now - time).TotalMilliseconds < waitTime);
					continue;

				sleep:
					System.Diagnostics.Debug.WriteLine($"{ nameof(ConnectionQueue<T>) }[{ name }]::Sleep()");
					if(sleepTime.HasValue) {
						this.condition.WaitOne(sleepTime.Value);
					} else {
						this.condition.WaitOne();
					}
				}
			}) {
				Name = name,
				IsBackground = true,
			}.Start();
		}

		public void Dispose() {
			this.isRun = false;
			this.queue = new ConcurrentBag<(ConnectionQueueItem<T> Item, IObserver<T> Observer)>();
			this.condition.Set();
		}

		public IObservable<T> Push(ConnectionQueueItem<T> item) {
			System.Diagnostics.Debug.Assert(item != null);
			return Observable.Create<T>(o => {
				lock(this.lockObj) {
					if(item.Tag != null) {
						var a1 = this.queue
							.Where(x => item.Tag.Equals(x.Item.Tag)) // ==ではobjectなのでうまくいかない
							.ToArray();
						var a2 = this.queue.Except(a1);
						this.queue = new ConcurrentBag<(ConnectionQueueItem<T> Item, IObserver<T> Observer)>(a2);
					}
					this.queue.Add((item, o));
				}
				if(item.FireTime <= DateTime.Now) {
					this.condition.Set();
				}

				return System.Reactive.Disposables.Disposable.Empty;
			});
		}

		public void RemoveFromTag(object tag) {
			System.Diagnostics.Debug.Assert(tag != null);
			lock(this.lockObj) {
				var a1 = this.queue
					.Where(x => tag.Equals(x.Item.Tag)) // ==ではobjectなのでうまくいかない
					.ToArray();
				var a2 = this.queue.Except(a1);
				this.queue = new ConcurrentBag<(ConnectionQueueItem<T> Item, IObserver<T> Observer)>(a2);
			}
		}

		public TTag[] ExceptTag<TTag>(IEnumerable<TTag> tags) {
			var a = this.queue
				.Select(x => x.Item.Tag)
				.ToArray();
			return tags
				.Where(x => a.Any(y => !y.Equals(x)))
				.ToArray();
		}
	}

	public class ConnectionQueueItem<T> {
		public Action<IObserver<T>> Action { get; private set; }
		public int Priority { get; private set; }
		public DateTime FireTime { get; private set; }
		public object Tag { get; private set; }

		public static ConnectionQueueItem<T> From(
			Action<IObserver<T>> action,
			int priority = 0,
			DateTime? fireTime = null,
			object tag = null) {

			System.Diagnostics.Debug.Assert(action != null);
			return new ConnectionQueueItem<T>() {
				Action = action,
				Priority = priority,
				FireTime = fireTime ?? DateTime.MinValue,
				Tag = tag,
			};
		}
	}
}
