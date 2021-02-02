using System;
using System.Reactive.Concurrency;
using System.Windows.Input;
using Reactive.Bindings;

namespace Yarukizero.Net.MakiMoki.Wpf.Reactive {
	/*
	 * NativeCommandはDisposeされていると例外吐いて制御が大変なので例外無視するラッパクラス
	 * 
	 */

	public class MakiMokiCommand<T> : IObservable<T>, ICommand, IDisposable {
		internal ReactiveCommand<T> NativeCommand { get; private set; }

		public MakiMokiCommand() {
			this.NativeCommand = new ReactiveCommand<T>();
		}
		public MakiMokiCommand(IScheduler scheduler) {
			this.NativeCommand = new ReactiveCommand<T>(scheduler);
		}
		public MakiMokiCommand(IObservable<bool> canExecuteSource, bool initialValue = true) {
			this.NativeCommand = new ReactiveCommand<T>(canExecuteSource, initialValue);
		}
		public MakiMokiCommand(IObservable<bool> canExecuteSource, IScheduler scheduler, bool initialValue = true) {
			this.NativeCommand = new ReactiveCommand<T>(canExecuteSource, scheduler, initialValue);
		}
		internal MakiMokiCommand(ReactiveCommand<T> command) {
			this.NativeCommand = command;
		}

		public void Dispose() {
			if(this.NativeCommand != null) {
				this.NativeCommand.Dispose();
				this.NativeCommand = null;
			}
		}

		public event EventHandler CanExecuteChanged {
			add {
				if(this.NativeCommand != null) {
					this.NativeCommand.CanExecuteChanged += value;
				}
			}
			remove {
				if(this.NativeCommand != null) {
					this.NativeCommand.CanExecuteChanged -= value;
				}
			}
		}

		public bool CanExecute() => this.NativeCommand?.CanExecute() ?? false;
		public void Execute(T parameter) => this.NativeCommand?.Execute(parameter);
		public IDisposable Subscribe(IObserver<T> observer) => this.NativeCommand?.Subscribe(observer) ?? System.Reactive.Disposables.Disposable.Empty;
	
		bool ICommand.CanExecute(object parameter) => (this.NativeCommand as ICommand)?.CanExecute(parameter) ?? false;
		void ICommand.Execute(object parameter) => (this.NativeCommand as ICommand)?.Execute(parameter);
	}

	public class MakiMokiCommand : MakiMokiCommand<object> {
		internal new ReactiveCommand NativeCommand { get; private set; }

		public MakiMokiCommand() : this(new ReactiveCommand()) { }
		public MakiMokiCommand(IScheduler scheduler)
			: this(new ReactiveCommand(scheduler)) { }
		public MakiMokiCommand(IObservable<bool> canExecuteSource, bool initialValue = true)
			: this(new ReactiveCommand(canExecuteSource, initialValue)) { }
		public MakiMokiCommand(IObservable<bool> canExecuteSource, IScheduler scheduler, bool initialValue = true)
			: this(new ReactiveCommand(canExecuteSource, scheduler, initialValue)) { }
		internal MakiMokiCommand(ReactiveCommand command) : base(command) {
			this.NativeCommand = command;
		}

		public IDisposable Subscribe(Action onNext) => this.NativeCommand?.Subscribe(onNext) ?? System.Reactive.Disposables.Disposable.Empty;
	}
}
