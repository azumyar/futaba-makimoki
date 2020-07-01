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
		private static readonly int DefaultWheelCount = 10;
		private ScrollViewer scrollViewer;
		private int deltaCount;

		public static readonly DependencyProperty WheelCountProperty =
			DependencyProperty.Register(
				nameof(WheelCount),
				typeof(int),
				typeof(Behavior<Control>),
				new PropertyMetadata(DefaultWheelCount));
		public static readonly DependencyProperty CommandProperty =
			DependencyProperty.RegisterAttached(
				nameof(Command), 
				typeof(ICommand), 
				typeof(Behavior<Control>), 
				new PropertyMetadata(null));

		public static readonly DependencyProperty CommandParameterProperty =
			DependencyProperty.RegisterAttached(
				nameof(CommandParameter), 
				typeof(object),
				typeof(Behavior<Control>),
				new PropertyMetadata(null));

		public static ICommand GetCommand(DependencyObject obj) {
			return (ICommand)obj.GetValue(CommandProperty);
		}
		public static void SetCommand(DependencyObject obj, ICommand value) {
			obj.SetValue(CommandProperty, value);
		}


		public static object GetCommandParameter(DependencyObject obj) {
			return (object)obj.GetValue(CommandParameterProperty);
		}
		public static void SetCommandParameter(DependencyObject obj, object value) {
			obj.SetValue(CommandParameterProperty, value);
		}



		public int WheelCount {
			get => (int)this.GetValue(WheelCountProperty);
			set {
				this.SetValue(WheelCountProperty, value);
			}
		}

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
			if(this.scrollViewer != null) {
				if(0 < e.Delta) {
					// 上スクロール
					if(this.scrollViewer.VerticalOffset <= 0) {
						deltaCount--;
					}
				} else {
					// 下スクロール
					if(this.scrollViewer.ScrollableHeight <= this.scrollViewer.VerticalOffset) {
						deltaCount++;
					}
				}

				if(this.WheelCount == Math.Abs(this.deltaCount)) {
					// 画面上のインタラクションがなく連続発行されてしまうので1秒間値をリセットしない
					// スクロールでリセットさせないのはスレ更新で新レスがないと普通スクロールをしないため
					Observable.Return(0)
						.Delay(TimeSpan.FromSeconds(1))
						.Subscribe(x => this.deltaCount = 0);

					if(this.Command?.CanExecute(this.CommandParameter) ?? false) {
						this.Command?.Execute(this.CommandParameter);
						e.Handled = true;
					}
				}
			}
		}

		private void OnScrollChanged(object sender, ScrollChangedEventArgs e) {
			// スクロールするとホイールはリセットされる
			this.deltaCount = 0;
		}
	}
}
