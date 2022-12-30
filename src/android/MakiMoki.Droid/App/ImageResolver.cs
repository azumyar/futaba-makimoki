using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Reactive.Linq;
using Reactive.Bindings;
using Android.Graphics;

namespace Yarukizero.Net.MakiMoki.Droid.App {
	internal class ImageResolver {
		private readonly Helpers.ConnectionQueue<Bitmap> imageQueue = new Helpers.ConnectionQueue<Bitmap>(
			name: "AndroidイメージQueue",
			maxConcurrency: 12,
			forceWait: false);
		private volatile Dictionary<string, WeakReference<Bitmap>> imageDic
			= new Dictionary<string, WeakReference<Bitmap>>();

		public static ImageResolver Instance { get; } = new ImageResolver();

		private bool TryGetImage(string file, out Bitmap image) {
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

		private Bitmap SetImage(string file, Bitmap image) {
			lock(imageDic) {
				var r = new WeakReference<Bitmap>(image);
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


		public IObservable<Bitmap> Get(string url, int? droidImageSize = null) {
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

			if(TryGetImage(url, out var src)) {
				return Observable.Return(src)
					.ObserveOn(UIDispatcherScheduler.Default);
			} else {
				return this.imageQueue.Push(Helpers.ConnectionQueueItem<Bitmap>.From(
					async o => {
						try {
							// TODO: DB保存確認

							var r = await App.MakiMokiContext.HttpClient.GetByteArrayAsync(url);
							// TODO: DBに保存処理
							Observable.Return(r)
								.Select(x => {
									using var s = new MemoryStream(x);
									return (Bmp: BitmapFactory.DecodeStream(s), Resize: droidImageSize) switch {
										var y when y.Resize is not null => resize(y.Bmp, y.Resize.Value),
										var y => y.Bmp,
									};
								})
								.ObserveOn(UIDispatcherScheduler.Default)
								.Subscribe(x => {
									o.OnNext(this.SetImage(url, x));
									o.OnCompleted();
								});
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
