using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Yarukizero.Net.MakiMoki.Wpf.WpfHelpers {
	internal class MouseGesture : IDisposable {
		private readonly double moveDelta = 5d;
		private readonly double commandDelta = 36d;
		private readonly int maxCommand = 10;

		private Window target;
		private IDisposable timer;
		private bool active;
		private (int X, int Y) startPosition;
		private (int X, int Y) prevPosition;
		private (int X, int Y) activePosition;
		private readonly List<PlatformData.MouseGestureCommand> commands = new List<PlatformData.MouseGestureCommand>();

		public Action<PlatformData.MouseGestureCommands> Update { get; set; }
		public Func<PlatformData.MouseGestureCommands, bool> Fire { get; set; }
		private bool prevCancelState = false;

		public MouseGesture(DependencyObject target) {
			this.target = Window.GetWindow(target);
			WpfUtil.GlobalMouseHook.MouseDown += OnMouseDown;
			WpfUtil.GlobalMouseHook.MouseUp += OnMouseUp;
			WpfUtil.GlobalMouseHook.MouseMove += OnMouseMove;
			this.timer = Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(50))
				.ObserveOn(global::Reactive.Bindings.UIDispatcherScheduler.Default)
				.Subscribe(_ => OnTimer());
		}

		public void Dispose() {
			WpfUtil.GlobalMouseHook.MouseDown -= OnMouseDown;
			WpfUtil.GlobalMouseHook.MouseUp -= OnMouseUp;
			WpfUtil.GlobalMouseHook.MouseMove -= OnMouseMove;
			this.timer.Dispose();
		}

		private void OnMouseDown(object sender, WpfUtil.GlobalMouseHook.MouseCaptureEventArgs e) {
			if((e.Button == WpfUtil.GlobalMouseHook.MouseButtons.Right) && this.target.IsActive) {
				System.Diagnostics.Debug.WriteLine($"マウスジェスチャ:開始");
				this.active = true;
				this.startPosition = (e.X, e.Y);
				this.prevPosition = (e.X, e.Y);
				this.activePosition = (e.X, e.Y);
			} else if(this.active) {
				System.Diagnostics.Debug.WriteLine($"マウスジェスチャ:キャンセル");
				this.Reset();
			}
		}

		private void OnMouseUp(object sender, WpfUtil.GlobalMouseHook.MouseCaptureEventArgs e) {
			if((e.Button == WpfUtil.GlobalMouseHook.MouseButtons.Right) && this.target.IsActive && this.active) {
#if DEBUG
				System.Diagnostics.Debug.WriteLine($"マウスジェスチャ:確定{new PlatformData.MouseGestureCommands(this.commands)}");
#endif
				this.prevCancelState = this.DoFire();
				this.target.ReleaseMouseCapture();
			} else if(this.active) {
				System.Diagnostics.Debug.WriteLine($"マウスジェスチャ:キャンセル");
				this.Reset();
				this.target.ReleaseMouseCapture();
			}
		}

		private void OnMouseMove(object sender, WpfUtil.GlobalMouseHook.MouseCaptureEventArgs e) {
			if(this.target.IsActive && this.active) {
				this.activePosition = (e.X, e.Y);
			} else if(this.active) {
				System.Diagnostics.Debug.WriteLine($"マウスジェスチャ:キャンセル");
				this.Reset();
			}
		}

		private void OnTimer() {
			static double length(double x, double y) => Math.Sqrt(x * x + y * y);
			if(this.active) {
				// 前回位置よりdelta px以上移動した場合コマンド入力とみなす
				var moveLength = length(this.activePosition.X - this.prevPosition.X, this.activePosition.Y - this.prevPosition.Y);
				if(this.moveDelta < moveLength) {
					var degree = Math.Atan2(this.activePosition.Y - this.prevPosition.Y, this.activePosition.X - this.prevPosition.X) * 180d / Math.PI + 180d;
					//System.Diagnostics.Debug.WriteLine($"__マウスジェスチャ:{(int)degree}");
					var command = degree switch {
						double d when(0 <= d) && (d < 45) => PlatformData.MouseGestureCommand.Left,
						double d when(d < 135) => PlatformData.MouseGestureCommand.Up,
						double d when(d < 225) => PlatformData.MouseGestureCommand.Right,
						double d when(d < 315) => PlatformData.MouseGestureCommand.Down,
						_ => PlatformData.MouseGestureCommand.Left
					};
					if(!this.commands.Any()) {
						this.commands.Add(command);
						this.Update?.Invoke(new PlatformData.MouseGestureCommands(this.commands));
#if DEBUG
						System.Diagnostics.Debug.WriteLine($"マウスジェスチャ:{new PlatformData.MouseGestureCommands(this.commands)}");
#endif
						// コマンド入力からキャプチャする
						this.target.CaptureMouse();
						this.target.MouseRightButtonUp += OnCaptureLbuttonUp;
					} else {
						var commandLength = length(this.activePosition.X - this.startPosition.X, this.activePosition.Y - this.startPosition.Y);
						if((this.commands.Last() != command) && (this.commandDelta < commandLength)) {
							if(this.commands.Count < maxCommand) {
								this.commands.Add(command);
							}
							this.startPosition = this.activePosition;
							this.Update?.Invoke(new PlatformData.MouseGestureCommands(this.commands));
#if DEBUG
							System.Diagnostics.Debug.WriteLine($"マウスジェスチャ:{new PlatformData.MouseGestureCommands(this.commands)}");
#endif
						}
					}
					// 前回位置を更新
					this.prevPosition = this.activePosition;
				}
			}
		}

		private void OnCaptureLbuttonUp(object _, MouseButtonEventArgs e) {
			e.Handled = this.prevCancelState;
			this.prevCancelState = false;
			this.target.MouseRightButtonUp -= OnCaptureLbuttonUp;
		}

		private bool DoFire() {
			var b = this.Fire?.Invoke(new PlatformData.MouseGestureCommands(this.commands));
			this.Reset();
			return b ?? false;
		}

		private void Reset() {
			this.active = false;
			this.startPosition = default;
			this.prevPosition = default;
			this.activePosition = default;
			this.commands.Clear();
			this.Update?.Invoke(new PlatformData.MouseGestureCommands(this.commands));
		}
	}
}
