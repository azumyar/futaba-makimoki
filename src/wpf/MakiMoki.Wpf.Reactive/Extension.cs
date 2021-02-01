using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using Reactive.Bindings;

namespace Yarukizero.Net.MakiMoki.Wpf.Reactive {
	public static class MakiMokiCommandExtensions {
		public static MakiMokiCommand ToMakiMokiCommand(this IObservable<bool> canExecuteSource, bool initialValue = true) {
			return new MakiMokiCommand(canExecuteSource.ToReactiveCommand(initialValue));
		}

		public static MakiMokiCommand ToMakiMokiCommand(this IObservable<bool> canExecuteSource, IScheduler scheduler, bool initialValue = true) {
			return new MakiMokiCommand(canExecuteSource.ToReactiveCommand(scheduler, initialValue));
		}

		public static MakiMokiCommand<T> ToMakiMokiCommand<T>(this IObservable<bool> canExecuteSource, bool initialValue = true) {
			return new MakiMokiCommand<T>(canExecuteSource.ToReactiveCommand<T>(initialValue));
		}

		public static MakiMokiCommand<T> ToMakiMokiCommand<T>(this IObservable<bool> canExecuteSource, IScheduler scheduler, bool initialValue = true) {
			return new MakiMokiCommand<T>(canExecuteSource.ToReactiveCommand<T>(scheduler, initialValue));
		}

		public static MakiMokiCommand WithSubscribe(this MakiMokiCommand self, Action onNext, Action<IDisposable> postProcess = null) {
			self.NativeCommand.WithSubscribe(onNext, postProcess);
			return self;
		}

		public static MakiMokiCommand<T> WithSubscribe<T>(this MakiMokiCommand<T> self, Action<T> onNext, Action<IDisposable> postProcess = null) {
			self.NativeCommand.WithSubscribe<T>(onNext, postProcess);
			return self;
		}

		public static MakiMokiCommand WithSubscribe(this MakiMokiCommand self, Action onNext, out IDisposable disposable) {
			self.NativeCommand.WithSubscribe(onNext, out disposable);
			return self;
		}

		public static MakiMokiCommand<T> WithSubscribe<T>(this MakiMokiCommand<T> self, Action<T> onNext, out IDisposable disposable) {
			self.NativeCommand.WithSubscribe<T>(onNext, out disposable);
			return self;
		}
	}
}
