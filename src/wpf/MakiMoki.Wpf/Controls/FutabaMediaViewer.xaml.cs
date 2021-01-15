using Prism.Events;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
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

		public static readonly DependencyProperty ContentsProperty
			= DependencyProperty.Register(
				nameof(Contents),
				typeof(PlatformData.FutabaMedia),
				typeof(FutabaMediaViewer),
				new PropertyMetadata(OnContentsChanged));
		public static RoutedEvent ContentsChangedEvent
			= EventManager.RegisterRoutedEvent(
				nameof(ContentsChanged),
				RoutingStrategy.Tunnel,
				typeof(RoutedPropertyChangedEventHandler<PlatformData.FutabaMedia>),
				typeof(FutabaMediaViewer));
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


		public PlatformData.FutabaMedia Contents {
			get => (PlatformData.FutabaMedia)this.GetValue(ContentsProperty);
			set {
				this.SetValue(ContentsProperty, value);
			}
		}

		public event RoutedPropertyChangedEventHandler<PlatformData.FutabaMedia> ContentsChanged {
			add { AddHandler(ContentsChangedEvent, value); }
			remove { RemoveHandler(ContentsChangedEvent, value); }
		}

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

			disposable = new Helpers.AutoDisposable()
				.Add(ViewModels.FutabaMediaViewerViewModel.Messenger.Instance
					.GetEvent<PubSubEvent<ViewModels.FutabaMediaViewerViewModel.ViewerCloseMessage>>()
					.Subscribe(x => {
						this.Contents = null;
					})
				).Add(ViewModels.FutabaMediaViewerViewModel.Messenger.Instance
					.GetEvent<PubSubEvent<ViewModels.FutabaMediaViewerViewModel.VideoLoadMessage>>()
					.Subscribe(x => {
						lock(this.VideoView) {
							if(!this.isDisposed) {
								this.VideoView.MediaPlayer.Play(new LibVLCSharp.Shared.Media(
										(Application.Current as App).LibVLC,
										x.Path,
										LibVLCSharp.Shared.FromType.FromPath));
							}
						}
					})
				).Add(ViewModels.FutabaMediaViewerViewModel.Messenger.Instance
					.GetEvent<PubSubEvent<ViewModels.FutabaMediaViewerViewModel.VideoPlayMessage>>()
					.Subscribe(_ => {
						lock(this.VideoView) {
							if(!this.isDisposed) {
								this.VideoView.MediaPlayer?.Play();
							}
						}
					})
				).Add(ViewModels.FutabaMediaViewerViewModel.Messenger.Instance
					.GetEvent<PubSubEvent<ViewModels.FutabaMediaViewerViewModel.VideoPauseMessage>>()
					.Subscribe(_ => {
						lock(this.VideoView) {
							if(!this.isDisposed) {
								this.VideoView.MediaPlayer.Pause();
							}
						}
					})
				).Add(ViewModels.FutabaMediaViewerViewModel.Messenger.Instance
					.GetEvent<PubSubEvent<ViewModels.FutabaMediaViewerViewModel.VideoStopMessage>>()
					.Subscribe(_ => {
						lock(this.VideoView) {
							if(!this.isDisposed) {
								this.VideoView.MediaPlayer.Stop();
							}
						}
					})
				).Add(ViewModels.FutabaMediaViewerViewModel.Messenger.Instance
					.GetEvent<PubSubEvent<ViewModels.FutabaMediaViewerViewModel.VideoPositionMessage>>()
					.Subscribe(x => {
						lock(this.VideoView) {
							if(!this.isDisposed) {
								this.VideoView.MediaPlayer.Position = x.Position;
							}
						}
					})
				);

			this.Loaded += (sender, ev) => {
				this.VideoView.MediaPlayer = new LibVLCSharp.Shared.MediaPlayer((Application.Current as App).LibVLC);
				this.VideoView.MediaPlayer.Playing += (s, e) => this.Dispatcher.Invoke(() => this.RaiseEvent(new RoutedEventArgs(VideoViewPlayingEvent)));
				this.VideoView.MediaPlayer.Paused += (s, e) => this.Dispatcher.Invoke(() => this.RaiseEvent(new RoutedEventArgs(VideoViewPausedEvent)));
				this.VideoView.MediaPlayer.Stopped += (s, e) => this.Dispatcher.Invoke(() => {
					this.RaiseEvent(new RoutedEventArgs(VideoViewStoppedEvent));
					// イベントが飛ばない
					this.VideoView.MediaPlayer.Position = 0;
					this.RaiseEvent(new RoutedPositionEventArgs(0, VideoViewPositionChangedEvent));
				});
				this.VideoView.MediaPlayer.EndReached += (s, e) => this.Dispatcher.Invoke(() => {
					this.RaiseEvent(new RoutedEventArgs(VideoViewEndReachedEvent));
					// イベントが飛ばない
					this.VideoView.MediaPlayer.Position = 0;
					this.RaiseEvent(new RoutedPositionEventArgs(0, VideoViewPositionChangedEvent));
				});
				this.VideoView.MediaPlayer.PositionChanged += (s, e) => this.Dispatcher.Invoke(() => this.RaiseEvent(new RoutedPositionEventArgs(e.Position, VideoViewPositionChangedEvent)));
			};

			this.Unloaded += (s, e) => {
				lock(this.VideoView) {
					if(!isDisposed) {
						// this.VideoView.MediaPlayer.Dispose();
						this.VideoView.Dispose();
						disposable.Dispose();
						this.isDisposed = true;
					}
				}
			};
		}

		private static void OnContentsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
			if(obj is UIElement el) {
				el.RaiseEvent(new RoutedPropertyChangedEventArgs<PlatformData.FutabaMedia>(
					e.OldValue as PlatformData.FutabaMedia,
					e.NewValue as PlatformData.FutabaMedia,
					ContentsChangedEvent));
			}
		}
	}
}