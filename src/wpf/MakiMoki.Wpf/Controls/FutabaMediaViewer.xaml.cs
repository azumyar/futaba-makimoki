using Prism.Events;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Yarukizero.Net.MakiMoki.Wpf.Controls {
	/// <summary>
	/// FutabaMediaViewer.xaml の相互作用ロジック
	/// </summary>
	public partial class FutabaMediaViewer : UserControl {
		public class RoutedPositionEventArgs : RoutedEventArgs {
			public float Position { get; }

			public RoutedPositionEventArgs(float position) : base() {
				this.Position = position;
			}

			public RoutedPositionEventArgs(float position, RoutedEvent routedEvent) : base(routedEvent) {
				this.Position = position;
			}

			public RoutedPositionEventArgs(float position, RoutedEvent routedEvent, object source) : base(routedEvent, source) {
				this.Position = position;
			}
		}
		public delegate void RoutedPositionEventHandler(object sender, RoutedPositionEventArgs e);

		public static RoutedEvent VideoViewPlayingEvent
			= EventManager.RegisterRoutedEvent(
				nameof(VideoViewPlaying),
				RoutingStrategy.Tunnel,
				typeof(RoutedEventHandler),
				typeof(FutabaMediaViewer));
		public static RoutedEvent VideoViewPausedEvent
			= EventManager.RegisterRoutedEvent(
				nameof(VideoViewPaused),
				RoutingStrategy.Tunnel,
				typeof(RoutedEventHandler),
				typeof(FutabaMediaViewer));
		public static RoutedEvent VideoViewStoppedEvent
			= EventManager.RegisterRoutedEvent(
				nameof(VideoViewStopped),
				RoutingStrategy.Tunnel,
				typeof(RoutedEventHandler),
				typeof(FutabaMediaViewer));
		public static RoutedEvent VideoViewEndReachedEvent
			= EventManager.RegisterRoutedEvent(
				nameof(VideoViewEndReached),
				RoutingStrategy.Tunnel,
				typeof(RoutedEventHandler),
				typeof(FutabaMediaViewer));
		public static RoutedEvent VideoViewPositionChangedEvent
			= EventManager.RegisterRoutedEvent(
				nameof(VideoViewPositionChanged),
				RoutingStrategy.Tunnel,
				typeof(RoutedPositionEventHandler),
				typeof(FutabaMediaViewer));

		public event RoutedEventHandler VideoViewPlaying {
			add { AddHandler(VideoViewPlayingEvent, value); }
			remove { RemoveHandler(VideoViewPlayingEvent, value); }
		}

		public event RoutedEventHandler VideoViewPaused {
			add { AddHandler(VideoViewPausedEvent, value); }
			remove { RemoveHandler(VideoViewPausedEvent, value); }
		}

		public event RoutedEventHandler VideoViewStopped {
			add { AddHandler(VideoViewStoppedEvent, value); }
			remove { RemoveHandler(VideoViewStoppedEvent, value); }
		}

		public event RoutedEventHandler VideoViewEndReached {
			add { AddHandler(VideoViewEndReachedEvent, value); }
			remove { RemoveHandler(VideoViewEndReachedEvent, value); }
		}

		public event RoutedPositionEventHandler VideoViewPositionChanged {
			add { AddHandler(VideoViewPositionChangedEvent, value); }
			remove { RemoveHandler(VideoViewPositionChangedEvent, value); }
		}

		private Helpers.AutoDisposable disposable;
		private volatile bool isDisposed = false;

		public FutabaMediaViewer() {
			InitializeComponent();
			IObservable<LibVLCSharp.Shared.MediaPlayer> GetMediaPlayer() {
				return Observable.Create<LibVLCSharp.Shared.MediaPlayer>(async o => {
					if(this.VideoView.MediaPlayer == null) {
						// Loadedより先行する場合があるので待つ
						await Task.Delay(1);
						o.OnError(new Exception());
					} else {
						o.OnNext(this.VideoView.MediaPlayer);
					}
					o.OnCompleted();
					return System.Reactive.Disposables.Disposable.Empty;
				}).Retry(5);
			}

			disposable = new Helpers.AutoDisposable()
				.Add(ViewModels.FutabaMediaViewerViewModel.Messenger.Instance
					.GetEvent<PubSubEvent<ViewModels.FutabaMediaViewerViewModel.ViewerCloseMessage>>()
					.Subscribe(x => {
						lock(this.VideoView) {
							if(!this.isDisposed && this.IsThisMedia(x.Media)) {
								this.isDisposed = true;
								GetMediaPlayer()
									.Finally(() => {
										//this.VideoView.MediaPlayer.Dispose();
										this.VideoView.Dispose();
										disposable.Dispose();
										Helpers.AutoDisposable.GetCompositeDisposable(this.DataContext).Dispose();
									}).Subscribe(y => {
										if(y.IsPlaying) {
											y.Stop();
										}
									});
							}
						}
					})
				).Add(ViewModels.FutabaMediaViewerViewModel.Messenger.Instance
					.GetEvent<PubSubEvent<ViewModels.FutabaMediaViewerViewModel.VideoLoadMessage>>()
					.Subscribe(x => {
						lock(this.VideoView) {
							if(!this.isDisposed && this.IsThisMedia(x.Media)) {
								GetMediaPlayer().Subscribe(y => {
									y.Play(new LibVLCSharp.Shared.Media(
										(Application.Current as App).LibVLC,
										x.Path,
										LibVLCSharp.Shared.FromType.FromPath));
								});
							}
						}
					})
				).Add(ViewModels.FutabaMediaViewerViewModel.Messenger.Instance
					.GetEvent<PubSubEvent<ViewModels.FutabaMediaViewerViewModel.VideoPlayMessage>>()
					.Subscribe(x => {
						lock(this.VideoView) {
							if(!this.isDisposed && this.IsThisMedia(x.Media)) {
								GetMediaPlayer().Subscribe(y => {
									y.Play();
								});
							}
						}
					})
				).Add(ViewModels.FutabaMediaViewerViewModel.Messenger.Instance
					.GetEvent<PubSubEvent<ViewModels.FutabaMediaViewerViewModel.VideoPauseMessage>>()
					.Subscribe(x => {
						lock(this.VideoView) {
							if(!this.isDisposed && this.IsThisMedia(x.Media)) {
								GetMediaPlayer().Subscribe(y => {
									y.Pause();
								});
							}
						}
					})
				).Add(ViewModels.FutabaMediaViewerViewModel.Messenger.Instance
					.GetEvent<PubSubEvent<ViewModels.FutabaMediaViewerViewModel.VideoStopMessage>>()
					.Subscribe(x => {
						lock(this.VideoView) {
							if(!this.isDisposed && this.IsThisMedia(x.Media)) {
								GetMediaPlayer().Subscribe(y => {
									y.Stop();
								});
							}
						}
					})
				).Add(ViewModels.FutabaMediaViewerViewModel.Messenger.Instance
					.GetEvent<PubSubEvent<ViewModels.FutabaMediaViewerViewModel.VideoPositionMessage>>()
					.Subscribe(x => {
						lock(this.VideoView) {
							if(!this.isDisposed && this.IsThisMedia(x.Media)) {
								GetMediaPlayer().Subscribe(y => {
									y.Position = x.Position;
								});
							}
						}
					})
				);

			this.Loaded += (sender, ev) => {
				if(this.VideoView.MediaPlayer != null) {
					return;
				}
				this.VideoView.MediaPlayer = new LibVLCSharp.Shared.MediaPlayer((Application.Current as App).LibVLC);
				this.VideoView.MediaPlayer.Playing += (s, e) => this.Dispatcher.Invoke(() => this.SafeRaiseEvent(new RoutedEventArgs(VideoViewPlayingEvent)));
				this.VideoView.MediaPlayer.Paused += (s, e) => this.Dispatcher.Invoke(() => this.SafeRaiseEvent(new RoutedEventArgs(VideoViewPausedEvent)));
				this.VideoView.MediaPlayer.Stopped += (s, e) => this.Dispatcher.Invoke(() => {
					this.SafeRaiseEvent(new RoutedEventArgs(VideoViewStoppedEvent));
					// イベントが飛ばない
					this.SafeRaiseEvent(new RoutedPositionEventArgs(0, VideoViewPositionChangedEvent));
				});
				this.VideoView.MediaPlayer.EndReached += (s, e) => this.Dispatcher.Invoke(async () => {
					await Task.Delay(1); // これをいれると安定する気がする
					this.SafeRaiseEvent(new RoutedEventArgs(VideoViewEndReachedEvent));
					// イベントが飛ばない
					this.SafeRaiseEvent(new RoutedPositionEventArgs(0, VideoViewPositionChangedEvent));
				});
				this.VideoView.MediaPlayer.PositionChanged += (s, e) => this.Dispatcher.Invoke(() => this.SafeRaiseEvent(new RoutedPositionEventArgs(e.Position, VideoViewPositionChangedEvent)));
			};
		}

		private bool IsThisMedia(PlatformData.FutabaMedia media) {
			if((media != null) && (this.DataContext is ViewModels.FutabaMediaViewerViewModel vm)) {
				if(vm.Media.Value?.GetToken() == media.GetToken()) {
					return true;
				}
			}
			return false;
		}

		private void SafeRaiseEvent(RoutedEventArgs e) {
			lock(this.VideoView) {
				if(!this.isDisposed) {
					this.RaiseEvent(e);
				}
			}
		}
	}
}