using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reflection;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Helpers {
	public class AutoDisposable : IDisposable {
		public class IgonoreDisposeAttribute : Attribute { }

		private CompositeDisposable disposables;

		public AutoDisposable() {
			disposables = new CompositeDisposable();
		}
	
		public AutoDisposable(object target) {
			System.Diagnostics.Debug.Assert(target != null);

			disposables = GetCompositeDisposable(target);
		}

		public AutoDisposable Add(IDisposable disposable) {
			disposables.Add(disposable);

			return this;
		}

		public void Dispose() {
			disposables.Dispose();
		}

		public static CompositeDisposable GetCompositeDisposable(object target) {
			System.Diagnostics.Debug.Assert(target != null);

			var disposables = new CompositeDisposable();
			foreach(var d in target.GetType()
				.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
				.Where(x => typeof(IDisposable).IsAssignableFrom(x.PropertyType))
				.Where(x => x.GetCustomAttributes(typeof(IgonoreDisposeAttribute), true).Count() == 0)
				.Select(x => x.GetValue(target))
				.Where(x => x != null)
				.Cast<IDisposable>()) {

				disposables.Add(d);
			}
			return disposables;
		}
	}
}
