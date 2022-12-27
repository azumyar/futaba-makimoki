using Android.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yarukizero.Net.MakiMoki.Droid.App {
	internal class Runnable : Java.Lang.Object, Java.Lang.IRunnable {
		private readonly Action runner;

		public Runnable(Action? runner) {
			this.runner = runner;
		}
		protected Runnable(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

		public void Run() => this.runner?.Invoke();
	}
}
