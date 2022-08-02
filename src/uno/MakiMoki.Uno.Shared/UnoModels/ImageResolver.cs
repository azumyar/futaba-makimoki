using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Reactive.Linq;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Reactive.Bindings;
#if __ANDROID__
using Android.Graphics;
#endif
namespace Yarukizero.Net.MakiMoki.Uno.UnoModels {
	class ImageResolver {
#if __ANDROID__
		class DroidImageSource : ImageSource {
			public DroidImageSource(Bitmap bmp) : base(bmp) { }
		}
#endif
		private readonly Helpers.ConnectionQueue<ImageSource> imageQueue = new Helpers.ConnectionQueue<ImageSource>(
			name: "UnoイメージQueue",
			maxConcurrency: 12,
			forceWait: false);
		private volatile Dictionary<string, WeakReference<ImageSource>> imageDic
			= new Dictionary<string, WeakReference<ImageSource>>();

		private bool TryGetImage(string file, out ImageSource image) {
			lock(imageDic) {
				if(imageDic.TryGetValue(file, out var v)) {
					if(v.TryGetTarget(out var b)) {
						image = b;
						return true;
					}
				}
				image = null;
				return false;
			}
		}

		private ImageSource SetImage(string file, ImageSource image) {
			lock(imageDic) {
				var r = new WeakReference<ImageSource>(image);
				if(imageDic.ContainsKey(file)) {
					imageDic[file] = r;
				} else {
					imageDic.Add(file, r);
				}

				foreach(var k in imageDic
					.Select(x => (Key: x.Key, Value: x.Value.TryGetTarget(out _)))
					.Where(x => !x.Value)
					.ToArray()) {

					imageDic.Remove(k.Key);
				}
				System.Diagnostics.Debug.WriteLine($"キャッシュサイズ= {imageDic.Count}");
				return image;
			}
		}


		public IObservable<ImageSource> Get(string url
#if __ANDROID__
			, int? droidImageSize = null
#endif
			) {
#if __ANDROID__
			static Bitmap resize(Bitmap @in, int maxSize) {
				var scale = (@in.Height < @in.Width) switch {
					true => (double)maxSize / @in.Width,
					false => (double)maxSize / @in.Height,
				};
				return (scale < 1.0) switch {
					true => Bitmap.CreateScaledBitmap(@in,
						(int)(@in.Width * scale),
						(int)(@in.Height * scale),
						true),
					false => @in
				};
			}
#endif

			if(TryGetImage(url, out var src)) {
				return Observable.Return(src)
					.ObserveOn(UIDispatcherScheduler.Default);
			} else {
				return this.imageQueue.Push(Helpers.ConnectionQueueItem<ImageSource>.From(
					async o => {
						try {
							// TODO: DB保存確認

							var r = await App.HttpClient.GetByteArrayAsync(url);
							// TODO: DBに保存処理
							Observable.Return(r)
#if __ANDROID__
								.Select(x => {
									using var s = new MemoryStream(x);
									return (Bmp: BitmapFactory.DecodeStream(s), Resize: droidImageSize) switch {
										var y when y.Resize is not null => resize(y.Bmp, y.Resize.Value),
										var y => y.Bmp,
									};
								})
								.Select(x => new DroidImageSource(x))
								.ObserveOn(UIDispatcherScheduler.Default)
								.Subscribe(x => {
									o.OnNext(this.SetImage(url, x));
									o.OnCompleted();
								});
#else
								.ObserveOn(UIDispatcherScheduler.Default)
								.Subscribe(async x => {
									var bi = new BitmapImage();
									await bi.SetSourceAsync(new System.IO.MemoryStream(x));
									o.OnNext(this.SetImage(url, bi));
									o.OnCompleted();
								});
#endif
						}
						catch(Exception e) {
							System.Diagnostics.Debug.WriteLine(e);
							var @throw = e switch {
								System.Net.Http.HttpRequestException _ => false,
								System.Threading.Tasks.TaskCanceledException _ => false,
								_ => true,
							};
							if(@throw) {
								o.OnError(e);
							}
							o.OnCompleted();
						}
					}));
			}
		}
	}
}
