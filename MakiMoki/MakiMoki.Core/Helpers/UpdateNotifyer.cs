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
}
