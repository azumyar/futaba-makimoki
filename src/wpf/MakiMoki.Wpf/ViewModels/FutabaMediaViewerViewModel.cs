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
using System.IO;
using Yarukizero.Net.MakiMoki.Util;
using Yarukizero.Net.MakiMoki.Reactive;
using Yarukizero.Net.MakiMoki.Wpf.WpfUtil;
using Prism.Regions;
using Prism.Navigation;
using System.Windows.Media.Animation;
using LibAPNG;
using Yarukizero.Net.MakiMoki.Wpf.Model;

namespace Yarukizero.Net.MakiMoki.Wpf.ViewModels {
	class FutabaMediaViewerViewModel : BindableBase, IDisposable, INavigationAware, IJournalAware, IDestructible {
		public class NavigationParameters {
			public string RegionName {get;}
			public PlatformData.FutabaMedia Media { get;}

			public NavigationParameters(string regionName, PlatformData.FutabaMedia media) {
				this.RegionName = regionName;
				this.Media = media;
			}
		}

		internal class Messenger : EventAggregator {
			public static Messenger Instance { get; } = new Messenger();
		}

		internal class BaseMessage {
			public PlatformData.FutabaMedia Media { get; }

			public BaseMessage(PlatformData.FutabaMedia media) {
				this.Media = media;
			}
		}

		internal class ViewerCloseMessage : BaseMessage {
			public string RegionName { get; }

			public ViewerCloseMessage(string regionName, PlatformData.FutabaMedia media) : base(media) {
				this.RegionName = regionName;
			}
		}

		internal class VideoLoadMessage : BaseMessage {
			public string Path { get; }

			public VideoLoadMessage(PlatformData.FutabaMedia media, string path ) : base(media) {
				this.Path = path;
			}
		}

		internal class VideoPlayMessage : BaseMessage {
			public VideoPlayMessage(PlatformData.FutabaMedia media) : base(media) {}
		}
		internal class VideoPauseMessage : BaseMessage {
			public VideoPauseMessage(PlatformData.FutabaMedia media) : base(media) { }
		}
		internal class VideoStopMessage : BaseMessage {
			public VideoStopMessage(PlatformData.FutabaMedia media) : base(media) { }
		}

		internal class VideoPositionMessage : BaseMessage {
			public float Position { get; }

			public VideoPositionMessage(PlatformData.FutabaMedia media, float position) : base(media) {
				this.Position = position;
			}
		}

		internal class VideoVolumeMessage : BaseMessage {
			public double Volume { get; }

			public VideoVolumeMessage(PlatformData.FutabaMedia media, double volume) : base(media) {
				this.Volume = volume;
			}
		}

		public MakiMokiCommand<MouseButtonEventArgs> MouseLeftButtonDownCommand { get; }
			= new MakiMokiCommand<MouseButtonEventArgs>();
		public MakiMokiCommand<MouseButtonEventArgs> MouseLeftButtonUpCommand { get; }
			= new MakiMokiCommand<MouseButtonEventArgs>();
		public MakiMokiCommand<MouseEventArgs> MouseMoveCommand { get; }
			= new MakiMokiCommand<MouseEventArgs>();
		public MakiMokiCommand<MouseEventArgs> MouseLeaveCommand { get; }
			= new MakiMokiCommand<MouseEventArgs>();
		public MakiMokiCommand<MouseWheelEventArgs> MouseWheelCommand { get; }
			= new MakiMokiCommand<MouseWheelEventArgs>();

		public ReactiveProperty<Visibility> ErrorViewVisibility { get; } = new ReactiveProperty<Visibility>(Visibility.Collapsed);

		public ReactiveProperty<Model.ImageObject> ImageSourceObject { get; } = new ReactiveProperty<Model.ImageObject>();
		public ReactiveProperty<ImageSource> ImageSource { get; }
		public ReactiveProperty<Visibility> ImageViewVisibility { get; } = new ReactiveProperty<Visibility>(Visibility.Collapsed);
		public ReactiveProperty<MatrixTransform> ImageMatrix { get; }
			= new ReactiveProperty<MatrixTransform>(new MatrixTransform(Matrix.Identity));

		public ReactiveProperty<Visibility> VideoViewVisibility { get; } = new ReactiveProperty<Visibility>(Visibility.Hidden);
		public ReactiveProperty<Visibility> VideoPlayButtonVisibility { get; } = new ReactiveProperty<Visibility>(Visibility.Visible);
		public ReactiveProperty<Visibility> VideoPauseButtonVisibility { get; }
		public ReactiveProperty<double> VideoSliderValue { get; } = new ReactiveProperty<double>(0);
		public ReactiveProperty<bool> VideoRepeatValue { get; }
		public ReactiveProperty<double> VideoVolumeValue { get; }

		public MakiMokiCommand CloseCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand VideoPlayCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand VideoPauseCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand VideoStopCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand<RoutedPropertyChangedEventArgs<double>> VideoSliderValueChangedCommand { get; } = new MakiMokiCommand<RoutedPropertyChangedEventArgs<double>>();
		public MakiMokiCommand VideoViewPlayingCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand VideoViewPausedCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand VideoViewStoppedCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand VideoViewEndReachedCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand<FutabaMediaViewer.RoutedPositionEventArgs> VideoViewPositionChangedCommand { get; } = new MakiMokiCommand<FutabaMediaViewer.RoutedPositionEventArgs>();

		public MakiMokiCommand<PlatformData.FutabaMedia> MenuItemClickSaveCommand { get; } = new MakiMokiCommand<PlatformData.FutabaMedia>();
		public MakiMokiCommand<PlatformData.MediaQuickSaveItem> MenuItemClickQuickSaveCommand { get; } = new MakiMokiCommand<PlatformData.MediaQuickSaveItem>();
		public MakiMokiCommand<PlatformData.FutabaMedia> MenuItemClickImageSearchGoogleCommand { get; } = new MakiMokiCommand<PlatformData.FutabaMedia>();
		public MakiMokiCommand<PlatformData.FutabaMedia> MenuItemClickGoogleLensCommand { get; } = new MakiMokiCommand<PlatformData.FutabaMedia>();
		public MakiMokiCommand<PlatformData.FutabaMedia> MenuItemClickImageSearchAscii2dCommand { get; } = new MakiMokiCommand<PlatformData.FutabaMedia>();
		public MakiMokiCommand<PlatformData.FutabaMedia> MenuItemClickQuickOpenBrowserCommand { get; } = new MakiMokiCommand<PlatformData.FutabaMedia>();

		public ReactiveProperty<bool> ImageContextMenuOpened { get; } = new ReactiveProperty<bool>(false);

		public ReactiveProperty<object> UpdateToken { get; } = new ReactiveProperty<object>(DateTime.Now);

		private bool isMenuOpend = false;
		private bool isMouseLeftButtonDown = false;
		private bool isMouseLeftClick = false;
		private Point clickDonwStartPoint = new Point(0, 0);
		private Point mouseDonwStartPoint = new Point(0, 0);
		private Point mouseCurrentPoint = new Point(0, 0);
		private FrameworkElement inputElement = null;
		private double prevPosition = double.NaN;

		private Action<PlatformData.WpfConfig> onSystemConfigUpdateNotifyer;
		public IRegionNavigationService RegionNavigationService { get; private set; }
		public ReactiveProperty<PlatformData.FutabaMedia> Media { get; }
		private NavigationParameters navigationParameters;

#pragma warning disable CS0067
		private IDisposable VideoRepeatSubscriber { get; }
		private IDisposable VideoVolumeSubscriber { get; }
#pragma warning restore CS0067

		public FutabaMediaViewerViewModel() {
			this.Media = new ReactiveProperty<PlatformData.FutabaMedia>();

			this.CloseCommand.Subscribe(_ => this.OnClose());

			this.MouseLeftButtonDownCommand.Subscribe(x => this.OnMouseLeftButtonDown(x));
			this.MouseLeftButtonUpCommand.Subscribe(x => this.OnMouseLeftButtonUp(x));
			this.MouseMoveCommand.Subscribe(x => this.OnMouseMove(x));
			this.MouseLeaveCommand.Subscribe(x => this.OnMouseLeave(x));
			this.MouseWheelCommand.Subscribe(x => this.OnMouseWheel(x));

			this.VideoPauseButtonVisibility = this.VideoPlayButtonVisibility
				.Select(x => (x == Visibility.Visible) ? Visibility.Hidden : Visibility.Visible)
				.ToReactiveProperty();
			this.VideoRepeatValue = new ReactiveProperty<bool>(false);
			this.VideoVolumeValue = new ReactiveProperty<double>(1d);
			this.VideoRepeatSubscriber = this.VideoRepeatValue.Subscribe(x => this.SubscribeVideoRepeat(x));
			this.VideoVolumeSubscriber = this.VideoVolumeValue.Subscribe(x => this.SubscribeVideoVolume(x));
			this.VideoPlayCommand.Subscribe(_ => this.OnVideoPlay());
			this.VideoPauseCommand.Subscribe(_ => this.OnVideoPause());
			this.VideoStopCommand.Subscribe(_ => this.OnVideoStop());
			this.VideoSliderValueChangedCommand.Subscribe(x => this.OnVideoSliderValueChanged(x));
			this.VideoViewPlayingCommand.Subscribe(_ => this.OnVideoViewPlaying());
			this.VideoViewPausedCommand.Subscribe(_ => this.OnVideoViewPaused());
			this.VideoViewStoppedCommand.Subscribe(_ => this.OnVideoViewStopped());
			this.VideoViewEndReachedCommand.Subscribe(_ => this.OnVideoViewEndReached());
			this.VideoViewPositionChangedCommand.Subscribe(x => this.OnVideoViewPositionChanged(x));

			this.MenuItemClickSaveCommand.Subscribe(x => this.OnMenuItemClickSave(x));
			this.MenuItemClickQuickSaveCommand.Subscribe(x => this.OnMenuItemClickQuickSave(x));
			this.MenuItemClickImageSearchGoogleCommand.Subscribe(x => this.OnMenuItemClickImageSearchGoogle(x));
			this.MenuItemClickGoogleLensCommand.Subscribe(x => this.OnMenuItemClickGoogleLens(x));
			this.MenuItemClickImageSearchAscii2dCommand.Subscribe(x => this.OnMenuItemClickImageSearchAscii2d(x));
			this.MenuItemClickQuickOpenBrowserCommand.Subscribe(x => this.OnMenuItemClickQuickOpenBrowser(x));

			 this.ImageSource = this.ImageSourceObject.Select(x => x switch {
				ImageObject v => v.Image as ImageSource,
				_ => null,
			}).ToReactiveProperty();

			this.ImageContextMenuOpened.Subscribe(x => {
				if(x) {
					this.isMenuOpend = true;
				} else {
					Observable.Return(false)
						.Delay(TimeSpan.FromMilliseconds(250))
						.ObserveOn(UIDispatcherScheduler.Default)
						.Select(y => y || this.ImageContextMenuOpened.Value)
						.Subscribe(y => this.isMenuOpend = y);
				}
			});

			onSystemConfigUpdateNotifyer = (_) => UpdateToken.Value = DateTime.Now;
			WpfConfig.WpfConfigLoader.SystemConfigUpdateNotifyer.AddHandler(onSystemConfigUpdateNotifyer);
		}

		public void Dispose() {
			// VLCが非同期で動いているのでここで削除してはいけない
			//Helpers.AutoDisposable.GetCompositeDisposable(this).Dispose();
			Messenger.Instance.GetEvent<PubSubEvent<ViewerCloseMessage>>()
				.Publish(new ViewerCloseMessage(this.navigationParameters.RegionName, this.Media.Value));
		}

		public void OnNavigatedTo(NavigationContext navigationContext) {
			RegionNavigationService = navigationContext.NavigationService;
			if(navigationContext.Parameters.TryGetValue<NavigationParameters>(
				typeof(NavigationParameters).FullName,
				out var np)) {

				this.navigationParameters = np;
				this.Media.Value = np.Media;
				this.ImageMatrix.Value = new MatrixTransform(Matrix.Identity);
				if(np.Media.IsExternalUrl) {
					this.OpenExternalUrl(np.Media.ExternalUrl);
				} else {
					this.OpenFutabaUrl(np.Media.BaseUrl, np.Media.Res);
				}
			}
		}

		public bool IsNavigationTarget(NavigationContext navigationContext) { return true; }

		public void OnNavigatedFrom(NavigationContext navigationContext) { }

		public bool PersistInHistory() { return false; }

		public void Destroy() {
			this.Dispose();
		}

		private void OnClose() {
			this.ImageViewVisibility.Value = Visibility.Hidden;
			this.VideoViewVisibility.Value = Visibility.Hidden;
			this.ImageSourceObject.Value = null;

			this.RegionNavigationService.Region.RemoveAll();
		}

		private void OpenFutabaUrl(Data.UrlContext url, Data.ResItem res) {
			Util.Futaba.GetThreadResImage(url, res)
				.ObserveOn(UIDispatcherScheduler.Default)
				.Subscribe(x => {
					if(x.Successed) {
						if(this.IsImageFile(res.Src)) {
							this.ImageViewVisibility.Value = Visibility.Visible;
							this.ImageSourceObject.Value = WpfUtil.ImageUtil.CreateImage(x.LocalPath, x.FileBytes);
						} else if(this.IsMovieFile(res.Src)) {
							this.VideoViewVisibility.Value = Visibility.Visible;
							Messenger.Instance.GetEvent<PubSubEvent<VideoLoadMessage>>()
								.Publish(new VideoLoadMessage(this.Media.Value, x.LocalPath));
						} else {
							// 不明なファイル
							this.ErrorViewVisibility.Value = Visibility.Visible;
						}
					} else {
						this.ErrorViewVisibility.Value = Visibility.Visible;
					}
				});
		}

		private void OpenExternalUrl(string u) {
			Util.Futaba.GetUploaderFile(u)
				.ObserveOn(UIDispatcherScheduler.Default)
				.Subscribe(x => {
					if(x.Successed) {
						if(this.IsImageFile(u)) {
							this.ImageViewVisibility.Value = Visibility.Visible;
							this.ImageSourceObject.Value = WpfUtil.ImageUtil.CreateImage(x.LocalPath, x.FileBytes);
						} else if(this.IsMovieFile(u)) {
							this.VideoViewVisibility.Value = Visibility.Visible;
							Messenger.Instance.GetEvent<PubSubEvent<VideoLoadMessage>>()
								.Publish(new VideoLoadMessage(this.Media.Value, x.LocalPath));
						} else {
							// TODO: 不明なファイル
							this.ErrorViewVisibility.Value = Visibility.Visible;
						}
					} else {
						// TODO: なんかエラー画像出す
						this.ErrorViewVisibility.Value = Visibility.Visible;
					}
				});
		}

		private bool IsImageFile(string url) {
			var ext = Regex.Match(url, @"\.[a-zA-Z0-9]+$");
			if(ext.Success) {
				if(Config.ConfigLoader.MimeFutaba.Types
					.Where(x => x.MimeContents == Data.MimeContents.Image)
					.Select(x => x.Ext).Contains(ext.Value.ToLower())) {
					
					return true;
				}
			}
			return false;
		}

		private bool IsMovieFile(string url) {
			var ext = Regex.Match(url, @"\.[a-zA-Z0-9]+$");
			if(ext.Success) {
				if(Config.ConfigLoader.MimeFutaba.Types
					.Where(x => x.MimeContents == Data.MimeContents.Video)
					.Select(x => x.Ext).Contains(ext.Value.ToLower())) {

					return true;
				}
			}
			return false;
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
				this.clickDonwStartPoint
					= this.mouseDonwStartPoint
					= e.GetPosition(this.inputElement);
				this.isMouseLeftButtonDown = true;
				if(!this.isMenuOpend) {
					this.isMouseLeftClick = true;
				}
			}
		}

		private void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
			this.inputElement = null;
			this.isMouseLeftButtonDown = false;
			if(this.isMouseLeftClick) {
				this.isMouseLeftClick = false;
				this.CloseCommand.Execute(null);
			}
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

				// ドラッグ距離判定
				// SystemParametersInfo()をSPI_SETDRAGWIDTHとSPI_SETDRAGHEIGHT付きで呼び出すとシステム設定が取れるけど必要…？
				if(this.isMouseLeftClick){
					var dragLength = 4;
					var x = this.clickDonwStartPoint.X - this.mouseCurrentPoint.X;
					var y = this.clickDonwStartPoint.X - this.mouseCurrentPoint.X;
					this.isMouseLeftClick = (x * x + y * y) < (dragLength * dragLength);
				}
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

		private void SubscribeVideoRepeat(bool b) {
		}

		private void SubscribeVideoVolume(double v) {
			Messenger.Instance.GetEvent<PubSubEvent<VideoVolumeMessage>>()
				.Publish(new VideoVolumeMessage(this.Media.Value, v));
		}

		private void OnVideoPlay() {
			Messenger.Instance.GetEvent<PubSubEvent<VideoPlayMessage>>()
				.Publish(new VideoPlayMessage(this.Media.Value));
		}
		private void OnVideoPause() {
			Messenger.Instance.GetEvent<PubSubEvent<VideoPauseMessage>>()
				.Publish(new VideoPauseMessage(this.Media.Value));
		}
		private void OnVideoStop() {
			Messenger.Instance.GetEvent<PubSubEvent<VideoStopMessage>>()
				.Publish(new VideoStopMessage(this.Media.Value));
		}

		private void OnVideoSliderValueChanged(RoutedPropertyChangedEventArgs<double> e) {
			if(e.NewValue != this.prevPosition) {
				Messenger.Instance.GetEvent<PubSubEvent<VideoPositionMessage>>()
					.Publish(new VideoPositionMessage(this.Media.Value, (float)e.NewValue));
			}
		}


		private void OnVideoViewPlaying() => this.VideoPlayButtonVisibility.Value = Visibility.Hidden;
		private void OnVideoViewPaused() => this.VideoPlayButtonVisibility.Value = Visibility.Visible;
		private void OnVideoViewStopped() => this.VideoPlayButtonVisibility.Value = Visibility.Visible;
		private void OnVideoViewEndReached() {
			if(this.VideoRepeatValue.Value) {
				Messenger.Instance.GetEvent<PubSubEvent<VideoStopMessage>>()
					.Publish(new VideoStopMessage(this.Media.Value));
				Messenger.Instance.GetEvent<PubSubEvent<VideoPlayMessage>>()
					.Publish(new VideoPlayMessage(this.Media.Value));
			}
			//this.VideoPlayButtonVisibility.Value = Visibility.Visible;
			//Messenger.Instance.GetEvent<PubSubEvent<VideoStopMessage>>()
			//	.Publish(new VideoStopMessage(this.Media.Value));
		}

		private void OnVideoViewPositionChanged(FutabaMediaViewer.RoutedPositionEventArgs e) {
			this.VideoSliderValue.Value
				= this.prevPosition
				= e.Position;
		}

		private string GetUrl(PlatformData.FutabaMedia m) => m.IsExternalUrl ? m.ExternalUrl : Util.Futaba.GetFutabaThreadImageUrl(m.BaseUrl, m.Res);
		private string GetPath(PlatformData.FutabaMedia m) => m.IsExternalUrl ? Util.Futaba.GetUploderLocalFilePath(m.ExternalUrl) : Util.Futaba.GetThreadResImageLocalFilePath(m.BaseUrl, m.Res);

		private string GetSavePath(string path) {
			var p = path;
			if(File.Exists(p)) {
				var dir = Path.GetDirectoryName(path);
				var fn = Path.GetFileNameWithoutExtension(path);
				var ext = Path.GetExtension(path);
				var c = 0;
				do {
					c++;
					p = Path.Combine(dir, $"{ fn }-{ c }{ ext }");
				} while(File.Exists(p));
			}
			return p;
		}

		private void OnMenuItemClickSave(PlatformData.FutabaMedia media) {
			var u = GetUrl(media);
			var m = Regex.Match(u, "/([^/]+)$");
			var ext = m.Success ? Path.GetExtension(m.Groups[1].Value) : null;
			var fileName = m.Success ? m.Groups[1].Value : "";
			var filter = m.Success ? $"ふたば画像ファイル|*{ ext }" : "すべてのファイル|*.*";

			var sfd = new Microsoft.Win32.SaveFileDialog() {
				FileName = fileName,
				Filter = filter,
			};
			if(sfd.ShowDialog() ?? false) {
				File.Copy(GetPath(media), sfd.FileName);
				Util.Futaba.PutInformation(new Data.Information("保存しました", this.ImageSourceObject.Value.Image));
			}
		}
		private void OnMenuItemClickQuickSave(PlatformData.MediaQuickSaveItem media) {
			var u = GetUrl(media.Media.Value);
			var m = Regex.Match(u, "/([^/]+)$");
			if(m.Success) {
				File.Copy(GetPath(media.Media.Value), GetSavePath(Path.Combine(media.Path.Value, m.Groups[1].Value)));

				Util.Futaba.PutInformation(new Data.Information("保存しました", this.ImageSourceObject.Value?.Image));
			}
		}
		private void OnMenuItemClickImageSearchGoogle(PlatformData.FutabaMedia media) {
			PlatformUtil.StartBrowser(new Uri(Util.Futaba.GetGoogleImageSearchdUrl(GetUrl(media))));
		}
		private void OnMenuItemClickGoogleLens(PlatformData.FutabaMedia media) {
			PlatformUtil.StartBrowser(new Uri(Util.Futaba.GetGoogleLensUrl(GetUrl(media))));
		}
		private void OnMenuItemClickImageSearchAscii2d(PlatformData.FutabaMedia media) { 
			PlatformUtil.StartBrowser(new Uri(Util.Futaba.GetAscii2dImageSearchUrl(GetUrl(media))));
		}
		private void OnMenuItemClickQuickOpenBrowser(PlatformData.FutabaMedia media) {
			PlatformUtil.StartBrowser(new Uri(GetUrl(media)));
		}
	}
}
