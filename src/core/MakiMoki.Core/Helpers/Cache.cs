using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Helpers {
	public class WeakCache<TKey, TValue> where TValue:class {
		private Dictionary<TKey, WeakReference<TValue>> cache = new Dictionary<TKey, WeakReference<TValue>>();
#pragma warning disable IDE0044
		private volatile object lockObj = new object();
#pragma warning restore IDE0044


		public void Add(TKey key, TValue val) {
			lock(this.lockObj) {
				if(cache.ContainsKey(key)) {
					this.cache[key] = new WeakReference<TValue>(val);
				} else {
					this.cache.Add(key, new WeakReference<TValue>(val));
				}
			}
		}

		public bool TryGetTarget(TKey key, out TValue val) {
			lock(this.lockObj) {
				var r = false;
				val = default;
				if(this.cache.TryGetValue(key, out var c)) {
					if(c.TryGetTarget(out var v)) {
						val = v;
						r = true;
					}
				}

				this.cache = this.cache
					.Where(x => x.Value.TryGetTarget(out var _))
					.ToDictionary(x => x.Key, y => y.Value);
				return r;
			}
		}
	}

	public class TimerCache<TKey, TValue> {
		private Dictionary<TKey, (DateTime Time, TValue Value)> cache = new Dictionary<TKey, (DateTime Time, TValue Value)>();
		private int spanSec;

		public TimerCache() : this(3 * 3600) { } // デフォルト3時間は保持する

		public TimerCache(int spanSec) {
			System.Diagnostics.Debug.Assert(0 < spanSec);
			this.spanSec = spanSec;
		}

		public void Add(TKey key, TValue val) {
			if(cache.ContainsKey(key)) {
				this.cache[key] = (DateTime.Now, val);
			} else {
				this.cache.Add(key, (DateTime.Now, val));
			}
		}

		public bool TryGetTarget(TKey key, out TValue val) {
			var r = false;
			val = default;
			if(this.cache.TryGetValue(key, out var v)) {
				val = v.Value;
				Add(key, val); // 登録時間を更新する
				r = true;
			}

			var now = DateTime.Now.AddSeconds(-this.spanSec);
			this.cache = this.cache
				.Where(x => now <= x.Value.Time)
				.ToDictionary(x => x.Key, y => y.Value);
			return r;
		}
	}

}
