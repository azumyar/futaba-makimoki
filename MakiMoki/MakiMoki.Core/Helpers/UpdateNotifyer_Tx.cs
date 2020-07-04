
using System;
using System.Collections.Generic;
using System.Linq;


namespace Yarukizero.Net.MakiMoki.Helpers {
	public class UpdateNotifyer<T1> {
		private List<WeakReference<Action<T1>>> notifyer = new List<WeakReference<Action<T1>>>();

		public void AddHandler(Action<T1> handler) {
			this.notifyer.Add(new WeakReference<Action<T1>>(handler));
		}

		public void Notify(T1 arg1) {
			foreach(var a in notifyer) {
				if(a.TryGetTarget(out var t)) {
					t(arg1);
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

		public void Notify(T1 arg1, T2 arg2) {
			foreach(var a in notifyer) {
				if(a.TryGetTarget(out var t)) {
					t(arg1, arg2);
				}
			}
			notifyer = notifyer.Where(x => x.TryGetTarget(out var _)).ToList();
		}
	}

	public class UpdateNotifyer<T1, T2, T3> {
		private List<WeakReference<Action<T1, T2, T3>>> notifyer = new List<WeakReference<Action<T1, T2, T3>>>();

		public void AddHandler(Action<T1, T2, T3> handler) {
			this.notifyer.Add(new WeakReference<Action<T1, T2, T3>>(handler));
		}

		public void Notify(T1 arg1, T2 arg2, T3 arg3) {
			foreach(var a in notifyer) {
				if(a.TryGetTarget(out var t)) {
					t(arg1, arg2, arg3);
				}
			}
			notifyer = notifyer.Where(x => x.TryGetTarget(out var _)).ToList();
		}
	}

	public class UpdateNotifyer<T1, T2, T3, T4> {
		private List<WeakReference<Action<T1, T2, T3, T4>>> notifyer = new List<WeakReference<Action<T1, T2, T3, T4>>>();

		public void AddHandler(Action<T1, T2, T3, T4> handler) {
			this.notifyer.Add(new WeakReference<Action<T1, T2, T3, T4>>(handler));
		}

		public void Notify(T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
			foreach(var a in notifyer) {
				if(a.TryGetTarget(out var t)) {
					t(arg1, arg2, arg3, arg4);
				}
			}
			notifyer = notifyer.Where(x => x.TryGetTarget(out var _)).ToList();
		}
	}

	public class UpdateNotifyer<T1, T2, T3, T4, T5> {
		private List<WeakReference<Action<T1, T2, T3, T4, T5>>> notifyer = new List<WeakReference<Action<T1, T2, T3, T4, T5>>>();

		public void AddHandler(Action<T1, T2, T3, T4, T5> handler) {
			this.notifyer.Add(new WeakReference<Action<T1, T2, T3, T4, T5>>(handler));
		}

		public void Notify(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
			foreach(var a in notifyer) {
				if(a.TryGetTarget(out var t)) {
					t(arg1, arg2, arg3, arg4, arg5);
				}
			}
			notifyer = notifyer.Where(x => x.TryGetTarget(out var _)).ToList();
		}
	}

	public class UpdateNotifyer<T1, T2, T3, T4, T5, T6> {
		private List<WeakReference<Action<T1, T2, T3, T4, T5, T6>>> notifyer = new List<WeakReference<Action<T1, T2, T3, T4, T5, T6>>>();

		public void AddHandler(Action<T1, T2, T3, T4, T5, T6> handler) {
			this.notifyer.Add(new WeakReference<Action<T1, T2, T3, T4, T5, T6>>(handler));
		}

		public void Notify(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) {
			foreach(var a in notifyer) {
				if(a.TryGetTarget(out var t)) {
					t(arg1, arg2, arg3, arg4, arg5, arg6);
				}
			}
			notifyer = notifyer.Where(x => x.TryGetTarget(out var _)).ToList();
		}
	}

	public class UpdateNotifyer<T1, T2, T3, T4, T5, T6, T7> {
		private List<WeakReference<Action<T1, T2, T3, T4, T5, T6, T7>>> notifyer = new List<WeakReference<Action<T1, T2, T3, T4, T5, T6, T7>>>();

		public void AddHandler(Action<T1, T2, T3, T4, T5, T6, T7> handler) {
			this.notifyer.Add(new WeakReference<Action<T1, T2, T3, T4, T5, T6, T7>>(handler));
		}

		public void Notify(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) {
			foreach(var a in notifyer) {
				if(a.TryGetTarget(out var t)) {
					t(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
				}
			}
			notifyer = notifyer.Where(x => x.TryGetTarget(out var _)).ToList();
		}
	}

	public class UpdateNotifyer<T1, T2, T3, T4, T5, T6, T7, T8> {
		private List<WeakReference<Action<T1, T2, T3, T4, T5, T6, T7, T8>>> notifyer = new List<WeakReference<Action<T1, T2, T3, T4, T5, T6, T7, T8>>>();

		public void AddHandler(Action<T1, T2, T3, T4, T5, T6, T7, T8> handler) {
			this.notifyer.Add(new WeakReference<Action<T1, T2, T3, T4, T5, T6, T7, T8>>(handler));
		}

		public void Notify(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) {
			foreach(var a in notifyer) {
				if(a.TryGetTarget(out var t)) {
					t(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
				}
			}
			notifyer = notifyer.Where(x => x.TryGetTarget(out var _)).ToList();
		}
	}

	public class UpdateNotifyer<T1, T2, T3, T4, T5, T6, T7, T8, T9> {
		private List<WeakReference<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>>> notifyer = new List<WeakReference<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>>>();

		public void AddHandler(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> handler) {
			this.notifyer.Add(new WeakReference<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>>(handler));
		}

		public void Notify(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9) {
			foreach(var a in notifyer) {
				if(a.TryGetTarget(out var t)) {
					t(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
				}
			}
			notifyer = notifyer.Where(x => x.TryGetTarget(out var _)).ToList();
		}
	}

	public class UpdateNotifyer<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> {
		private List<WeakReference<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>> notifyer = new List<WeakReference<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>>();

		public void AddHandler(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> handler) {
			this.notifyer.Add(new WeakReference<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>(handler));
		}

		public void Notify(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10) {
			foreach(var a in notifyer) {
				if(a.TryGetTarget(out var t)) {
					t(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
				}
			}
			notifyer = notifyer.Where(x => x.TryGetTarget(out var _)).ToList();
		}
	}

	public class UpdateNotifyer<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> {
		private List<WeakReference<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>> notifyer = new List<WeakReference<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>>();

		public void AddHandler(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> handler) {
			this.notifyer.Add(new WeakReference<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>(handler));
		}

		public void Notify(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11) {
			foreach(var a in notifyer) {
				if(a.TryGetTarget(out var t)) {
					t(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
				}
			}
			notifyer = notifyer.Where(x => x.TryGetTarget(out var _)).ToList();
		}
	}

	public class UpdateNotifyer<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> {
		private List<WeakReference<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>> notifyer = new List<WeakReference<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>>();

		public void AddHandler(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> handler) {
			this.notifyer.Add(new WeakReference<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>(handler));
		}

		public void Notify(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12) {
			foreach(var a in notifyer) {
				if(a.TryGetTarget(out var t)) {
					t(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
				}
			}
			notifyer = notifyer.Where(x => x.TryGetTarget(out var _)).ToList();
		}
	}

	public class UpdateNotifyer<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> {
		private List<WeakReference<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>> notifyer = new List<WeakReference<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>>();

		public void AddHandler(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> handler) {
			this.notifyer.Add(new WeakReference<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>(handler));
		}

		public void Notify(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13) {
			foreach(var a in notifyer) {
				if(a.TryGetTarget(out var t)) {
					t(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
				}
			}
			notifyer = notifyer.Where(x => x.TryGetTarget(out var _)).ToList();
		}
	}

	public class UpdateNotifyer<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> {
		private List<WeakReference<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>> notifyer = new List<WeakReference<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>>();

		public void AddHandler(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> handler) {
			this.notifyer.Add(new WeakReference<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>(handler));
		}

		public void Notify(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14) {
			foreach(var a in notifyer) {
				if(a.TryGetTarget(out var t)) {
					t(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
				}
			}
			notifyer = notifyer.Where(x => x.TryGetTarget(out var _)).ToList();
		}
	}

	public class UpdateNotifyer<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> {
		private List<WeakReference<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>> notifyer = new List<WeakReference<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>>();

		public void AddHandler(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> handler) {
			this.notifyer.Add(new WeakReference<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>(handler));
		}

		public void Notify(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15) {
			foreach(var a in notifyer) {
				if(a.TryGetTarget(out var t)) {
					t(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
				}
			}
			notifyer = notifyer.Where(x => x.TryGetTarget(out var _)).ToList();
		}
	}

	public class UpdateNotifyer<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> {
		private List<WeakReference<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>>> notifyer = new List<WeakReference<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>>>();

		public void AddHandler(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> handler) {
			this.notifyer.Add(new WeakReference<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>>(handler));
		}

		public void Notify(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16) {
			foreach(var a in notifyer) {
				if(a.TryGetTarget(out var t)) {
					t(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
				}
			}
			notifyer = notifyer.Where(x => x.TryGetTarget(out var _)).ToList();
		}
	}

}