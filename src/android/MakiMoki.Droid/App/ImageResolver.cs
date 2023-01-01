using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Reactive.Linq;
using Reactive.Bindings;
using Android.Graphics;
using Android.Graphics.Drawables;

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
				return MakiMokiApplication.Current.MakiMoki.Db.Connect()
					.SelectMany(con => {
						var image = con.Table<DroidData.Db.ImageTable>().FirstOrDefault(x => x.Url == url);
						if(image == null) {
							return Observable.Create<byte[]?>(async o => {
								try {
									o.OnNext(await MakiMokiApplication.Current.MakiMoki.HttpClient.GetByteArrayAsync(url));
								}
								catch(Exception e) {
									System.Diagnostics.Debug.WriteLine(e);
									var @throw = e switch {
										System.Net.Http.HttpRequestException _ => false,
										System.Threading.Tasks.TaskCanceledException _ => false,
										_ => true,
									};
									if(@throw) {
										o.OnNext(null);
										o.OnError(e);
									}
								}
								finally {
									o.OnCompleted();
								}
							}).ObserveOn(MakiMokiApplication.Current.MakiMoki.Db.DbScheduler)
							.Select(x => {
								if(x != null) {
									con.Insert(new DroidData.Db.ImageTable(url, x));
								}
								return x;
							});
							/*
							return this.imageQueue.Push(Helpers.ConnectionQueueItem<(SQLite.SQLiteConnection Con, byte[]? Data)>.From(async o => {
								try {
									o.OnNext((con, await MakiMokiApplication.Current.MakiMoki.HttpClient.GetByteArrayAsync(url)));
								}
								catch(Exception e) {
									System.Diagnostics.Debug.WriteLine(e);
									var @throw = e switch {
										System.Net.Http.HttpRequestException _ => false,
										System.Threading.Tasks.TaskCanceledException _ => false,
										_ => true,
									};
									if(@throw) {
										o.OnNext((con, null));
										o.OnError(e);
									}
								}
								finally {
									o.OnCompleted();
								}
							})).ObserveOn(MakiMokiApplication.Current.MakiMoki.Db.DbScheduler)
							.Select(x => {
								if(x.Data != null) {
									x.Con.Insert(new DroidData.Db.ImageTable(url, x.Data));
								}
								return x.Data;
							});
							*/
						} else {
							return Observable.Return(image.Data);
						}
					}).ObserveOn(System.Reactive.Concurrency.ThreadPoolScheduler.Instance)
					.Select(x => {
						if(x == null) {
							return null;
						} else {
							using var s = new MemoryStream(x);
							return (Bmp: BitmapFactory.DecodeStream(s), Resize: droidImageSize) switch {
								var y when y.Resize is not null => resize(y.Bmp, y.Resize.Value),
								var y => y.Bmp,
							};
						}
					}).ObserveOn(UIDispatcherScheduler.Default)
					.Select(x => x switch {
						Bitmap b => this.SetImage(url, x),
						_ => null
					});
			}
		}
	}
}
