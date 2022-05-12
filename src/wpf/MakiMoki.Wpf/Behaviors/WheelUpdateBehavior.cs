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
using Reactive.Bindings;

namespace Yarukizero.Net.MakiMoki.Wpf.Behaviors {
	class WheelUpdateBehavior : Behavior<Control> {
		public enum WheelUpdatePosition {
			Default,
			Top,
			Bottom
		}

		public enum WheelUpdateState {
			Default,
			Begin,
			Post
		}

		private static readonly string BeginUpdateMessage = "約１秒間ホイールで更新";
		private static readonly string FireUpdateMessage = "更新を実行";
		private static readonly int WheelWaitMiliSec = 750;
		private static readonly int WheelResetMiliSec = 1500;
		private ScrollViewer scrollViewer;
		private int deltaStep;
		private DateTime? delataTime;

		public static readonly DependencyProperty UpdatePositionProperty =
			DependencyProperty.RegisterAttached(
				nameof(UpdatePosition),
				typeof(WheelUpdatePosition),
				typeof(WheelUpdateBehavior),
				new PropertyMetadata(WheelUpdatePosition.Default));

		public static readonly DependencyProperty UpdateStateProperty =
			DependencyProperty.RegisterAttached(
				nameof(UpdateState),
				typeof(WheelUpdateState),
				typeof(WheelUpdateBehavior),
				new PropertyMetadata(WheelUpdateState.Default));

		public static readonly DependencyProperty StatusMessageProperty =
			DependencyProperty.RegisterAttached(
				nameof(StatusMessage),
				typeof(string),
				typeof(WheelUpdateBehavior),
				new PropertyMetadata(""));

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

		public WheelUpdatePosition UpdatePosition {
			get => (WheelUpdatePosition)this.GetValue(UpdatePositionProperty);
			set => this.SetValue(UpdatePositionProperty, value);
		}

		public WheelUpdateState UpdateState {
			get => (WheelUpdateState)this.GetValue(UpdateStateProperty);
			set => this.SetValue(UpdateStateProperty, value);
		}

		public string StatusMessage {
			get => (string)this.GetValue(StatusMessageProperty);
			set => this.SetValue(StatusMessageProperty, value);
		}

		public ICommand Command {
			get => (ICommand)this.GetValue(CommandProperty);
			set => this.SetValue(CommandProperty, value);
		}

		public object CommandParameter {
			get => this.GetValue(CommandParameterProperty);
			set => this.SetValue(CommandParameterProperty, value);
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
						this.UpdateState = WheelUpdateState.Default;
						this.StatusMessage = "";
						this.deltaStep = 0;
						this.delataTime = null;
					});
			}
			void exec() {
				this.UpdateState = WheelUpdateState.Post;
				this.StatusMessage = FireUpdateMessage;
				if(this.Command?.CanExecute(this.CommandParameter) ?? false) {
					this.Command?.Execute(this.CommandParameter);
					e.Handled = true;
				}
			}

			if(this.scrollViewer != null) {
				if(0 < e.Delta) {
					// 上スクロール
					if(this.scrollViewer.VerticalOffset <= 0) {
						switch(this.deltaStep) {
						case 0:
							this.UpdatePosition = WheelUpdatePosition.Top;
							this.deltaStep--;
							if(!this.delataTime.HasValue) {
								this.delataTime = DateTime.Now.AddMilliseconds(WheelWaitMiliSec);
							}
							observe();
							break;
						case -1:
						case -2:
							this.deltaStep--;
							break;
						case -3:
							this.UpdateState = WheelUpdateState.Begin;
							this.StatusMessage = BeginUpdateMessage;
							this.deltaStep--;
							break;
						case -4:
							if(this.delataTime.HasValue && (this.delataTime < DateTime.Now)) {
								this.deltaStep = -5;
								exec();
							}
							break;
						}
					}
				} else {
					// 下スクロール
					if(this.scrollViewer.ScrollableHeight <= this.scrollViewer.VerticalOffset) {
						switch(this.deltaStep) {
						case 0:
							this.UpdatePosition = WheelUpdatePosition.Bottom;
							this.deltaStep++;
							if(!this.delataTime.HasValue) {
								this.delataTime = DateTime.Now.AddMilliseconds(WheelWaitMiliSec);
							}
							observe();
							break;
						case 1:
						case 2:
							this.deltaStep++;
							break;
						case 3:
							this.UpdateState = WheelUpdateState.Begin;
							this.StatusMessage = BeginUpdateMessage;
							this.deltaStep++;
							break;
						case 4:
							if(this.delataTime.HasValue && (this.delataTime < DateTime.Now)) {
								this.deltaStep = 5;
								exec();
							}
							break;
						}
					}
				}
			}
		}

		private void OnScrollChanged(object sender, ScrollChangedEventArgs e) {
			// スクロールするとホイールはリセットされる
			this.UpdateState = WheelUpdateState.Default;
			this.StatusMessage = "";
			this.deltaStep = 0;
			this.delataTime = null;
		}
	}
}
