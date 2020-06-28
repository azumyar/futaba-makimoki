using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Helpers {
	public class UpdateNotifyer {
		private List<WeakReference<Action>> notifyer = new List<WeakReference<Action>>();

		public void AddHandler(Action handler) {
			this.notifyer.Add(new WeakReference<Action>(handler));
		}

		public void Notify() {
			foreach(var a in notifyer) {
				if(a.TryGetTarget(out var t)) {
					t();
				}
			}
			notifyer = notifyer.Where(x => x.TryGetTarget(out var _)).ToList();
		}
	}

	public class UpdateNotifyer<T1> {
		private List<WeakReference<Action<T1>>> notifyer = new List<WeakReference<Action<T1>>>();

		public void AddHandler(Action<T1> handler) {
			this.notifyer.Add(new WeakReference<Action<T1>>(handler));
		}

		public void Notify(T1 args) {
			foreach(var a in notifyer) {
				if(a.TryGetTarget(out var t)) {
					t(args);
				}
			}
			notifyer = notifyer.Where(x => x.TryGetTarget(out var _)).ToList();
		}
	}

	public class UpdateNotifyer<T1, T2> {
		private List<WeakReference<Action<T1, T2>>> notifyer = new List<WeakReference<Action<T1, T2>>>();

		public void AddHandler(Action<T1, T2> handler) {
			this.notifyer.Add(new WeakReference<Action<T1, T2>>(handler));
		}

		public void Notify(T1 args1, T2 args2) {
			foreach(var a in notifyer) {
				if(a.TryGetTarget(out var t)) {
					t(args1, args2);
				}
			}
			notifyer = notifyer.Where(x => x.TryGetTarget(out var _)).ToList();
		}
	}
}
