using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reflection;
using System.Text;
using Reactive.Bindings;

namespace Yarukizero.Net.MakiMoki.Helpers {
	public class AutoDisposable : IDisposable {
		public class IgonoreDisposeAttribute : Attribute { }
		public class IgonoreDisposeReactiveBindingsValueAttribute : Attribute { }

		private class BindingsProxy : IDisposable {
			private object target;
			private PropertyInfo propertyInfo;

			public BindingsProxy(IReactiveProperty target) {
				this.target = target;
			}
			public BindingsProxy(IEnumerable<IDisposable> target) {
				this.target = target;
			}
			public BindingsProxy(
				object target,
				PropertyInfo p) {

				this.target = target;
				this.propertyInfo = p;
			}

			public void Dispose() {
				if(this.propertyInfo == null) {
					if(this.target is IReactiveProperty rp) {
						if(rp.Value is IDisposable d) {
							d.Dispose();
						}
					} else if(this.target is IEnumerable<IDisposable> e) {
						foreach(var d in e) {
							d.Dispose();
						}
					}
				} else if((this.propertyInfo.GetValue(this.target) is IReactiveProperty rp)
					&& (rp.Value is IDisposable d)) {

					d.Dispose();
				}
			}
		}

		private CompositeDisposable disposables;

		public AutoDisposable() {
			disposables = new CompositeDisposable();
		}
	
		public AutoDisposable(object target) {
			System.Diagnostics.Debug.Assert(target != null);

			disposables = GetCompositeDisposable(target);
		}

		public AutoDisposable Add(IDisposable disposable, bool processBindingsProperty = true) {
			if(processBindingsProperty) {
				if(disposable is IReactiveProperty rp) {
					this.disposables.Add(new BindingsProxy(rp));
				}
			}
			this.disposables.Add(disposable);

			return this;
		}
		public AutoDisposable AddEnumerable(IEnumerable<IDisposable> disposable) {
			this.disposables.Add(new BindingsProxy(disposable));
			if(disposable is IDisposable d) {
				this.disposables.Add(d);
			}

			return this;
		}

		public void Dispose() {
			disposables.Dispose();
		}

		public static CompositeDisposable GetCompositeDisposable(object target) {
			System.Diagnostics.Debug.Assert(target != null);

			var r = new CompositeDisposable();
			
			foreach(var d in target.GetType()
				.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
				.Where(x => typeof(IReactiveProperty).IsAssignableFrom(x.PropertyType))
				.Where(x => !x.GetCustomAttributes(typeof(IgonoreDisposeReactiveBindingsValueAttribute), true).Any())
				.Select(x => new BindingsProxy(target, x))
				.Cast<IDisposable>()) {

				r.Add(d);
			}

			foreach(var e in target.GetType()
				.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
				.Where(x => typeof(IEnumerable<IDisposable>).IsAssignableFrom(x.PropertyType))
				.Where(x => !x.GetCustomAttributes(typeof(IgonoreDisposeAttribute), true).Any())
				.Select(x => x.GetValue(target))
				.Where(x => x != null)
				.Cast<IEnumerable<IDisposable>>()) {
				foreach(var d in e) {
					r.Add(d);
				}
			}

			foreach(var d in target.GetType()
				.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
				.Where(x => typeof(IDisposable).IsAssignableFrom(x.PropertyType))
				.Where(x => !x.GetCustomAttributes(typeof(IgonoreDisposeAttribute), true).Any())
				.Select(x => x.GetValue(target))
				.Where(x => x != null)
				.Cast<IDisposable>()) {

				r.Add(d);
			}

			return r;
		}
	}
}
