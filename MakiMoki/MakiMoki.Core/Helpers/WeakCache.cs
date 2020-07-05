using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Helpers {
	public class WeakCache<TKey, TValue> where TValue:class {
		private Dictionary<TKey, WeakReference<TValue>> cach = new Dictionary<TKey, WeakReference<TValue>>();

		public void Add(TKey key, TValue val) {
			if(cach.ContainsKey(key)) {
				this.cach[key] = new WeakReference<TValue>(val);
			} else {
				this.cach.Add(key, new WeakReference<TValue>(val));
			}
		}

		public bool TryGetTarget(TKey key, out TValue val) {
			var r = false;
			val = default;
			if(this.cach.TryGetValue(key, out var c)) {
				if(c.TryGetTarget(out var v)) {
					val = v;
					r = true;
				}
			}

			this.cach = this.cach
				.Where(x => x.Value.TryGetTarget(out var _))
				.ToDictionary(x => x.Key, y => y.Value);
			return r;
		}
	}
}
