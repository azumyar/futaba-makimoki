using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Yarukizero.Net.MakiMoki.Wpf.Behaviors {
	class WheelUpdateBehavior : Behavior<Control> {
		private static readonly int WheelWaitMiliSec = 750;
		private static readonly int WheelResetMiliSec = 1500;
		private ScrollViewer scrollViewer;
		private int deltaStep;
		private DateTime? delataTime;

		public static readonly DependencyProperty CommandProperty =
			DependencyProperty.RegisterAttached(
				nameof(Command), 
				typeof(ICommand), 
				typeof(WheelUpdateBehavior), 
				new PropertyMetadata(null));

		public static readonly DependencyProperty CommandParameterProperty =
			DependencyProperty.RegisterAttached(
				nameof(CommandParameter), 
				typeof(object),
				typeof(WheelUpdateBehavior),
				new PropertyMetadata(null));

		public ICommand Command {
			get => (ICommand)this.GetValue(CommandProperty);
			set {
				this.SetValue(CommandProperty, value);
			}
		}

		public object CommandParameter {
			get => this.GetValue(CommandParameterProperty);
			set {
				this.SetValue(CommandParameterProperty, value);
			}
		}

		protected override void OnAttached() {
			base.OnAttached();
			this.AssociatedObject.Loaded += OnLoadedObject;
			this.AssociatedObject.PreviewMouseWheel += OnMouseWheel;

		}

		protected override void OnDetaching() {
			base.OnDetaching();

			if(this.scrollViewer != null) {
				this.scrollViewer.ScrollChanged -= OnScrollChanged;
				this.scrollViewer = null;
			}
			this.AssociatedObject.PreviewMouseWheel -= OnMouseWheel;
			this.AssociatedObject.Loaded -= OnLoadedObject;
		}

		private void OnLoadedObject(object sender, RoutedEventArgs e) {
			if((e.Source is DependencyObject o) && ((this.scrollViewer = WpfUtil.WpfHelper.FindFirstChild<ScrollViewer>(o)) != null)) {
				this.scrollViewer.ScrollChanged += OnScrollChanged;
			};
		}

		private void OnMouseWheel(object sender, MouseWheelEventArgs e) {
			void observe() {
				Observable.Return(this.delataTime.Value)
					.Delay(TimeSpan.FromMilliseconds(WheelResetMiliSec))
					.ObserveOn(global::Reactive.Bindings.UIDispatcherScheduler.Default)
					.Subscribe(x => {
						this.deltaStep = 0;
						this.delataTime = null;
					});
			}
			void exec() {
				this.deltaStep = 0;
				this.delataTime = null;
				if(this.Command?.CanExecute(this.CommandParameter) ?? false) {
					this.Command?.Execute(this.CommandParameter);
					e.Handled = true;
				}
			}

			if(this.scrollViewer != null) {
				if(0 < e.Delta) {
					// 上スクロール
					if(this.scrollViewer.VerticalOffset <= 0) {
						if(this.deltaStep == 0) {
							this.deltaStep = -1;
							if(!this.delataTime.HasValue) {
								this.delataTime = DateTime.Now.AddMilliseconds(WheelWaitMiliSec);
							}
							observe();
						} else if(this.deltaStep == -1) {
							if(this.delataTime.HasValue && (this.delataTime < DateTime.Now)) {
								exec();
							}
						}
					}
				} else {
					// 下スクロール
					if(this.scrollViewer.ScrollableHeight <= this.scrollViewer.VerticalOffset) {
						if(this.deltaStep == 0) {
							this.deltaStep = 1;
							if(!this.delataTime.HasValue) {
								this.delataTime = DateTime.Now.AddMilliseconds(WheelWaitMiliSec);
							}
							observe();
						} else if(this.deltaStep == 1) {
							if(this.delataTime.HasValue && (this.delataTime < DateTime.Now)) {
								exec();
							}
						}
					}
				}
			}
		}

		private void OnScrollChanged(object sender, ScrollChangedEventArgs e) {
			// スクロールするとホイールはリセットされる
			this.deltaStep = 0;
			this.delataTime = null;
		}
	}
}
