using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Prism.Mvvm;
using Reactive.Bindings;
using Prism.Events;
using Yarukizero.Net.MakiMoki.Wpf.Controls;

namespace Yarukizero.Net.MakiMoki.Wpf.ViewModels {
	class FutabaMediaViewerViewModel : BindableBase, IDisposable {
		internal class Messenger : EventAggregator {
			public static Messenger Instance { get; } = new Messenger();
		}

		internal class VideoLoadMessage {
			public string Path { get; }

			public VideoLoadMessage(string path ) {
				this.Path = path;
			}
		}

		internal class VideoPlayMessage { }
		internal class VideoPauseMessage { }
		internal class VideoStopMessage { }

		internal class VideoPositionMessage {
			public float Position { get; }

			public VideoPositionMessage(float position) {
				this.Position = position;
			}
		}

		private CompositeDisposable Disposable { get; } = new CompositeDisposable();
		public ReactiveCommand<RoutedPropertyChangedEventArgs<PlatformData.FutabaMedia>> ContentsChangedCommand { get; } 
			= new ReactiveCommand<RoutedPropertyChangedEventArgs<PlatformData.FutabaMedia>>();

		public ReactiveCommand SaveClickCommand { get; }
			= new ReactiveCommand();

		public ReactiveCommand<MouseButtonEventArgs> MouseLeftButtonDownCommand { get; }
			= new ReactiveCommand<MouseButtonEventArgs>();
		public ReactiveCommand<MouseButtonEventArgs> MouseLeftButtonUpCommand { get; }
			= new ReactiveCommand<MouseButtonEventArgs>();
		public ReactiveCommand<MouseEventArgs> MouseMoveCommand { get; }
			= new ReactiveCommand<MouseEventArgs>();
		public ReactiveCommand<MouseEventArgs> MouseLeaveCommand { get; }
			= new ReactiveCommand<MouseEventArgs>();
		public ReactiveCommand<MouseWheelEventArgs> MouseWheelCommand { get; }
			= new ReactiveCommand<MouseWheelEventArgs>();

		public ReactiveProperty<Visibility> ViewVisibility { get; } = new ReactiveProperty<Visibility>(Visibility.Hidden);

		public ReactiveProperty<ImageSource> ImageSource { get; } = new ReactiveProperty<ImageSource>();
		public ReactiveProperty<Visibility> ImageViewVisibility { get; } = new ReactiveProperty<Visibility>(Visibility.Collapsed);
		public ReactiveProperty<MatrixTransform> ImageMatrix { get; }
			= new ReactiveProperty<MatrixTransform>(new MatrixTransform(Matrix.Identity));

		public ReactiveProperty<Visibility> VideoViewVisibility { get; } = new ReactiveProperty<Visibility>(Visibility.Hidden);
		public ReactiveProperty<Visibility> VideoPlayButtonVisibility { get; } = new ReactiveProperty<Visibility>(Visibility.Visible);
		public ReactiveProperty<Visibility> VideoPauseButtonVisibility { get; }
		public ReactiveProperty<double> VideoSliderValue { get; } = new ReactiveProperty<double>(0);

		public ReactiveCommand VideoPlayCommand { get; } = new ReactiveCommand();
		public ReactiveCommand VideoPauseCommand { get; } = new ReactiveCommand();
		public ReactiveCommand VideoStopCommand { get; } = new ReactiveCommand();
		public ReactiveCommand<RoutedPropertyChangedEventArgs<double>> VideoSliderValueChangedCommand { get; } = new ReactiveCommand<RoutedPropertyChangedEventArgs<double>>();
		public ReactiveCommand VideoViewPlayingCommand { get; } = new ReactiveCommand();
		public ReactiveCommand VideoViewPausedCommand { get; } = new ReactiveCommand();
		public ReactiveCommand VideoViewStoppedCommand { get; } = new ReactiveCommand();
		public ReactiveCommand VideoViewEndReachedCommand { get; } = new ReactiveCommand();
		public ReactiveCommand<FutabaMediaViewer.RoutedPositionEventArgs> VideoViewPositionChangedCommand { get; } = new ReactiveCommand<FutabaMediaViewer.RoutedPositionEventArgs>();


		private bool isMouseLeftButtonDown = false;
		private Point mouseDonwStartPoint = new Point(0, 0);
		private Point mouseCurrentPoint = new Point(0, 0);
		private FrameworkElement inputElement = null;
		private double prevPosition = double.NaN;
		public FutabaMediaViewerViewModel() {
			this.ContentsChangedCommand.Subscribe(x => this.OnContentsChanged(x));

			this.SaveClickCommand.Subscribe(x => this.OnSaveClick());

			this.MouseLeftButtonDownCommand.Subscribe(x => this.OnMouseLeftButtonDown(x));
			this.MouseLeftButtonUpCommand.Subscribe(x => this.OnMouseLeftButtonUp(x));
			this.MouseMoveCommand.Subscribe(x => this.OnMouseMove(x));
			this.MouseLeaveCommand.Subscribe(x => this.OnMouseLeave(x));
			this.MouseWheelCommand.Subscribe(x => this.OnMouseWheel(x));

			this.VideoPauseButtonVisibility = this.VideoPlayButtonVisibility
				.Select(x => (x == Visibility.Visible) ? Visibility.Hidden : Visibility.Visible)
				.ToReactiveProperty();
			this.VideoPlayCommand.Subscribe(_ => OnVideoPlay());
			this.VideoPauseCommand.Subscribe(_ => OnVideoPause());
			this.VideoStopCommand.Subscribe(_ => OnVideoStop());
			this.VideoSliderValueChangedCommand.Subscribe(x => OnVideoSliderValueChanged(x));
			this.VideoViewPlayingCommand.Subscribe(_ => OnVideoViewPlaying());
			this.VideoViewPausedCommand.Subscribe(_ => OnVideoViewPaused());
			this.VideoViewStoppedCommand.Subscribe(_ => OnVideoViewStopped());
			this.VideoViewEndReachedCommand.Subscribe(_ => OnVideoViewEndReached());
			this.VideoViewPositionChangedCommand.Subscribe(x => OnVideoViewPositionChanged(x));
		}

		public void Dispose() {
			Disposable.Dispose();
		}

		private void OnContentsChanged(RoutedPropertyChangedEventArgs<PlatformData.FutabaMedia> e) {
			if(e.NewValue == null) {
				this.ViewVisibility.Value = Visibility.Collapsed;
				this.ImageViewVisibility.Value = Visibility.Hidden;
				this.VideoViewVisibility.Value = Visibility.Hidden;
				this.ImageSource.Value = null;
				Messenger.Instance.GetEvent<PubSubEvent<VideoStopMessage>>()
					.Publish(new VideoStopMessage());
			} else {
				this.ImageMatrix.Value = new MatrixTransform(Matrix.Identity);
				if(e.NewValue.IsExternalUrl) {
					OpenExternalUrl(e.NewValue.ExternalUrl);
				} else {
					OpenFutabaUrl(e.NewValue.BaseUrl, e.NewValue.Res);
				}
			}
		}

		private void OpenFutabaUrl(Data.UrlContext url, Data.ResItem res) {
			this.ViewVisibility.Value = Visibility.Visible;
			Util.Futaba.GetThreadResImage(url, res)
				.ObserveOnDispatcher()
				.Subscribe(x => {
					if(x.Successed) {
						if(this.IsImageFile(res.Src)) {
							this.ImageViewVisibility.Value = Visibility.Visible;
							this.ImageSource.Value = WpfUtil.ImageUtil.LoadImage(x.LocalPath);
							this.ImageViewVisibility.Value = Visibility.Visible;
						} else if(this.IsMovieFile(res.Src)) {
							this.VideoViewVisibility.Value = Visibility.Visible;
							Messenger.Instance.GetEvent<PubSubEvent<VideoLoadMessage>>()
								.Publish(new VideoLoadMessage(x.LocalPath));
						}
						// TODO: 不明なファイル
					} else {
						// TODO: なんかエラー画像出す
					}
				});
		}

		private void OpenExternalUrl(string u) {
			this.ViewVisibility.Value = Visibility.Visible;
			Util.Futaba.GetUploaderFile(u)
				.ObserveOnDispatcher()
				.Subscribe(x => {
					if(x.Successed) {
						if(this.IsImageFile(u)) {
							this.ImageViewVisibility.Value = Visibility.Visible;
							this.ImageSource.Value = WpfUtil.ImageUtil.LoadImage(x.LocalPath);
							this.ImageViewVisibility.Value = Visibility.Visible;
						} else if(this.IsMovieFile(u)) {
							this.VideoViewVisibility.Value = Visibility.Visible;
							Messenger.Instance.GetEvent<PubSubEvent<VideoLoadMessage>>()
								.Publish(new VideoLoadMessage(x.LocalPath));
						}
						// TODO: 不明なファイル
					} else {
						// TODO: なんかエラー画像出す
					}
				});
		}

		private bool IsImageFile(string url) {
			var ext = Regex.Match(url, @"\.[a-zA-Z0-9]+$");
			if(ext.Success) {
				if(new string[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" }.Contains(ext.Value.ToLower())) {
					return true;
				}
			}
			return false;
		}

		private bool IsMovieFile(string url) {
			var ext = Regex.Match(url, @"\.[a-zA-Z0-9]+$");
			if(ext.Success) {
				if(new string[] { ".mp4", ".webm" }.Contains(ext.Value.ToLower())) {
					return true;
				}
			}
			return false;
		}

		private void OnSaveClick() {
			MessageBox.Show("未実装！画像はキャッシュフォルダにあるよ！"); //TODO: 実装する
		}


		private void OnManipulationDelta(ManipulationDeltaEventArgs e) {
			var delta = e.DeltaManipulation;
			Matrix matrix = this.ImageMatrix.Value.Matrix;
			matrix.Translate(delta.Translation.X, delta.Translation.Y);

			var scaleDelta = delta.Scale.X;
			var orgX = e.ManipulationOrigin.X;
			var orgY = e.ManipulationOrigin.Y;
			matrix.ScaleAt(scaleDelta, scaleDelta, orgX, orgY);
			this.ImageMatrix.Value = new MatrixTransform(matrix);
		}

		private void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
			if(e.Source is FrameworkElement el) {
				this.inputElement = el;
				this.mouseDonwStartPoint = e.GetPosition(this.inputElement);
				this.isMouseLeftButtonDown = true;
			}
		}

		private void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
			this.inputElement = null;
			this.isMouseLeftButtonDown = false;
		}

		private void OnMouseLeave(MouseEventArgs e) {
			this.isMouseLeftButtonDown = false;
		}

		private void OnMouseMove(MouseEventArgs e) {
			if(isMouseLeftButtonDown == false) return;

			if(this.inputElement != null) {
				this.mouseCurrentPoint = e.GetPosition(this.inputElement);
				var offsetX = this.mouseCurrentPoint.X - this.mouseDonwStartPoint.X;
				var offsetY = this.mouseCurrentPoint.Y - this.mouseDonwStartPoint.Y;
				var matrix = this.ImageMatrix.Value.Matrix;
				matrix.Translate(offsetX, offsetY);
				this.ImageMatrix.Value = new MatrixTransform(matrix);
				this.mouseDonwStartPoint = this.mouseCurrentPoint;
			}
		}

		private void OnMouseWheel(MouseWheelEventArgs e) {
			if(e.Source is FrameworkElement el) {
				var el2 = WpfUtil.WpfHelper.FindFirstChild<Viewbox>(el);
				var matrix = this.ImageMatrix.Value.Matrix;
				if(matrix.M11 <= 1.0 && e.Delta < 0) {
					// 100%より小さくしない
					return;
				}
				var w = el2.ActualWidth / 2;
				var h = el2.ActualHeight / 2;
				var scale = (0 < e.Delta) ? 1.25 : (1.0 / 1.25);
				matrix.ScaleAt(scale, scale,
					w-matrix.OffsetX / matrix.M11,
					h-matrix.OffsetY / matrix.M22);

				this.ImageMatrix.Value = new MatrixTransform(matrix);
			}
		}

		private void OnVideoPlay() {
			Messenger.Instance.GetEvent<PubSubEvent<VideoPlayMessage>>()
				.Publish(new VideoPlayMessage());
		}
		private void OnVideoPause() {
			Messenger.Instance.GetEvent<PubSubEvent<VideoPauseMessage>>()
				.Publish(new VideoPauseMessage());
		}
		private void OnVideoStop() {
			Messenger.Instance.GetEvent<PubSubEvent<VideoStopMessage>>()
				.Publish(new VideoStopMessage());
		}

		private void OnVideoSliderValueChanged(RoutedPropertyChangedEventArgs<double> e) {
			if(e.NewValue != this.prevPosition) {
				Messenger.Instance.GetEvent<PubSubEvent<VideoPositionMessage>>()
					.Publish(new VideoPositionMessage((float)e.NewValue));
			}
		}


		private void OnVideoViewPlaying() => this.VideoPlayButtonVisibility.Value = Visibility.Hidden;
		private void OnVideoViewPaused() => this.VideoPlayButtonVisibility.Value = Visibility.Visible;
		private void OnVideoViewStopped() => this.VideoPlayButtonVisibility.Value = Visibility.Visible;
		private void OnVideoViewEndReached() {
			this.VideoPlayButtonVisibility.Value = Visibility.Visible;
			Messenger.Instance.GetEvent<PubSubEvent<VideoStopMessage>>()
				.Publish(new VideoStopMessage());
		}

		private void OnVideoViewPositionChanged(FutabaMediaViewer.RoutedPositionEventArgs e) {
			this.VideoSliderValue.Value
				= this.prevPosition
				= e.Position;
		}
	}
}
