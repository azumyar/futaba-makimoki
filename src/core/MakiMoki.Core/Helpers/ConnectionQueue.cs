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
			int waitTime = 2000) {

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
						q = this.queue.OrderBy(x => x.Item.Priority).ToArray();
						this.queue = new ConcurrentBag<(ConnectionQueueItem<T> Item, IObserver<T> Observer)>(
							q.Skip(maxConcurrency));
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
					this.condition.WaitOne();
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
					this.queue.Add((item, o));
				}
				this.condition.Set();

				return System.Reactive.Disposables.Disposable.Empty;
			});
		}
	}

	public class ConnectionQueueItem<T> {
		public Action<IObserver<T>> Action { get; private set; }
		public int Priority { get; private set; }

		public static ConnectionQueueItem<T> From(
			Action<IObserver<T>> action,
			int priority = 0) {

			System.Diagnostics.Debug.Assert(action != null);
			return new ConnectionQueueItem<T>() {
				Action = action,
				Priority = priority
			};
		}
	}
}
