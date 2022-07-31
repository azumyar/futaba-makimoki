using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reactive.Linq;
using Windows.UI.Xaml.Media.Imaging;
using Reactive.Bindings;

namespace Yarukizero.Net.MakiMoki.Uno.UnoModels {
	class ImageResolver {
		private readonly Helpers.ConnectionQueue<BitmapSource> imageQueue = new Helpers.ConnectionQueue<BitmapSource>(
			name: "UnoイメージQueue",
			maxConcurrency: 12,
			forceWait: true);
		private volatile Dictionary<string, WeakReference<BitmapSource>> imageDic
			= new Dictionary<string, WeakReference<BitmapSource>>();

		private bool TryGetImage(string file, out BitmapSource image) {
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

		private BitmapSource SetImage(string file, BitmapSource image) {
			lock(imageDic) {
				var r = new WeakReference<BitmapSource>(image);
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


		public IObservable<BitmapSource> Get(string url) {
			if(TryGetImage(url, out var src)) {
				return Observable.Return(src)
					.ObserveOn(UIDispatcherScheduler.Default);
			} else {
				return this.imageQueue.Push(Helpers.ConnectionQueueItem<BitmapSource>.From(
					async o => {
						try {
							var r = await App.HttpClient.GetByteArrayAsync(url);
							// TODO: DBに保存処理
							Observable.Return(r)
								.ObserveOn(UIDispatcherScheduler.Default)
								.Subscribe(async x => {
									var bi = new BitmapImage();
									await bi.SetSourceAsync(new System.IO.MemoryStream(x));
									o.OnNext(this.SetImage(url, bi));
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
