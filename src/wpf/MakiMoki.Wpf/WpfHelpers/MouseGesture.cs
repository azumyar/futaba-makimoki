using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Yarukizero.Net.MakiMoki.Wpf.WpfHelpers {
	internal class MouseGesture : IDisposable {
		public enum Command {
			Left,
			Up,
			Right,
			Down
		}

		private Window target;
		private IDisposable timer;
		private readonly double moveDelta = 5d;
		private readonly double commandDelta = 36d;
		private bool active;
		private (int X, int Y) startPosition;
		private (int X, int Y) prevPosition;
		private (int X, int Y) activePosition;
		private Command? currentCommand = null;
		private　readonly List<Command> commands = new List<Command>();

		public Action<IEnumerable<Command>> Update { get; set; }
		public Action<IEnumerable<Command>> Fire { get; set; }

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
				System.Diagnostics.Debug.WriteLine($"マウスジェスチャ:確定{GetCommandText()}");
				this.DoFire();
			} else if(this.active) {
				System.Diagnostics.Debug.WriteLine($"マウスジェスチャ:キャンセル");
				this.Reset();
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
					var left = this.currentCommand switch {
						Command.Up => Command.Right,
						_ => Command.Left
					};
					var right = left switch {
						Command.Left => Command.Right,
						_ => Command.Left
					};
					var degree = Math.Atan2(this.activePosition.Y - this.prevPosition.Y, this.activePosition.X - this.prevPosition.X) * 180d / Math.PI + 180d;
					//System.Diagnostics.Debug.WriteLine($"__マウスジェスチャ:{(int)degree}");
					var command = degree switch {
						double d when(0 <= d) && (d < 45) => Command.Left,
						double d when(d < 135) => Command.Up,
						double d when(d < 225) => Command.Right,
						double d when(d < 315) => Command.Down,
						_ => Command.Left
					};
					if(this.currentCommand == null) {
						this.currentCommand = command;
						this.Update?.Invoke(this.commands.Append(this.currentCommand.Value));
						System.Diagnostics.Debug.WriteLine($"マウスジェスチャ:{this.GetCommandText()}");
					} else {
						var commandLength = length(this.activePosition.X - this.startPosition.X, this.activePosition.Y - this.startPosition.Y);
						if((this.currentCommand != command) && (this.commandDelta < commandLength)) {
							this.commands.Add(this.currentCommand.Value);
							this.currentCommand = command;
							this.startPosition = this.activePosition;
							this.Update?.Invoke(this.commands.Append(this.currentCommand.Value));
							System.Diagnostics.Debug.WriteLine($"マウスジェスチャ:{this.GetCommandText()}");
						}
					}
					// 前回位置を更新
					this.prevPosition = this.activePosition;
				}
			}
		}

		private void DoFire() {
			this.Fire?.Invoke(this.commands.Append(this.currentCommand.Value));
			this.Reset();
		}

		private void Reset() {
			this.active = false;
			this.startPosition = default;
			this.prevPosition = default;
			this.activePosition = default;
			this.currentCommand = null;
			this.commands.Clear();
			this.Update?.Invoke(this.commands);
		}

		public string GetCommandText(bool isSymbol = false) {
			var left = isSymbol switch {
				_ => "←",
			};
			var up = isSymbol switch {
				_ => "↑",
			};
			var right = isSymbol switch {
				_ => "→",
			};
			var down = isSymbol switch {
				_ => "↓",
			};
			var c = this.commands.ToList();
			if(this.currentCommand != null) {
				c.Add(this.currentCommand.Value);
			}
			return string.Join(',', c.Select(x => x switch {
				Command.Left => left,
				Command.Up => up,
				Command.Right => right,
				Command.Down => down,
				_ => null
			}).Where(x => x != null).ToArray());
		}
	}
}
